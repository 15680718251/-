using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;


namespace TrustValueAndReputation
{
    #region 判断是否是有效实例(已注释)
    public class Dal_IntersectArea
    {
        /// <summary>
        /// 判断是否是有效实例
        /// </summary>
        /// <returns></returns>
        public string IsValid(long objectid)
        {
            string isValid = "false";
            StringBuilder strSql = new StringBuilder();
            //strSql.Append("DECLARE @g geometry;");

            strSql.Append("select SDO_GEOM.VALIDATE_GEOMETRY(shape,0.05) isValid from polygonsonversion where objectid =" + objectid);
            //strSql.Append(" SELECT isvalid (the_geom) FROM (select geom from polygonsonversion where objectid =" + objectid + ")As foo(the_geom);");
            //strSql.Append(" SELECT @g.STIsValid() isValid;");
            //strSql.Append(" SELECT geom.STArea()*1000000 IntersectArea from polygonsonversion;");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    isValid = dr["isValid"].ToString();

                }

            }
            return isValid;
        }
       #endregion

        /// <summary>
        /// 判断是否是有效实例
        /// </summary>
        /// <returns></returns>
        public bool IsValidtest(string p)
        {
            bool isValid = false;
            StringBuilder strSql = new StringBuilder();
            //strSql.Append("DECLARE @g geometry;");
            strSql.Append(" SELECT isvalid ('polygon(("+p+"))');");
            //strSql.Append(" SELECT @g.STIsValid() isValid;");
            //strSql.Append(" SELECT geom.STArea()*1000000 IntersectArea from polygonsonversion;");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    isValid = Convert.ToBoolean(dr["isValid"]);
                }
            }
            return isValid;
        }
        /// <summary>
        /// 获取面积
        /// </summary>
        public InterstArea GetArea1(string p)
        {
            Dal_polygonsonversion dal = new Dal_polygonsonversion();
            StringBuilder strSql = new StringBuilder();
            //strSql.Append("DECLARE @g geometry;");
            strSql.Append(" select st_area  ('polygon(("+p+"))') as area;");
            //strSql.Append(" SELECT convert(decimal(18,4),@g.STArea()*1000000) area;");
            //strSql.Append(" SELECT geom.STArea()*1000000 IntersectArea from polygonsonversion;");
            InterstArea model = new InterstArea();
            //    using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            //    {

            //        InterstArea model = GetModel(dr);
            //        return model;
            //    }
            //}

            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                if (IsValidtest(p))
                {
                    while (dr.Read())
                    {
                        if (dr["area"] != DBNull.Value)
                        {
                            model.IntersectArea = Convert.ToDouble(dr["area"]);
                        }
                        else
                        {
                            model.IntersectArea = 0;
                        }
                        //model.IntersectArea = Convert.ToDouble(dr["area"]);

                    }
                }
                else
                {
                    //string areadiffsim = "0";
                    //model.IntersectArea = -1;
                    // string isArea = "无效";
                     model.isArea = -1;
                    //dal.UpdateIsValid(objectid, isArea);
                    //FindSonWayVersion.FindSonVersion sub = new FindSonWayVersion.FindSonVersion();
                    //sub.isValid = false;
                }
                return model;
            }
        }
      
     

        /// <summary>
        /// 获取面积
        /// </summary>
        public InterstArea GetArea(long objectid)
        {
            Dal_polygonsonversion dal = new Dal_polygonsonversion();
            StringBuilder strSql = new StringBuilder();
            //strSql.Append("DECLARE @g geometry;");
            strSql.Append(" select sdo_geom.sdo_area(shape,0.05) as area from polygonsonversion where objectid =" + objectid );
            //strSql.Append(" SELECT convert(decimal(18,4),@g.STArea()*1000000) area;");
            //strSql.Append(" SELECT geom.STArea()*1000000 IntersectArea from polygonsonversion;");
            InterstArea model = new InterstArea();
        //    using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
        //    {

        //        InterstArea model = GetModel(dr);
        //        return model;
        //    }
        //}
           
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                //if (IsValid(objectid)=="TRUE")
                //{//董海峰判断是否为有效实例注释部分
                    while (dr.Read())
                    {
                        if (dr["area"] != DBNull.Value)
                        {
                            model.IntersectArea = Convert.ToDouble(dr["area"]);
                        }
                        else
                        {
                            model.IntersectArea = 0;
                        }
                        
                    }
                //}
                //else
                //{
                //    //string areadiffsim = "0";
                //    string isArea = "无效";
                //    dal.UpdateIsValid(objectid, isArea);
                //    TrustValueAndReputation.FindSonVersion sub = new TrustValueAndReputation.FindSonVersion();
                //    sub.isValid = false;
                //}
                return model;
            }
        }
      
     
        
        /// <summary>
        /// 获取相交面积
        /// </summary>
        //public InterstArea GetIntersectArea(long nextid, long currentid)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("DECLARE @g geometry;");
        //    strSql.Append(" DECLARE @h geometry;");
        //    strSql.Append(" SET @g = (select geom from polygonsonversion where objectid =" + nextid + ");");
        //    strSql.Append(" SET @h = (select geom from polygonsonversion where objectid =" + currentid + ");");
        //    strSql.Append(" SELECT convert(decimal(18,4),@g.STIntersection(@h).STArea()*1000000)IntersectArea;");
        //    using (OracleDataReader dr = DbHelperSQL.ExecuteReader(strSql.ToString()))
        //    {
        //        InterstArea model = GetModel(dr);
        //        return model;
        //    }
        //}
        public InterstArea GetIntersectArea(long nextid, long currentid)
        {
            //polygonsonversion currentWay = new polygonsonversion();
            //Dal_polygonsonversion dal = new Dal_polygonsonversion();
            //currentWay = dal.GetModel(currentid);
            //string p = currentWay.points;
            //polygonsonversion nextWay = new polygonsonversion();
            //nextWay = dal.GetModel(nextid);
            //string b = nextWay.points;
            //StringBuilder strSql = new StringBuilder();
            string strSql = String.Format("select SDO_GEOM.SDO_AREA (SDO_GEOM.SDO_INTERSECTION((select shape from polygonsonversion where objectid={0}),(select shape from polygonsonversion where objectid={1}),0.05),0.05) area from daul", nextid, currentid);
            //strSql.Append("select SDO_GEOM.SDO_AREA (SDO_GEOM.SDO_INTERSECTION((select shape from polygonsonversion where osmid={0}),(select shape from polygonsonversion where osmid={1}),0.05),0.05) from daul",nextid,currentid);
            InterstArea model = new InterstArea();
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    if (dr["area"] != DBNull.Value)
                    {
                        model.IntersectArea = Convert.ToDouble(dr["area"]);
                    }
                    else
                    {
                        model.IntersectArea = 0;
                    }
                }
                return model;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextid"></param>
        /// <param name="currentid"></param>
        /// <returns></returns>
        public InterstArea GettestArea(string p, string b)
        {

            Dal_polygonsonversion dal = new Dal_polygonsonversion();
            StringBuilder strSql = new StringBuilder();
            InterstArea model = new InterstArea();
            strSql.Append("select area( ST_Intersection('POLYGON((" + p + "))','POLYGON((" + b + "))'))");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    if (dr["area"] != DBNull.Value)
                    {
                        model.IntersectArea = Convert.ToDouble(dr["area"]);
                    }
                    else
                    {
                        model.IntersectArea = 0;
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// 判断线是否是有效
        /// </summary>
        /// <param name="objectid"></param>
        /// <returns></returns>
        public string IsValidline(long objectid)
        {
            string isValid = "false";
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select SDO_GEOM.VALIDATE_GEOMETRY(shape,0.05) isValid from polylinesonversion where objectid =" + objectid);
            //strSql.Append(" SELECT isvalid (the_geom) FROM (select geomline from polylinesonversion where objectid =" + objectid + ")As foo(the_geom);");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    //isValid = Convert.ToBoolean(dr["isValid"]);
                    isValid = dr["isValid"].ToString();
                }
            }
            return isValid;
        }
        /// <summary>
        /// 获取长度
        /// </summary>
        /// <param name="objectid"></param>
        /// <returns></returns>
        public InterstArea Getlength (long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select sdo_geom.sdo_length(shape,0.05) area from polylinesonversion where objectid =" + objectid);
            InterstArea model = new InterstArea();
            OracleDBHelper helper=new OracleDBHelper();

            //using (OracleConnection con = helper.getOracleConnection())
            //{
                using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
                {
                    try
                    {

                        while (dr.Read())
                        {
                            if (dr["area"] != DBNull.Value)
                            {
                                model.IntersectArea = Convert.ToDouble(dr["area"]);
                            }
                            else
                            {
                                model.IntersectArea = 0;
                            }
                        }
                        //dr.Close();

                        return model;

                    }

                    catch (System.Exception ex)
                {
                    Console.WriteLine("获取长度 获取长度 获取长度 objectid=" + objectid + "时候出错。。。。。。。。。。。。。。。。。。。。。。。。。。。长度为：" + dr["area"]);
                    Console.WriteLine(ex);
                    return model;
                    
                }
                //}
            }
        }
        
        /// <summary>
        /// 获取相交长度
        /// </summary>
        /// <param name="nextid"></param>
        /// <param name="currentid"></param>
        /// <returns></returns>
        public InterstArea GetIntersectLength(long nextid, long currentid)
        {
           
            string strSql = String.Format("select SDO_GEOM.SDO_LENGTH( SDO_GEOM.SDO_INTERSECTION((select shape from polylinesonversion where objectid={0}),(select shape from polylinesonversion where objectid={1}),0.05),0.05) length from daul", currentid, nextid);
           
            InterstArea model = new InterstArea();
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    if (dr["length"] != DBNull.Value)
                    {
                        model.IntersectArea = Convert.ToDouble(dr["length"]);
                    }
                    else
                    {
                        model.IntersectArea = 0;
                    }

                }
                return model;
            }
        }
        /// <summary>
        /// 获取缓冲区面积
        /// </summary>
        /// <param name="objectid"></param>
        /// <returns></returns>
        public InterstArea GetBuffer(long objectid)
        {
           
            StringBuilder strSql = new StringBuilder();
            
            strSql.Append("select sdo_geom.sdo_area(sdo_geom.sdo_buffer(shape,10,0.05),0.05) area from polylinesonversion where objectid =" + objectid);
            InterstArea model = new InterstArea();
       
            OracleDBHelper helper = new OracleDBHelper();

            //using (OracleConnection con = helper.getOracleConnection())
            //{
                using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
                {
                    try
                    {

                        while (dr.Read())
                        {
                            if (dr["area"] != DBNull.Value)
                            {
                                model.IntersectArea = Convert.ToDouble(dr["area"]);
                            }
                            else
                            {
                                model.IntersectArea = 0;
                            }
                        }
                        //dr.Close();

                        return model;

                    }

                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                        return model;

                    }
                //}
            }
        }
        /// <summary>
        /// 获取缓冲区相交面积
        /// </summary>
        /// <param name="nextid"></param>
        /// <param name="currentid"></param>
        /// <returns></returns>
        public InterstArea GetIntersectBuffer(long nextid, long currentid)
        {
            string strSql = String.Format("select SDO_GEOM.SDO_area(SDO_GEOM.SDO_INTERSECTION(sdo_geom.sdo_buffer((select shape from polylinesonversion where objectid={0}),10,0.05),sdo_geom.sdo_buffer((select shape from polylinesonversion where objectid={1}),10,0.05),0.05),0.05) area from daul",nextid,currentid);
            InterstArea model = new InterstArea();
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    if (dr["area"] != DBNull.Value)
                    {
                        model.IntersectArea = Convert.ToDouble(dr["area"]);
                    }
                    else
                    {
                        model.IntersectArea = 0;
                    }
                  
                    
                }
                return model;
            }
        }


        #region 获取相交面积（注释部分）

        //    using (OracleDataReader dr = DbHelperSQL.ExecuteReader(strSql.ToString()))
        //    {
        //        if (IsValid())
        //        {
        //            while (dr.Read())
        //            {
        //                model.IntersectArea = Convert.ToDouble(dr["area"]);
        //            }
        //        }
        //        else
        //        {
        //            string isArea = "无效";
        //            dal.UpdateIsValid(objectid, isArea);
        //            FindSonWayVersion.FindSonVersion sub = new FindSonWayVersion.FindSonVersion();
        //            sub.isValid = false;
        //        }
        //        return model;
        //    }
        //}
        //    using (OracleDataReader dr = DbHelperSQL.ExecuteReader(strSql.ToString()))
        //    {
        //        model.IntersectArea = Convert.ToDouble(dr["IntersectArea"]);
        //        return model;
        //    }
        //}
        #endregion 获取相交面积（注释部分）

        #region 判断有效实例（已注释）

        ///// <summary>
        ///// 判断是否是有效实例
        ///// </summary>
        ///// <returns></returns>
        //public bool IsValid(int objectid)
        //{
        //    bool isValid = false;
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("DECLARE @g geometry;");
        //    strSql.Append(" SET @g = (select [geom] from [laowo].[dbo].[polygonsonversion] where [objectid] =" + objectid + ");");
        //    strSql.Append(" SELECT @g.STIsValid();");
        //    //strSql.Append(" SELECT geom.STArea()*1000000 IntersectArea from polygonsonversion;");
        //    using (SqlCommand cmd = new SqlCommand(DbHelperSQL.connectionString))
        //    {
        //        using (SqlDataReader sr = cmd.ExecuteReader())
        //        {
        //            if (sr.HasRows)
        //            {
        //                while (sr.Read())
        //                {
        //                    isValid =Convert.ToBoolean( sr.GetValue(0));
        //                }
        //            }
        //        }

        //    }
        //    return isValid;
        //}
        #endregion 判断有效实例（已注释）

        #region  计算形状相似度（已注释）

        //        /// <summary>
