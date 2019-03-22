using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;
using System.IO;
using GIS.Toplogical;
using GIS.GeoData;
//using OSGeo.OGR;

namespace GIS.TreeIndex.Index
{

    #region Nested type: GeoObjects
    public class GeoObjects
    {

        #region new
        public GeoDataRow Row;
        public GeoObjects ParentObj;        // 需要测试Parent，确保是所有候选Parent中Box最小的候选者
        public GeoLinearRing RingInParent;        //在父多边形中对应的环
        public List<GeoObjects> Childrens;        //子多边形

        #region properties
        /// <summary>
        /// 当前对象Bound
        /// </summary>
        public GeoBound Box
        {
            get { return Row.Geometry.Bound; }
            set { Row.Geometry.Bound = value; }
        }
        /// <summary>
        /// 当前对象PlgID
        /// </summary>
        public int ID
        {
            get { return System.Convert.ToInt32(Row["PlgID"]); }    // Column 2
            set { Row["PlgID"] = value; }
        }
        /// <summary>
        /// 当前对象多边形
        /// </summary>
        public GeoPolygon CurrentPolygon
        {
            get { return Row.Geometry as GeoPolygon; }
            set { Row.Geometry = value as  Geometry; }   //留意此处可能存在赋值不成功的情况！！！！
        }
        /// <summary>
        /// 父多边形
        /// </summary>
        public GeoPolygon ParentPolygon
        {
            get { return ParentObj.Row.Geometry as GeoPolygon; }
            set { ParentObj.Row.Geometry = value as Geometry; }   //留意此处可能存在赋值不成功的情况！！！！
        }
        /// <summary>
        /// 父多边形PlgID
        /// </summary>
        public int ParentID
        {
            get { return System.Convert.ToInt32(ParentObj.Row["PlgID"]); }
            set { ParentObj.Row["PlgID"] = value; }
        }
        #endregion
        #endregion

        #region 构造函数
        public GeoObjects(GeoDataRow r)
        {
            Row = r;
            ParentObj = null;
            RingInParent = null;
            Childrens = new List<GeoObjects>();
        }
        #endregion 
        public GeoObjects Clone()
        {
            GeoObjects geoObjects = new GeoObjects(Row);
            geoObjects.ParentObj = ParentObj;
            geoObjects.RingInParent = RingInParent;
            geoObjects.Childrens = new List<GeoObjects>();
            for (int i = 0; i < Childrens.Count(); i++) 
            {
                geoObjects.Childrens.Add(Childrens[i]);
            }
            return geoObjects;
        }
        public void Dispose()
        {
            for (int i = 0; i < CurrentPolygon.InteriorRings.Count; i++)
                CurrentPolygon.InteriorRings.Clear();
            Row.Geometry = null;
            Row.Delete();
            ParentObj = null;
            RingInParent = null;
            Childrens.Clear();
            Childrens = null;
        }

        #region 打印子多边形，zh修改，2018年1月15日
        public string printChildrens()
        {
            string obj = "";
            if (Childrens != null&&Childrens.Count>0)
            {
                for (int i = 0; i < Childrens.Count - 1; i++)
                {
                    obj = obj + Childrens[i].ID + ",";
                }
                obj = obj + Childrens[Childrens.Count - 1].ID;
            }
            return obj;
        }
        #endregion
    }


    #endregion



    public class QuadTreeNode 
    {

        #region PrivateMember
        private int m_ID;  //结点编号,在存储四叉树时使用
        private int m_NodeType; //结点类型，一种为树结点（中间结点0），另一种为对象结点（叶子1）
        private GeoBound m_Bound; //当前四叉树结点的象限矩形；
        private GeoBound o_Bound; //当前结点对象和的外包矩形
        /// <summary>
        /// 若为对象结点，则四个子树为空    按Z-Order顺序
        /// </summary>
        private QuadTreeNode m_ChildNW;//西北方向的子树     1
        private QuadTreeNode m_ChildNE;//东北方向的子树     2 
        private QuadTreeNode m_ChildSW;//西南方向的子树     3
        private QuadTreeNode m_ChildSE;//东南方向的子树     4  
        private int m_Depth;      //节点的深度
        public List<GeoObjects> m_objList = null;///当前节点包含的对象
        public QuadTreeNode PNode;//指向父结点
        #endregion

