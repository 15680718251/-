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
 * 实现线面拓扑计算的功能
 * 拆分交线，以单元交为基础细分类型
 * 2019.3.6
 * zh编写
 */
namespace QualityControl.Topo
{
    class TopoLineByAreaCalculation2
    {

        #region 全局变量
        #region 线面交线类型
        //-----------------------------
        int ta = 0;
        int tb = 0;
        int tc = 0;
        int td = 0;
        int te = 0;
        int tf = 0;
        int tg = 0;
        int th = 0;
        int ti = 0;
        int tj = 0;
        int tk = 0;
        int tl = 0;
        int tm = 0;
        int tn = 0;
        int to = 0;
        int tp = 0;
        int tq = 0;
        int tr = 0;
        int ts = 0;
        int tt = 0;
        int tu = 0;
        //-----------------------------
        #endregion
        #endregion

        //获取osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tablename)
        {
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
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
        public ArrayList getOSMIDList(OracleConnection conn, string tableosc, string tableosm)
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
        public ArrayList getOSMIDList(OracleConnection conn, string tableosc, string oscid, string tableosm)
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
        public ArrayList getOSCIDList(OracleConnection conn, string tableosc, string tableosm)
        {
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            HashSet<string> set = new HashSet<string>();
            string queryString = String.Format(
                                @"SELECT A.objectid
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
        #region
        //public ArrayList getOSCIDList(OracleConnection conn, string tablename)
        //{
        //    if (conn.State == ConnectionState.Closed)
        //    {
        //        conn.Open();
        //    }
        //    #region 连接读取oracle中的数据
        //    ArrayList idList = new ArrayList();
        //    string queryString = String.Format(
        //        //"select objectid from {0} where updatestate=2 or updatestate=3",
        //                        "select objectid from {0}",
        //                         tablename);
        //    OracleCommand command = new OracleCommand(queryString, conn);
        //    using (OracleDataReader dr = command.ExecuteReader())
        //    {
        //        while (dr.Read())
        //        {
        //            idList.Add(dr["objectid"].ToString());

        //        }
        //        //Console.WriteLine(gml);
        //        //myConnection.Close();
        //    }
        //    #endregion
        //    return idList;
        //}
        #endregion

        //获取线面的拓扑关系
        //table1为线，table2为面
        public ArrayList getTopoByLineInsectArea(OracleConnection conn, string table1, string table2, string id1, string id2)
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


            if (gml == null || gml.Equals("null")) return result;
            if (!gml.Equals("null"))
            {

                Console.WriteLine("交线id " + id1 + " 交面id " + id2);
                //Console.WriteLine(gml);

                string line = getOSCLine(conn, table1, "" + id1);//线目标A的点串-----------------------------------------------------------------------------------------------

                #region 匹配字符串组

                #region 变量
                ArrayList jiedianList = new ArrayList();//zh 非排序节点集合（包括交点和交线的端点）
                ArrayList pointList = new ArrayList();//zh 交点的集合
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
                    ArrayList duandianList = new ArrayList();//zh 非排序交线点集合
                    ArrayList insectLinelist = new ArrayList();//zh 交线的线序列集合
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
                    //tuple = tuple.Replace(" ", "*");
                    //tuple = tuple.Replace(",", " ");
                    //tuple = tuple.Replace("*", ",");
                    //insectLinelist.Add(tuple);
                    jiedianList.Add(zu[0]);
                    jiedianList.Add(zu[zu.Length - 1]);
                    //将单元线端点加入集合
                    int start = duandianList.Count;
                    insertDuanDianList(conn, zu, table2, id2, duandianList);
                    int end = duandianList.Count;



                    #region 重构线的点串,排序单元交线端点
                    //重构线A
                    ArrayList temp = (ArrayList)duandianList.Clone();//临时的节点串
                    ArrayList lineList = new ArrayList();//线的点串
                    string[] linezu = line.Split(' ');
                    for (int k = 0; k < linezu.Length; k++)
                    {
                        string[] xy = linezu[k].Split(',');
                        string zuobiao = xy[0] + "," + xy[1];
                        lineList.Add(zuobiao);
                    }
                    //将节点按顺序加入线
                    for (int m = 0; m < lineList.Count - 1; m++)
                    {
                        for (int j = 0; j < jiedianList.Count; j++)
                        {
                            if (isIn((string)lineList[m], (string)lineList[m + 1], (string)jiedianList[j]))
                            {
                                if (!((string)lineList[m]).Equals((string)jiedianList[j]) && !((string)lineList[m + 1]).Equals((string)jiedianList[j]))
                                {
                                    lineList.Insert(m + 1, (string)jiedianList[j]);

                                }
                                jiedianList.RemoveAt(j);
                                m--;
                                break;
                            }
                        }
                    }
                    //排序单元线端点
                    ArrayList newline = new ArrayList();//新的排序的节点序列
                    for (int m = 0; m < lineList.Count; m++)
                    {
                        string lp = (string)lineList[m];
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

                    #endregion
                    //将单元线加入集合
                    insertLineList(conn, linezu, newline, insectLinelist);

                    #region 求线/面 交为线的情况
                    ArrayList du = new ArrayList();
                    string duan1 = (string)lineList[0];
                    string duan2 = (string)lineList[lineList.Count - 1];
                    int flag = 0;
                    for (int m = 0; m < newline.Count; m++)//---------------------------------------------
                    {
                        string dian = (string)newline[m];//---------------------------------------------

                        if (flag % 2 == 1)
                        {
                            m--;
                        }
                        int index = 0;//节点在点串中的位置
                        for (int j = 0; j < lineList.Count; j++)
                        {
                            string lpoint = (string)lineList[j];
                            if (lpoint.Equals(dian)) index = j;
                        }
                        //判端交线点是否为端点
                        if (dian.Equals(duan1) || dian.Equals(duan2))
                        {
                            //判断点是否在面的边界上
                            if (isTouchArea(conn, dian, table2, id2))
                            {

                                string next = null;
                                if (index == 0) next = (string)lineList[index + 1];
                                else next = (string)lineList[index - 1];
                                //判断下一个附近线是否在面的边界上
                                if (isTouchArea(conn, dian, next, table2, id2)) du.Add(2);
                                else du.Add(3);
                            }
                            else
                            {
                                du.Add(1);
                            }
                        }
                        else
                        {
                            if (index == 0) continue;
                            string next = (string)lineList[index + 1];
                            string pre = (string)lineList[index - 1];
                            //判断相邻线是否在面的边界上
                            if (isTouchArea(conn, dian, pre, table2, id2) || isTouchArea(conn, dian, next, table2, id2))
                            {
                                string type = "";
                                if (flag % 2 == 0)//---------------------------------------------
                                {
                                    type = getType(conn, dian, pre, table2, id2);
                                    du.Add(3 + "," + type);
                                }
                                else
                                {
                                    type = getType(conn, dian, next, table2, id2);
                                    du.Add(3 + "," + type);
                                }

                            }
                            else
                            {
                                string type = "";
                                if (flag % 2 == 0)//---------------------------------------------
                                {
                                    type = getType(conn, dian, pre, table2, id2);
                                    du.Add(4 + "," + type);
                                }
                                else
                                {
                                    type = getType(conn, dian, next, table2, id2);
                                    du.Add(4 + "," + type);
                                }
                            }
                        }
                        flag++;//---------------------------------------------
                        //
                        if (flag % 2 == 0)
                        {
                            #region 统计线面交类型个数
                            if ((m + 1) == du.Count) break;
                            string type = du[flag - 2].ToString() + "," + du[flag - 1].ToString();
                            if (type.Equals("1,1"))
                            {
                                ta++;
                                Console.WriteLine("a");
                                result.Add("A");
                            }
                            if (type.Equals("1,3") || type.Equals("3,1"))
                            {
                                tb++;
                                Console.WriteLine("b");
                                result.Add("B");
                            }
                            if (type.Equals("1,3,O") || type.Equals("3,O,1"))
                            {
                                tc++;
                                Console.WriteLine("c");
                                result.Add("C");
                            }
                            if (type.Equals("1,4,E") || type.Equals("4,E,1"))
                            {
                                td++;
                                Console.WriteLine("d");
                                result.Add("D");
                            }
                            if (type.Equals("1,4,I") || type.Equals("4,I,1"))
                            {
                                te++;
                                Console.WriteLine("e");
                                result.Add("E");
                            }
                            if (type.Equals("2,3,E") || type.Equals("3,E,1"))
                            {
                                tf++;
                                Console.WriteLine("f");
                                result.Add("F");
                            }
                            if (type.Equals("2,3,I") || type.Equals("3,I,1"))
                            {
                                tg++;
                                Console.WriteLine("g");
                                result.Add("G");
                            }
                            if (type.Equals("2,2"))
                            {
                                th++;
                                Console.WriteLine("h");
                                result.Add("H");
                            }
                            if (type.Equals("3,E,3,E"))
                            {
                                ti++;
                                Console.WriteLine("i");
                                result.Add("I");
                            }
                            if (type.Equals("3,I,3,E") || type.Equals("3,E,3,I"))
                            {
                                tj++;
                                Console.WriteLine("j");
                                result.Add("J");
                            }
                            if (type.Equals("3,I,3,I"))
                            {
                                tk++;
                                Console.WriteLine("k");
                                result.Add("K");
                            }
                            if (type.Equals("3,3"))
                            {
                                tl++;
                                Console.WriteLine("l");
                                result.Add("L");
                            }
                            if (type.Equals("3,3,O") || type.Equals("3,O,3"))
                            {
                                tm++;
                                Console.WriteLine("m");
                                result.Add("M");
                            }
                            if (type.Equals("3,O,3,O"))
                            {
                                tn++;
                                Console.WriteLine("n");
                                result.Add("N");
                            }
                            if (type.Equals("3,4,E") || type.Equals("4,E,3"))
                            {
                                to++;
                                Console.WriteLine("o");
                                result.Add("O");
                            }
                            if (type.Equals("3,O,4,E") || type.Equals("4,E,3,O"))
                            {
                                tp++;
                                Console.WriteLine("p");
                                result.Add("P");
                            }
                            if (type.Equals("3,4,I") || type.Equals("4,I,3"))
                            {
                                tq++;
                                Console.WriteLine("q");
                                result.Add("Q");
                            }
                            if (type.Equals("3,O,4,I") || type.Equals("4,I,3,O"))
                            {
                                tr++;
                                Console.WriteLine("r");
                                result.Add("R");
                            }
                            if (type.Equals("4,E,4,E"))
                            {
                                ts++;
                                Console.WriteLine("s");
                                result.Add("S");
                            }
                            if (type.Equals("4,I,4,E") || type.Equals("4,E,4,I"))
                            {
                                tt++;
                                Console.WriteLine("t");
                                result.Add("T");
                            }
                            if (type.Equals("4,I,4,I"))
                            {
                                tu++;
                                Console.WriteLine("u");
                                result.Add("U");
                            }
                            #endregion

                            insertInsectLine(conn, table1, id1, table2, id2, (string)insectLinelist[m / 2], (string)result[result.Count - 1]);
                        }
                    }


                    #endregion
                }


                #endregion

                #region //重构线A
                //重构线A
                ArrayList temp1 = (ArrayList)pointList.Clone();//临时的节点串
                ArrayList lineList1 = new ArrayList();//线的点串
                string[] linezu1 = line.Split(' ');
                for (int k = 0; k < linezu1.Length; k++)
                {
                    string[] xy = linezu1[k].Split(',');
                    string zuobiao = xy[0] + "," + xy[1];
                    lineList1.Add(zuobiao);
                }
                //将节点按顺序加入线
                for (int i = 0; i < lineList1.Count - 1; i++)
                {
                    for (int j = 0; j < jiedianList.Count; j++)
                    {
                        if (isIn((string)lineList1[i], (string)lineList1[i + 1], (string)jiedianList[j]))
                        {
                            if (!((string)lineList1[i]).Equals((string)jiedianList[j]) && !((string)lineList1[i + 1]).Equals((string)jiedianList[j]))
                            {
                                lineList1.Insert(i + 1, (string)jiedianList[j]);

                            }
                            jiedianList.RemoveAt(j);
                            i--;
                            break;
                        }
                    }
                }
                #endregion

                #region 求线/面 交点的节点度
                int count = 0;
                ArrayList du1 = new ArrayList();
                for (int i = 0; i < pointList.Count; i++)
                {
                    string dian = (string)pointList[i];
                    int indexOfA = getIndexOfLine(dian, lineList1);//节点在点串A中的位置
                    //判端交线点是否为线目标A的端点
                    if (indexOfA == 0 || indexOfA == lineList1.Count - 1)
                    {
                        result.Add("V");
                    }
                    else
                    {
                        result.Add("W");
                    }
                    //Console.WriteLine("交点--" + du1[i].ToString());
                    dian = dian.Replace(",", " ");
                    insertInsectPoint(conn, table1, id1, table2, id2, dian, (string)result[result.Count - 1]);
                }
                #endregion



            }
            return result;
        }




        //更新osc数据
        public void updateOSCLine(OracleConnection conn, string table, string id, string wkt)
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
        public bool isIn(string str1, string str2, string str)
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
        public string getOSCLine(OracleConnection myConnection, string table1, string id1)
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
        public int getIndexOfLine(string dian, ArrayList lineList)
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
        public bool isInLine(OracleConnection myConnection, string dian1, string dian2, string table1, string id1)
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
        public int getOrientation(string dian, string dA, string dB)
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
        public Boolean isTrueTopo(OracleConnection conn, string featureA, string featureB, string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (type.Equals("A") && !featureA.Equals(featureB)) return false;
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            string queryString = String.Format(
                                "select * from topologyruler where featurea='{0}' and featureb='{1}' and type='{2}'",
                                 featureA, featureB, type);
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
            string feature = null;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            string queryString = String.Format(
                                "select nationelename from {0} where objectid={1}",
                                 table, objectid);
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
        public void insertInsectPoint(OracleConnection conn, string table1, string id1, string table2, string id2, string wkt, string type)
        {
            if (wkt == null || wkt.Trim().Equals("")) return;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            //string feature1 = getNationeleName(conn, table1, id1);
            //string feature2 = getNationeleName(conn, table2, id2);
            string feature1 = "";
            string feature2 = "";
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
            if (wkt == null || wkt.Trim().Equals("")) return;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            //string feature1 = getNationeleName(conn, table1, id1);
            //string feature2 = getNationeleName(conn, table2, id2);
            string feature1 = "";
            string feature2 = "";
            OracleDBHelper db = new OracleDBHelper();
            //数据入库
            StringBuilder strSql = new StringBuilder();
            string sql1 = string.Format("insert into insectline(objectid1,objectid2,feature1,feature2,shape,type,layer1,layer2) values");
            strSql.Append(sql1);
            wkt = string.Format("LINESTRING({0})", wkt);
            //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
            string sql2 = string.Format("({0},{1},'{2}','{3}',sdo_geometry ('{4}',4326),'{5}','{6}','{7}')", id1, id2, feature1, feature2, wkt, type, table1, table2);
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




        //判断点是否在面的边界上
        public static bool isTouchArea(OracleConnection myConnection, string point, string table1, string id1)
        {
            string queryString = String.Format(
                "select sdo_geom.relate(c_a.SHAPE,'touch',MDSYS.SDO_GEOMETRY(2001 , 4326 , MDSYS.SDO_POINT_TYPE ({0},4326),  NULL ,  NULL),0.000001) from {1} c_a where c_a.OBJECTID={2}"
                , point
                , table1
                , id1);
            OracleCommand command = new OracleCommand(queryString, myConnection);
            command.ExecuteScalar();//执行command命令
            string result = "";
            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                result = (string)command.ExecuteScalar();
            }
            //Console.WriteLine(result);
            if (result.Equals("FALSE") || result.Equals("false")) return false;
            return true;
        }



        //判断线是否在面的边界上
        public static bool isTouchArea(OracleConnection myConnection, string dian1, string dian2, string table1, string id1)
        {
            string queryString = String.Format(
                "select sdo_geom.relate(MDSYS.SDO_GEOMETRY(2002,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,2,1),MDSYS.SDO_ORDINATE_ARRAY({0},{1})),'on',c_a.SHAPE,0.000001) from {2} c_a where c_a.OBJECTID={3}"
                , dian1
                , dian2
                , table1
                , id1);
            OracleCommand command = new OracleCommand(queryString, myConnection);
            command.ExecuteScalar();//执行command命令
            string result = "";
            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                result = (string)command.ExecuteScalar();
            }
            //Console.WriteLine(result);
            if (result.Equals("FALSE") || result.Equals("false")) return false;
            return true;
        }


        //判断线与面的关系，On为O,Touch为E,Coveredby为I
        public static string getType(OracleConnection myConnection, string dian1, string dian2, string table1, string id1)
        {
            string type = null;
            string queryString = String.Format(
                "select sdo_geom.relate(MDSYS.SDO_GEOMETRY(2002,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,2,1),MDSYS.SDO_ORDINATE_ARRAY({0},{1})),'determine',c_a.SHAPE,0.000001) from {2} c_a where c_a.OBJECTID={3}"
                , dian1
                , dian2
                , table1
                , id1);
            OracleCommand command = new OracleCommand(queryString, myConnection);
            command.ExecuteScalar();//执行command命令
            string result = "";
            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                result = (string)command.ExecuteScalar();
            }
            switch (result)
            {
                case "ON":
                    type = "O";
                    break;
                case "TOUCH":
                    type = "E";
                    break;
                case "DISJOINT":
                    type = "E";
                    break;
                case "COVEREDBY":
                    type = "I";
                    break;
                case "INSIDE":
                    type = "I";
                    break;
                case "OVERLAPBDYDISJOINT":
                    type = "I";
                    break;
                case "OVERLAPBDYINTERSECT":
                    type = "I";
                    break;
                default:
                    type = result;
                    break;
            }
            //Console.WriteLine(type);
            return type;
        }


        //---------------------------------------------
        //获取线面相交的端点,线面的单元交
        public void insertDuanDianList(OracleConnection myConnection, string[] zu, string table1, string id1, ArrayList list)
        {
            HashSet<string> set = new HashSet<string>();
            set.Add(zu[0]);
            set.Add(zu[zu.Length - 1]);
            //获取第一个端点到最后一个端点方向的断点
            for (int i = 1; i < zu.Length - 1; i++)
            {
                string mid = getMid(zu[i], zu[i + 1]);
                if (isTouchArea(myConnection, zu[i], table1, id1) && !isTouchArea(myConnection, mid, table1, id1))
                {
                    set.Add(zu[i]);
                }
            }
            //获取最后一个端点到第一个端点方向的断点
            for (int i = zu.Length - 2; i > 0; i--)
            {
                string mid = getMid(zu[i], zu[i - 1]);
                if (isTouchArea(myConnection, zu[i], table1, id1) && !isTouchArea(myConnection, mid, table1, id1))
                {
                    set.Add(zu[i]);
                }
            }
            //
            foreach (var team in set)
            {
                list.Add(team);
            }
        }

        //获取两点之间的中间点
        public string getMid(string str1, string str2)
        {
            string[] dian1 = str1.Trim().Split(',');
            string[] dian2 = str2.Trim().Split(',');
            double x = double.Parse(dian1[0]) + double.Parse(dian2[0]);
            double y = double.Parse(dian1[1]) + double.Parse(dian2[1]);
            x = x / 2;
            y = y / 2;
            string dian = x + "," + y;
            return dian;
        }

        //---------------------------------------------
        //获取线面相交的端点,线面的单元交
        public void insertLineList(OracleConnection myConnection, string[] zu, ArrayList duandianList, ArrayList linelist)
        {
            for (int i = 0; i < duandianList.Count - 1; i++)
            {
                string dian1 = (string)duandianList[i];
                string dian2 = (string)duandianList[i + 1];
                string line = "";
                bool flag = false;
                for (int j = 0; j < zu.Length; j++)
                {
                    if (zu[j].Equals(dian1)) flag = true;
                    if (flag) line = line + " " + zu[j];
                    if (zu[j].Equals(dian2)) break;
                }
                line = line.Trim();
                line = line.Replace(" ", "*");
                line = line.Replace(",", " ");
                line = line.Replace("*", ",");
                linelist.Add(line);
            }
        }
    }
}