//        /// 计算起始点与X轴正方向的逆时针方向的角度
//        /// </summary>
//        /// <param name="slat"></param>
//        /// <param name="slon"></param>
//        /// <param name="elat"></param>
//        /// <param name="elon"></param>
//        /// <returns></returns>
//        public static double turnAngle(double slat, double slon, double elat, double elon)
//        {
//            //求从起点slat，slon到终点elat，elon的向量与X轴正方向的旋转角
//            double vectorx = 0, vectory = 0, angle = 0;
//            vectorx = elon - slon;//纬度差
//            vectory = elat - slat;//经度差
//            if (vectorx < 1e-6 && vectory < 1e-6) return 0;
//            angle = Math.Acos((vectorx * 1 + vectory * 0)
//                    / (Math.Sqrt(vectorx * vectorx + vectory * vectory) * Math
//                            .Sqrt(1 * 1 + 0 * 0)));
//            if (vectory < 0)
//                angle = 2 * Math.PI - angle;
//            return angle;
//        }
//        /// <summary>
//        /// 计算旋转函数的相似度
//        /// </summary>
//        /// <param name="p"></param>
//        /// <param name="turnAngleDist"></param>
//        /// <param name="turn180"></param>
//        public static void turnFunction(String p, double[][] turnAngleDist, bool turn180)
//        {
//        //* @function:////旋转函数的相似度  = 旋转函数距离/（最大角度-最小角度）
//        //计算polyline中每一线段的角度和距离（首先求出线段长度，然后更新成到起点的长度），并将距离归一化（除以总长）。
//        //存在二维数组中,第一维是线段数，第二维共两个数，分别旋转角度【0】和长度【1】.turn180表示是否将线的点数据倒序（eg. id4041237，v4v5）,默认为false，不翻转
//        // * @parm ：
//        // */
//        double[][] points = new double[p.Split(';').Length][];//将点串以分号（；）分开存为二维数组，一个点存为一个数组。
//        int pointNum = points2Array(p, points);
//        double x,y;
//        if(turn180 == true)
//        {
//            for(int i=0;i<pointNum/2;i++)
//            {
//                y = points[i][0];//纬度？
//                x = points[i][1];//经度？
//                points[i][0] = points[pointNum-1-i][0];
//                points[i][1] = points[pointNum-1-i][1];				
//                points[pointNum-1-i][0] = y;
//                points[pointNum-1-i][1] = x;
//            }
//        }
//        for(int i=0;i<pointNum-1;i++)
//        {   //ProcessPolylineData.GetDistance
//            turnAngleDist[i][0] = turnAngle(points[i][0],points[i][1],points[i+1][0],points[i+1][1]);//求出旋转角
//            //System.out.println(turnAngleDist[i][0]);
//            turnAngleDist[i][1] = GetDistance(points[i][0],points[i][1],points[i+1][0],points[i+1][1]);//求出两个点之间的线段长度?
//            //update 首先求出线段长度，然后更新成到起点的长度
//            if(i>0)
//            {
//                turnAngleDist[i][1] = turnAngleDist[i][1]+turnAngleDist[i-1][1];//算出到起点的总长度。
//            }
//        }
		
