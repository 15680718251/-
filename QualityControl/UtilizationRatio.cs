using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms.DataVisualization.Charting;

namespace QualityControl
{
    public partial class UtilizationRatio : Form
    {
        public UtilizationRatio()
        {
            InitializeComponent();
            trustDistribution("POLYLINESONVERSION");
            
            //Console.WriteLine(specRatio());
            //basicSource("aaa");
            //dataSources("RESIDENTIAL_NEWAREA", "RESIDENTIAL_AREA", out ratioTrust, out ratioSpec);
            specRatio();//计算专业数据利用率
        }
        public static double ratioTrust;
        public static double ratioSpec;
        public enum Basic
        {
            osm = 0,
            spec = 1
        }

        public enum Increment
        {
            osm = 0,
            spec = 1
        }
       
       // //判断基态数据来源
       //public Basic basicSource(string basicDataTable)
       //{
       //    Basic basic=Basic.spec;
       //    OracleDBHelper helper = new OracleDBHelper();
       //    //查寻基态表中所有字段，看是否包含osmid，包含则基态数据来源于osm数据，不包含则来自于专业数据
       //    string sql = string.Format("select   column_name   from   user_tab_columns   where   table_name=UPPER('{0}')", basicDataTable);
       //    using (OracleDataReader rd = helper.queryReader(sql))
       //    {
       //        while (rd.Read())
       //        {
       //            string aa = rd["column_name"].ToString();
       //            if (aa == "OSMID")
       //            {
       //                basic = Basic.osm;
       //                break;
       //            }
                   
       //        }
       //        return basic;
       //    }
       //}

        //判断增量数据来源
        public Increment incrementSource(string incrementDataTable)
        {
            Increment increment = Increment.spec;
            OracleDBHelper helper = new OracleDBHelper();
            //查寻增量表中所有字段，看是否包含osmid，包含则增量数据来源于osm数据，不包含则来自于专业数据
            string sql = string.Format("select   column_name   from   user_tab_columns   where   table_name=UPPER('{0}')", incrementDataTable);
            using (OracleDataReader rd = helper.queryReader(sql))
            {
                while (rd.Read())
                {
                    string aa = rd["column_name"].ToString();
                    if (aa == "OSMID")
                    {
                        increment = Increment.osm;
                        break;
                    }
                }
                return increment;
            }
        }

       //public void dataSources( out double ratioTrust, out double ratioSpec)//此处应该引入更新数据表，但更新表中的数据存储情况未知
       // {
       //      ratioTrust = 0.0;
       //      ratioSpec = 0.0;
       //     //Basic bas= basicSource(basicDataTable);
       //     //Increment inc = incrementSource(updateDataTable);
       //     //if (bas == Basic.osm && inc == Increment.osm)//统计更新数据中osm数据的可信度分布情况
       //     //{
       //     //    //ratioTrust = trustDistribution("polygons");
       //     //    textBox1.Text = specRatio(basicDataTable, updateDataTable).ToString();
       //     //}
       //     //else if (bas == Basic.osm && inc == Increment.spec)//统计更新数据中osm数据的可信度分布情况和专业矢量数据的使用率
       //     //{
       //     //    //specRatio();
       //     //    //ratioTrust = trustDistribution("polygons");
       //     //    textBox1.Text = specRatio(basicDataTable, updateDataTable).ToString();
       //     //}
       //     //else if  (bas == Basic.spec && inc == Increment.osm)//统计更新数据中osm数据的可信度分布情况和专业矢量数据的使用率
       //     //{
       //     //    //specRatio();
       //     //    //ratioTrust = trustDistribution("polygons");
       //     //    textBox1.Text = specRatio(basicDataTable, updateDataTable).ToString();
       //     //}
       //     //else (bas == Basic.spec && inc == Increment.spec)//
       //     //{

       //     //}

       //     textBox1.Text = specRatio().ToString();
       //     //return Ratio;
       // }

        //计算专业数据的利用率
       public double specRatio()
       {
           double speccount=0;
           double updatecount=0;
           double specratio = 0;
           string[] elementType = { "RESIDENTIAL", "SOIL", "TRAFFIC", "VEGETATION", "WATER" };
           foreach (string ELE in elementType)
           {
               string[] shapeType = { "AREA", "LINE", "POINT" };
               foreach (string SHP in shapeType)
               {
                   speccount += specCount(ELE +"_"+ SHP);
                   updatecount += updateCount(ELE + "_" + SHP);
               }
                  
           }
           specratio= speccount / updatecount;
           Console.WriteLine("专业数据利用率为：" + specratio);
           textBox1.Text = specratio.ToString();
           return specratio;

        }

       