        #region StaticMember
        public static int treeDepth = 0;   //共享变量，树的深度
        public static int NodeCount = 0;   //共享变量，结点计数器，树中结点总数
        public static int GetTeeDepth()
        {
            return treeDepth;
        }
        public static int GetNodeCount()
        {
            return NodeCount;
        }
        #endregion


        #region Properties
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }
        public int NodeType
        {
            get { return m_NodeType; }
            set { m_NodeType = value; }
        }
        public int Depth
        {
            get { return m_Depth; }
            set { m_Depth = value; }
        }
        public QuadTreeNode ChildNW
        {
            get { return m_ChildNW; }
            set { m_ChildNW = value; }
        }
        public QuadTreeNode ChildSW
        {
            get { return m_ChildSW; }
            set { m_ChildSW = value; }
        }
        public GeoBound mBound
        {
            get { return m_Bound; }
            set { m_Bound = value; }
        }
        public GeoBound oBound
        {
            get { return o_Bound; }
            set { o_Bound = value; }
        }
        public QuadTreeNode ChildSE
        {
            get { return m_ChildSE; }
            set { m_ChildSE = value; }
        }
        public QuadTreeNode ChildNE
        {
            get { return m_ChildNE; }
            set { m_ChildNE = value; }
        }
        #endregion


        ///
        ///树的构造过程：
        ///首先将所有多边形对象放在对象结点（根）中
        ///然后判断对象个数大于阈值时，则分裂对象结点（将原结点变为树的中间结点，将多边形对象筛选到四个孩子结点中）

        /// <summary>
        /// 对象结点构造函数
        /// </summary>
        public QuadTreeNode()
        {
            m_ID = NodeCount++;
            m_NodeType = 1;
            m_Bound = null;
            o_Bound = null;
            m_ChildNE = null;
            m_ChildNW = null;
            m_ChildSE = null;
            m_ChildSW = null;
            m_Depth = 1;
            m_objList =new List<GeoObjects>();
            PNode = null;
        }



        #region Function

        //根据当前节点的范围，确定被插入对象的位置
        //返回 1 为 西北NW， 2为东北NE ， 3为西南， 4为东南    Z-Order
        /// <summary>
        /// -------------
        /// |  1  |  2  |
        /// -------------
        /// |  3  |  4  |
        /// -------------
        /// </summary>
        private QuadTreeNode GetInsertedTree(GeoObjects obj)     //判断对象obj应插入到当前结点的位置
        {
            GeoBound objBox = obj.Box;
            if (NodeType == 1)   //如果当前结点是叶子结点
                return this;
            else         // 如果是树中间结点，则判断插入的哪一棵子树，并不递归寻找最终位置
            if (GetChildBound(1).Contains(objBox))
                return m_ChildNW;
            else if(GetChildBound(2).Contains(objBox))
                return m_ChildNE;
            else if(GetChildBound(3).Contains(objBox))
                return m_ChildSW;
            else if(GetChildBound(4).Contains(objBox))
                return m_ChildSE;
            else
                return this;//返回0，即将当前对象与中轴线相交，应留在结点中，不分配到子树中
        }


