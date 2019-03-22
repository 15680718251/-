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
    public class MapAddLabelTool : MapTool
    {
        private MapAddLabelTool()
        {
        }
        public MapAddLabelTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private bool bMouseDowing = false; //是否开始拖动
   
        private GeoLabel m_label ;
        private bool bValid = true;//本次操作是否有效
        private bool bIsNorth = true;
        private GeoDataRow LabelRow;
        public bool ValidOperator
        {
            get { return bValid; }
            set { bValid = value; }
        }

        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (ValidOperator && e.Button == MouseButtons.Left)
            {
                if (!bMouseDowing)
                {
                    FirstMouseDown(sender, e);
                }
                else
                {
                    SecondMouseDown(sender, e);
                }
            }
            else
            {
                ValidOperator = true;
                base.Cancel();  
            }
        }
       
        private void SecondMouseDown(object sender, MouseEventArgs e)
        {
            AddLabelToLayer();
        }

        private void FirstMouseDown(object sender, MouseEventArgs e)
        {
            m_label.StartPt = m_MapUI.TransFromMapToWorld(e.Location);
           

            Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
            Graphics g = Graphics.FromImage(imgTemp);

            float fontsize = m_MapUI.TransFromWorldToMap(m_label.TextSize);  //字体的像素大小，通过实地大小转换                  
            Font font = new Font(m_label.FontName, fontsize, GraphicsUnit.Pixel); //设置字体
            SizeF size = g.MeasureString(m_label.Text, font);
            Point EndPt = new Point(e.X + (int)size.Width, e.Y);
           
            DrawTrack(EndPt, g, imgTemp);
           
            if (!bIsNorth)
            {
                bMouseDowing = true;
                Cursor.Position = m_MapUI.PointToScreen(EndPt);
                m_MapUI.OutPutTextInfo("添加注记 ： 指定终点 \r\n");
            }
            else   //切换到移动节点工具，并将绘制的东西放进图层
            {               
                AddLabelToLayer();  
            }
        }

        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ValidOperator )
            {
                if (bMouseDowing)
                {
                    Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                    Graphics g = Graphics.FromImage(imgTemp);
                    DrawTrack(e.Location, g, imgTemp);
                }
                else
                {
                    m_label.StartPt = m_MapUI.TransFromMapToWorld(e.Location);

                    Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                    Graphics g = Graphics.FromImage(imgTemp);

                    float fontsize = m_MapUI.TransFromWorldToMap(m_label.TextSize);  //字体的像素大小，通过实地大小转换                  
                    Font font = new Font(m_label.FontName, fontsize, GraphicsUnit.Pixel); //设置字体
                    SizeF size = g.MeasureString(m_label.Text, font);
                    Point EndPt = new Point(e.X + (int)size.Width, e.Y);
                     
                    DrawTrack(EndPt, g, imgTemp);
                }
            }
        }

        public void DrawTrack(Point e, Graphics g, Image img)
        {
            m_label.EndPt = m_MapUI.TransFromMapToWorld(e);            
            m_MapUI.RePaint(g);
            Render.RenderAPI.DrawLabel(g, m_label, m_label.Color, m_MapUI);
            m_MapUI.Image.Dispose();
            m_MapUI.Image = img;
            g.Dispose();
            m_MapUI.BaseRefresh();

        }
         public void AddLabelToLayer() //将注记添加到图层，并结束添加功能
        {
            GeoVectorLayer lyr = m_MapUI.GetActiveLabelLayer() as GeoVectorLayer;
            LabelRow = lyr.AddGeometry(m_label);
            m_MapUI.InitialNewGeoFeature(LabelRow);
            LabelRow.EditState = EditState.Appear;
            m_MapUI.Refresh();
        
            m_MapUI.BoundingBoxChangedBy(LabelRow);//重新计算边界矩形
          
            base.Cancel();
        }
         public override void Finish()
         {
             m_MapUI.OutPutTextInfo("添加注记功能结束。\r\n");
         }
        public override void initial()
        {
            LabelForm form = new LabelForm(m_MapUI);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (LabelForm.LabelText == "" || !form.LayerExits)
                {
                    ValidOperator = false;
                    return;
                }
                bMouseDowing = false;
                bValid = true;
                m_MapUI.OutPutTextInfo("添加注记 ： 指定起始点 \r\n");
                bIsNorth = LabelForm.IsNorth;
                m_label = new GeoLabel(m_MapUI);
                m_label.Color = LabelForm.Color;
                m_label.TextSize = LabelForm.TextSize;
                m_label.FontName = LabelForm.FontName;
                m_label.Text = LabelForm.LabelText;

            }
            else
            {
                ValidOperator = false;
            }
        }
    }
}
