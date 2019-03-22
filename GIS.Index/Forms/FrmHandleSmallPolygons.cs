using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmHandleSmallPolygons : Form
    {
        private string[] strPathNames;
        public FrmHandleSmallPolygons()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "矢量文件|*.shp";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strPathNames = dlg.FileNames;

                    foreach (string str in strPathNames)
                    {
                        this.listView1.Items.Add(str);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("请选择矢量文件","提示");
                return;
            }

            if (this.txtArea.Text == "")
            {
                MessageBox.Show("请输入筛选条件","提示");
                return;
            }

            double tmp;

            if (!isNumberic(this.txtArea.Text.Trim(),out tmp))
            {
                MessageBox.Show("必须输入数字","警告");
                return;
            }

            foreach(string str in strPathNames)
            {
                ThreadPool.QueueUserWorkItem(dowork, new object[] { str, tmp });
            }


            this.Close();


        }

        protected bool isNumberic(string message, out double result)
        {
            //判断是否为整数字符串
            //是的话则将其转换为数字并将其设为out类型的输出值、返回true, 否则为false
            result = -1;   //result 定义为out 用来输出值
            try
            {
                //当数字字符串的为是少于4时，以下三种都可以转换，任选一种
                //如果位数超过4的话，请选用Convert.ToInt32() 和int.Parse()

                //result = int.Parse(message);
                //result = Convert.ToInt16(message);
                result = Convert.ToDouble(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void dowork(object parameter)
        {

            object[] parameters = (object[])parameter;

            string str1 = (string)parameters[0];
            double str2 = (double)parameters[1];

            try
            {
                GIS.HandleShpsAndImage.HandleSmallPolygons hsp = new GIS.HandleShpsAndImage.HandleSmallPolygons(str1, str2);
                hsp.UninTouchSmallPolygon();

            }
            catch (Exception ex)
            {
                //m_MapUI.OutPutTextInfo(ex.ToString());
            }
            finally
            {
                //autoResetEvent.Set();
            }
        }
    }
}