        ///获取子树边界矩形
        private GeoBound GetChildBound(int iDrection)
        {
            GeoPoint centPt = m_Bound.GetCentroid();
            if (iDrection == 1)
                return new GeoBound(m_Bound.Left, centPt.Y, centPt.X, m_Bound.Top);
            else if (iDrection == 2)
                return new GeoBound(centPt, m_Bound.RightUpPt);
            else if (iDrection == 3)
                return new GeoBound(m_Bound.LeftBottomPt, centPt);
            else if (iDrection == 4)
                return new GeoBound(centPt.X, m_Bound.Bottom, m_Bound.Right, centPt.Y);
            else
                return this.m_Bound;
        }

        //结点分裂，对于叶子结点进行判断，如果深度不达给定阈值且对象数目超限，则进行结点分裂
        public void Split(Heuristic heurdata)
        {
            if (NodeType == 1 && m_Depth < heurdata.maxdepth && m_objList.Count > heurdata.mintricnt)
            {
                NodeType = 0;//改类型为树结点
                m_ChildNW = new QuadTreeNode();//生成子树（对象结点）
                m_ChildNE = new QuadTreeNode();
                m_ChildSW = new QuadTreeNode();
                m_ChildSE = new QuadTreeNode();
                m_ChildNW.PNode = this;
                m_ChildNE.PNode = this;
                m_ChildSW.PNode = this;
                m_ChildSE.PNode = this;
                m_ChildNW.m_Bound = GetChildBound(1);//所在象限矩形（为上级矩形四分之一）
                m_ChildNE.m_Bound = GetChildBound(2);
                m_ChildSW.m_Bound = GetChildBound(3);
                m_ChildSE.m_Bound = GetChildBound(4);

                m_ChildNW.Depth = Depth + 1;//各对象结点深度加1
                m_ChildNE.Depth = Depth + 1;
                m_ChildSW.Depth = Depth + 1;
                m_ChildSE.Depth = Depth + 1;
                if (Depth + 1 > treeDepth)
                    treeDepth = Depth + 1;
                NodeCount = NodeCount + 4;

                ///
                /// 将当前结点中的对象筛选到子树中，先判断属于哪个象限，并将其加入到相应树中
                for (int i = 0; i < m_objList.Count; i++)
                {
                    QuadTreeNode disNode = GetInsertedTree(m_objList[i]);
                    if (disNode == this)   //与中轴线相交的多边形不需要筛选到子树中去
                        continue;
                    disNode.m_objList.Add(m_objList[i]);//对象复制到相应子树结点中
                    
                    //将当前对象的MBR合并到相应子树中的o_Bound
                    if (disNode.o_Bound == null)
                    {
                        disNode.o_Bound = m_objList[i].Box.Clone();
                    }
                    else
                        disNode.o_Bound.UnionBound(m_objList[i].Box);
                    m_objList.RemoveAt(i);  //将其从当前树结点中删除
                    i--;
                }

                m_ChildNE.Split(heurdata);//子树结点（对象结点）分裂
                m_ChildSE.Split(heurdata);
                m_ChildSW.Split(heurdata);
                m_ChildNW.Split(heurdata);
            }
        }

