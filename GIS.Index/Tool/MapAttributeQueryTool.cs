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
    public class MapAttributeQueryTool : MapTool
    {
        AttributeQueryForm Form;           //属性查询设定窗口
        private int  AttrElevation;
        private string AttrPlgType;
        string predicate;

        public string newPlgValue;
        public AttributeQueryForm.TPType tpType;     //操作类型

        public MapAttributeQueryTool(MapUI ui): base(ui)
        {
            Form = new AttributeQueryForm();
            if (Form.ShowDialog() == DialogResult.OK)
            {
                predicate = Form.predicate;
                AttrElevation = Form.ElevaValue;
                AttrPlgType = Form.PlgValue;

                newPlgValue = Form.newPlgValue;
                tpType = Form.tpType;

                AttributeQuery();
            }
        }

        public void AttributeQuery()
        {
            if (Form.Cancel)
                return;

            if (AttrElevation >= 0)
            {
                List<GeoData.GeoDataRow> rows = m_MapUI.SelectByAttribute(AttrElevation, AttrPlgType, newPlgValue,tpType,predicate);

                if (tpType == AttributeQueryForm.TPType.Delete)
                {
                    m_MapUI.ToolDeleteObj();
                }

                //if (rows.Count > 0)
                //{
                //    m_MapUI.Refresh();
                //}
            }
            else
            {
                MessageBox.Show("高程数据不合法","提示");
                return;
            }
        }
    }
}