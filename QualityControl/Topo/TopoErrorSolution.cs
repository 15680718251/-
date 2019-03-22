using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace QualityControl.Topo
{
    public class TopoErrorSolution
    {
        #region 线线的拓扑处理方法

        #region 分开线
        //获取线的wkt点序列
        private string getLine(OracleConnection myConnection, string table1, string id1)
        {
            string sql = String.Format("select (SHAPE).get_wkt() from {0}  where objectid={1}",
                table1,
                  id1);
            //Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string wkt = null;
            string line = "";

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                wkt = (string)command.ExecuteScalar();
                //Console.WriteLine(gml);
            }
            if (wkt != null)
            {
                Match match1 = Regex.Match(wkt, @"[-|0-9].+[0-9]");
                line = match1.Value;
            }
            return line;
        }

        //将不同属性的相连线分开------------------------------------------------------------------------------
        public void cutLine(OracleConnection conn, string table1, string id1, string table2, string id2)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            ArrayList pointlist = new ArrayList();
            TopoLineCalculation lineCal = new TopoLineCalculation();
            #region 连接读取oracle中的数据
            string queryString = String.Format(
                "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,c_c.SHAPE,0.00001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
    //"select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,SDO_GEOM.SDO_BUFFER(c_c.SHAPE, 1, 0.5, 'unit= M'),0.0001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);
            
            string gml = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    gml = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                return;
            }
            #endregion


            if (gml.Equals("null")) return;
            if (!gml.Equals("null"))
            {

                Console.WriteLine("交线id " + id2 + " 交线id " + id1);
                //Console.WriteLine(gml);

                
                string line = lineCal.getOSMLine(conn, table1, "" + id1);//线目标A的点串-----------------------------------------------------------------------------------------------
                string line2 = lineCal.getOSMLine(conn, table2, "" + id2);//--------------------------------------------------------------------------------------

                #region 匹配字符串组

                #region 变量
                ArrayList jiedianList = new ArrayList();//zh 非排序节点集合（包括交点和交线的端点）
                ArrayList pointList = new ArrayList();//zh 交点的集合
                ArrayList duandianList = new ArrayList();//zh 非排序交线点集合
                ArrayList insectLinelist = new ArrayList();//zh 交线的点序列集合
                #endregion
                //2.匹配Point
                MatchCollection matches = Regex.Matches(gml, @"(<gml:Point)((?!Point).)*(gml:Point>)");
                //匹配组从0开始
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    //match.Value是匹配的内容
                    //Console.WriteLine(match.Value);

                    string pointString = match.Value;
                    Match match0 = Regex.Match(pointString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                    string Point = match0.Value;
                    Match match1 = Regex.Match(Point, @"[-|0-9].+[0-9]");
                    string tuple = match1.Value;
                    jiedianList.Add(tuple);
                    pointList.Add(tuple);//zh
                    //Console.WriteLine(match1.Value);
                }

                #endregion

                #region 重构线的点串,排序节点
                //重构线A
                ArrayList temp = (ArrayList)jiedianList.Clone();//临时的节点串
                ArrayList lineList = new ArrayList();//线的点串
                string[] linezu = line.Split(' ');
                for (int k = 0; k < linezu.Length; k++)
                {
                    string[] xy = linezu[k].Split(',');
                    string zuobiao = xy[0] + "," + xy[1];
                    lineList.Add(zuobiao);
                }
                //将节点按顺序加入线
                for (int i = 0; i < lineList.Count - 1; i++)
                {
                    for (int j = 0; j < jiedianList.Count; j++)
                    {
                        if (lineCal.isIn((string)lineList[i], (string)lineList[i + 1], (string)jiedianList[j]))
                        {
                            if (!((string)lineList[i]).Equals((string)jiedianList[j]) && !((string)lineList[i + 1]).Equals((string)jiedianList[j]))
                            {
                                lineList.Insert(i + 1, (string)jiedianList[j]);

                            }
                            jiedianList.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                //排序节点
                ArrayList newline = new ArrayList();//新的排序的节点序列
                for (int i = 0; i < lineList.Count; i++)
                {
                    string lp = (string)lineList[i];
                    if (temp.Count < 1) break;
                    for (int j = 0; j < temp.Count; j++)
                    {
                        string jp = (string)temp[j];
                        if (lp.Equals(jp))
                        {
                            newline.Add(jp);
                            temp.RemoveAt(j);
                            break;
                        }
                    }
                }





                //重构线B------------------------------------------------------------
                ArrayList temp2 = (ArrayList)newline.Clone();//临时的节点串
                ArrayList lineList2 = new ArrayList();//线的点串
                string[] linezu2 = line2.Split(' ');
                for (int k = 0; k < linezu2.Length; k++)
                {
                    string[] xy = linezu2[k].Split(',');
                    string zuobiao = xy[0] + "," + xy[1];
                    lineList2.Add(zuobiao);
                }
                //将节点按顺序加入线
                for (int i = 0; i < lineList2.Count - 1; i++)
                {
                    for (int j = 0; j < temp2.Count; j++)
                    {
                        if (lineCal.isIn((string)lineList2[i], (string)lineList2[i + 1], (string)temp2[j]))
                        {
                            if (!((string)lineList2[i]).Equals((string)temp2[j]) && !((string)lineList2[i + 1]).Equals((string)temp2[j]))
                            {
                                lineList2.Insert(i + 1, (string)temp2[j]);

                            }
                            temp2.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                //------

                //Console.WriteLine("---------------------------------" + lineList.Count);
                //for (int i = 0; i < lineList.Count; i++)
                //{
                //    Console.WriteLine(lineList[i]);
                //}
                #endregion

                #region 求线/线 交点的节点度,获取a类型的交点集合
                int count = 0;
                ArrayList du1 = new ArrayList();
                for (int i = 0; i < pointList.Count; i++)
                {
                    string dian = (string)pointList[i];
                    int indexOfA = lineCal.getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = lineCal.getIndexOfLine(dian, lineList2);//节点在点串B中的位置
                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList.Count - 1)
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            //dian = dian.Replace(",", " ");
                            pointlist.Add(dian);
                        }
                    }
                }
                #endregion
            }

            //根据a类交点，将线分开
            for (int i = 0; i < pointlist.Count; i++)
            {
                string bufferpoint1 = getPointByBuffer(conn, table1, id1, (string)pointlist[i]);
                cutByBuffer(conn, table1, id1, (string)pointlist[i], bufferpoint1);
                string bufferpoint2 = getPointByBuffer(conn, table2, id2, (string)pointlist[i]);
                cutByBuffer(conn, table2, id2, (string)pointlist[i], bufferpoint2);
            }
        }

        //获取线与点缓冲区边界的交点
        private string getPointByBuffer(OracleConnection conn, string table1, string id1, string insectP)
        {
            string point = "";
            string wkt = insectP.Replace(",", " ");
            #region 连接读取oracle中的数据
            string queryString = String.Format(
                "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,SDO_GEOM.SDO_BUFFER(sdo_geometry('POINT({0})',4326), 0.5, 0.5, 'unit= M'),0.0001)) from {1} c_a where c_a.objectid={2}",
                wkt,
                table1,
                id1);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string gml = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    gml = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                return point;
            }
            #endregion

            #region 匹配字符串组

            #region 变量
            ArrayList jiedianList = new ArrayList();//zh 非排序节点集合（包括交点和交线的端点）
            ArrayList pointList = new ArrayList();//zh 交点的集合
            ArrayList duandianList = new ArrayList();//zh 非排序交线点集合
            ArrayList insectLinelist = new ArrayList();//zh 交线的点序列集合
            #endregion
            //3.匹配LineString

            MatchCollection matches = Regex.Matches(gml, @"(<gml:LineString)((?!LineString).)*(gml:LineString>)");
            //匹配组从0开始
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                //match.Value是匹配的内容
                //Console.WriteLine(match.Value);

                string LineString = match.Value;
                Match match0 = Regex.Match(LineString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                string tuple = match1.Value;
                //截取交线的端点 zh
                string[] zu = tuple.Split(' ');
                if (zu[0].Equals(insectP) )
                {
                    point = zu[zu.Length - 1];
                }
                if (zu[zu.Length - 1].Equals(insectP))
                {
                    point = zu[0];
                }
            }
            #endregion

            return point;
        }

        //截取更新线
        private void cutByBuffer(OracleConnection conn, string table1, string id1, string insectP, string bufferpoint)
        {
            //将buffer相交的点加入线串，重构线串
            TopoLineCalculation lineCal = new TopoLineCalculation();
            string line = lineCal.getOSCLine(conn, table1, id1);
            ArrayList lineList = new ArrayList();//线的点串
            string[] linezu = line.Split(' ');
            for (int k = 0; k < linezu.Length; k++)
            {
                string[] xy = linezu[k].Split(',');
                string zuobiao = xy[0] + "," + xy[1];
                lineList.Add(zuobiao);
            }
            for (int i = 0; i < lineList.Count - 1; i++)
            {
                if (lineCal.isIn((string)lineList[i], (string)lineList[i + 1], bufferpoint))
                {
                    if (!((string)lineList[i]).Equals(bufferpoint) && !((string)lineList[i + 1]).Equals(bufferpoint))
                    {
                        lineList.Insert(i + 1, bufferpoint);
                    }
                    break;
                }
            }

            //获取截取后的新点串
            string newline = "";
            if (((string)lineList[0]).Equals(insectP))
            {
                for (int i = lineList.Count - 1; i >= 0; i--)
                {
                    newline = newline + " " + (string)lineList[i];
                    if (((string)lineList[i]).Equals(bufferpoint)) break;
                }
            }
            else
            {
                for (int i = 0; i <= lineList.Count - 1; i++)
                {
                    newline = newline + " " + (string)lineList[i];
                    if (((string)lineList[i]).Equals(bufferpoint)) break;
                }
            }
            
            //更新线串
            newline = newline.Trim();
            newline = newline.Replace(" ", "*");
            newline = newline.Replace(",", " ");
            newline = newline.Replace("*", ",");
            newline = string.Format("LINESTRING({0})", newline);
            string sql = string.Format("update {0} set shape=sdo_geometry ('{1}',4326 ) where objectid={2}", table1, newline , id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region 打断点成线
        //将同属性的相连线分开------------------------------------------------------------------------------
        public void breakLine(OracleConnection conn, string table1, string id1, string table2, string id2)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            ArrayList pointlist = new ArrayList();
            TopoLineCalculation lineCal = new TopoLineCalculation();
            #region 连接读取oracle中的数据
            string queryString = String.Format(
                "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,c_c.SHAPE,0.00001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
                //"select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,SDO_GEOM.SDO_BUFFER(c_c.SHAPE, 1, 0.5, 'unit= M'),0.0001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string gml = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    gml = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                return;
            }
            #endregion


            if (gml.Equals("null")) return;
            if (!gml.Equals("null"))
            {

                Console.WriteLine("交线id " + id2 + " 交线id " + id1);
                //Console.WriteLine(gml);


                string line = lineCal.getOSMLine(conn, table1, "" + id1);//线目标A的点串-------------
                string line2 = lineCal.getOSMLine(conn, table2, "" + id2);//------------

                #region 匹配字符串组

                #region 变量
                ArrayList jiedianList = new ArrayList();//zh 非排序节点集合（包括交点和交线的端点）
                ArrayList pointList = new ArrayList();//zh 交点的集合
                ArrayList duandianList = new ArrayList();//zh 非排序交线点集合
                ArrayList insectLinelist = new ArrayList();//zh 交线的点序列集合
                #endregion
                //2.匹配Point
                MatchCollection matches = Regex.Matches(gml, @"(<gml:Point)((?!Point).)*(gml:Point>)");
                //匹配组从0开始
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    //match.Value是匹配的内容
                    //Console.WriteLine(match.Value);

                    string pointString = match.Value;
                    Match match0 = Regex.Match(pointString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                    string Point = match0.Value;
                    Match match1 = Regex.Match(Point, @"[-|0-9].+[0-9]");
                    string tuple = match1.Value;
                    jiedianList.Add(tuple);
                    pointList.Add(tuple);//zh
                    //Console.WriteLine(match1.Value);
                }

                #endregion

                #region 重构线的点串,排序节点
                //重构线A
                ArrayList temp = (ArrayList)jiedianList.Clone();//临时的节点串
                ArrayList lineList = new ArrayList();//线的点串
                string[] linezu = line.Split(' ');
                for (int k = 0; k < linezu.Length; k++)
                {
                    string[] xy = linezu[k].Split(',');
                    string zuobiao = xy[0] + "," + xy[1];
                    lineList.Add(zuobiao);
                }
                //将节点按顺序加入线
                for (int i = 0; i < lineList.Count - 1; i++)
                {
                    for (int j = 0; j < jiedianList.Count; j++)
                    {
                        if (lineCal.isIn((string)lineList[i], (string)lineList[i + 1], (string)jiedianList[j]))
                        {
                            if (!((string)lineList[i]).Equals((string)jiedianList[j]) && !((string)lineList[i + 1]).Equals((string)jiedianList[j]))
                            {
                                lineList.Insert(i + 1, (string)jiedianList[j]);

                            }
                            jiedianList.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                //排序节点
                ArrayList newline = new ArrayList();//新的排序的节点序列
                for (int i = 0; i < lineList.Count; i++)
                {
                    string lp = (string)lineList[i];
                    if (temp.Count < 1) break;
                    for (int j = 0; j < temp.Count; j++)
                    {
                        string jp = (string)temp[j];
                        if (lp.Equals(jp))
                        {
                            newline.Add(jp);
                            temp.RemoveAt(j);
                            break;
                        }
                    }
                }





                //重构线B------------------------------------------------------------
                ArrayList temp2 = (ArrayList)newline.Clone();//临时的节点串
                ArrayList lineList2 = new ArrayList();//线的点串
                string[] linezu2 = line2.Split(' ');
                for (int k = 0; k < linezu2.Length; k++)
                {
                    string[] xy = linezu2[k].Split(',');
                    string zuobiao = xy[0] + "," + xy[1];
                    lineList2.Add(zuobiao);
                }
                //将节点按顺序加入线
                for (int i = 0; i < lineList2.Count - 1; i++)
                {
                    for (int j = 0; j < temp2.Count; j++)
                    {
                        if (lineCal.isIn((string)lineList2[i], (string)lineList2[i + 1], (string)temp2[j]))
                        {
                            if (!((string)lineList2[i]).Equals((string)temp2[j]) && !((string)lineList2[i + 1]).Equals((string)temp2[j]))
                            {
                                lineList2.Insert(i + 1, (string)temp2[j]);

                            }
                            temp2.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                //------

                //Console.WriteLine("---------------------------------" + lineList.Count);
                //for (int i = 0; i < lineList.Count; i++)
                //{
                //    Console.WriteLine(lineList[i]);
                //}
                #endregion

                #region 求线/线 交点的节点度,获取b,c,d类型的交点集合
                int count = 0;
                ArrayList du1 = new ArrayList();
                for (int i = 0; i < pointList.Count; i++)
                {
                    string dian = (string)pointList[i];
                    int indexOfA = lineCal.getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = lineCal.getIndexOfLine(dian, lineList2);//节点在点串B中的位置
                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList.Count - 1)
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du1.Add(2);
                        }
                        else
                        {
                            du1.Add(3);
                            pointlist.Add(dian);
                        }
                    }
                    else
                    {
                        pointlist.Add(dian);
                    }
                }
                #endregion
            }

            //根据b,c,d类交点，将线分开
            for (int i = 0; i < pointlist.Count; i++)
            {
                breakByPoint(conn, table1, id1, (string)pointlist[i]);
                breakByPoint(conn, table2, id2, (string)pointlist[i]);
            }
        }

        //断开更新线
        private void breakByPoint(OracleConnection conn, string table1, string id1, string insectP)
        {
            //将交点加入线串，重构线串
            TopoLineCalculation lineCal = new TopoLineCalculation();
            string line = lineCal.getOSCLine(conn, table1, id1);
            ArrayList lineList = new ArrayList();//线的点串
            string[] linezu = line.Split(' ');
            for (int k = 0; k < linezu.Length; k++)
            {
                string[] xy = linezu[k].Split(',');
                string zuobiao = xy[0] + "," + xy[1];
                lineList.Add(zuobiao);
            }
            for (int i = 0; i < lineList.Count - 1; i++)
            {
                if (lineCal.isIn((string)lineList[i], (string)lineList[i + 1], insectP))
                {
                    if (!((string)lineList[i]).Equals(insectP) && !((string)lineList[i + 1]).Equals(insectP))
                    {
                        lineList.Insert(i + 1, insectP);
                    }
                    break;
                }
            }

            //获取截取后的新点串
            string newline1 = "";
            string newline2 = "";
            if (!((string)lineList[0]).Equals(insectP) && !((string)lineList[lineList.Count-1]).Equals(insectP))
            {
                for (int i = lineList.Count - 1; i >= 0; i--)
                {
                    newline1 = newline1 + " " + (string)lineList[i];
                    if (((string)lineList[i]).Equals(insectP)) break;
                }

                for (int i = 0; i <= lineList.Count - 1; i++)
                {
                    newline2 = newline2 + " " + (string)lineList[i];
                    if (((string)lineList[i]).Equals(insectP)) break;
                }


                //插入新线串1
                insertNewLine(conn, table1, id1, newline1);
                //插入新线串2
                insertNewLine(conn, table1, id1, newline2);
                //删除原有线
                deleteData(conn, table1, id1);
            }

            
        }

        //插入新线串,将除shape字段的其他字段复制过来，newline1为gml格式
        private void insertNewLine(OracleConnection conn, string table1, string id1, string newline1)
        {
            
            newline1 = newline1.Trim();
            newline1 = newline1.Replace(" ", "*");
            newline1 = newline1.Replace(",", " ");
            newline1 = newline1.Replace("*", ",");
            newline1 = string.Format("LINESTRING({0})", newline1);
            //修改数据shape
            string sql = string.Format("update {0} set shape=sdo_geometry ('{1}',4326 ) where objectid={2}", table1, newline1, id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
            //插入数据
            sql = string.Format(@"insert into {0}(OSMID,VERSIONID,STARTTIME,ENDTIME,CHANGESET,USERID,USERNAME,FC,DSG,TAGS,TRUSTVALUE,USERREPUTATION,SHAPE,NATIONELENAME,NATIONCODE,MATCHID,UPDATESTATE,SOURCE) 
                                                          (select OSMID,VERSIONID,STARTTIME,ENDTIME,CHANGESET,USERID,USERNAME,FC,DSG,TAGS,TRUSTVALUE,USERREPUTATION,SHAPE,NATIONELENAME,NATIONCODE,MATCHID,UPDATESTATE,SOURCE from {1} where objectid={2})",
                                                          table1,
                                                          table1,
                                                          id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
            
        }

        //删除数据
        private void deleteData(OracleConnection conn, string table1, string id1)
        {
            string sql = string.Format("delete from {0}  where objectid={1}", table1, id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region 
        #endregion

        #endregion


        #region 线面的拓扑处理方法

        #region 线删除相交部分
        //------------------------------------------------------------------------------------------------
        //table1为线
        public void deleteInsect(OracleConnection conn, string table1, string id1, string table2, string id2)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            #region 连接读取oracle中的数据
            string queryString = String.Format(
                "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_DIFFERENCE(c_a.SHAPE,c_c.SHAPE,0.00001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string gml = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    gml = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                return;
            }
            #endregion
            //匹配LineString
            MatchCollection matches = Regex.Matches(gml, @"(<gml:LineString)((?!LineString).)*(gml:LineString>)");
            //匹配组从0开始
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                string LineString = match.Value;
                Match match0 = Regex.Match(LineString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                string tuple = match1.Value;
                //插入新线串2
                insertNewLine(conn, table1, id1, tuple);
            }
            //删除原有线
            deleteData(conn, table1, id1);
        }
        #endregion

        #region 分割面
        #endregion

        #endregion

        #region 面面的拓扑处理方案
        #region 合并面
        //将2个相交的同类型的面合并-----------------------------------------------------------------------------------------
        public void mergeArea(OracleConnection conn, string table1, string id1, string table2, string id2)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            #region 获取合并后的shape
            string queryString = String.Format(
                "select SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,c_c.SHAPE,0.005).get_wkt() from {0} c_a,{1} c_c where c_a.OBJECTID={2} and c_c.OBJECTID={3};",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string wkt = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    wkt = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                return;
            }
            #endregion

            //插入面
            insertNewArea(conn, table1, id1,wkt);
            //删除原有面
            string sql = string.Format("delete from {0}  where objectid={1}", table1, id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
            sql = string.Format("delete from {0}  where objectid={1}", table2, id2);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        //插入新面
        private void insertNewArea(OracleConnection conn, string table1, string id1, string newarea)
        {
            newarea = string.Format("POLYGON(({0}))", newarea);
            //插入数据
            string sql = string.Format(@"insert into {0}(OSMID,VERSIONID,STARTTIME,ENDTIME,CHANGESET,USERID,USERNAME,FC,DSG,TAGS,TRUSTVALUE,USERREPUTATION,SHAPE,NATIONELENAME,NATIONCODE,MATCHID,UPDATESTATE,SOURCE) 
                                                          (select OSMID,VERSIONID,STARTTIME,ENDTIME,CHANGESET,USERID,USERNAME,FC,DSG,TAGS,TRUSTVALUE,USERREPUTATION,SHAPE,NATIONELENAME,NATIONCODE,MATCHID,UPDATESTATE,SOURCE from {1} where objectid={2})",
                                                          table1,
                                                          table1,
                                                          id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }

        }
        #endregion

        #region 挖空洞
        //对完全包含的同类型的面挖空洞-----------------------------------------------------------------------------------------
        //table1为面积大的面
        public void digArea(OracleConnection conn, string table1, string id1, string table2, string id2)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            double area1 = getShapeArea(conn,table1,id1);
            double area2 = getShapeArea(conn, table2, id2);
            if (area2 > area1)
            {
                string table = table1;
                string id = id1;
                table1 = table2;
                id1 = id2;
                table2 = table;
                id2 = id;
            }
            #region 获取挖空洞后的shape
            string queryString = String.Format(
                "select SDO_GEOM.SDO_DIFFERENCE(c_a.SHAPE,c_c.SHAPE,0.005).get_wkt() from {0} c_a,{1} c_c where c_a.OBJECTID={2} and c_c.OBJECTID={3}",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string wkt = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    wkt = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                //return;
            }
            #endregion

            //插入面
            insertNewArea(conn, table1, id1, wkt);
            //删除原有面
            string sql = string.Format("delete from {0}  where objectid={1}", table1, id1);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
            sql = string.Format("delete from {0}  where objectid={1}", table2, id2);
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        //获取面的面积
        private double getShapeArea(OracleConnection conn, string table1, string id1)
        {
            string queryString = String.Format(
                "select SDO_GEOM.SDO_AREA(c_a.SHAPE,0.0001) from {0} c_a where c_a.OBJECTID={1}",
    table1,
    id1);
            OracleCommand command0 = new OracleCommand(queryString, conn);

            string area = "null";
            try
            {
                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    area = (string)command0.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                //return;
            }
            return Double.Parse(area);
        }
        #endregion

        #endregion
    }
}