        public void Coalesce(Heuristic heurdata)//从当前结点开始向上合并结点
        {
            //if (NodeType == 1)
            if (ChildNE == null && ChildNW == null && ChildSE == null && ChildSW == null) 
            {
                QuadTreeNode parentNode = this.PNode;
                //if (parentNode.ChildNE.NodeType == 1 && parentNode.ChildNW.NodeType == 1 && parentNode.ChildSE.NodeType == 1 && parentNode.ChildSW.NodeType == 1)
                if((parentNode.ChildNE.ChildNE==null&&parentNode.ChildNE.ChildNW==null&&parentNode.ChildNE.ChildSE==null&&parentNode.ChildNE.ChildSW==null)&&
                   (parentNode.ChildNW.ChildNE==null&&parentNode.ChildNW.ChildNW==null&&parentNode.ChildNW.ChildSE==null&&parentNode.ChildNW.ChildSW==null)&&
                   (parentNode.ChildSE.ChildNE==null&&parentNode.ChildSE.ChildNW==null&&parentNode.ChildSE.ChildSE==null&&parentNode.ChildSE.ChildSW==null)&&
                   (parentNode.ChildSW.ChildNE==null&&parentNode.ChildSW.ChildNW==null&&parentNode.ChildSW.ChildSE==null&&parentNode.ChildSW.ChildSW==null))
                {
                    if ((parentNode.ChildNE.m_objList.Count + parentNode.ChildNW.m_objList.Count + parentNode.ChildSE.m_objList.Count + parentNode.ChildSW.m_objList.Count) < heurdata.mintricnt)
                    {
                        for (int i = parentNode.ChildNE.m_objList.Count - 1; i >= 0; i--)
                        {
                            parentNode.m_objList.Add(parentNode.ChildNE.m_objList[i]);
                            parentNode.ChildNE.m_objList.RemoveAt(i);
                        }
                        for (int i = parentNode.ChildNW.m_objList.Count - 1; i >= 0; i--)
                        {
                            parentNode.m_objList.Add(parentNode.ChildNW.m_objList[i]);
                            parentNode.ChildNW.m_objList.RemoveAt(i);
                        }
                        for (int i = parentNode.ChildSE.m_objList.Count - 1; i >= 0; i--)
                        {
                            parentNode.m_objList.Add(parentNode.ChildSE.m_objList[i]);
                            parentNode.ChildSE.m_objList.RemoveAt(i);
                        }
                        for (int i = parentNode.ChildSW.m_objList.Count - 1; i >= 0; i--)
                        {
                            parentNode.m_objList.Add(parentNode.ChildSW.m_objList[i]);
                            parentNode.ChildSW.m_objList.RemoveAt(i);
                        }
                        parentNode.ChildNE = null;
                        parentNode.ChildNW = null;
                        parentNode.ChildSE = null;
                        parentNode.ChildSW = null;
                        parentNode.NodeType = 1;
                        NodeCount = NodeCount - 4;
                        //parentNode.Coalesce(heurdata);
                    }
                }
            }
        }

        //在树中插入一个对象，判断两种插入位置，一种为叶子结点，另一种为树中间结点，
        public void Insert(GeoObjects obj, Heuristic heurdata)
        {
            if (o_Bound == null)
            {
                o_Bound = obj.Box.Clone();
            }
            else
                o_Bound.UnionBound(obj.Box);    //将插入对象MBR并入当前结点对象MBR和
            if (NodeType == 1) //如果是叶子结点，则直接插入对象，并结点分裂
            {
                m_objList.Add(obj);
                Split(heurdata);
            }
            else
            {
                QuadTreeNode node = GetInsertedTree(obj);  //判断插入对象插入位置

                if (node == this)     //如果插入对象MBR不被子树包含
                    node.m_objList.Add(obj);
                else
                    node.Insert(obj, heurdata);//插入到相应子树中
            }
        }


        /// <summary>
        /// 在索引树上找到obj对象，并返回在索引树上的位置
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public GeoObjects Search(GeoObjects obj)
        {
            GeoObjects gobj = null;
            if (!o_Bound.Contains(obj.Box))    
            {
                return gobj;
            }
            else
            {
                for (int i = 0; i < m_objList.Count; i++)//在当前结点中查找，若找不到在子树中找
                {
                    if (m_objList[i].ID == obj.ID)
                    {
                        gobj = m_objList[i];
                        return gobj;
                    }
                }
                if (NodeType == 0)
                {
                    if (ChildNE.oBound.Contains(obj.Box))
                        gobj = ChildNE.Search(obj);
                    else if (ChildSE.oBound.Contains(obj.Box))
                        gobj = ChildSE.Search(obj);
                    else if (ChildSW.oBound.Contains(obj.Box))
                        gobj = ChildSW.Search(obj);
                    else if (ChildNW.oBound.Contains(obj.Box))
                        gobj = ChildNW.Search(obj);
                }
                return gobj;
            }
        }
        /// <summary>
        /// 在索引树上找到指定多边形对象，并返回在索引树上的位置
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public GeoObjects Search(GeoPolygon g)
        {
            GeoObjects gobj = null;
            for (int i = 0; i < m_objList.Count; i++)//在当前结点中查找，若找不到在子树中找
            {
                if (m_objList[i].CurrentPolygon == g)
                {
                    gobj = m_objList[i];
                    return gobj;
                }
            }
            if (NodeType == 0)
            {
                if (ChildNE.oBound.Contains(g.Bound))
                    gobj = ChildNE.Search(g);
                else if (ChildSE.oBound.Contains(g.Bound))
                    gobj = ChildSE.Search(g);
                else if (ChildSW.oBound.Contains(g.Bound))
                    gobj = ChildSW.Search(g);
                else if (ChildNW.oBound.Contains(g.Bound))
                    gobj = ChildNW.Search(g);
            }
            return gobj;

        }

