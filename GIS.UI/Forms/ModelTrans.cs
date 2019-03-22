using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.ModelTranByOSM;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;

namespace GIS.UI.Forms
{
    //*************by dy20180711*********
    public partial class ModelTrans : Form
    {
        private string conString;
        private string tableName;
        public ModelTrans()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            tableName = tableNameCBX.Text;
            new System.Threading.Thread(new System.Threading.ThreadStart(modelTrans)).Start();
           
        }

        private void modelTrans_onProgress(int layerIndex, int layerCount, string layerName, int maximum, int current)
        {

            if (this.InvokeRequired)
            {
                this.Invoke(new TEModelTrans.dModelTransProgress(modelTrans_onProgress), new object[] { layerIndex, layerCount, layerName, maximum, current });
            }
            else
            {

                this.pgBar.Maximum = maximum;
                this.pgBar.Value = current;
                this.stateTBox.Text = string.Format("\r\n  进度：{0}/{1}\r\n\r\n    正在提取图层：{2}", layerIndex, layerCount, layerName);
            }
        }
        public void modelTrans()
        {
   
            TEModelTrans mt = new TEModelTrans("type_rule",tableName);
            mt.onModelTransProgress += new TEModelTrans.dModelTransProgress(modelTrans_onProgress);
            mt.start();
            MessageBox.Show("模型转换完成！");
        }

        private void deleteTableBtn_Click(object sender, EventArgs e)
        {
            TypicalElementTableHelper.deleteAllTable();
            this.stateTBox.Text = "\r\n    删除完成！";
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取所有数据库中的表名
        /// </summary>
        /// <param name="conString"></param>
        /// <returns></returns>
        private List<string> getTableNames(string conString)
        {
            List<string> baseTables = new List<string>();//系统自带的数据表
            List<string> tableNames = new List<string>();

            try
            {
                OracleDBHelper conHelper = new OracleDBHelper();
                OracleConnection con = conHelper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                //选择获取Oracle数据库中的所有基态数据表名
                string sqlString = "select distinct table_name  from user_col_comments where lower(table_name) like '%point%' or  lower(table_name) like '%line%' or lower(table_name) like '%poly%'or lower(table_name) like '%road_rule%'or lower(table_name) like '%area%'order by  lower(table_name) ASC";
                using (OracleCommand command = new OracleCommand(sqlString, con))
                {
                    OracleDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        string name = dr[0].ToString();
                        if (baseTables.Contains(name))
                        {
                            continue;
                        }
                        else
                        {
                            tableNames.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return tableNames;
        }

        private void ModelTrans_Load(object sender, EventArgs e)
        {
            List<string> tableNames = getTableNames(conString);
            this.tableNameCBX.Items.Clear();
            if (tableNames != null && tableNames.Count > 0)
            {
                foreach (string name in tableNames)
                {
                    this.tableNameCBX.Items.Add(name);
                }

            }

        }
    }
}