//        for(int i=0;i<pointNum-1;i++)
//        {   //把多边形展开成直线计算长度。因为i<pointNum-1，所以imax=pointNum - 2，得到的是归一化之后的边长。
//            turnAngleDist[i][1] = turnAngleDist[i][1] / turnAngleDist[pointNum - 2][1];
//        }
//    }
//        /// <summary>
//        /// 计算面积相似度
//        /// </summary>
//        /// <param name="p1"></param>
//        /// <param name="p2"></param>
//        /// <returns></returns>
//        public static double turnFunctionSim(String p1, String p2)
//        {
//        p1 = seqSameSimp(p1);
//        p2 = seqSameSimp(p2);
//        int pointsNum1 = strPointsNum(p1);
//        double[][] turn1 = new double[pointsNum1][];
//        double minAngle1,maxAngle1,minAngle2,maxAngle2;
//        minAngle1=maxAngle1=minAngle2=maxAngle2=0;
		
//        turnFunction(p1,turn1,false);//默认turn180为fasle，不翻转。
		
//        int pointsNum2 = p2.Split(';').Length;//strPointsNum(p2);
//        double[][] turn2 = new double[pointsNum2][];
//        turnFunction(p2,turn2,false);
//        int lineNum = pointsNum1-1+pointsNum2-1;
//        //System.out.println("pointsNum1:"+pointsNum1+"  pointsNum2:"+pointsNum2+"  lineNum:"+lineNum);
//        //分成分段三个数组，一个数组保存间隔点（m+n=lineNum,关键点数量应该再减1，因为最后一个都为1），第二个数组保存p1的间断时候的角度值，第三个保存p2的间断时候的角度值		
//        double []criticalPoint = new double[lineNum];
//        double []angle1 = new double[lineNum];
//        double []angle2 = new double[lineNum];
//        angle1[0] = turn1[0][0];
//        angle2[0] = turn2[0][0];

