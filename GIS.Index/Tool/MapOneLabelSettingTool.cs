using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.GeoData;
using GIS.Geometries;
using GIS.Layer;
using GIS.Style;
using GIS.TreeIndex.Forms;
namespace GIS.TreeIndex.Tool
{
   public class MapOneLabelSettingTool:MapTool
    {
       public MapOneLabelSettingTool(MapUI ui)
           : base(ui)
       {
           initial();
       }
      
       public override void initial()
       {
           if (m_MapUI.SltLabelSet.Count != 1)
           {
               m_MapUI.ClearAllSlt();
               m_MapUI.OutPutTextInfo("提示：注记修改工具激活，请选择注记 \r\n");
           }
           else
           {
               LabelEdit();
           } 
       }
       private void LabelEdit()
       {
           if (m_MapUI.SltLabelSet.Count == 0)
               return;
           GeoDataRow row = m_MapUI.SltLabelSet[0];
           GeoLabel label = row.Geometry as GeoLabel;
           m_MapUI.OutPutTextInfo("提示：开始修改注记 \r\n");
           LabelForm form = new LabelForm(m_MapUI, row);
           if (form.ShowDialog() == DialogResult.OK)
           {
               label.Text = LabelForm.LabelText;
               label.FontName = LabelForm.FontName;
               label.TextSize = LabelForm.TextSize;
               label.Color = LabelForm.Color;
               LabelResize();
               m_MapUI.Refresh();
               base.Cancel();
           }
       }
       public override void OnMouseDown(object sender, MouseEventArgs e)
       {
          GeoDataRow row =  m_MapUI.SelectByPt(e.Location, SelectType.Label);
      
          if (row != null && row.Geometry is GeoLabel)
          {
              LabelEdit();
          }
          m_MapUI.ClearAllSlt();
       }
       public void LabelResize()
       {
           GeoDataRow row = m_MapUI.SltLabelSet[0];
           GeoLabel label = row.Geometry as GeoLabel;
           float fontsize = (float)Math.Ceiling(m_MapUI.TransFromWorldToMap(label.TextSize));  //字体的像素大小，通过实地大小转换 

           Font font = new Font(label.FontName, fontsize, GraphicsUnit.Pixel); //设置字体

           Graphics g = m_MapUI.CreateGraphics();
           float fontheight = font.GetHeight(g); //字体的高度  

           SizeF size = g.MeasureString(label.Text, font);
           double width = m_MapUI.TransFromMapToWorld(size.Width);
           GeoPoint NoRotatePt = new GeoPoint(label.StartPt.X + width, label.StartPt.Y);
           GeoPoint EndPt1 = SpatialRelation.GeoAlgorithm.PointRotate(NoRotatePt, label.StartPt,-label.RotateAngle * Math.PI / 180);
           label.EndPt = EndPt1; 

       }

    }
}
