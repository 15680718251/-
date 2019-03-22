using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Collections;
using ESRI.ArcGIS.Display;

/*
 * 拓扑冲突的显示
 * zh编写
 * 2018年9月19日
 */
namespace QualityControl.Forms
{
    public partial class showErrorData : Form
    {
        AxMapControl axMapControl1;
        string layer1;
        string layer2;
        public showErrorData()
        {
            InitializeComponent();
        }

        public showErrorData(AxMapControl axMapControl1,string layer1,string layer2,DataTable dt)
        {
            InitializeComponent();
            errorDataGV.DataSource = dt;
            this.axMapControl1 = axMapControl1;
            this.layer1 = layer1;
            this.layer2 = layer2;
        }

        private void showErrorData_Load(object sender, EventArgs e)
        {
            
        }

        private void showFeatrue_headerMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //this.errorDataGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;//设置为整行被选中
            string objectid1 = this.errorDataGV.CurrentRow.Cells["图层1ID"].Value.ToString();
            string objectid2 = this.errorDataGV.CurrentRow.Cells["图层2ID"].Value.ToString();
            OperateMap m_OperateMap = new OperateMap();
            AddMap map = new AddMap();
            if (!layer1.Equals(layer2))
            {
                IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                //map.showByAttibute(axMapControl1, "objectid", "=", int.Parse(objectid));
                map.showByAttibute(pFeatLyr, "objectid", "=", objectid1, 255, 0, 0);

                IFeatureLayer pFeatLyr2 = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer2);
                map.showByAttibute(pFeatLyr2, "objectid", "=", objectid2,255,0,0);
            }
            else
            {
                IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                ArrayList list = new ArrayList();
                list.Add(objectid1);
                list.Add(objectid2);
                map.showByAttibute(pFeatLyr, "objectid", "=", list,255,0,0);

                //IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                //map.showByAttibute(pFeatLyr, "objectid", "=", objectid1, 255, 0, 0);

                //IFeatureLayer pFeatLyr2 = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer2);
                //map.showByAttibute(pFeatLyr2, "objectid", "=", objectid2, 0, 0, 255);
            }
            axMapControl1.Refresh();
        }
    }
}
