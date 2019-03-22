using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Oracle.ManagedDataAccess.Client;
//正则表达式
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using GIS.UI.AdditionalTool;
/*
 * 实现线线拓扑计算的功能
 * zh编写
 * 2018年9月19日
 */
 
namespace QualityControl.Topo
{
    class TopoLineCalculation
    {
        #region 全局变量
        
        #endregion
        /*
         *检查同一要素图层中交点容差是否与端点相交
         *相交删除交点到端点的部分
         */
        public string toleranceDeal(OracleConnection conn, ArrayList lineList, ArrayList jiedianList, string tolerance)
        {
            
            if (jiedianList == null || jiedianList.Count < 1) return null;
            int start = -1;
            int end = -1;
            //判断截取容差后线的头节点
            for (int i = 0; i < jiedianList.Count; i++)
            {
                #region 判断交点缓冲区是否与端点相交
                string queryString = String.Format(
                                    @"select sdo_geom.relate(
sdo_geom.sdo_buffer(MDSYS.SDO_GEOMETRY(2001 , NULL , MDSYS.SDO_POINT_TYPE ({0},NULL),  NULL ,  NULL),{1},0.005,'arc_tolerance=0.005'),
'anyinteract',
MDSYS.SDO_GEOMETRY(2001 , NULL , MDSYS.SDO_POINT_TYPE ({2},NULL),  NULL ,  NULL),
0.005) from dual",
                                     lineList[0],
                                     tolerance,
                                     jiedianList[i]);
                OracleCommand command0 = new OracleCommand(queryString, conn);
                string result = "null";

                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    result = (string)command0.ExecuteScalar();
                }
                #endregion

                if (result.Equals("TRUE"))//当交点容差是否与端点相交
                {
                    start=i;
                }
            }

            //判断截取容差后线的尾节点
            for (int i = jiedianList.Count-1; i>=0; i--)
            {
                #region 判断交点缓冲区是否与端点相交
                string queryString = String.Format(
                                    @"select sdo_geom.relate(
sdo_geom.sdo_buffer(MDSYS.SDO_GEOMETRY(2001 , NULL , MDSYS.SDO_POINT_TYPE ({0},NULL),  NULL ,  NULL),{1},0.005,'arc_tolerance=0.005'),
'anyinteract',
MDSYS.SDO_GEOMETRY(2001 , NULL , MDSYS.SDO_POINT_TYPE ({2},NULL),  NULL ,  NULL),
0.005) from dual",
                                     lineList[0],
                                     tolerance,
                                     jiedianList[i]);
                OracleCommand command0 = new OracleCommand(queryString, conn);
                command0.ExecuteScalar();//执行command命令
                string result = "null";

                if (command0.ExecuteScalar() != System.DBNull.Value)
                {
                    result = (string)command0.ExecuteScalar();
                }
                #endregion

