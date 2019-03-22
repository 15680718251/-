using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Geometries;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.Layer;
using System.IO;
using System.Threading;
using GIS.TreeIndex.Index;
using GIS.GeoData;
using System.Data;
//oracle的引用
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {   
        //
        // 摘要:
        //    创建四叉树索引。
        public void CreateQuadTreeIndex()
        {
            //OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            //UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("开始建立索引..................................\r\n");
            //this.Invoke(evt, null, e1);

            List<GeoObjects> objList = new List<GeoObjects>();
            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null)||(actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return ;
            }
            int objNum = actlyr.DataTable.Count;
            for (int i = 0; i < objNum; i++)
            {
                //GeoObjects boxObject = new GeoObjects();
                //boxObject.CurrentPolygon = actlyr.DataTable[i].Geometry as GeoPolygon;
                //boxObject.ID = System.Convert.ToInt32(actlyr.DataTable.Rows[i]["PlgID"]);
                #region test
                //GeoData.GeoDataRow r = actlyr.DataTable[i];
                //int iColumnCount = actlyr.DataTable.Columns.Count;
                //string columnName1 = actlyr.DataTable.Columns[0].ColumnName;
                //string columnName2 = actlyr.DataTable.Columns[1].ColumnName;
                //string columnName3 = actlyr.DataTable.Columns[2].ColumnName;
                //string columnName4 = actlyr.DataTable.Columns[3].ColumnName;
                //string id1 = r[0].ToString(); //FID
                //string id2 = r[1].ToString(); //PlgAttr
                //string id3 = r[2].ToString(); //PlgID
                //string id4 = r[3].ToString(); //Changed
                #endregion
                GeoDataRow r = actlyr.DataTable[i];
                GeoObjects go = new GeoObjects(r);
                objList.Add(go);
            }
            Heuristic heurdata = new Heuristic(10,30);
            ////时间测试
            DateTime startTime = DateTime.Now;
            QuadIndex = new QuadTreeIndex(objList, heurdata, actlyr);  //MapUI成员

            //try
            //{
            //    QuadIndex = new QuadTreeIndex(objList, heurdata, actlyr);  //MapUI成员
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("索引建立错误： " + e.Message);
            //}
            ////时间测试
            DateTime stopTime = DateTime.Now;
            TimeSpan elapsedTime = stopTime - startTime;

            #region 连接oracle 写入索引文件 zh修改
            //string connectionString;
            //connectionString = "User Id=system;Password=Zh522700;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.92)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=myoracle)))";
            //OracleConnection myConnection = new OracleConnection(connectionString);
            //myConnection.Open();
            


            //zh 20180712 如果存在表就删除重建
            createIndexTable();

            //zh 20180713 建立插入geoobjects表的进程
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection myConnection = db.getOracleConnection();
            myConnection.Open();
            string sql = @"create or replace procedure insertGEO
(v_id  number,
v_pid number,
Childrens clob,
RingInParent clob)
is
  content clob;
  ring clob;
begin
  content := Childrens;
  ring:=RingInParent;
  insert into geoobjects(PLGID,PARENTID,CHILDRENLIST,RINGINPARENT) values(v_id,v_pid,content,ring );
  update geoobjects set CHILDRENLIST = content,RINGINPARENT=ring where PLGID = v_id;
commit;
end;";
            OracleCommand cmd = new OracleCommand(sql, myConnection);
            cmd.ExecuteScalar();//执行command命令

            //写入quadtreeindex表
            string queryString = String.Format(
                    "insert into QUADTREEINDEX(M_ROOT,MAXDEPTH,MINTRICNT) values({0},{1},{2})"
                    , QuadIndex.Root.ID
                    , QuadIndex.heurdata.maxdepth
                    , QuadIndex.heurdata.mintricnt);
            OracleCommand command = new OracleCommand(queryString, myConnection);
            command.ExecuteScalar();//执行command命令

            //写入quadtreenode表
            proOrder(QuadIndex.Root, myConnection);

            myConnection.Close();
            #endregion

            //UIEventArgs.OutPutEventArgs e2 = new GIS.UI.UIEventArgs.OutPutEventArgs("Elapsed: " + elapsedTime+"\n");
            //this.Invoke(evt, null, e2);
            //UIEventArgs.OutPutEventArgs e3 = new GIS.UI.UIEventArgs.OutPutEventArgs("in seconds: " + elapsedTime.TotalSeconds + "\n");
            //this.Invoke(evt, null, e3);
            //UIEventArgs.OutPutEventArgs e4 = new GIS.UI.UIEventArgs.OutPutEventArgs("in minutes: " + elapsedTime.TotalMinutes + "\n");
            //this.Invoke(evt, null, e4);
            //UIEventArgs.OutPutEventArgs e5 = new GIS.UI.UIEventArgs.OutPutEventArgs("in hours: " + elapsedTime.TotalHours + "\n");
            //this.Invoke(evt, null, e5);

            ////时间测试 
            //UIEventArgs.OutPutEventArgs e6 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("索引建立完成..................................\r\n");
            //this.Invoke(evt, null, e6);

            //UIEventArgs.OutPutEventArgs e3 = new GIS.UI.UIEventArgs.OutPutEventArgs("开始储存索引..................................\r\n");
            //this.Invoke(evt, null, e3);
            //SaveMapIndex(mapIndex.Root);
            //UIEventArgs.OutPutEventArgs e4 = new GIS.UI.UIEventArgs.OutPutEventArgs("索引存储完成..................................\r\n");
            //this.Invoke(evt, null, e4);
            string idxHeight = QuadIndex.Depth.ToString();
            string idxNdNum = QuadIndex.TreeNodeNum.ToString();
            string ndAvgNum = (objNum / QuadIndex.TreeNodeNum).ToString();
            string crtIdxTime = elapsedTime.ToString();
            FrmCreateIndex frmCreateIndex = new FrmCreateIndex(idxHeight, idxNdNum, ndAvgNum, crtIdxTime);
            frmCreateIndex.ShowDialog();
            MessageBox.Show("根结点中对象数量为：" + QuadIndex.Root.m_objList.Count().ToString()
                          + " 四个子结点中对象数量分别为" + QuadIndex.Root.ChildNE.m_objList.Count().ToString() +"  "
                                                          + QuadIndex.Root.ChildNW.m_objList.Count().ToString()+"  "
                                                          + QuadIndex.Root.ChildSE.m_objList.Count().ToString() + "  "
                                                          + QuadIndex.Root.ChildSW.m_objList.Count().ToString());
        }

        //zh 20180712 创建索引表,如果存在就删除重建
        public void createIndexTable()
        {
            OracleDBHelper db = new OracleDBHelper();
            
            
            string tableName = "";
            //建表quadtreeindex
            tableName = "quadtreeindex";
            string createTableSql1 = "(m_root number,maxdepth number,mintricnt number)";
            db.createTable(tableName, createTableSql1);

            //建表quadtreenode
            tableName = "quadtreenode";
            createTableSql1 = "(m_id number primary key NOT NULL,m_nodetype number,m_depth number,m_childnw number,m_childne number,m_childsw number,m_childse number,pnode number,m_bound varchar2(80),o_bound varchar2(80),m_objlist varchar2(2000))";
            db.createTable(tableName, createTableSql1);

            //建表geoobjects
            tableName = "geoobjects";
            createTableSql1 = "(plgid number primary key NOT NULL,parentid number,childrenlist clob,ringinparent clob)";
            db.createTable(tableName, createTableSql1);
        }

        public void CreateQuadTreeIndexWithNoTp()
        {
            OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("开始建立索引..................................\r\n");
            this.Invoke(evt, null, e1);

            List<GeoObjects> objList = new List<GeoObjects>();
            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return;
            }
            int objNum = actlyr.DataTable.Count;
            for (int i = 0; i < objNum; i++)
            {
                GeoDataRow r = actlyr.DataTable[i];
                GeoObjects go = new GeoObjects(r);
                objList.Add(go);
            }
            Heuristic heurdata = new Heuristic(10, 10);
            try
            {
                QuadIndex = new QuadTreeIndex(objList, heurdata, actlyr,false);  //MapUI成员
            }
            catch (Exception e)
            {
                MessageBox.Show("索引建立错误： " + e.Message);
            }
            MessageBox.Show("根结点中对象数量为：" + QuadIndex.Root.m_objList.Count().ToString());
            UIEventArgs.OutPutEventArgs e6 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("索引建立完成..................................\r\n");
            this.Invoke(evt, null, e6);

        }

        #region 遍历索引四叉树 zh修改 2018年1月10日
        public void proOrder(QuadTreeNode node,OracleConnection myConnection)
        {
            if (node != null) 
            {
                int pNodeID=-1, ChildNWID=-1, ChildNEID=-1, ChildSWID=-1, ChildSEID = -1;
                if (node.PNode != null)
                {
                    pNodeID = node.PNode.ID;
                }
                if (node.ChildNW != null)
                {
                    ChildNWID = node.ChildNW.ID;
                }
                if (node.ChildNE != null)
                {
                    ChildNEID = node.ChildNE.ID;
                }
                if (node.ChildSW != null)
                {
                    ChildSWID = node.ChildSW.ID;
                }
                if (node.ChildSE != null)
                {
                    ChildSEID = node.ChildSE.ID;
                }
                string queryString = String.Format(
                    "insert into QUADTREENODE(m_ID,m_NodeType,m_Depth,pNode,M_CHILDNW,M_CHILDNE,M_CHILDSW,M_CHILDSE,M_BOUND,O_BOUND,M_OBJLIST) values({0},{1},{2},{3},{4},{5},{6},{7},'{8}','{9}','{10}')"
                    ,node.ID
                    ,node.NodeType
                    ,node.Depth
                    , pNodeID
                    , ChildNWID
                    , ChildNEID
                    , ChildSWID
                    , ChildSEID
                    ,node.mBound.toString()
                    ,node.oBound.toString()
                    , node.print_m_objList());//---------------------------------------------写入m_objList时可能超过数据库m_objList字段字符串的最大个数限制
                OracleCommand command = new OracleCommand(queryString, myConnection);
                command.ExecuteScalar();//执行command命令

                try
                {
                    for (int i = 0; i < node.m_objList.Count; i++)
                    {
                        int parentID = -1;
                        if (node.m_objList[i].ParentObj != null)
                        {
                            parentID = node.m_objList[i].ParentID;
                        }

                        //string queryString1 = String.Format(
                        //"insert into GEOOBJECTS(PLGID,PARENTID,CHILDRENLIST) values({0},{1},'{2}')"
                        //, node.m_objList[i].ID
                        //, parentID
                        //, node.m_objList[i].printChildrens()
                        //);//---------------------------------------------写入childrens时可能超过数据库m_objList字段字符串的最大个数限制
                        //OracleCommand command1 = new OracleCommand(queryString1, myConnection);
                        //command1.ExecuteScalar();//执行command命令

                        //执行储存过程
                        OracleCommand om = myConnection.CreateCommand();
                        om.CommandType = CommandType.StoredProcedure;
                        om.CommandText = "insertGEO";
                        om.Parameters.Add("v_id", OracleDbType.Int32).Direction = ParameterDirection.Input;
                        om.Parameters["v_id"].Value = node.m_objList[i].ID;
                        om.Parameters.Add("v_pid", OracleDbType.Int32).Direction = ParameterDirection.Input;
                        om.Parameters["v_pid"].Value = parentID;
                        om.Parameters.Add("Childrens", OracleDbType.NClob).Direction = ParameterDirection.Input;
                        om.Parameters["Childrens"].Value = node.m_objList[i].printChildrens();
                        string ring = "";
                        if (node.m_objList[i].RingInParent != null)
                        {
                            ring = node.m_objList[i].RingInParent.printGeoLineRing();
                        }
                        om.Parameters.Add("RingInParent", OracleDbType.NClob).Direction = ParameterDirection.Input;
                        om.Parameters["RingInParent"].Value = ring;
                        om.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("索引建立错误： " + e.Message);
                    //string queryString1 =
                    //"delete from quadtreenode";
                    //OracleCommand command1 = new OracleCommand(queryString1, myConnection);
                    //command1.ExecuteScalar();//执行command命令

                    //string queryString2 =
                    //"delete from quadtreeindex";
                    //OracleCommand command2 = new OracleCommand(queryString2, myConnection);
                    //command2.ExecuteScalar();//执行command命令

                    //string queryString3 =
                    //"delete from geoobjects";
                    //OracleCommand command3 = new OracleCommand(queryString3, myConnection);
                    //command3.ExecuteScalar();//执行command命令
                }
                
                

                proOrder(node.ChildNW, myConnection);
                proOrder(node.ChildNE, myConnection);
                proOrder(node.ChildSW, myConnection);
                proOrder(node.ChildSE, myConnection);
            }
        }
        #endregion


        //ClsQuadTreeNode clsQuadTreeNode = new ClsQuadTreeNode();
        //ClsObjRelationship clsObjRelationship = new ClsObjRelationship();

        //public void SaveMapIndex(QuadTreeNode node)
        //{
        //    if (node.NodeType == 1)//如果是叶子结点，则所有孩子均为空（-1）
        //    {
        //        clsQuadTreeNode.AddSingle(node, -1, -1, -1, -1);//写结点表
        //        for (int i = 0; i < node.m_objList.Count; i++)
        //        {
        //            clsObjRelationship.AddSingle(node.m_objList[i]);//写子对象关系表
        //        }
        //    }
        //    else
        //    {                      //如果是中间结点，则存储孩子结点ID
        //        clsQuadTreeNode.AddSingle(node, node.ChildNE.ID, node.ChildSE.ID, node.ChildSW.ID, node.ChildNW.ID);
        //        //if (node.ChildNE != null)
        //            SaveMapIndex(node.ChildNE);
        //        //if (node.ChildSE != null)
        //            SaveMapIndex(node.ChildSE);
        //        //if (node.ChildSW != null)
        //            SaveMapIndex(node.ChildSW);
        //        //if (node.ChildNW != null)
        //            SaveMapIndex(node.ChildNW);
        //    }
        //}
    }
}
