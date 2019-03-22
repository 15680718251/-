using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Layer;
using GIS.Geometries;
using GIS.Render;

namespace GIS.TreeIndex.Forms
{
    public partial class AttributeForm : Form
    {
        public AttributeForm(MapUI ui ,string strLayerName)
        { 
            InitializeComponent();
            m_MapUI = ui;
            m_Layer = ui.GetLayerByName(strLayerName);
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;

            if (m_Layer is GeoVectorLayer)
            {
                GeoVectorLayer vlyr = m_Layer as GeoVectorLayer;

                if (vlyr.DataTable.FillData )
                {
                    dataGridView.DataSource = vlyr.DataTable;
                }
            }
       
            if (DataGridView.DataSource == null)
            {
                Text = m_Layer.LayerName + "层属性尚未导入！请稍等";
            }
            else
            {
                DataGridView.Columns[0].ReadOnly = true;
                
                //dataGridView.Columns["ClasID"].ReadOnly = true;

                #region 修改
                if (dataGridView.Columns.Contains("ChangeType"))
                {
                    dataGridView.Columns["ChangeType"].Visible = false;
                }
                
                #endregion

                Text = m_Layer.LayerName + "层属性";
            }
        }

        #region 属性窗口的私有成员
        private MapUI m_MapUI;
        private GIS.Layer.GeoLayer m_Layer;
        #endregion


        public AttributeForm()
        { 
            InitializeComponent();            
        }
        private int ColumnIndex;
        private void dataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                ColumnIndex = e.ColumnIndex;
            }
        }

        #region 2012.09.26修改
        private void 整列赋值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (ColumnIndex == 0 || dataGridView.Columns[ColumnIndex].Name == "ClasID")
            //{
            //    MessageBox.Show("此列为只读！！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}
            
            Type type = dataGridView.Columns[ColumnIndex].ValueType;           
            InputTextBox form = new InputTextBox(type);
            if (form.ShowDialog() == DialogResult.OK)
            {
                 
               for (int i = 0; i < dataGridView.Rows.Count; i++)
               {                    
               dataGridView.Rows[i].Cells[ColumnIndex].Value = form.TextValue;
               }
               
            }
        }
        #endregion

        private void dataGridView_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int index = e.RowIndex;
            if (m_Layer is GeoVectorLayer)
            {
                m_MapUI.ClearAllSltWithoutRefresh();
                GeoVectorLayer vlyr = m_Layer as GeoVectorLayer;
                GeoData.GeoDataRow row = vlyr.DataTable[index];
                if(vlyr.LayerTypeDetail!= LAYERTYPE_DETAIL.SurveyLayer)
                     m_MapUI.AddSltObj(row);
                m_MapUI.SetCenterGeoPoint(row.Geometry.Bound.GetCentroid());
                m_MapUI.Refresh();
            }

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
         
        }
    }
}