        public List<GeoObjects> IntersectQuery(GeoPolygon g, Heuristic heurdata)//带heurdata是为了将不相交（只是MBR相交）的多边形再次插入到索引中，heurdata是插入所需参数
        {
            List<GeoObjects> gg = IntersectQueryIncludeFake(g, heurdata);//返回与树中g相交的多边形，包含假相交的情形（多边形g包含在gg的内环中，不算真相交）
            gg = ExcludeFakeIntersect(gg, g, heurdata);
            return gg;
        }

        public List<GeoObjects> IntersectQueryIncludeFake(GeoPolygon g, Heuristic heurdata)//带heurdata是为了将不相交（只是MBR相交）的多边形再次插入到索引中，heurdata是插入所需参数
        {//在树中查找与增量多边形相交的多边形，并将其从树中删除
            List<GeoObjects> gg = new List<GeoObjects>();
            if (!oBound.IsIntersectWith(g.Bound))
                return gg; 
            OSGeo.OGR.Geometry ogr_g = Converters.GeoConverter.GeometryConverter.GeometricToOGR(g as Geometry);
            for (int i = 0; i < m_objList.Count; i++)//在当前结点中查找，若找不到在子树中找
            {
                GeoObjects s = m_objList[i];
                if (s.Box.IsIntersectWith(g.Bound))
                {
                    GeoPolygon ex_s = new GeoPolygon(s.CurrentPolygon.ExteriorRing);
                    OSGeo.OGR.Geometry ogr_s = Converters.GeoConverter.GeometryConverter.GeometricToOGR(ex_s as Geometry);
                    if (ogr_s.Intersect(ogr_g))//&&!ogr_s.Touches(ogr_g))//排除相离、相接关系
                    {
                        gg.Add(s);
                        //s.ParentObj.Childrens.Remove(s);//将其从父结点的孩子列表中删除
                        m_objList.RemoveAt(i);//将s从索引中删除
                        i--;
                    }
                }
            }
            if (this != null && NodeType == 0)//在子树中找
            {
                List<GeoObjects> g1 = new List<GeoObjects>();
                List<GeoObjects> g2 = new List<GeoObjects>();
                List<GeoObjects> g3 = new List<GeoObjects>();
                List<GeoObjects> g4 = new List<GeoObjects>();
                if (ChildNW != null && ChildNW.oBound != null && ChildNW.oBound.IsIntersectWith(g.Bound))
                {
                    g1 = ChildNW.IntersectQueryIncludeFake(g, heurdata);
                    for (int j = 0; j < g1.Count; j++)
                        gg.Add(g1[j]);
                }
                if (ChildNE != null && ChildNE.oBound != null && ChildNE.oBound.IsIntersectWith(g.Bound))
                {
                    g2 = ChildNE.IntersectQueryIncludeFake(g, heurdata);
                    for (int j = 0; j < g2.Count; j++)
                        gg.Add(g2[j]);
                }
                if (ChildSW != null && ChildSW.oBound != null && ChildSW.oBound.IsIntersectWith(g.Bound))
                {
                    g3 = ChildSW.IntersectQueryIncludeFake(g, heurdata);
                    for (int j = 0; j < g3.Count; j++)
                        gg.Add(g3[j]);
                }
                if (ChildSE != null && ChildSE.oBound != null && ChildSE.oBound.IsIntersectWith(g.Bound))
                {
                    g4 = ChildSE.IntersectQueryIncludeFake(g, heurdata);
                    for (int j = 0; j < g4.Count; j++)
                        gg.Add(g4[j]);
                }
            }
            //if (NodeType == 1)
            //    Coalesce(heurdata);
            return gg;
        }