//        int i=0,j=0,k=0;
//        for(i=0;i<lineNum-1;i++)
//        {
//            //System.out.println(turn1[j][1]+"  K:"+turn2[k][1]);
//            if(turn1[j][1]<turn2[k][1])
//            {
//                criticalPoint[i] = turn1[j][1];
//                angle1[i] = turn1[j][0];
//                angle2[i] = turn2[k][0];
//                //System.out.println("j="+j+": "+criticalPoint[i]);
//                j++;
		
//            }
//            else
//            {
//                criticalPoint[i] = turn2[k][1];
//                angle1[i] = turn1[j][0];
//                angle2[i] = turn2[k][0];
//                //System.out.println("K="+k+": "+criticalPoint[i]);
//                k++;
				
//            }
////			if(1-turn2[k][1]<1e-6 && 1-turn1[j][1]<1e-6){
////				break;
////			}
//        }
		
//        double turnValue1=0;
//        turnValue1 = criticalPoint[0]*Math.Abs(angle1[0]-angle2[0]);
//        //System.out.println("@@ 0"+" : "+criticalPoint[0]+"  "+angle1[0]+"  "+angle2[0]);
//        for(i=1;i<lineNum-1;i++)
//        {
//            //System.out.println("@@ "+i+" : "+criticalPoint[i]+"  "+angle1[i]+"  "+angle2[i]);
//            //i=0的时候没有i-1，所以分两种情况讨论
//            turnValue1 = turnValue1+Math.Abs(criticalPoint[i]-criticalPoint[i-1])*Math.Abs(angle1[i]-angle2[i]);
//        }
//        //求最大最小的angle
		
