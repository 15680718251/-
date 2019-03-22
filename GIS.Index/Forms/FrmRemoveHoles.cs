﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmRemoveHoles : Form
    {
        public string[] strPathNames;
        public double darea;

        public FrmRemoveHoles()
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("请选择矢量文件", "提示");
                return;
            }
            if (this.txtArea.Text == "")
            {
                MessageBox.Show("请输入筛选条件", "提示");
                return;
            }
            if (!isNumberic(this.txtArea.Text.Trim(), out darea))
            {
                MessageBox.Show("必须输入数字", "警告");
                return;
            }

            //foreach (string str in strPathNames)
            //{
            //    //ThreadPool.QueueUserWorkItem(dowork, new object[] { str, tmp });

            //    try
            //    {
            //        GIS.HandleShpsAndImage.HandleRemoveHoles hrh = new GIS.HandleShpsAndImage.HandleRemoveHoles(str,tmp);
            //        hrh.Work();
            //        hrh.Dispose();

            //    }
            //    catch (Exception ex)
            //    {
            //        //m_MapUI.OutPutTextInfo(ex.ToString());
            //    }
            //    finally
            //    {
            //        //autoResetEvent.Set();
            //    }
            //}


            //this.Close();
        }

        private void dowork(object parameter)
        {

            //object[] parameters = (object[])parameter;

            //string str = (string)parameters[0];

            //try
            //{
            //    GIS.HandleShpsAndImage.HandleRemoveHoles hrh = new GIS.HandleShpsAndImage.HandleRemoveHoles(str);
            //    hrh.Work();
            //    hrh.Dispose();

            //}
            //catch (Exception ex)
            //{
            //    //m_MapUI.OutPutTextInfo(ex.ToString());
            //}
            //finally
            //{
            //    //autoResetEvent.Set();
            //}
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