                if (result.Equals("TRUE"))//当交点容差是否与端点不相交
                {
                    end=i;
                }
            }
            
            //从点串中截取不在容差范围的子串
            int startIndex=0;
            int endIndex=lineList.Count-1;
            if (start != -1)
            {
                for (int i = 0; i < lineList.Count; i++)
                {
                    if (((string)lineList[i]).Equals((string)jiedianList[start]))
                    {
                        startIndex = i;
                        break;
                    }
                }
            }
            if (end != -1)
            {
                for (int i = lineList.Count-1; i >=0; i--)
                {
                    if (((string)lineList[i]).Equals((string)jiedianList[end]))
                    {
                        endIndex = i;
                        break;
                    }
                }
            }

            StringBuilder lineWkt = new StringBuilder("LINESTRING(");
            for (int i = startIndex; i<=endIndex; i++)
            {
                string s = ((string)lineList[i]).Replace(",", " ");
                if(i<endIndex) s = s + ",";
                lineWkt.Append(s);
            }
            lineWkt.Append(")");
            return lineWkt.ToString();
        }

        //获取osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tablename)
        {
            #region 连接读取oracle中的数据
            ArrayList idList=new ArrayList();
            string queryString = String.Format(
                                //"select objectid from {0} where updatastate=0 ",
                                "select objectid from {0} ",
                                 tablename);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    idList.Add(dr["objectid"].ToString());

                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return idList;
        }

        //获取与表集合相交的osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tableosc,string tableosm)
        {
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            HashSet<string> set = new HashSet<string>();
            string queryString = String.Format(
                                @"SELECT B.objectid
  FROM {0} A ,{1} B
WHERE SDO_WITHIN_DISTANCE(B.shape, 
                           A.shape, 
                            'distance=' || 10 || ' unit=m') = 'TRUE'",
                                 tableosc,
                                 tableosm);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (set.Add(dr["objectid"].ToString()))
                    {
                        idList.Add(dr["objectid"].ToString());
                    }
                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return idList;
        }

        //获取osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tableosc,string oscid, string tableosm)
        {
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            HashSet<string> set = new HashSet<string>();
            string queryString = String.Format(
                                @"SELECT B.objectid
  FROM {0} A ,{1} B
WHERE SDO_WITHIN_DISTANCE(B.shape, 
                           (select shape from {2} where OBJECTID={3}), 
                            'distance=' || 10 || ' unit=m') = 'TRUE'",
                                 tableosc,
                                 tableosm,
                                 tableosc,
                                 oscid);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (set.Add(dr["objectid"].ToString()))
                    {
                        idList.Add(dr["objectid"].ToString());
                    }
                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return idList;
        }


        //获取osc的id集合
        public ArrayList getOSCIDList(OracleConnection conn, string tablename)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            string queryString = String.Format(
                                //"select objectid from {0} where updatestate=2 or updatestate=3",
                                "select objectid from {0}",
                                 tablename);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    idList.Add(dr["objectid"].ToString());

                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return idList;
        }

        //获取线线的拓扑关系
        public ArrayList getTopoByLineInsectLine(OracleConnection conn, string table1, string table2, string id1, string id2)
        {
            ArrayList result = new ArrayList();

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
                return result;
            }
            //Console.WriteLine(gml);
            //myConnection.Close();
            #endregion

            
            if (gml.Equals("null")) return result;
            if (!gml.Equals("null"))
            {
                
                Console.WriteLine("交线id " + id2 + " 交线id " + id1);
                //Console.WriteLine(gml);

                string line = getOSCLine(conn, table1, "" + id1);//线目标A的点串-----------------------------------------------------------------------------------------------
                string line2 = getOSMLine(conn, table2, "" + id2);//--------------------------------------------------------------------------------------

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

                //3.匹配LineString
                
                matches = Regex.Matches(gml, @"(<gml:LineString)((?!LineString).)*(gml:LineString>)");
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
                    tuple = tuple.Replace(" ", "*");
                    tuple = tuple.Replace(",", " ");
                    tuple = tuple.Replace("*", ",");
                    insectLinelist.Add(tuple);
                    jiedianList.Add(zu[0]);
                    jiedianList.Add(zu[zu.Length - 1]);
                    duandianList.Add(zu[0]);
                    duandianList.Add(zu[zu.Length - 1]);
                    //Console.WriteLine(match1.Value);
                    //--
                }

                //Console.WriteLine(jiedianList.Count);
                //for (int i = 0; i < jiedianList.Count; i++)
                //{
                //    Console.WriteLine(jiedianList[i]);
                //}


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
                        if (isIn((string)lineList[i], (string)lineList[i + 1], (string)jiedianList[j]))
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
                        if (isIn((string)lineList2[i], (string)lineList2[i + 1], (string)temp2[j]))
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
                #endregion

                #region 容差处理重构线串
                //string AWkt = toleranceDeal(conn, lineList, newline, "0.0005");
                //string BWkt = toleranceDeal(conn, lineList2, newline, "0.0005");
                //更新线串
                //updateOSCLine(conn, table1, id1, AWkt);
                //updateOSMLine(conn, table2, id2, BWkt);
                #endregion
                

                #region 求线/线 交点的节点度
                int count = 0;
                ArrayList du1 = new ArrayList();
                for (int i = 0; i < pointList.Count; i++)
                {
                    string dian = (string)pointList[i];
                    int indexOfA = getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = getIndexOfLine(dian, lineList2);//节点在点串B中的位置
                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList.Count - 1)
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du1.Add(2);
                            result.Add("A");
                        }
                        else
                        {
                            du1.Add(3);
                            result.Add("B");
                        }
                    }
                    else
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du1.Add(3);
                            result.Add("B");
                        }
                        else
                        {
                            int orien1 = getOrientation(dian, (string)lineList[indexOfA - 1], (string)lineList2[indexOfB - 1]);
                            int orien2 = getOrientation(dian, (string)lineList2[indexOfB + 1], (string)lineList[indexOfA + 1]);
                            if (orien1 == orien2)
                            {
                                du1.Add(4 + ",T");
                                result.Add("C");
                            }
                            else
                            {
                                du1.Add(4 + ",C");
                                result.Add("D");
                            }
                        }
                    }
                    Console.WriteLine("交点--" + du1[i].ToString());
                    //result = result +"(0," +du1[i].ToString() + "),";
                    dian = dian.Replace(",", " ");
                    insertInsectPoint(conn, table1, id1, table2, id2, dian, (string)result[result.Count - 1]);
                }
                #endregion

                #region 交为线，且目标为线/线
                ArrayList du = new ArrayList();
                int flag = 0;//1为在向量线的左，-1为右
                for (int i = 0; i < duandianList.Count; i++)
                {
                    string dian = (string)duandianList[i];
                    int indexOfA = getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = getIndexOfLine(dian, lineList2);//节点在点串B中的位置


                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList.Count - 1)
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du.Add(1);
                        }
                        else//不是线目标B的端点
                        {
                            du.Add(2);
                        }
                    }
                    else//不是线目标A的端点
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du.Add(2);
                        }
                        else//不是线目标B的端点
                        {
                            if (i % 2 == 0)
                            {
                                du.Add(3);
                                flag = getOrientation(dian, (string)lineList[indexOfA - 1], (string)lineList2[indexOfB - 1]);
                            }
                            else
                            {
                                if (flag == 0) du.Add(3);
                                else
                                {
                                    //判断线A是否在线B的一边
                                    if ( flag == getOrientation(dian, (string)lineList[indexOfA + 1], (string)lineList2[indexOfB + 1]))
                                    {
                                        du.Add(3 + ",C");
                                        flag = 0;
                                    }
                                    else
                                    {
                                        du.Add(3 + ",T");
                                        flag = 0;
                                    }
                                }
                            }

                        }
                    }
                    //
                    if (i % 2 == 1)
                    {
                        Console.WriteLine(du[i - 1].ToString() + du[i].ToString());
                        //result = result +"(1," +du[i - 1].ToString() + "," + du[i].ToString() + "),";
                        string type=du[i - 1].ToString() +","+ du[i].ToString();
                        #region 判断分类 线交重分类
                        //if (type.Equals("1,1") || type.Equals("1,2") || type.Equals("2,1") || type.Equals("2,2")) result.Add("C");
                        //else if (type.Equals("1,3") || type.Equals("3,1") || type.Equals("2,3") || type.Equals("3,2") || type.Equals("3,3,T")) result.Add("D");
                        //else if (type.Equals("3,3,C")) result.Add("E");
                        #endregion

                        #region 判断分类 线交原始分类
                        if (type.Equals("1,1")) result.Add("E");
                        else if (type.Equals("1,2") || type.Equals("2,1") ) result.Add("F");
                        else if (type.Equals("2,2")) result.Add("G");
                        else if (type.Equals("1,3") || type.Equals("3,1")) result.Add("H");
                        else if (type.Equals("2,3") || type.Equals("3,2")) result.Add("I");
                        else if (type.Equals("3,3,T")) result.Add("J");
                        else if (type.Equals("3,3,C")) result.Add("K");
                        #endregion

                        insertInsectLine(conn, table1, id1, table2, id2, (string)insectLinelist[i/2], (string)result[result.Count - 1]);
                    }
                }
                #endregion

                
            }
            return result ;
        }

        //获取线线缓冲区的拓扑关系
        public ArrayList getTopoByLineInsectBufferLine(OracleConnection conn, string table1, string table2, string id1, string id2)
        {
            ArrayList result = new ArrayList();

            #region 连接读取oracle中的数据
            string queryString = String.Format(
                //"select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,c_c.SHAPE,0.0001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
                "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,SDO_GEOM.SDO_BUFFER(c_c.SHAPE, 1, 0.5, 'unit= M'),0.0001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
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
                return result;
            }
            //Console.WriteLine(gml);
            //myConnection.Close();
            #endregion


            if (gml.Equals("null")) return result;
            if (!gml.Equals("null"))
            {

                Console.WriteLine("交线id " + id2 + " 交线id " + id1);
                //Console.WriteLine(gml);

                string line = getOSCLine(conn, table1, "" + id1);//线目标A的点串-----------------------------------------------------------------------------------------------
                string line2 = getOSMLine(conn, table2, "" + id2);//--------------------------------------------------------------------------------------

                #region 匹配字符串组
                ArrayList jiedianList = new ArrayList();//zh 非排序节点集合（包括交点和交线的端点）
                ArrayList pointList = new ArrayList();//zh 交点的集合
                ArrayList duandianList = new ArrayList();//zh 非排序交线点集合
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

                //3.匹配LineString

                matches = Regex.Matches(gml, @"(<gml:LineString)((?!LineString).)*(gml:LineString>)");
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
                    jiedianList.Add(zu[0]);
                    jiedianList.Add(zu[zu.Length - 1]);
                    duandianList.Add(zu[0]);
                    duandianList.Add(zu[zu.Length - 1]);
                    //Console.WriteLine(match1.Value);
                    //--
                }

                //Console.WriteLine(jiedianList.Count);
                //for (int i = 0; i < jiedianList.Count; i++)
                //{
                //    Console.WriteLine(jiedianList[i]);
                //}


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
                        if (isIn((string)lineList[i], (string)lineList[i + 1], (string)jiedianList[j]))
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
                        if (isIn((string)lineList2[i], (string)lineList2[i + 1], (string)temp2[j]))
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

                #region 容差处理重构线串
                //string AWkt = toleranceDeal(conn, lineList, newline, "0.0005");
                //string BWkt = toleranceDeal(conn, lineList2, newline, "0.0005");
                //更新线串
                //updateOSCLine(conn, table1, id1, AWkt);
                //updateOSMLine(conn, table2, id2, BWkt);
                #endregion


                #region 求线/线 交点的节点度
                int count = 0;
                ArrayList du1 = new ArrayList();
                for (int i = 0; i < pointList.Count; i++)
                {
                    string dian = (string)pointList[i];
                    int indexOfA = getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = getIndexOfLine(dian, lineList2);//节点在点串B中的位置
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
                            result.Add("A");
                        }
                    }
                    else
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du1.Add(3);
                            result.Add("A");
                        }
                        else
                        {
                            int orien1 = getOrientation(dian, (string)lineList[indexOfA - 1], (string)lineList2[indexOfB - 1]);
                            int orien2 = getOrientation(dian, (string)lineList2[indexOfB + 1], (string)lineList[indexOfA + 1]);
                            if (orien1 == orien2)
                            {
                                du1.Add(4 + ",T");
                                result.Add("A");
                            }
                            else
                            {
                                du1.Add(4 + ",C");
                                result.Add("B");
                            }
                        }
                    }
                    Console.WriteLine("交点--" + du1[i].ToString());
                    //result = result +"(0," +du1[i].ToString() + "),";

                }
                #endregion

                #region 交为线，且目标为线/线
                ArrayList du = new ArrayList();
                int flag = 0;//1为在向量线的左，-1为右
                for (int i = 0; i < duandianList.Count; i++)
                {
                    string dian = (string)duandianList[i];
                    int indexOfA = getIndexOfLine(dian, lineList);//节点在点串A中的位置
                    int indexOfB = getIndexOfLine(dian, lineList2);//节点在点串B中的位置


                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList.Count - 1)
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du.Add(1);
                        }
                        else//不是线目标B的端点
                        {
                            du.Add(2);
                        }
                    }
                    else//不是线目标A的端点
                    {
                        //判端交线点是否为线目标B的端点
                        if (indexOfB == 0 || indexOfB == lineList2.Count - 1)
                        {
                            du.Add(2);
                        }
                        else//不是线目标B的端点
                        {
                            if (i % 2 == 0)
                            {
                                du.Add(3);
                                flag = getOrientation(dian, (string)lineList[indexOfA - 1], (string)lineList2[indexOfB - 1]);
                            }
                            else
                            {
                                if (flag == 0) du.Add(3);
                                else
                                {
                                    //判断线A是否在线B的一边
                                    if (flag == getOrientation(dian, (string)lineList2[indexOfB - 1], (string)lineList[indexOfA - 1]))
                                    {
                                        du.Add(3 + ",T");
                                        flag = 0;
                                    }
                                    else
                                    {
                                        du.Add(3 + ",C");
                                        flag = 0;
                                    }
                                }
                            }

                        }
                    }
                    //
                    if (i % 2 == 1)
                    {
                        Console.WriteLine(du[i - 1].ToString() + du[i].ToString());
                        //result = result +"(1," +du[i - 1].ToString() + "," + du[i].ToString() + "),";
                        string type = du[i - 1].ToString() + "," + du[i].ToString();
                        #region 判断分类
                        if (type.Equals("1,1") || type.Equals("1,2") || type.Equals("2,1") || type.Equals("2,2")) result.Add("C");
                        else if (type.Equals("1,3") || type.Equals("3,1") || type.Equals("2,3") || type.Equals("3,2") || type.Equals("3,3,T")) result.Add("D");
                        else if (type.Equals("3,3,C")) result.Add("E");
                        #endregion
                    }
                }
                #endregion


            }
            return result;
        }


        //更新osc数据
        public void updateOSCLine(OracleConnection conn, string table, string id,string wkt)
        {
            if (wkt != null && !wkt.Equals("LINESTRING()"))
            {
                string sql = string.Format("update {0} set shape={1} where objectid={2}", table, "sdo_geometry ( :geom,4326 )", id);

                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                    {
                        p1.Value = wkt;
                        cmd.Parameters.Add(p1);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        //更新osm数据
        public void updateOSMLine(OracleConnection conn, string table, string id, string wkt)
        {
            if (wkt != null && !wkt.Equals("LINESTRING()"))
            {
                string sql = string.Format("update {0} set shape={1} where objectid={2}", table, "sdo_geometry ( :geom,4326 )", id);

                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                    {
                        p1.Value = wkt;
                        cmd.Parameters.Add(p1);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        //判断节点是否在两点之间
        public  bool isIn(string str1, string str2, string str)
        {
            string[] dian1 = str1.Trim().Split(',');
            string[] dian2 = str2.Trim().Split(',');
            string[] dian = str.Trim().Split(',');
            //判断x在两点之间
            if (double.Parse(dian1[0]) > double.Parse(dian2[0]))
            {
                if (double.Parse(dian[0]) < double.Parse(dian2[0])) return false;
                if (double.Parse(dian[0]) > double.Parse(dian1[0])) return false;
            }
            else
            {
                if (double.Parse(dian[0]) < double.Parse(dian1[0])) return false;
                if (double.Parse(dian[0]) > double.Parse(dian2[0])) return false;
            }

            //判断y在两点之间
            if (double.Parse(dian1[1]) > double.Parse(dian2[1]))
            {
                if (double.Parse(dian[1]) < double.Parse(dian2[1])) return false;
                if (double.Parse(dian[1]) > double.Parse(dian1[1])) return false;
            }
            else
            {
                if (double.Parse(dian[1]) < double.Parse(dian1[1])) return false;
                if (double.Parse(dian[1]) > double.Parse(dian2[1])) return false;
            }
            return true;
        }


        //获取osc线的GML
        public  string getOSCLine(OracleConnection myConnection, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SHAPE) from {0}  where objectid={1}",
                table1,
                  id1);
            //Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;
            string line = "";

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                //Console.WriteLine(gml);
            }
            if (gml != null)
            {
                Match match0 = Regex.Match(gml, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                line = match1.Value;
            }
            return line;
        }

        //获取osm线的GML
        public string getOSMLine(OracleConnection myConnection, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SHAPE) from {0}  where objectid={1}",
                table1,
                  id1);
            //Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;
            string line = "";

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                //Console.WriteLine(gml);
            }
            if (gml != null)
            {
                Match match0 = Regex.Match(gml, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                line = match1.Value;
            }
            return line;
        }


        //判断点在线目标中的位置
        public  int getIndexOfLine(string dian, ArrayList lineList)
        {
            int index = 0;//点在线点串中的位置
            for (int j = 0; j < lineList.Count; j++)
            {
                string lpoint = (string)lineList[j];
                if (lpoint.Equals(dian)) index = j;
            }
            return index;
        }


        //判断线是否在线内
        public  bool isInLine(OracleConnection myConnection, string dian1, string dian2, string table1, string id1)
        {
            //string queryString = String.Format(
            //    "select sdo_geom.relate(MDSYS.SDO_GEOMETRY(2002,NULL,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,2,1),MDSYS.SDO_ORDINATE_ARRAY({0},{1})),'determine',c_a.SHAPE,0.0001) from {2} c_a where c_a.osmid={3}"
            //    , dian1
            //    , dian2
            //    , table1
            //    , id1);
            //OracleCommand command = new OracleCommand(queryString, myConnection);
            //command.ExecuteScalar();//执行command命令
            //string result = "";
            //if (command.ExecuteScalar() != System.DBNull.Value)
            //{
            //    result = (string)command.ExecuteScalar();
            //}
            //Console.WriteLine(result);
            //if (result.Equals("COVERS") || result.Equals("COVERSBY") || result.Equals("CONTAINS") || result.Equals("INSIDE") || result.Equals("EQUAL")) return true;
            return false;
        }



        //判断向量线A与线B的关系，1表示向量线A在B的左边，-1表示向量A在B的右边
        public  int getOrientation(string dian, string dA, string dB)
        {
            string[] xy = dian.Trim().Split(',');
            string[] dAxy = dA.Trim().Split(',');
            string[] dBxy = dB.Trim().Split(',');
            double ax = double.Parse(dAxy[0]) - double.Parse(xy[0]);
            double ay = double.Parse(dAxy[1]) - double.Parse(xy[1]);
            double bx = double.Parse(dBxy[0]) - double.Parse(xy[0]);
            double by = double.Parse(dBxy[1]) - double.Parse(xy[1]);

            if (ax * by - ay * bx > 0) return -1;
            if (ax * by - ay * bx < 0) return 1;
            return 0;
        }


        //求目标的维数,------------------------------------------------------------------------------------------存在问题，还未考虑完
        //0为点，1为线，2为面
        public int getOSCDimension(OracleConnection myConnection, string table1, string id1)
        {
            int i = 0;
            string sql = String.Format("select c_a.SHAPE.GET_GTYPE() from {0} c_a where c_a.objectid={1}",
                 table1, id1);

            //myConnection.Open();
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            OracleDataReader myDataReader = command.ExecuteReader();
            string dimension = "";
            while (myDataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了  
            {
                dimension = myDataReader.GetOracleValue(0).ToString();
                //Console.WriteLine(dimension);
            }
            myDataReader.Close();
            //myConnection.Close();
            if (!dimension.Equals("null"))
            {
                i = Convert.ToInt32(dimension);
            }
            return i % 4 - 1;
        }

        public int getOSMDimension(OracleConnection myConnection, string table1, string id1)
        {
            int i = 0;
            string sql = String.Format("select c_a.SHAPE.GET_GTYPE() from {0} c_a where c_a.objectid={1}",
                 table1, id1);

            //myConnection.Open();
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            OracleDataReader myDataReader = command.ExecuteReader();
            string dimension = "";
            while (myDataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了  
            {
                dimension = myDataReader.GetOracleValue(0).ToString();
                //Console.WriteLine(dimension);
            }
            myDataReader.Close();
            //myConnection.Close();
            if (!dimension.Equals("null"))
            {
                i = Convert.ToInt32(dimension);
            }
            return i % 4 - 1;
        }



        //查询规则库判断是否是错误的拓扑
        //false代表有错
        public Boolean isTrueTopo(OracleConnection conn,string featureA,string featureB,string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (type.Equals("A") && !featureA.Equals(featureB)) return false;
            if (!type.Equals("A") && featureA.Equals(featureB)) return true;
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            string queryString = String.Format(
                                "select * from topologyruler where featurea='{0}' and featureb='{1}' and type='{2}'",
                                 featureA,featureB,type);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                if (dr.Read())
                {
                    return false;
                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return true;
        }


        //查询要素属性名
        public string getNationeleName(OracleConnection conn, string table, string objectid)
        {
            string feature=null;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            string queryString = String.Format(
                                "select nationelename from {0} where objectid={1}",
                                 table,objectid);
            OracleCommand command = new OracleCommand(queryString, conn);
            using (OracleDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    feature = dr["nationelename"].ToString();
                }
                //Console.WriteLine(gml);
                //myConnection.Close();
            }
            #endregion
            return feature;
        }


        //将交点的拓扑关系存入表
        public void insertInsectPoint(OracleConnection conn, string table1, string id1,string table2,string id2,string wkt,string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            string feature1 = getNationeleName(conn, table1, id1);
            string feature2 = getNationeleName(conn, table2, id2);
            if (!isTrueTopo(conn, feature1, feature2, type)) return;
            OracleDBHelper db = new OracleDBHelper();
            //数据入库
            StringBuilder strSql = new StringBuilder();
            string sql1 = string.Format("insert into insectpoint(objectid1,objectid2,feature1,feature2,shape,type,layer1,layer2) values ");
            strSql.Append(sql1);
            wkt = string.Format("POINT({0})", wkt);
            //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
            string sql2 = string.Format("({0},{1},'{2}','{3}',sdo_geometry ('{4}',4326),'{5}','{6}','{7}')", id1, id2, feature1, feature2, wkt, type, table1, table2);
            strSql.Append(sql2);
            using (OracleCommand cmd = new OracleCommand(strSql.ToString(), conn))
            {
                //using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                //{
                //    wkt = string.Format("'POINT({0})'",wkt);
                //    p1.Value = wkt;
                //    cmd.Parameters.Add(p1);
                    cmd.ExecuteNonQuery();
                //}
            }
        }


        //将交线的拓扑关系存入表
        public void insertInsectLine(OracleConnection conn, string table1, string id1, string table2, string id2, string wkt, string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            string feature1 = getNationeleName(conn, table1, id1);
            string feature2 = getNationeleName(conn, table2, id2);
            if (!isTrueTopo(conn, feature1, feature2, type)) return;
            OracleDBHelper db = new OracleDBHelper();
            //数据入库
            StringBuilder strSql = new StringBuilder();
            string sql1 = string.Format("insert into insectline(objectid1,objectid2,feature1,feature2,shape,type,layer1,layer2) values");
            strSql.Append(sql1);
            wkt = string.Format("LINESTRING({0})", wkt);
            //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
            string sql2 = string.Format("({0},{1},'{2}','{3}',sdo_geometry ('{4}',4326),'{5}','{6}','{7}')", id1, id2, feature1, feature2, wkt, type,table1,table2);
            strSql.Append(sql2);
            using (OracleCommand cmd = new OracleCommand(strSql.ToString(), conn))
            {
                //using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                //{
                //    wkt = string.Format("'LINESTRING({0})'", wkt);
                //    p1.Value = wkt;
                //    cmd.Parameters.Add(p1);
                    cmd.ExecuteNonQuery();
                //}
            }
        }




    }
}
