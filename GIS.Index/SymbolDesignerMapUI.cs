using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using GIS.Map;
using GIS.Geometries;
using GIS.Layer;
using GIS.Toplogical;
using GIS.Utilities;
using GIS.TreeIndex.Forms;
using GIS.mm_Conv_Symbol;
using GIS.GeoData;
using GIS.Render;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {

        #region private members
        public mm_conv_geometry m_conv_gtr;     //负责转换
        
        #endregion

        //public Symbolize SymbolValues; //记录某字段特定值相关的要素属性 李海欧

        public void SymbolDesigner()
        {
            if (m_conv_gtr == null)
            {
                m_conv_gtr = new mm_conv_geometry();
            }
            //m_conv_gtr.WriteSymbol2Disk();

            SymbolDesigner form = new SymbolDesigner(this);

            form.ShowDialog();
        }

        public void ShowSymbol()
        {
            if (m_conv_gtr == null)
                m_conv_gtr = new mm_conv_geometry();

            //m_bShowSymbol = true /*!m_bShowSymbol*/;//李海欧 此开关始终打开
            
            //if (m_bShowSymbol)
            //    this.OutPutTextInfo("符号显示开\r\n");
            //else
            //    this.OutPutTextInfo("符号显示关\r\n");

            // this.Refresh();//李海欧 后续会调用
        }

        //多边形符号化 李海欧
        public void PolygonSymbolize()
        {
            PolygonSymbolizeFrm SymbolizeFrm = new PolygonSymbolizeFrm(this);
            SymbolizeFrm.ShowDialog();
            //SymbolValues = SymbolizeFrm.SymbolValues;
            //this.Refresh();     // 吴志强，没有更改符号化时避免刷新
        }

        private void InitializationSymbol()
        {
            dic.Add("草地",Color.FromArgb(255, 255, 128, 255));
            dic.Add("森林", Color.FromArgb(255, 0, 255, 0));
            dic.Add("灌木", Color.FromArgb(255, 0, 255, 255));
            dic.Add("耕地", Color.FromArgb(255, 255, 255, 0));
            dic.Add("水体", Color.FromArgb(255, 0, 0, 255));
            dic.Add("人造覆盖", Color.FromArgb(255, 255, 0, 0));
            dic.Add("裸地", Color.FromArgb(255, 118, 143, 139));
            dic.Add("湿地", Color.FromArgb(255, 255, 128, 255));
        }
    }
}
