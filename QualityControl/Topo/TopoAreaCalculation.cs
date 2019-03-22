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
 * 实现面面拓扑计算的功能
 * zh编写
 */

namespace QualityControl.Topo
{
    class TopoAreaCalculation
    {
         

        //获取osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tablename)
        {
            #region 连接读取oracle中的数据
            ArrayList idList=new ArrayList();
            string queryString = String.Format(
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

        //获取osm的id集合
        public ArrayList getOSMIDList(OracleConnection conn, string tableosc,string tableosm)
        {
            #region 连接读取oracle中的数据
            ArrayList idList = new ArrayList();
            HashSet<string> set = new HashSet<string>();
            string queryString = String.Format(
                                @"SELECT B.objectid
  FROM {0} A ,{1} B
WHERE SDO_WITHIN_DISTANCE(A.shape, 
                           B.shape, 
                            'distance=' || 100 || ' unit=m') = 'TRUE'",
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

        //获取osc的id集合
        public ArrayList getOSCIDList(OracleConnection conn, string tablename)
        {
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

        //获取面面的拓扑关系
        public ArrayList getTopoByAreaInsectArea(OracleConnection conn, string table1, string table2, string id1, string id2)
        {
             #region 连接读取oracle中的数据
            string queryString = String.Format(
    "select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(c_a.SHAPE,c_c.SHAPE,0.0001)) from {0} c_a,{1} c_c where c_a.objectid={2} and c_c.objectid={3}",
    table1,
    table2,
    id1,
    id2);
            OracleCommand command0 = new OracleCommand(queryString, conn);
            
            string gml = "null";

            if (command0.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command0.ExecuteScalar();
            }
            //Console.WriteLine(gml);
            //myConnection.Close();
            #endregion

            ArrayList result = new ArrayList();
            if (gml.Equals("null")) return result;
            if (!gml.Equals("null"))
            {
                #region 匹配字符串组
                //1.匹配Polygon
                MatchCollection matches = Regex.Matches(gml, @"(<gml:Polygon)((?!Polygon).)*(gml:Polygon>)");
                //匹配组从0开始
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    //match.Value是匹配的内容
                    Console.WriteLine(match.Value);

                    string Polygon = match.Value;
                    Match match0 = Regex.Match(Polygon, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                    string poly = match0.Value;
                    Match match1 = Regex.Match(poly, @"[-|0-9].+[0-9]");
                    string tuple = match1.Value;
                    string wkt = tuple;
                    tuple = tuple.Replace(" ", ",");
                    Console.WriteLine(tuple);

                    #region 交为面，求面差的欧拉数，目标为面/面
                    int ae = getOSCAreaDifferenceEuler(conn, tuple, table1, id1);
                    int be = getOSMAreaDifferenceEuler(conn, tuple, table2, id2);
                    //result = result + "(" + 2 + "," + ae + "," + be + "),";
                    #region 面面交面分类
                    if (ae == -1 && be == -1) result.Add("A");
                    else if ((ae == -1 && be == 0) || (ae == 0 && be == -1)) result.Add("B");
                    else if ((ae == -1 && be == 1) || (ae == 1 && be == -1)) result.Add("C");
                    else if ((ae == -1 && be == 2) || (ae == 2 && be == -1)) result.Add("D");
                    else if (ae == 1 && be == 1) result.Add("E");
                    else if ((ae == 1 && be == 2) || (ae == 2 && be == 1)) result.Add("F");
                    else if (ae == 2 && be == 2) result.Add("G");
                    else if ((ae == -1 && be > 2) || (ae > 2 && be == -1)) result.Add("H");
                    else if (ae > 2  && be > 2) result.Add("I");
                    #endregion

                    //将交面插入交面表
                    wkt = wkt.Replace(" ", "*");
                    wkt = wkt.Replace(",", " ");
                    wkt = wkt.Replace("*", ",");
                    if (result.Count == 0) return result;
                    insertInsectArea(conn, table1, id1, table2, id2, wkt, (string)result[result.Count - 1]);
                    #endregion
                }

                #region 匹配点线，拓扑检测不用，废弃
                ArrayList jiedianList = new ArrayList();//zh 非排序节点集合
                ArrayList pointList = new ArrayList();//zh 交点的集合
                //2.匹配Point
                //matches = Regex.Matches(gml, @"(<gml:Point)((?!Point).)*(gml:Point>)");
                ////匹配组从0开始
                //for (int i = 0; i < matches.Count; i++)
                //{
                //    Match match = matches[i];
                //    //match.Value是匹配的内容

                //    string pointString = match.Value;
                //    Match match0 = Regex.Match(pointString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");//---------------------------------------修改
                //    string Point = match0.Value;//---------------------------------------------------------------------------------------------------------------修改
                //    Match match1 = Regex.Match(Point, @"[-|0-9].+[0-9]");//--------------------------------------------------------------------------------------zh修改0927
                //    string tuple = match1.Value;
                //    jiedianList.Add(tuple);//zh
                //    pointList.Add(tuple);//zh
                //}

                //ArrayList lpList = new ArrayList();//zh 非排序交线点集合
                ////3.匹配LineString
                //matches = Regex.Matches(gml, @"(<gml:LineString)((?!LineString).)*(gml:LineString>)");
                ////匹配组从0开始
                //for (int i = 0; i < matches.Count; i++)
                //{
                //    Match match = matches[i];
                //    //match.Value是匹配的内容

                //    string LineString = match.Value;
                //    Match match0 = Regex.Match(LineString, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                //    string lineString = match0.Value;
                //    Match match1 = Regex.Match(lineString, @"[-|0-9].+[0-9]");
                //    string tuple = match1.Value;
                //    //截取交线的端点 zh
                //    string[] zu = tuple.Split(' ');
                //    jiedianList.Add(zu[0]);
                //    jiedianList.Add(zu[zu.Length - 1]);
                //    lpList.Add(zu[0]);
                //    lpList.Add(zu[zu.Length - 1]);
                //    Console.WriteLine(match1.Value);
                //    //--
                //}
                #endregion

                #endregion

                #region 细分类型，交为点，且目标为面/面，拓扑检测不用，废弃
                //                    string area1 = getOSCArea(conn, table1, id1);
//                    string area2 = getOSMArea(conn, table2, id2);

//                    string box = getExterBox(area1, area2);
//                    Console.WriteLine(box);
//                    //box="-6,16.5,18,16.5,18,-1.5,-6,-1.5,-6,16.5"
//                    string sql = String.Format(@"select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_INTERSECTION(
//                                       SDO_GEOM.SDO_DIFFERENCE(MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1),MDSYS.SDO_ORDINATE_ARRAY({0})),c_a.SHAPE,0.005),
//                                       SDO_GEOM.SDO_DIFFERENCE(MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1),MDSYS.SDO_ORDINATE_ARRAY({1})),c_c.SHAPE,0.005),
//                                       0.005)) from {2} c_a,{3} c_c where c_a.objectid={4} and c_c.objectid={5}", box, box,table1,table2,id1,id2);
//                    OracleCommand command = new OracleCommand(sql, conn);
//                    gml = null;
//                    if (command.ExecuteScalar() != System.DBNull.Value)
//                    {
//                        gml = (string)command.ExecuteScalar();
//                        Console.WriteLine("--------------------------------------------------------------------------------------------------");
//                        Console.WriteLine(gml);
//                    }
//                    if (gml != null)
//                    {

//                        ArrayList emptyAreaList = new ArrayList();//有空洞的交面的集合
//                        ArrayList AreaList = new ArrayList();//没有空洞的交面的集合
//                        #region 获取外部交集的集合
//                        //匹配Polygon
//                        MatchCollection polygons = Regex.Matches(gml, @"(<gml:Polygon)((?!Polygon).)*(gml:Polygon>)");
//                        //匹配组从0开始
//                        for (int i = 0; i < polygons.Count; i++)
//                        {
//                            Match match = polygons[i];
//                            //match.Value是匹配的内容
//                            Console.WriteLine(match.Value);

//                            string Polygon = match.Value;
//                            //匹配内部外部面
//                            Match outArea = Regex.Match(Polygon, @"outerBoundaryIs>.+outerBoundaryIs>");
//                            Match inArea = Regex.Match(Polygon, @"innerBoundaryIs>.+innerBoundaryIs>");

//                            if (inArea.Value == null || inArea.Value.Trim().Equals(""))
//                            {
//                                Match match1 = Regex.Match(outArea.Value, @"[-|0-9].+[0-9]");
//                                string tuple = match1.Value;
//                                tuple = tuple.Replace(" ", ",");
//                                Console.WriteLine(tuple);
//                                AreaList.Add(tuple);
//                            }
//                            else
//                            {
//                                Match match1 = Regex.Match(outArea.Value, @"[-|0-9].+[0-9]");
//                                string outa = match1.Value;
//                                outa = outa.Replace(" ", ",");
//                                Console.WriteLine(outa);
//                                emptyAreaList.Add(outa);

//                                Match match2 = Regex.Match(inArea.Value, @"[-|0-9].+[0-9]");
//                                string ina = match2.Value;
//                                ina = ina.Replace(" ", ",");
//                                Console.WriteLine(ina);
//                                emptyAreaList.Add(ina);
//                            }

//                        }
//                        #endregion

//                        string zuo = "";
//                        string you = "";
//                        for (int i = 0; i < pointList.Count; i++)
//                        {
//                            string point = (string)pointList[i];
//                            //遍历没有空洞的交面的集合
//                            for (int j = 0; j < AreaList.Count; j++)
//                            {
//                                string outa = (string)AreaList[j];
//                                //判断点面是否相交
//                                if (isInsect(conn, point, outa))
//                                {
//                                    if (zuo.Equals("")) zuo = "B";
//                                    else you = "B";
//                                }
//                            }
//                            //遍历有空洞的交面的集合
//                            for (int j = 0; j < emptyAreaList.Count; j = j + 2)
//                            {
//                                string outa = (string)emptyAreaList[j];
//                                string ina = (string)emptyAreaList[j + 1];
//                                //判断点面是否相交
//                                if (isInsect(conn, point, outa, ina))
//                                {
//                                    if (zuo.Equals("")) zuo = "U";
//                                    else you = "U";
//                                }
//                            }
//                            //result = result + "(" + "0," + zuo + "," + you + "),";
//                        }
//                    }
                #endregion

                #region 求交线的细分类型，目标为面面，拓扑检测不用，废弃
//                        sql = String.Format(@"select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_DIFFERENCE(
//                                       MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1),MDSYS.SDO_ORDINATE_ARRAY({0})),
//                                       SDO_GEOM.SDO_UNION(c_a.SHAPE,c_c.SHAPE,0.005),
//                                       0.005)) from {1} c_a,{2} c_c where c_a.objectid={3} and c_c.objectid={4}", box,table1,table2, id1, id2);
//                        command = new OracleCommand(sql, conn);
//                        gml = null;
//                        if (command.ExecuteScalar() != System.DBNull.Value)
//                        {
//                            gml = (string)command.ExecuteScalar();
//                            Console.WriteLine("--------------------------------------------------------------------------------------------------");
//                            Console.WriteLine(gml);
//                        }
//                        if (gml != null)
//                        {

//                            ArrayList emptyAreaList = new ArrayList();//有空洞的交面的集合
//                            ArrayList AreaList = new ArrayList();//没有空洞的交面的集合
//                            #region 获取外部交集的集合
//                            //匹配Polygon
//                            MatchCollection polygons = Regex.Matches(gml, @"(<gml:Polygon)((?!Polygon).)*(gml:Polygon>)");
//                            //匹配组从0开始
//                            for (int i = 0; i < polygons.Count; i++)
//                            {
//                                Match match = polygons[i];
//                                //match.Value是匹配的内容
//                                Console.WriteLine(match.Value);

//                                string Polygon = match.Value;
//                                //匹配内部外部面
//                                Match outArea = Regex.Match(Polygon, @"outerBoundaryIs>.+outerBoundaryIs>");
//                                Match inArea = Regex.Match(Polygon, @"innerBoundaryIs>.+innerBoundaryIs>");

//                                if (inArea.Value == null || inArea.Value.Trim().Equals(""))
//                                {
//                                    Match match1 = Regex.Match(outArea.Value, @"[-|0-9].+[0-9]");
//                                    string tuple = match1.Value;
//                                    tuple = tuple.Replace(" ", ",");
//                                    Console.WriteLine(tuple);
//                                    AreaList.Add(tuple);
//                                }
//                                else
//                                {
//                                    Match match1 = Regex.Match(outArea.Value, @"[-|0-9].+[0-9]");
//                                    string outa = match1.Value;
//                                    outa = outa.Replace(" ", ",");
//                                    Console.WriteLine(outa);
//                                    emptyAreaList.Add(outa);

//                                    Match match2 = Regex.Match(inArea.Value, @"[-|0-9].+[0-9]");
//                                    string ina = match2.Value;
//                                    ina = ina.Replace(" ", ",");
//                                    Console.WriteLine(ina);
//                                    emptyAreaList.Add(ina);
//                                }

//                            }
//                            #endregion

//                            string zuo = "";
//                            string you = "";
//                            for (int i = 0; i < lpList.Count; i++)
//                            {
//                                string point = (string)lpList[i];
//                                //遍历没有空洞的交面的集合
//                                for (int j = 0; j < AreaList.Count; j++)
//                                {
//                                    string outa = (string)AreaList[j];
//                                    //判断点面是否相交
//                                    if (isInsect(conn, point, outa))
//                                    {
//                                        if (zuo.Equals("")) zuo = "B";
//                                        else you = "B";
//                                    }
//                                }
//                                //遍历有空洞的交面的集合
//                                for (int j = 0; j < emptyAreaList.Count; j = j + 2)
//                                {
//                                    string outa = (string)emptyAreaList[j];
//                                    string ina = (string)emptyAreaList[j + 1];
//                                    //判断点面是否相交
//                                    if (isInsect(conn, point, outa, ina))
//                                    {
//                                        if (zuo.Equals("")) zuo = "U";
//                                        else you = "U";
//                                    }
//                                }
//                                if (i % 2 == 1)
//                                {
//                                    if (zuo.Equals("") && you.Equals("")) result = result + "(2,N),";
//                                    else if (zuo.Equals("U") && you.Equals("U")) result = result + "(2,T,U),";
//                                    else if (zuo.Equals("B") && you.Equals("B")) result = result + "(2,T,B),";
//                                    else result = result + "(2,C),";
//                                    zuo = "";
//                                    you = "";
//                                }
//                            }
//                        }
                   #endregion

            }
            return result;
        }


        //求2个数据的差积的欧拉数
        public int getOSMAreaDifferenceEuler(OracleConnection myConnection, string tuple, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_DIFFERENCE(c_a.SHAPE,MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1), MDSYS.SDO_ORDINATE_ARRAY({0})),0.005)) from {1} c_a where c_a.objectid={2}",
                 tuple, table1, id1);

            //myConnection.Open();
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                Console.WriteLine(gml);
            }
            int i = -1;
            if (gml != null)
            {
                MatchCollection polygonNum = Regex.Matches(gml, "Polygon");
                Console.WriteLine("Polygon的个数为：{0}", polygonNum.Count);

                MatchCollection multiPolygonNum = Regex.Matches(gml, "MultiPolygon");
                Console.WriteLine("MultiPolygon的个数为：{0}", multiPolygonNum.Count);

                Match match0 = Regex.Match(gml, @"(<gml:innerBoundaryIs)((?!innerBoundaryIs).)*(gml:innerBoundaryIs>)");
                string inner = match0.Value;
                MatchCollection kongdong = Regex.Matches(inner, "LinearRing");
                Console.WriteLine("MultiPolygon的个数为：{0}", kongdong.Count);

                i = polygonNum.Count / 2 - multiPolygonNum.Count / 2 - kongdong.Count / 2;
            }
            return i;
        }

