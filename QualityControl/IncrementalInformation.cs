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

namespace QualityControl
{
    public partial class IncrementalInformation : Form
    {
        public IncrementalInformation()
        {
            InitializeComponent();
            dataTable.Columns.Add("类别");

            dataTable.Columns.Add("新建点个数");
            dataTable.Columns.Add("新建线个数");
            dataTable.Columns.Add("新建线长度");
            dataTable.Columns.Add("新建面个数");
            dataTable.Columns.Add("新建面面积");

            dataTable.Columns.Add("修改点个数");
            dataTable.Columns.Add("修改线个数");
            dataTable.Columns.Add("修改线长度");
            dataTable.Columns.Add("修改面个数");
            dataTable.Columns.Add("修改面面积");

            dataTable.Columns.Add("删除点个数");
            dataTable.Columns.Add("删除线个数");
            dataTable.Columns.Add("删除线长度");
            dataTable.Columns.Add("删除面个数");
            dataTable.Columns.Add("删除面面积");

            //dataTable.Columns.Add("居民地");
            //dataTable.Columns.Add("土质");
            //dataTable.Columns.Add("交通");
            //dataTable.Columns.Add("植被");
            //dataTable.Columns.Add("水系");

            calculate();
            dataGridView1.DataSource = dataTable;
        }

         OracleDBHelper helper = new OracleDBHelper();
         DataTable dataTable = new DataTable();

         public void calculate()
         {
             string[] elementTypeList = { "RESIDENTIAL", "SOIL", "TRAFFIC", "VEGETATION", "WATER" };
             foreach (string elementType in elementTypeList)
             {
                 DataRow dataTableRow = dataTable.NewRow();
                 dataTableRow["新建点个数"] = calculateCount(elementType + "_NEWPOINT", "create");
                 dataTableRow["新建线个数"] = calculateCount(elementType + "_NEWLINE", "create");
                 dataTableRow["新建线长度"] = calculateLength(elementType + "_NEWLINE", "create");
                 dataTableRow["新建面个数"] = calculateCount(elementType + "_NEWAREA", "create");
                 dataTableRow["新建面面积"] = calculateArea(elementType + "_NEWAREA", "create");

                 dataTableRow["修改点个数"] = calculateCount(elementType + "_NEWPOINT", "modify");
                 dataTableRow["修改线个数"] = calculateCount(elementType + "_NEWLINE", "modify");
                 dataTableRow["修改线长度"] = calculateLength(elementType + "_NEWLINE", "modify");
                 dataTableRow["修改面个数"] = calculateCount(elementType + "_NEWAREA", "modify");
                 dataTableRow["修改面面积"] = calculateArea(elementType + "_NEWAREA", "modify");

                 dataTableRow["删除点个数"] = calculateCount(elementType + "_NEWPOINT", "delete");
                 dataTableRow["删除线个数"] = calculateCount(elementType + "_NEWLINE", "delete");
                 dataTableRow["删除线长度"] = calculateLength(elementType + "_NEWLINE", "delete");
                 dataTableRow["删除面个数"] = calculateCount(elementType + "_NEWAREA", "delete");
                 dataTableRow["删除面面积"] = calculateArea(elementType + "_NEWAREA", "delete");
                 dataTableRow["类别"] = elementType;
                 dataTable.Rows.Add(dataTableRow);
             }
         }
       
        //统计个数
        public int calculateCount(string tablename, string changetype)
        {
            int count = 0;
            string sql = "select count(*) from user_tables where table_name = '" + tablename + "'";
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
                        sql = "select count(*)  from " + tablename + " where changetype ='" + changetype + "'";
                        using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sql))
                        {
                            while(dr1.Read())
                            {
                                count = dr1.GetInt32(0);
                                Console.WriteLine("表"+tablename+"中变化类型为" + changetype + "的总个数为：" + count);
                            }
                        }
                       
                    }
                   
                }
                
            }
            return count;
        }

        //统计长度
        public decimal calculateLength(string tablename, string changetype)
        {
            decimal length = 0;
             string sql = "select count(*) from user_tables where table_name = '" + tablename + "'";
             using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sql))
             {
                 while (dr.Read())
                 {
                     if (dr["count(*)"].ToString() == "0")
                     {
                         length = 0;
                     }
                     else
                     {
                         sql = "select sum(SDO_GEOM.SDO_LENGTH(shape,0.05)) as length  from " + tablename + " where changetype ='" + changetype + "'";
                         using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sql))
                         {
                             while (dr1.Read())
                             {
                                 if (dr1.IsDBNull(0))
                                 {
                                     length = 0;
                                 }
                                 else
                                 {
                                     length = Math.Round(dr1.GetDecimal(0), 2); 
                                 }
                                 Console.WriteLine("表" + tablename + "中变化类型为" + changetype + "的总长度为：" + length);
                             }
                         }

                     }
                 }
             }
            return length;
        }

        //统计面积
        public Double calculateArea(string tablename, string changetype)
        {
            Double area = 0;
             string sql = "select count(*) from user_tables where table_name = '" + tablename + "'";
             using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sql))
             {
                 while (dr.Read())
                 {
                     if (dr["count(*)"].ToString() == "0")
                     {
                         area = 0;
                     }
                     else
                     {
                         sql = "select sum(SDO_GEOM.SDO_AREA(shape,0.05)) as area  from " + tablename + " where changetype ='" + changetype + "'";
                         using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sql))
                         {
                             while (dr1.Read())
                             {
                                 if (dr1.IsDBNull(0))
                                 {
                                     area = 0;
                                 }
                                 else
                                 {
                                     Type t = dr1.GetFieldType(0);

                                     area = Math.Round(dr1.GetDouble(0), 2);//保留两位小数
                                     Console.WriteLine(sql);
                                     Console.WriteLine("表" + tablename + "中" + "变化类型为" + changetype + "的总面积为：" + area);
                                 }
                                 
                             }
                         }
                     
                     }
                 }
             }
            
            return area;
        }



    }
}