//        for(i=0;i<pointsNum1-1;i++)
//        {
//            if (minAngle1 > turn1[i][0]) minAngle1 = turn1[i][0];//最大最小值都是从0开始的。
//            if(maxAngle1<turn1[i][0]) maxAngle1 = turn1[i][0];
//        }
//        for(i=0;i<pointsNum2-1;i++)
//        {
//            if(minAngle1>turn2[i][0]) minAngle1 = turn2[i][0];
//            if(maxAngle1<turn2[i][0]) maxAngle1 = turn2[i][0];
//        }
		
		
//        //System.out.print(maxAngle1+" - "+minAngle1);
//        /** 将第二个子串旋转180度***/
//        //第二个str倒序求dist
//        //double[][] turn2 = new double[pointsNum2][2];
//        turnFunction(p2,turn2,true);		
		
//        //分成分段三个数组，一个数组保存间隔点（m+n=lineNum），第二个数组保存p1的间断时候的角度值，第三个保存p2的间断时候的角度值
//        angle1[0] = turn1[0][0];
//        angle2[0] = turn2[0][0];
		
//        i=j=k=0;
//        for(i=0;i<lineNum-1;i++){
//            if(turn1[j][1]<turn2[k][1])
//            {
//                criticalPoint[i] = turn1[j][1];
//                angle1[i] = turn1[j][0];
//                angle2[i] = turn2[k][0];
//                j++;				
//            }else
//            {
//                criticalPoint[i] = turn2[k][1];
//                angle1[i] = turn1[j][0];
//                angle2[i] = turn2[k][0];
//                k++;
//            }
////			if(1-turn2[k][1]<1e-6 && 1-turn1[j][1]<1e-6){
////				break;
////			}
//        }
			
