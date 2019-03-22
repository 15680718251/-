using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GIS.UI.AdditionalTool;
using GIS.UI.Entity;
namespace GIS.UI.Forms
{
    public partial class professionDataToDB : Form
    {
        string fileName = "";
        string fileNametxt = "";
        OracleDBHelper helper = new OracleDBHelper();
        Poly poly = new Poly();
        

        public professionDataToDB()
        {
            InitializeComponent();
        }

        private void opentxtincBt_Click(object sender, EventArgs e)
        {
            OpenFileDialog osmFileDialog = new OpenFileDialog();
            osmFileDialog.Title = "输入txt文件";
            osmFileDialog.Filter = "txt文件|*.txt|inc文件|*.inc|XML文件|*.xml";
            osmFileDialog.Multiselect = true;
            if (osmFileDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < osmFileDialog.FileNames.Length; i++)
                {
                    //string currentStorage = "当前入库：" + osmFileDialog.FileNames[i] + "[" + (i + 1) + "/" + osmFileDialog.FileNames.Length + "]\r\n";
                    //txtOSMPath.Text = currentStorage;
                    fileName = osmFileDialog.FileNames[i];
                    txtTb1.Text = fileName;
                    //txtOSMPath.Text = fileNames;//zh
                }


            }      
        }

        private void opentxtBt_Click(object sender, EventArgs e)
        {
            OpenFileDialog osmFileDialog = new OpenFileDialog();
            osmFileDialog.Title = "输入txt文件";
            osmFileDialog.Filter = "osm文件|*.osm|txt文件|*.txt|XML文件|*.xml";
            osmFileDialog.Multiselect = true;
            if (osmFileDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < osmFileDialog.FileNames.Length; i++)
                {
                    //string currentStorage = "当前入库：" + osmFileDialog.FileNames[i] + "[" + (i + 1) + "/" + osmFileDialog.FileNames.Length + "]\r\n";
                    //txtOSMPath.Text = currentStorage;
                     fileNametxt = osmFileDialog.FileNames[i];
                     txtTb2.Text = fileNametxt;
                }
            }      
        }

        public List<string> getTxtData(string fileName)
        {
            List<string> getData = new List<string>();
            using (StreamReader sr = new StreamReader(fileName,Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string readStr = sr.ReadLine();
                    if (readStr.StartsWith("RecordBegin"))
                    {
                        string[] readStrs = readStr.Split(' ');
                        poly.setChangetype(readStrs[0]);
                        poly.setChangeset(readStrs[1]);

                        getData.Add(readStr); 
                    }

                }
            }
            return getData;
        }

        private void StratToDB_Click(object sender, EventArgs e)
        {
            getTxtData(fileName);
        }
    }
}
