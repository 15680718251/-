using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;
using System.IO;
using System.Runtime.InteropServices;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex;
using GIS.SpatialRelation;
using GIS.GeoData;
using GIS.Converters.GeoConverter;



namespace GIS.TreeIndex.Index
{
    #region Heuristics
    /// Heuristics设置树的参数
    public struct Heuristic
    {
        /// 最大深度
        public int maxdepth;
        /// 结点分裂最少对象个数
        public int mintricnt;

        public Heuristic(int depth, int tricnt)
        {
            maxdepth = depth;
            mintricnt = tricnt;
        }
    }
    #endregion


    public class QuadTreeIndex
    {
        [DllImport("kernel32")]
        static extern uint GetTickCount();//计时用
        /// <summary>
        /// The root of QuadTree
        /// </summary>
        private QuadTreeNode m_root = null;
//        MapUI m_MapUI;
        private GeoVectorLayer m_actlyr;//当前活动图层
        private int [] PlgIDToFID =new int[500000];    //索引维护需要刷新这些数组

        public Heuristic heurdata;
        
        #region Properties
        public QuadTreeNode Root
        {
            get { return m_root; }
        }
        public int Depth
        {
            get { return QuadTreeNode.treeDepth; }
        }
        public int TreeNodeNum
        {
            get { return QuadTreeNode.NodeCount; }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="objList">建立索引用的数组</param>
        /// <param name="heurd">索引阈值（树深度、结点分裂阈值）</param>
        /// <param name="actlyr">工作图层</param>
        public QuadTreeIndex(List<GeoObjects> objList, Heuristic heurd,GeoVectorLayer actlyr)
        {
            m_actlyr = actlyr;
            m_root = new QuadTreeNode();

            for (int k = 0; k < 500000; k++)
            {
                PlgIDToFID[k] = -1;
            }

            m_root.mBound =objList[0].Box.Clone();
            m_root.m_objList.Add(objList[0]);
            PlgIDToFID[objList[0].ID] = 0;
            heurdata = heurd;

            

            for (int i = 1; i < objList.Count; i++)    
            {
                m_root.mBound.UnionBound(objList[i].Box);//求全图的幅面矩形
                m_root.m_objList.Add(objList[i]);   //将所有对象加入根结点
                PlgIDToFID[objList[i].ID] = i;      //objList是按顺序从DataTable中读取出来的，所以有这样的对应关系

                
            }

            m_root.oBound = m_root.mBound.Clone();//根的对象外包和矩形幅面相同

            

            if(objList.Count > heurdata.mintricnt)
                m_root.Split(heurdata);           //结点分裂
            CreateComplexObjectIndex(objList,heurdata);//在索引上建立包含关系
            PlgIDToFID = null;
            GC.Collect();
        }
        public QuadTreeIndex(List<GeoObjects> objList, Heuristic heurd, GeoVectorLayer actlyr,bool topo)//重载构造函数
        {
            m_actlyr = actlyr;
            m_root = new QuadTreeNode();

            for (int k = 0; k < 500000; k++)
            {
                PlgIDToFID[k] = -1;
            }

            m_root.mBound = objList[0].Box.Clone();
            m_root.m_objList.Add(objList[0]);
            PlgIDToFID[objList[0].ID] = 0;
            heurdata = heurd;
            for (int i = 1; i < objList.Count; i++)
            {
                m_root.mBound.UnionBound(objList[i].Box);//求全图的幅面矩形
                m_root.m_objList.Add(objList[i]);   //将所有对象加入根结点
                PlgIDToFID[objList[i].ID] = i;      //objList是按顺序从DataTable中读取出来的，所以有这样的对应关系
            }
            m_root.oBound = m_root.mBound.Clone();//根的对象外包和矩形幅面相同

            if (objList.Count > heurdata.mintricnt)
                m_root.Split(heurdata);           //结点分裂
            PlgIDToFID = null;
            GC.Collect();
        }



        //建立有包含关系的索引
        private void CreateComplexObjectIndex(List<GeoObjects> objList, Heuristic heurdata)
        {
            for (int i = 0; i < objList.Count; i++)
            {
                FindParent(m_root,objList[i]);
            }
            WriteParentInfoToChildren(objList);
        }

        //从当前结点（根）开始向下查找obj的父多边形，并在obj中记录Parent
        private void FindParent(QuadTreeNode Node, GeoObjects obj)
        {

            if (!Node.mBound.Contains(obj.Box)) //如果当前结点不包含目标对象的box，则返回
            {
                return;
            }
            for (int i = 0; i < Node.m_objList.Count; i++)
            {   //判断每个对象结点外包矩形是否包含目标对象的外包矩形，若包含才可能是其父结点
                if (Node.m_objList[i].ID != obj.ID && Node.m_objList[i].Box.Contains(obj.Box))
                {
                    GeoLinearRing r = null;
                    if (IsInclude(Node.m_objList[i], obj,ref r)) 
                    {
                        if (obj.ParentObj == null || obj.ParentPolygon.Bound.Contains(Node.m_objList[i].Box))//找到一个比已存储的父多边形还要小的父多边形
                        {
                            obj.ParentObj = Node.m_objList[i];
                            obj.RingInParent = r;
                        }
                    }
                }
            }
            if (Node.NodeType == 0)    //如果当前结点是树中间结点，则还需要分别在四个子树中进行递归查找
            {
                FindParent(Node.ChildNE, obj);
                FindParent(Node.ChildSE, obj);
                FindParent(Node.ChildSW, obj);
                FindParent(Node.ChildNW, obj);
            }
        }

        private void WriteParentInfoToChildren(List<GeoObjects> objList)
        {
            for (int i = 0; i < objList.Count; i++)
                if (objList[i].ParentObj != null)
                {
                    int j = PlgIDToFID[objList[i].ParentID];
                    GeoObjects g = objList[i];
                    objList[j].Childrens.Add(g);
                }
        }

        private bool IsInclude(GeoObjects Parent, GeoObjects Child,ref GeoLinearRing r)//能确定parent包含Child，但不能确定是不是直接包含
        {
            GeoPolygon plg1 = Parent.CurrentPolygon;
            GeoPolygon plg2 = Child.CurrentPolygon;
            for (int i = 0; i < plg1.InteriorRings.Count; i++)
            {
                if (plg1.InteriorRings[i].Bound.IsIntersectWith(plg2.Bound))
                {
                    if (plg2.ExteriorRing.IsEqual(plg1.InteriorRings[i]))
                        return true;
                    //GeoPolygon p1 = new GeoPolygon(plg1.InteriorRings[i]);
                    //OSGeo.OGR.Geometry ogr_poly = GeometryConverter.GeometricToOGR(p1 as Geometries.Geometry);
                    //OSGeo.OGR.Geometry ogr_point = GeometryConverter.GeometricToOGR(plg2.ExteriorRing.Vertices[0] as Geometries.Geometry);
                    if (GeoAlgorithm.IsInLinearRingWHS(plg2.ExteriorRing.Vertices[0], plg1.InteriorRings[i]) >= 1)
                    {
                        r = plg1.InteriorRings[i];
                        return true;
                    }
                    //if ((GeoAlgorithm.IsInLinearRingWHS(plg2.ExteriorRing.Vertices[0], plg1.InteriorRings[i]) >= 1) != (ogr_point.Distance(ogr_poly) < Geometry.EPSIONAL))
                    //{
                    //    int testdata = 0;
                    //}
                }
            }
            r = null;
            return false;
        }

        /// <summary>
        /// 在树中插入一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="heurdata"></param>
        public void Insert(GeoObjects obj)   
        {
            m_root.Insert(obj, heurdata);
        }
    }

}