        private List<GeoObjects> ExcludeFakeIntersect(List<GeoObjects> gg, GeoPolygon g, Heuristic heurdata)
        {
            //开始排除增量多边形MBR相交，但与增量多边形并未实质相交的情形
            OSGeo.OGR.Geometry ogr_g = Converters.GeoConverter.GeometryConverter.GeometricToOGR(g as Geometry);
            List<GeoObjects> ggs = new List<GeoObjects>();//存储真相交多边形
            for (int i = 0; i < gg.Count; i++)
            {
                if (gg[i].Childrens.Count == 0)//原目标无孩子
                {
                    ggs.Add(gg[i]);
                    continue;
                }
                bool flag = false;
                for (int j = 0; j < gg.Count; j++)
                {
                    GeoPolygon RingPolygonOfChild = new GeoPolygon(gg[j].RingInParent);//RingInParent构成多边形（环多边形）
                    OSGeo.OGR.Geometry ogr_childP = Converters.GeoConverter.GeometryConverter.GeometricToOGR(RingPolygonOfChild as Geometry);
                    if (gg[i] == gg[j].ParentObj && ogr_g.Intersect(ogr_childP) && ogr_g.Difference(ogr_childP).GetArea() == 0) //如果增量多边形与内环相交，且差的面积为0，即：排除增量多边形与内环的包含和覆盖关系
                    {                                                        //原为ogr_g.Intersection(ogr_childP).GetArea() == 0（2017-2-5改）
                        flag = true;
                        this.Insert(gg[i], heurdata);
                        // gg[i];//将取出来的GeoObject再插入索引中
                        break;
                    }
                }
                if (flag)
                    continue;
                else
                    ggs.Add(gg[i]);//与所有内环相离、与某一内环相交（非包含、非覆盖）等情形
            }
            return ggs;
        }
        public GeoObjects QueryByID(int n, ref int depth)
        {
            GeoObjects g = null;
            depth = 0;
            for (int i = 0; i < m_objList.Count; i++)
            {
                if (m_objList[i].ID == n)
                {
                    g = m_objList[i];
                    depth = m_Depth;
                }
            }
            if (NodeType == 0)
            {
                if (g == null && ChildNW != null)
                {
                    g = ChildNW.QueryByID(n, ref depth);
                }
                if (g == null && ChildNE != null)
                {
                    g = ChildNE.QueryByID(n, ref depth);
                }
                if (g == null && ChildSE != null)
                {
                    g = ChildSE.QueryByID(n, ref depth);
                }
                if (g == null && ChildSW != null)
                {
                    g = ChildSW.QueryByID(n, ref depth);
                }
            }
            return g;
        }
        #endregion

        #region 打印m_objList,zh修改 2018年1月15日
        public string print_m_objList()
        {
            string obj = "";
            if (m_objList != null)
            {
                for (int i = 0; i < m_objList.Count - 1; i++)
                {
                    obj = obj + m_objList[i].ID + ",";
                }
                obj = obj + m_objList[m_objList.Count - 1].ID;
            }
            return obj;
        }
        #endregion
    }
}
