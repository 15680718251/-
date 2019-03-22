using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.GeoData;
using GIS.Layer;
namespace GIS.TreeIndex.Forms
{
    public partial class IncAttributeForm : Form
    { 
        private IncAttributeForm()
        {
            InitializeComponent(); 
        }
        private MapUI m_MapUI;
        private GeoDataRow m_OrignRow;
        public IncAttributeForm(GeoDataRow OrignRow, MapUI ui)
        {
            InitializeComponent();
            m_MapUI = ui;
            m_OrignRow = OrignRow;
            GeoDataTable table = ((GeoDataTable)OrignRow.Table).Clone();
            GeoLayer layer = ui.GetLayerByTable((GeoDataTable)OrignRow.Table);//所属图层
            table.ImportRow(OrignRow);

            DataGridView.DataSource = table;

            string temp = OrignRow.Geometry.ToString();
            int index = temp.LastIndexOf('.');
            string strGeomType = temp.Substring(index + 1, temp.Length - index - 1);
            string strBelongLayer = ((GeoData.GeoDataTable)OrignRow.Table).BelongLayer.LayerName;
            string strLength = OrignRow.Geometry.Length.ToString();
            string strArea = OrignRow.Geometry.Area.ToString();

            listView1.Items.Add( "几何类型:");
            listView1.Items[0].SubItems.Add(strGeomType);
            listView1.Items.Add("所属图层:");
            listView1.Items[1].SubItems.Add(strBelongLayer);
            listView2.Items.Add("长度:");
            listView2.Items[0].SubItems.Add(strLength);
            listView2.Items.Add("面积:");
            listView2.Items[1].SubItems.Add(strArea);

            DataGridView.Columns["FID"].ReadOnly = true;
            //DataGridView.Columns["ClasID"].Visible = false;
            //DataGridView.Columns["FeatID"].Visible = false;

            #region 20121010修改
            DataGridView.Columns["ChangeType"].ReadOnly = true;
            DataGridView.Columns["ChangeType"].Visible = false;
            #endregion


            //DataGridView.Columns["BeginTime"].Visible = false; 

            int curClasIndex = -1;
            foreach (var item in MapUI.FeatureCodes)
            { 
                string strTemp = string.Format("{0} {1}", item.Key.ToString(), item.Value);
                curClasIndex =comboClasIDLevel2.Items.Add(strTemp);
                if (item.Key % 100000 == 0)
                {
                    comboClasIDLevel1.Items.Add(strTemp);
                }
                //if (item.Key.ToString() == table.Rows[0]["ClasID"].ToString())
                //{
                //    comboClasIDLevel2.SelectedIndex = curClasIndex;         
                //}
            }

            //string featID = table.Rows[0]["FeatID"].ToString();
            //TextBoxFeatID.Text = (featID != "") ? featID : (MapUI.FeatID).ToString();
            //string beginTime = table.Rows[0]["BeginTime"].ToString();
            //TextBoxBeginTime.Text = (beginTime != "") ? beginTime : MapUI.BeginTime;
            //string clasID = table.Rows[0]["ClasID"].ToString();
            //if (clasID == "")
            //{
            //    string NameID = null;
            //    try
            //    {
            //        NameID = MapUI.FeatureCodes[layer.ClasID];
            //    }
            //    catch
            //    {
            //        NameID = "未知";
            //    }
            //    comboClasIDLevel2.Text = string.Format("{0} {1}", layer.ClasID, NameID);
            //}
            //else comboClasIDLevel2.Text = clasID;
        
        }

    

        private void comboClasIDLevel1_SelectedIndexChanged(object sender, EventArgs e)
        {
            long ulCode = (comboClasIDLevel1.SelectedIndex+1)*100000;
            comboClasIDLevel2.Items.Clear();
            foreach (var item in MapUI.FeatureCodes)
            {
                string strTemp = string.Format("{0} {1}", item.Key.ToString(), item.Value);

                if (item.Key >= ulCode && item.Key < ulCode + 100000)
                {
                    comboClasIDLevel2.Items.Add(strTemp);
                }
            }
            comboClasIDLevel2.SelectedIndex = 0;
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
           
            //string[] strTemp = comboClasIDLevel2.Text.Split(' ');
            //Int64 clasID = long.Parse(strTemp[0]);
            //string beginTime = TextBoxBeginTime.Text;

            DataTable EditTable = (DataTable)DataGridView.DataSource;
            DataRow EditRow = EditTable.Rows[0];
            //EditRow["BeginTime"] = beginTime; 
            //EditRow["FeatID"] = TextBoxFeatID.Text;      
            //EditRow["ClasID"] = clasID;

            GeoLayer layer = m_MapUI.GetLayerByTable((GeoDataTable)m_OrignRow.Table);//给系统设置赋值
            //layer.ClasID = clasID;
            MapUI.BeginTime = TextBoxBeginTime.Text;

            for (int i = 0; i < EditTable.Columns.Count; i++)
            {
                if (EditRow[i].ToString() != m_OrignRow[i].ToString())
                {
                    if (m_OrignRow.EditState == EditState.Original)//如果变化类型为原图，则记录增量，新添加数据
                    {
                        GeoDataRow RowBackup = m_OrignRow.Clone(); //原记录的备份
                        m_OrignRow.ItemArray = EditRow.ItemArray;

                        ((GeoDataTable)m_OrignRow.Table).AddRow(RowBackup);
                        RowBackup.EditState = EditState.AttributeBef;
                        m_OrignRow.EditState = EditState.AttributeAft;
                      //  Increment.IncAttribute incInfo = new GIS.Increment.IncAttribute(RowBackup, m_OrignRow);
                      //  m_MapUI.m_IncManager.AddIncInfo(incInfo);
                    }
                    else //如果是其他变化类型，则直接修改属性
                    {
                        m_OrignRow.ItemArray = EditRow.ItemArray;
                    }
                    return;
                }
            }
        }
    }
}