        //计算更新数据表中有多少条专业矢量数据
        public double specCount(string updateTableName)
        {
            double count = 0;
            string sql = "select count(*) from user_tables where table_name = '" + updateTableName + "'";
             using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sql))
             {
                 while (dr.Read())
                 {
                     if (dr["count(*)"].ToString() == "0")
                     {
                         count = 0;
                     }
                     else
                     {
                         OracleDBHelper helper = new OracleDBHelper();
                         sql = string.Format("select count(*) from {0} where osmid is  null", updateTableName);
                         using (OracleDataReader rd = helper.queryReader(sql))
                         {
                             while (rd.Read())
                             {
                                 count = Convert.ToDouble(rd["count(*)"]);
                             }
                         }
                     }
                 }
             }
             return count;
        }

        //统计更新数据表中的目标总数量
        public double updateCount(string updateTableName)
        {
            double count = 0;
            string sql = "select count(*) from user_tables where table_name = '" + updateTableName + "'";
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sql))
            {
                while (dr.Read())
                {
                    if (dr["count(*)"].ToString() == "0")//如果没有表名为updateTableName
                    {
                        count = 0;
                    }
                    else
                    {
                        OracleDBHelper helper = new OracleDBHelper();
                        sql = string.Format("select count(*) from {0}", updateTableName);
                        using (OracleDataReader rd = helper.queryReader(sql))
                        {
                            while (rd.Read())
                            {
                                count = Convert.ToDouble(rd["count(*)"]);
                            }
                        }
                    }
                }
            }
           
            return count;
        }

        //可信度分布情况
        public double trustDistribution(string tableName)
        {
            double Ratio;
            DataTable trustDistributionTable = new DataTable();
            double[] countlist = null;
            trustDistributionTable.Columns.Add("trustvalue");
            //DataRow dataTableRow;
            OracleDBHelper helper = new OracleDBHelper();

            string sql = string.Format("select trustvalue from {0}", tableName);
            using (OracleDataReader rd = helper.queryReader(sql))
            {
                while (rd.Read())
                {
                    DataRow dataTableRow = trustDistributionTable.NewRow();
                    dataTableRow["trustvalue"] = rd["trustvalue"];
                    //dataTableRow["trustvalue"] = rd["trustvalue"].ToString() ;
                    trustDistributionTable.Rows.Add(dataTableRow);

                }
            }

            int a = 0, b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, h = 0, j = 0, k = 0;

            for (int i = 0; i < trustDistributionTable.Rows.Count; i++)
            {
                //int a = 0;
                double dou = Convert.ToDouble(trustDistributionTable.Rows[i]["trustvalue"]);
                if (dou > 0.0 && dou < 0.1)
                {
                    a++;
                }
                //int b = 0;
                if (dou > 0.1 && dou < 0.2)
                {
                    b++;
                }
                //int c = 0;
                if (dou > 0.2 && dou < 0.3)
                {
                    c++;
                }
                //int d = 0;
                if (dou > 0.3 && dou < 0.4)
                {
                    d++;
                }
                //int e = 0;
                if (dou > 0.4 && dou < 0.5)
                {
                    e++;
                }
                //int f = 0;
                if (dou > 0.5 && dou < 0.6)
                {
                    f++;
                }
                //int g = 0;
                if (dou > 0.6 && dou < 0.7)
                {
                    g++;
                }
                //int h = 0;
                if (dou > 0.7 && dou < 0.8)
                {
                    h++;
                }
                //int j = 0;
                if (dou > 0.8 && dou < 0.9)
                {
                    j++;
                }
                //int k = 0;
                if (dou > 0.9 && dou < 1)
                {
                    k++;
                }
                countlist = new double[10] { a, b, c, d, e, f, g, h, j, k };

            }

            double sum = 0;
            double[] trustRatio = new double[10];
            for (int i = 0; i < countlist.Count(); i++)
            {
                sum += countlist[i];
            }
            for (int i = 0; i < countlist.Count(); i++)
            {
                trustRatio[i] = countlist[i] / sum;
            }
            string[] xValue = new string[10] { "0-0.1", "0.1-0.2", "0.2-0.3", "0.3-0.4", "0.4-0.5", "0.5-0.6", "0.6-0.7", "0.7-0.8", "0.8-0.9", "0.9-1.0" };
            //清除默认的series


            try
            {
                chart2.Series.Clear();
                Series Ratios = new Series("Ratio");
                Ratios.ChartType = SeriesChartType.Pie;
                for (int i = 0; i < countlist.Count(); i++)
                {
                    Ratios.Points.AddXY(xValue[i], trustRatio[i]);
                }
                //Ratio.ChartArea = "ChartArea2";
                chart2.Series.Add(Ratios);
            }

            catch (Exception ex)
            {

            }

            try
            {
                chart1.Series.Clear();
                Series Count = new Series("Count");
                Count.BorderWidth = 3;
                Count.ShadowOffset = 2;
                Count.ChartType = SeriesChartType.Column;
                Count.IsValueShownAsLabel = true;
                for (int i = 0; i < countlist.Count(); i++)
                {
                    Count.Points.AddXY(xValue[i], countlist[i]);
                }
                //Count.ChartArea = "ChartArea1";
                chart1.Series.Add(Count);


                chart1.ChartAreas[0].AxisX.Title = "区间";
                chart1.ChartAreas[0].AxisY.Title = "数量";

                //chart1.ChartAreas[1].AxisX.Title = "区间";
                //chart1.ChartAreas[1].AxisY.Title = "比率";

                chart1.ChartAreas[0].AxisX.Interval = 1;//设置X轴间距，这样的话，就间距固定为1 
                //chart1.ChartAreas[1].AxisX.Interval = 1;
            }

            catch (Exception ex)
            {

            }

            return Ratio = g + h + j + k;
        }
        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
       
    }
}