        //求2个数据的差积的欧拉数
        public int getOSCAreaDifferenceEuler(OracleConnection myConnection, string tuple, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SDO_GEOM.SDO_DIFFERENCE(c_a.SHAPE,MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1), MDSYS.SDO_ORDINATE_ARRAY({0})),0.005)) from {1} c_a where c_a.objectid={2}",
                 tuple, table1, id1);

            //myConnection.Open();
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                Console.WriteLine(gml);
            }
            int i = -1;
            if (gml != null)
            {
                MatchCollection polygonNum = Regex.Matches(gml, "Polygon");
                Console.WriteLine("Polygon的个数为：{0}", polygonNum.Count);

                MatchCollection multiPolygonNum = Regex.Matches(gml, "MultiPolygon");
                Console.WriteLine("MultiPolygon的个数为：{0}", multiPolygonNum.Count);

                Match match0 = Regex.Match(gml, @"(<gml:innerBoundaryIs)((?!innerBoundaryIs).)*(gml:innerBoundaryIs>)");
                string inner = match0.Value;
                MatchCollection kongdong = Regex.Matches(inner, "LinearRing");
                Console.WriteLine("MultiPolygon的个数为：{0}", kongdong.Count);

                i = polygonNum.Count / 2 - multiPolygonNum.Count / 2 - kongdong.Count / 2;
            }
            return i;
        }

        //获取面的点串
        public string getOSCArea(OracleConnection myConnection, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SHAPE) from {0}  where objectid={1}",
                 table1, id1);
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;
            string area = "";

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                Console.WriteLine(gml);
            }
            if (gml != null)
            {
                Match match0 = Regex.Match(gml, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                area = match1.Value;
            }
            return area;
        }

        //获取面的点串
        public string getOSMArea(OracleConnection myConnection, string table1, string id1)
        {
            string sql = String.Format("select sdo_util.TO_GMLGEOMETRY(SHAPE) from {0}  where objectid={1}",
                 table1, id1);
            Console.WriteLine(sql);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string gml = null;
            string area = "";

            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                gml = (string)command.ExecuteScalar();
                Console.WriteLine(gml);
            }
            if (gml != null)
            {
                Match match0 = Regex.Match(gml, @"(<gml:coordinates)((?!coordinates).)*(gml:coordinates>)");
                string linestring = match0.Value;
                Match match1 = Regex.Match(linestring, @"[-|0-9].+[0-9]");
                area = match1.Value;
            }
            return area;
        }


        //获取外部矩形
        public string getExterBox(string str1, string str2)
        {
            string box = "";
            string[] area1 = str1.Trim().Split(new char[2] { ' ', ',' });
            string[] area2 = str2.Trim().Split(new char[2] { ' ', ',' });
            double minX = double.Parse(area1[0]);
            double maxX = double.Parse(area1[0]);
            double minY = double.Parse(area1[1]);
            double maxY = double.Parse(area1[1]);
            for (int i = 0; i < area1.Length; i = i + 2)
            {
                double x = double.Parse(area1[i]);
                if (minX > x) minX = x;
                if (maxX < x) maxX = x;
            }
            for (int i = 1; i < area1.Length; i = i + 2)
            {
                double y = double.Parse(area1[i]);
                if (minY > y) minY = y;
                if (maxY < y) maxY = y;
            }

            for (int i = 0; i < area2.Length; i = i + 2)
            {
                double x = double.Parse(area2[i]);
                if (minX > x) minX = x;
                if (maxX < x) maxX = x;
            }
            for (int i = 1; i < area2.Length; i = i + 2)
            {
                double y = double.Parse(area2[i]);
                if (minY > y) minY = y;
                if (maxY < y) maxY = y;
            }
            double w = (maxX - minX) / 2;
            double c = (maxY - minY) / 2;
            minX = minX - w;
            maxX = maxX + w;
            minY = minY - c;
            maxY = maxY + c;
            box = box + minX + "," + maxY + ","
                      + maxX + "," + maxY + ","
                      + maxX + "," + minY + ","
                      + minX + "," + minY + ","
                      + minX + "," + maxY;
            return box;
        }


        //判断点面是否相交
        public bool isInsect(OracleConnection myConnection, string point, string outa)
        {
            string sql = String.Format(@"select sdo_geom.relate(
                                      MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1),MDSYS.SDO_ORDINATE_ARRAY({0})),
                                      'anyinteract',
                                      MDSYS.SDO_GEOMETRY(2001 , 4326 , MDSYS.SDO_POINT_TYPE ({1},NULL),  NULL ,  NULL), 0.005) from area where OBJECTID=1", outa, point);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string result = null;
            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                result = (string)command.ExecuteScalar();
                Console.WriteLine(result);
            }
            if (result == null || result.Equals("FALSE") || result.Equals("false")) return false;
            return true;
        }

        //判断点面是否相交
        public bool isInsect(OracleConnection myConnection, string point, string outa, string ina)
        {
            string sql = String.Format(@"select sdo_geom.relate(
                                      MDSYS.SDO_GEOMETRY(2003,4326,NULL,MDSYS.SDO_ELEM_INFO_ARRAY(1,1003,1,11,2003,1),MDSYS.SDO_ORDINATE_ARRAY({0},{1})),
                                      'anyinteract',
                                      MDSYS.SDO_GEOMETRY(2001 , 4326 , MDSYS.SDO_POINT_TYPE ({2},NULL),  NULL ,  NULL), 0.005) from area where OBJECTID=1", outa, ina, point);
            OracleCommand command = new OracleCommand(sql, myConnection);
            string result = null;
            if (command.ExecuteScalar() != System.DBNull.Value)
            {
                result = (string)command.ExecuteScalar();
                Console.WriteLine(result);
            }
            if (result == null || result.Equals("FALSE") || result.Equals("false")) return false;
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


        //查询规则库判断是否是错误的拓扑
        //false代表有错
        public Boolean isTrueTopo(OracleConnection conn, string featureA, string featureB, string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
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


        //将交面的拓扑关系存入表
        public void insertInsectArea(OracleConnection conn, string table1, string id1, string table2, string id2, string wkt, string type)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            string feature1 = getNationeleName(conn, table1, id1);
            string feature2 = getNationeleName(conn, table2, id2);
            OracleDBHelper db = new OracleDBHelper();
            //数据入库
            StringBuilder strSql = new StringBuilder();
            string sql1 = string.Format("insert into insectarea(objectid1,objectid2,feature1,feature2,shape,type,layer1,layer2) values");
            strSql.Append(sql1);
            wkt = string.Format("POLYGON(({0}))", wkt);
            //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
            string sql2 = string.Format("({0},{1},'{2}','{3}',sdo_geometry ('{4}',4326),'{5}','{6}','{7}')", id1, id2, feature1, feature2, wkt, type, table1, table2);
            strSql.Append(sql2);
            using (OracleCommand cmd = new OracleCommand(strSql.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }




    }
}
