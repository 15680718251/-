using System;
 
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace GIS.TreeIndex.Forms
{
    public partial class CheckIncInfoForm : Form
    {
        public CheckIncInfoForm()
        {
            InitializeComponent();
        }
        public CheckIncInfoForm(MapUI ui)
        {
      
            InitializeComponent();
            m_MapUI = ui;
            for (int i = 0; i < ui.m_IncManager.m_UpdateCheckDatas.Count; i++)
            {
                GIS.Increment.IncManager.CheckRow row =  ui.m_IncManager.m_UpdateCheckDatas[i];
                Int64 clasid =row.m_ClassID;

                listView1.Items.Add((i + 1).ToString());//编号
                string typeName;
                try
                {
                    typeName = MapUI.FeatureCodes[clasid];
                }
                catch
                {
                    typeName = "未知";
                    clasid = 0;
                }
   
                listView1.Items[i].SubItems.Add(typeName);//要素名称
                listView1.Items[i].SubItems.Add(clasid.ToString());//要素编码

                listView1.Items[i].SubItems.Add(row.m_changetype);//变化类型

                listView1.Items[i].SubItems.Add(row.m_rows.Count.ToString());//变化个数

                listView1.Items[i].SubItems.Add(row.m_IncArea.ToString("f3"));//新增面积
                listView1.Items[i].SubItems.Add(row.m_IncLength.ToString("f3"));//新增长度
                listView1.Items[i].SubItems.Add(row.m_DecArea.ToString("f3"));//减少面积
                listView1.Items[i].SubItems.Add(row.m_DecLength.ToString("f3"));//减少长度
                
            }
        }
        private MapUI m_MapUI;

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_MapUI == null)
                return;
              int count = listView1.SelectedItems.Count ;
              if (count == 1)
              {
                  int index = listView1.SelectedItems[0].Index;
                  m_MapUI.ClearAllSltWithoutRefresh();
                  GIS.Increment.IncManager.CheckRow row = m_MapUI.m_IncManager.m_UpdateCheckDatas[index];
                  for (int i = 0; i < row.m_rows.Count; i++)
                  {
                      m_MapUI.AddSltObj(row.m_rows[i]);
                      m_MapUI.ZoomToFullExtent();
                  }
                  m_MapUI.Refresh();
              }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "增量检核文件|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
              
                    FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    try
                    {
                        string firstline = sr.ReadLine();
                        if (firstline == null
                            || firstline != "编号   要素类型     要素编码     变化类型     变化个数     新增面积     新增长度     减少面积     减少长度")
                        {
                            MessageBox.Show("增量文件错误！");
                            return;
                        }
                        string items = sr.ReadLine();
                        List<string> results = new List<string>();
                        while (items != null)
                        {  
                            string[] array = items.Split(' ');
                            string  result = "" ;
                            foreach (string s in array)
                            {
                                if (s != "")
                                    result += s+",";
                            }
                            results.Add(result);
                            items = sr.ReadLine();
                        }
                        if (m_MapUI.m_IncManager.CheckIncFile(results))
                        {
                            MessageBox.Show("检核成功");
                        }
                        else
                        {
                            MessageBox.Show("检核失败，该增量文件有问题");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("增量文件错误");
                    }
                    finally
                    {
                        sr.Close();
                        fs.Close();
                    }
            }
            
            
        }


    }
}
