using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using GIS.TreeIndex;
using GIS.Buffer;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.Forms;
namespace GIS.TreeIndex.Tool
{
    public class MapAreaQueryTool : MapTool
    {
        AreaQueryForm Form;           //属性查询设定窗口
        private double AreaVal;
        private string Predicate;

        public MapAreaQueryTool(MapUI ui) : base(ui)
        {
            Form = new AreaQueryForm();
            if (Form.ShowDialog() == DialogResult.OK)
            {
                AreaVal = Form.AreaValue;
                Predicate = Form.sel;
                AreaQuery();
            }
        }

        #region 按给定条件-查询缓冲区与剔除
        public void AreaQuery()
        {
            if (Form.Cancel)
                return;

            if (AreaVal >= 0)
            {
                List<GeoData.GeoDataRow> rows = m_MapUI.SelectByArea(AreaVal,Predicate);
                //if (rows != null)
                //{
                //    if (rows.Count > 0)
                //    {
                //        m_MapUI.Refresh();
                //    }
                //}
            }
        }
        #endregion
    }
}