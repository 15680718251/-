using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;

namespace QualityControl
{
    public partial class AttributeAccuracy : Form
    {
        public AttributeAccuracy()
        {
            InitializeComponent();
        }

        public static double ratio;
        private void AttributeAccuracy1_Load(object sender, EventArgs e)
        {
            List<string> basicTables =null;
                //= getTableNames(conStr);//获取基态数据库表名
            this.comboBox1.Items.Clear();
            if (basicTables != null && basicTables.Count > 0)
            {
                foreach (string name in basicTables)
                {
                    this.comboBox1.Items.Add(name);
                }
            }
        }
       
        public double calculateRatio()
        {
            //double ratio;
            int erroeCount = calculateCount("RESIDENTIAL_AREA", Constraint()) + calculateCount("RESIDENTIAL_LINE", Constraint()) + calculateCount("RESIDENTIAL_POINT", Constraint()) + 
                             calculateCount("SOIL_AREA", Constraint()) + calculateCount("SOIL_LINE", Constraint()) + calculateCount("SOIL_POINT", Constraint()) + 
                             calculateCount("TRAFFIC_AREA", Constraint())+ calculateCount("TRAFFIC_LINE", Constraint()) + calculateCount("TRAFFIC_POINT", Constraint()) +
                             calculateCount("VEGETATION_AREA", Constraint())+ calculateCount("VEGETATION_LINE", Constraint()) + calculateCount("VEGETATION_POINT", Constraint()) +
                             calculateCount("WATER_AREA", Constraint()) + calculateCount("WATER_LINE", Constraint()) + calculateCount("WATER_POINT", Constraint());
            Console.WriteLine("更新数据中属性错误的数据一共有" + erroeCount + "条");

            int totalCount = calculateCount("RESIDENTIAL_AREA","") + calculateCount("RESIDENTIAL_LINE", "") + calculateCount("RESIDENTIAL_POINT", "") +
                             calculateCount("SOIL_AREA", "") + calculateCount("SOIL_LINE", "") + calculateCount("SOIL_POINT", "") +
                             calculateCount("TRAFFIC_AREA", "") + calculateCount("TRAFFIC_LINE", "") + calculateCount("TRAFFIC_POINT", "") +
                             calculateCount("VEGETATION_AREA", "") + calculateCount("VEGETATION_LINE", "") + calculateCount("VEGETATION_POINT", "") +
                             calculateCount("WATER_AREA", "") + calculateCount("WATER_LINE", "") + calculateCount("WATER_POINT", "");
            Console.WriteLine("更新数据中一共有" + totalCount + "条数据");

            ratio= 1-erroeCount / totalCount;
            this.textBox1.Text = (ratio * 100).ToString() + "%";
            return ratio;
        }

        //统计数据
        public int calculateCount(string tablename ,string constraint)
        {
            int count = 0;
            string sql = "select count(*)  from " + tablename + constraint;
            using (OracleDataReader dr= OracleDBHelper.ExecuteReader(sql))
            {
                while (dr.Read())
                {
                    count = Convert.ToInt32( dr["count(*)"]);
                }
            }
            return count;
        }
       

        public string Constraint()//属性检查的约束条件
        {
            //lengthb(string)计算string所占的字节长度：返回字符串的长度，单位是字节
            //length(string)计算string所占的字符长度：返回字符串的长度，单位是字符

            StringBuilder sql = new StringBuilder( " where objectid is null  or ");

            sql.Append("NATIONCODE is null or ");

            sql.Append("NATIONELENAME is null or ");

            sql.Append("starttime is null or ");

            sql.Append("source is null or ");

            //sql.Append("instr(NATIONELENAME,'！')>0 or ");
            //sql.Append("instr(NATIONELENAME,'？')>0 or ");
            //sql.Append("instr(NATIONELENAME,'<')>0 or ");
            //sql.Append("instr(NATIONELENAME,'>')>0 or ");
            //sql.Append("instr(NATIONELENAME,'@')>0 or ");
            //sql.Append("instr(NATIONELENAME,'#')>0 or ");
            //sql.Append("instr(NATIONELENAME,'$')>0 or ");
            //sql.Append("instr(NATIONELENAME,'%')>0 or ");
            //sql.Append("instr(NATIONELENAME,'&')>0 or ");
            //sql.Append("instr(NATIONELENAME,'*')>0 or ");
            //sql.Append("instr(NATIONELENAME,'(')>0 or ");
            //sql.Append("instr(NATIONELENAME,')')>0 or ");
            //sql.Append("instr(NATIONELENAME,'|')>0 or ");
            //sql.Append("instr(NATIONELENAME,'|')>0 or ");
           
            sql.Append("shape is null");
            return sql.ToString();
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calculateRatio();
        }
    }
}