//        double turnValue2=0;
//        turnValue2 = criticalPoint[0]*Math.Abs(angle1[0]-angle2[0]);
//        for(i=1;i<lineNum-1;i++)
//        {
//            turnValue2 = turnValue2+Math.Abs(criticalPoint[i]-criticalPoint[i-1])*Math.Abs(angle1[i]-angle2[i]);
//        }
		
//        for(i=0;i<pointsNum1-1;i++)
//        {
//            if(minAngle2>turn1[i][0]) minAngle2 = turn1[i][0];
//            if(maxAngle2<turn1[i][0]) maxAngle2 = turn1[i][0];
//        }
//        for(i=0;i<pointsNum2-1;i++)
//        {
//            if(minAngle2>turn2[i][0]) minAngle2 = turn2[i][0];
//            if(maxAngle2<turn2[i][0]) maxAngle2 = turn2[i][0];
//        }
		
//        double sim1=0,sim2=0,similarity=0;
//        sim1 = 1-turnValue1/Math.Abs(maxAngle1-minAngle1);   //(2*Math.PI)
//        sim2 = 1-turnValue2/Math.Abs(maxAngle2-minAngle2); //
//        //System.out.println(turnValue1+ "  "+ turnValue2+"  "+(maxAngle1-minAngle1));
//        similarity = sim1>=sim2 ? sim1 : sim2;
		
//        if(similarity>1) 
//            similarity = 1;
		
//        return similarity;
//        //return turnValue1<turnValue2?turnValue1:turnValue2;
//    }
//        public static int strPointsNum(String str)
//        {
//            return str.Split(';').Length;
//        }
//        public static int points2Array(String str, double[][] points)
//        {
//            // 将点串转化成数组points经度 纬度二维数组，纬度在前
//            String[] p = str.Split(';');
//            int n = p.Length;
//            // points = new double[n][2];
//            int count = 0;
//            for (int i = 0; i < n; i++)
//            {
//                String[] latlon = p[i].Split(',');
//                points[i][0] = Double.Parse(latlon[0]);//i表示点，0和1分别表示经纬度？
//                points[i][1] = Double.Parse(latlon[1]);
//            }
//            return n;
//        }
//        public static String seqSameSimp(String str)
//        {
//            //删除序列中连续相同的点
//            String points = "";
//            String[] p = str.Split(';');
//            points = p[0];
//            for (int i = 1; i < p.Length; i++)
//            {
//                if (!p[i].Equals(p[i - 1]))
//                    points = points + ";" + p[i];
//            }
//            return points;
            
//        }
//        public static double rad(double d) 
//        { 
//          return d * Math.PI / 180.0; 
//        }
//        public static double GetDistance(double lat1, double lng1, double lat2, double lng2) 
//        { 
//        ///**
//        // * @author ZhaoYJ
//        // * @date : 2013-10-10 下午9:44:25
//        // * @function:根据经纬度计算两点距离（单位是米）
//        // * @parm ：
//        // */
//        double EARTH_RADIUS = 6378.137*1000*100;  //现转化成cm级别，避免过小距离，四舍五入后变成0
//        double radLat1 = rad(lat1); 
//        double radLat2 = rad(lat2); 
//        double a = radLat1 - radLat2; 
//        double b = rad(lng1) - rad(lng2); 
//        double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a/2),2) + Math.Cos(radLat1)*Math.Cos(radLat2)*Math.Pow(Math.Sin(b/2),2))); 
		           
//        s = s * EARTH_RADIUS; 
//        s = Math.Round(s * 10000) / 10000; 
//        return s/100;
        //        } 
       #endregion 计算形状相似度（已注释）

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private InterstArea GetModel(OracleDataReader dr)
        {
            InterstArea model = new InterstArea();
            model.IntersectArea = Convert.ToDouble(dr[0]);

            return model;
        }


        /// <summary>
        /// 由OracleDataReader得到泛型数据列表
        /// </summary>
        private List<InterstArea> GetList(OracleDataReader dr)
        {
            List<InterstArea> lst = new List<InterstArea>();
            while (dr.Read())
            {
                lst.Add(GetModel(dr));
            }
            return lst;
        }

        #endregion
    }
}

