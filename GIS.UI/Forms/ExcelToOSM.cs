using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Data.OleDb;
using System.IO;
using GIS.UI.AdditionalTool;

namespace GIS.UI.Forms
{
    //*************by dy20180711*********
    public partial class ExcelToOSM : Form
    {
        OracleDBHelper helper = new OracleDBHelper();
       
        string[] myDataType = { "NUMBER", "FLOAT", "VARCHAR2" };
        public ExcelToOSM()
        {
            InitializeComponent();
            foreach (string s in myDataType)
            {
                DataType.Items.Add(s);
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "excel文件(*.xls)|*.xls";
            file.Multiselect = true;
            file.Title = "选择导入数据库的excel文件";
            if (file.ShowDialog() == DialogResult.OK)
            {
                if (file.FileNames.Length != 0)
                {
                    string temp = null;

                    for (int i = 0; i < file.FileNames.Length; i++)
                    {
                        temp = temp + file.FileNames[i] + ";";
                    }
                    textBox1.Text = temp;
                }
                else
                {

                    textBox1.Text = "";
                }
                DataTable dt = getDataSet().Tables[0];
                int fieldNo = dt.Columns.Count;

                for (int i = 0; i < fieldNo; i++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    dataGridView1.Rows.Add(row); //添加行
                    dataGridView1.Rows[i].Cells[0].Value = dt.Columns[i].ColumnName;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            pgShow.Text = "导入中....";
            Application.DoEvents();
            string[] str = textBox1.Text.Split('\\');//获取表名
            string tableName = str[str.Count() - 1];
            tableName = tableName.TrimEnd('.', 'x', 'l', 's', ';');// 删除字符串中的.xls;

            DataTable dt = getDataSet().Tables[0];
            if (IsExistTable(tableName))
            {
                helper.DropTable(tableName);
            }
            string mysql = getCreatTableStr(tableName, dt, dataGridView1);
            OracleConnection con = new OracleConnection(OSMDataBaseLinkForm.conStringTemp);//连接OSM数据库OSMDataFilterRuleLinkForm

            changeTable(mysql, con);//建表
            inputData(tableName, dt, con, dataGridView1); //对表中填入数据
            dt.Dispose();
            this.pgBar.Value = 0;
            pgShow.Text = "导入完成！";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        /// <summary>
        /// 根据DataTable获取建表的SQL语句
        /// </summary>
        /// <param name="myDataTable"></param>
        /// <returns></returns>
        private string getCreatTableStr(string tableName, DataTable myDataTable, DataGridView dgv)
        {
            
                     
            string creatTableStr = "CREATE TABLE " + tableName + "(";


            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {

                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridView1[1, i];
                string myStr;
                if (cell.Value == null)
                {
                    MessageBox.Show("请选择" + myDataTable.Columns[i].ColumnName + "字段的数据类型！");
                }
                else
                {
                    if (dataGridView1[2, i].Value == null)
                    {
                        if ("character varying".Equals(cell.Value.ToString()))
                            myStr = cell.Value.ToString() + " (256)";
                        else
                            myStr = cell.Value.ToString();
                    }
                    else
                    {

                        myStr = cell.Value.ToString() + " " + dataGridView1[2, i].Value.ToString();

                    }
                    creatTableStr = creatTableStr + myDataTable.Columns[i].ColumnName + " " + myStr + ",";

                }
            }


            if (constraint.Text == "")//如果没有约束条件
            {
                creatTableStr = creatTableStr.TrimEnd(',');
            }
            else
            {
                creatTableStr = creatTableStr + constraint.Text;
            }
            creatTableStr = creatTableStr + ")";
            myDataTable.Dispose();
            return creatTableStr;
        }

        /// <summary>
        /// 获取excel表中数据存入数据集myDataSet
        /// </summary>
        /// <returns></returns>
        private DataSet getDataSet()
        {
            //创建一个数据链接
            string strCon = " Provider = Microsoft.Jet.OLEDB.4.0 ; Data Source = " + textBox1.Text + "Extended Properties=Excel 8.0";
            OleDbConnection myConn = new OleDbConnection(strCon);
            myConn.Open();
            DataTable schemaTable = myConn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
            string tableName = schemaTable.Rows[0][2].ToString().Trim();
            string strCom = string.Format(" SELECT * FROM [{0}] ", tableName);

            OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
            System.Data.DataSet myDataSet = new DataSet();
            myCommand.Fill(myDataSet, "[Sheet1$]");
            myConn.Close();
            return myDataSet;
        }

        /// <summary>
        /// 对表中填入数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="myDataTable"></param>
        /// <param name="conn"></param>
        private void inputData(string tableName, DataTable myDataTable, OracleConnection conn, DataGridView myDataGridView)
        {

            string mystr = "insert into " + tableName + "(";
            List<int> specialType = new List<int>();//记录特殊字段所在的列号

            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {

                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridView1[1, i];
                if (cell.Value == null)
                {
                    MessageBox.Show("请选择" + myDataTable.Columns[i].ColumnName + "字段的数据类型！");
                }
                else if (cell.Value.ToString() == "integer" || cell.Value.ToString() == "smallint" || cell.Value.ToString() == "double precision" || cell.Value.ToString() == "bigint")
                {

                    specialType.Add(i);
                }

                mystr = mystr + dataGridView1[0, i].Value.ToString() + ",";

            }
            mystr = mystr.TrimEnd(',');
            mystr = mystr + ") values (";


            if (specialType.Count > 0)
            {
                for (int i = 0; i < myDataTable.Rows.Count; i++)
                {
                    string inputsql = mystr;
                    for (int j = 0; j < myDataTable.Columns.Count; j++)
                    {
                        if (specialType.Contains(j))
                        {
                            string aa = myDataTable.Rows[i][j].ToString();
                            if (myDataTable.Rows[i][j].ToString() != "")
                            {
                                inputsql = inputsql + myDataTable.Rows[i][j] + ",";

                            }
                            else
                            {
                                inputsql = inputsql + "null,";

                            }

                        }
                        else
                        {
                            if (myDataTable.Rows[i][j].ToString() == "")
                            {
                                inputsql = inputsql + " null,";
                            }
                            else if (myDataTable.Rows[i][j].ToString().Contains("'"))
                            {
                                string tem = myDataTable.Rows[i][j].ToString();
                                tem = tem.Replace("'", "''");
                                inputsql = inputsql + "\'" + tem + "\'" + ",";
                            }
                            else
                            {
                                inputsql = inputsql + "\'" + myDataTable.Rows[i][j] + "\'" + ",";
                            }
                        }
                    }

                    inputsql = inputsql.TrimEnd(',');
                    inputsql = inputsql + ")";
                    changeTable(inputsql, conn);//写入数据
                    this.pgBar.Value = (i + 1) * 100 / myDataTable.Rows.Count;
                }

            }
            else
            {
                for (int i = 0; i < myDataTable.Rows.Count; i++)
                {
                    string inputsql = mystr;
                    for (int j = 0; j < myDataTable.Columns.Count; j++)
                    {

                        if (myDataTable.Rows[i][j].ToString().Contains('\''))
                        {
                            string dataTableStr = myDataTable.Rows[i][j].ToString();
                            dataTableStr = dataTableStr.Replace("'", "''");
                            inputsql = inputsql + "\'" + dataTableStr + "\'" + ",";

                        }
                        else
                        {
                            inputsql = inputsql + "\'" + myDataTable.Rows[i][j] + "\'" + ",";
                        }

                    }

                    inputsql = inputsql.TrimEnd(',');
                    inputsql = inputsql + ")";
                    changeTable(inputsql, conn);//写入数据
                    this.pgBar.Value = (i + 1) * 100 / myDataTable.Rows.Count;
                }
                conn.Dispose();
                myDataTable.Dispose();

            }


        }
        /// <summary>
        /// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除一条记录的操作
        /// </summary>
        /// <param name="changeString"></param>
        /// <param name="conn"></param>
        private void changeTable(string changeString, OracleConnection conn)
        {
            try
            {
                conn.Open();
                OracleCommand objCommand = new OracleCommand(changeString, conn);
                objCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //AdditionalTool.SomeTool.writeData(changeString, "log");
                Console.WriteLine(e.Message);

            }
            conn.Close();
        }


        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsExistTable(string tableName)
        {
            string sql = string.Format("select count(*) from user_tab_comments where TABLE_NAME=upper('" + tableName + "')");
            OracleConnection con = helper.getOracleConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand com = new OracleCommand(sql, con))
            {
                using (OracleDataReader dr = com.ExecuteReader())
                {
                    if (dr != null && dr.HasRows)
                    {
                        dr.Read();
                        if ("1".Equals(dr.GetValue(0).ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


    }
}
