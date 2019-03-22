using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Layer;
using GIS.Render;
using GIS.Style;
using System.Collections.Generic;
using GIS.GeoData;
using GIS.GdiAPI;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        public delegate void RefreshEventHandler();
        public volatile bool bStopDrawLayer = false;
        //private static object m_MonitorObject = new object();

        #region old
        //public void DrawSpecifiedObject(List<GeoDataRow> rowlist)
        //{
            
        //    Graphics g = Graphics.FromImage(m_ImgBackUp);

        //    for (int i = 0; i < rowlist.Count; i++)
        //    {
        //        GeoDataTable table = rowlist[i].Table as GeoDataTable;
        //        GeoVectorLayer lyr = table.BelongLayer as GeoVectorLayer;
        //        GIS.Render.RenderAPI.DrawGeometry(g, rowlist[i].Geometry, lyr.LayerStyle, this);
        //    }

        //    //this.Image = m_ImgBackUp;
        //    Graphics gg = Graphics.FromImage(this.Image);
        //    gg.DrawImage(m_ImgBackUp, new Point(0, 0));
        //    base.Refresh();
        //}

        //public void DrawSpecifiedObject(GeoDataRow row)
        //{
        //    Graphics g = Graphics.FromImage(m_ImgBackUp);
        //    GeoDataTable table = row.Table as GeoDataTable;
        //    GeoVectorLayer lyr = table.BelongLayer as GeoVectorLayer;
        //    GIS.Render.RenderAPI.DrawGeometry(g, row.Geometry, lyr.LayerStyle, this);

        //    //this.Image = m_ImgBackUp;
        //    Graphics gg = Graphics.FromImage(this.Image);
        //    gg.DrawImage(m_ImgBackUp, new Point(0, 0));
        //    base.Refresh();
        //}
        #endregion old

        private void DrawSltObject(Graphics g)
        {
            VectorStyle styleSelect = new VectorStyle(2, 2, Color.Blue, Color.Blue, true);
            VectorStyle styleSelectsymbol = new VectorStyle(2, 2, Color.Red, Color.Red, true);

            for (int i = 0; i < SltGeoSet.Count; i++)
            {
                if ((int)SltGeoSet[i].EditState <= 3)
                {
                    if (SltGeoSet[i].symbol != null && SltGeoSet[i].bsymrender == true)
                        Render.RenderAPI.DrawSymbol(g, SltGeoSet[i], this.m_ViewExtents.Clone(), (float)DBlc, styleSelectsymbol);
                    else
                        GIS.Render.RenderAPI.DrawGeometry(g, SltGeoSet[i].Geometry, styleSelect, this);
                }
                else
                    RemoveSltObj(SltGeoSet[i]);
            }

            for (int i = 0; i < SltLabelSet.Count; i++)
            {
                GIS.Render.RenderAPI.DrawLabel(g, (GeoLabel)SltLabelSet[i].Geometry, Color.Blue, this);
            }
          
        }
        public bool VectorLayerRender(Graphics g, GeoVectorLayer layer, GeoBound renderbound )
        {
            VectorStyle unsltStyle = layer.LayerStyle;

            int nNumsGeo = layer.DataTable.Count;
            for (int k = 0; k < nNumsGeo; k++)
            {
                if (bStopDrawLayer)
                    return false;

                GeoData.GeoDataRow row = layer.DataTable[k];
                Geometry geom = row.Geometry;
                if (geom == null || (int)row.EditState >= 6 ||
                    !geom.Bound.IsIntersectWith(renderbound))    /// 如果已经删除或者与当前屏幕坐标范围不相交 则不画
                    continue;

                if (row.SelectState == true)
                {
                    continue;           //如果选中 则不绘制
                }

                Render.RenderAPI.DrawGeometry(g, geom, unsltStyle, this); 

            }
            return true;
        }

        // 符号化
        public bool VectorLayerSymbolRender(Graphics g, GeoVectorLayer layer, GeoBound renderbound)
        {
            VectorStyle unsltStyle = layer.LayerStyle;
            //unsltStyle.LineColor = Color.Transparent;
            //VectorStyle unsltStyle = new VectorStyle(layer.LayerStyle.SymbolSize, layer.LayerStyle.LineSize,
            //    Color.Black, Color.Black, layer.LayerStyle.IsSelected);

            int nNumsGeo = layer.DataTable.Count;
            for (int k = 0; k < nNumsGeo; k++)
            {
                if (bStopDrawLayer)
                    return false;

                GeoData.GeoDataRow row = layer.DataTable[k];
                Geometry geom = row.Geometry;
                if (geom == null || (int)row.EditState >= 6 ||
                    !geom.Bound.IsIntersectWith(renderbound))    /// 如果已经删除或者与当前屏幕坐标范围不相交 则不画
                    continue;

                if (row.SelectState == true)
                {
                    continue;           //如果选中 则不绘制
                }

                if (m_conv_gtr == null)
                    m_conv_gtr = new GIS.mm_Conv_Symbol.mm_conv_geometry();

                if (row.bsymrender == true || row.bsymbolupdate == true)
                {
                    m_conv_gtr.conv_geometry(row);
                    row.bsymbolupdate = false;
                }
                if (row.symbol != null && row.bsymrender == true)
                    Render.RenderAPI.DrawSymbol(g, row, this.m_ViewExtents.Clone(), (float)DBlc, unsltStyle);
                else
                {
                    /*************************************************/
                    #region //改动：李海欧 2012.6.28-2012.7.5 设置多边形符号化的颜色
                    // 吴志强 2013.04.27  
                    //string FieldName = layer.SymbolField;
                    string FieldName = this.symField;
                    string value = layer.DataTable[k][FieldName].ToString();
                    Color color = new Color();
                    //layer.FileSymbol.TryGetValue(value,out color);//时间复杂度O(1)  避免遍历
                    this.dic.TryGetValue(value,out color);

                    if (color != null)
                    {
                        unsltStyle.FillColor = color;
                    }
                    #endregion
                    /**************************************************/
                    Render.RenderAPI.DrawSymbolGeometry(g, geom, unsltStyle, this);
                }
            }
            return true;
        }

        private bool DrawLayerByType(Graphics g, GIS.Layer.LAYERTYPE_DETAIL type)
        {
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoLayer layer = GetLayerAt(j);
                if (layer.Enable == false)
                    continue;
                if (layer.LayerTypeDetail == type)
                {
                    try
                    {
                        if (layer.LayerType == LAYERTYPE.VectorLayer)
                        {
                            GeoVectorLayer lyr = layer as GeoVectorLayer;

                            if (lyr.LayerTypeDetail == LAYERTYPE_DETAIL.PolygonLayer && this.VecSymbolize == true)
                            {
                                this.VectorLayerSymbolRender(g, lyr, m_ViewExtents);
                                continue;
                            }

                            if (lyr.bShowSymbol)  //有符号化
                            {
                                this.VectorLayerSymbolRender(g, lyr, m_ViewExtents);
                            }
                            else
                            {
                                if (!VectorLayerRender(g, lyr, m_ViewExtents))
                                    return false;
                            }
                        }
                        else if (layer.LayerType == LAYERTYPE.RasterLayer)
                        {
                            GeoRasterLayer rasterlyr = layer as GeoRasterLayer;
                            GIS.Render.LayerRender.RasterLayerRender(g, rasterlyr, Size, this);
                        }
                        else
                            continue;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            return true;
        }
        //  private Style.VectorStyle[] styles = new VectorStyle[3];
        //  private static int randNumb = 0;
        //private void DrawIncThreadProc()
        //{
        //   Bitmap bmp = new Bitmap(Width, Height);
        //   Graphics g = Graphics.FromImage(bmp);

        //   try
        //   {
               
        //       RePaint(g);
        //       List<GeoData.GeoDataRow> rows = m_IncManager.GetGeomForRender();
        //       randNumb = randNumb % 3 ;
        //       VectorStyle style = styles[randNumb];
        //       for (int i = 0; i < rows.Count; i++)
        //       {
        //           GIS.Render.RenderAPI.DrawGeometry(g, rows[i].Geometry, style, this);
        //       }
        //       RefreshEventHandler refresh = new RefreshEventHandler(BaseRefresh);
        //       Image = bmp;
        //       this.Invoke(refresh);
        //       randNumb++;
        //   }
        //   catch (Exception e)
        //   {
        //       g.Dispose();
        
        //   } 
        //}
        //绘制增量信息
        private void DrawIncrement(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Thread thread = new Thread(new ThreadStart(DrawIncThreadProc));
            //thread.IsBackground = true;
            //thread.Start();
            //DrawIncThreadProc();
        }
        private void DrawGeoTag(Graphics g)
        {
            System.Drawing.Font font = new Font("Times New Roman", 15, GraphicsUnit.Pixel);
            Brush brush = new SolidBrush(Color.Blue);
            string cur_path = string.Format("{0}\\相机.PNG", Application.StartupPath);
            Image img = new Bitmap(cur_path);
            for (int i = 0; i < m_GeoTagList.Count; i++)
            {
                GeoPoint pt = m_GeoTagList[i].m_Pos;
                Point spt = TransFromWorldToMap(pt);             
                g.DrawString(i.ToString(), font, brush,spt.X,spt.Y+15);                
                g.DrawImage(img,new Rectangle(new Point( spt.X-10,spt.Y-10),new Size(20,20)));
            }

            font.Dispose();
            brush.Dispose();
        }
        private void DrawSurveyPts(Graphics g)
        {
            //m_SurveyPt
            //需要进行特别处理，绘制出方向来
            //tag:9.27
            Pen pen = new Pen(Color.Red, 0.5f);
            Brush brush = new SolidBrush(Color.Blue);
            System.Drawing.Font font = new Font("Times New Roman", 15, GraphicsUnit.Pixel);

            GeoVectorLayer surLayer = GetLayerByName("测点层") as GeoVectorLayer;
            for (int i = 0; i < surLayer.DataTable.Count; i++)
            {
                GeoPoint pt = surLayer.DataTable[i].Geometry as GeoPoint;

                Point spt = TransFromWorldToMap(pt); 
                g.DrawLine(pen, spt.X - 12, spt.Y, spt.X + 12, spt.Y);
                g.DrawLine(pen, spt.X, spt.Y - 12, spt.X, spt.Y +12); 
                g.DrawString(i.ToString(), font, brush, spt); 
            }
            pen.Dispose();
            brush.Dispose();
            font.Dispose();
           
        }

        public delegate void ThreadProcEventHandle();

        public void GetImage()
        {
            //bStopDrawLayer = true;
            //Thread ThreadDrawLayer = new Thread(new ThreadStart(ThreadProcDrawLayers));
            //ThreadDrawLayer.IsBackground = true;
            //ThreadDrawLayer.Start();
            //bStopDrawLayer = false;

            ThreadProcEventHandle ThreadProc = new ThreadProcEventHandle(ThreadProcDrawLayers);
            if (this.IsHandleCreated)
                this.Invoke(ThreadProc);
            //ThreadProcDrawLayers();
        }

        private void DrawLayerRePaint(object sender, System.Timers.ElapsedEventArgs e)
        {
            RePaint();
        }

        //绘制图形函数     
        private void ThreadProcDrawLayers()
        {
            try
            {
                {
                    Graphics g = Graphics.FromImage(m_ImgBackUp);
                    g.Clear(this.BackColor);

                    if (!DrawLayerByType(g, LAYERTYPE_DETAIL.RasterLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.PolygonLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.MixLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.DraftLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.LineLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.PointLayer)
                    || !DrawLayerByType(g, LAYERTYPE_DETAIL.LabelLayer))
                    {
                        if (TimerDrawLayer != null)
                            TimerDrawLayer.Stop();
                        return;
                    }      

                    DrawSltObject(g);
                    if (ShowSurveyPoint)
                        DrawSurveyPts(g);
                    DrawGeoTag(g);
                    RePaint();
                    g.Dispose();
                }

            }
            catch  (Exception e)
            {
                //MessageBox.Show(e.Message, "lyr绘图出问题啦~~~");
            }
            finally
            {
              
            }
        }
        public void SetBackGroundColor()
        {
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = dlg.Color;
                this.Refresh();
            }
        }
        /// <summary>
        /// 在指定画布上绘制，并绘制出捕捉端点。
        /// </summary>
        /// <param name="g"></param>
        public void RePaint(Graphics g)
        {
            try
            {
                g.DrawImage(m_ImgBackUp, 0, 0);
                if (m_SnapPoint != null)
                {
                    Point pt = TransFromWorldToMap(m_SnapPoint);
                    using (Pen pen = new Pen(Color.Red, 1.7f))
                    {
                        g.DrawRectangle(pen, pt.X - SnapPixels, pt.Y - SnapPixels, SnapPixels * 2, SnapPixels * 2);
                        g.DrawLine(pen, pt.X - SnapPixels, pt.Y, pt.X + SnapPixels, pt.Y);
                        g.DrawLine(pen, pt.X, pt.Y - SnapPixels, pt.X, pt.Y + SnapPixels);
                    }
                }
                if (m_SurveyPt != null)
                {
                    Point pt = TransFromWorldToMap(m_SurveyPt);
                    using (Pen pen = new Pen(Color.Red, 1.7f))
                    {   
                        g.DrawLine(pen, pt.X - 50, pt.Y, pt.X + 50, pt.Y);
                        g.DrawLine(pen, pt.X, pt.Y - 50, pt.X, pt.Y + 50);
                    }
                }
            }
            catch (Exception e)
            {
                 MessageBox.Show(e.Message, "repaint出错啦~~");
            }
        }
        public delegate void BaseRefreshEventHandle();
        public void RePaint()
        {
            try
            {
                //lock (Image)
                {
                    Graphics g = Graphics.FromImage(Image);
                    g.Clear(this.BackColor);
                    g.DrawImage(m_ImgBackUp, 0, 0);
                    if (m_SnapPoint != null)
                    {
                        Point pt = TransFromWorldToMap(m_SnapPoint);
                        using (Pen pen = new Pen(Color.Red, 1.7f))
                        {
                            g.DrawRectangle(pen, pt.X - SnapPixels, pt.Y - SnapPixels, SnapPixels * 2, SnapPixels * 2);  //跟踪小方块
                            g.DrawLine(pen, pt.X - SnapPixels, pt.Y, pt.X + SnapPixels, pt.Y);
                            g.DrawLine(pen, pt.X, pt.Y - SnapPixels, pt.X, pt.Y + SnapPixels);
                        }
                    }
                    if (m_SurveyPt != null)
                    {
                        Point pt = TransFromWorldToMap(m_SurveyPt);
                        using (Pen pen = new Pen(Color.Red, 1.7f))
                        {
                            g.DrawLine(pen, pt.X - 50, pt.Y, pt.X + 50, pt.Y);
                            g.DrawLine(pen, pt.X, pt.Y - 50, pt.X, pt.Y + 50);
                        }
                    }
                    g.Dispose();

                    BaseRefreshEventHandle refresh = new BaseRefreshEventHandle(BaseRefresh);

                    if (this.IsHandleCreated)
                        this.Invoke(refresh);
                }
            }
            catch
            {
                MessageBox.Show("Repaint出错啦~~~");
            }
         
        }
        /// <summary>
        /// 改变位图的大小，在屏幕大小发生变化时
        /// </summary>
        private void InitialDrawContext()
        {
            if (Image != null)
            {
                Image.Dispose();
            }
            if (m_ImgBackUp != null)
            {
                m_ImgBackUp.Dispose();
            }
            Image = new Bitmap(Width, Height);
            m_ImgBackUp = new Bitmap(Width, Height);
        }
        /// <summary>
        /// 屏幕大小发生变化时
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InitialDrawContext();
            ComputeStartPtAndBlc();
            Refresh();

        }

        public void BaseRefresh()
        {
            base.Refresh();
        }
        //
        // 摘要:
        //     强制控件使其工作区无效并立即重绘自己和任何子控件。
        public override void Refresh()
        {
            if (m_Map != null)
            {
                GetImage();
               // ThreadProcDrawLayers();
                m_ScaleBound = TransFromMapToWorld(new Rectangle(0, 0, Width, Height));
            }
        }
    
        // 放到每一个图层中  2013.04.27 吴志强
        //public bool m_bShowSymbol = false;
   
        //public bool bShowSymbol
        //{
        //    get { return m_bShowSymbol; }
        //    set { m_bShowSymbol = value; }
        //}
        
        /// <summary>
        /// 调用刷新鹰眼的事件，是否刷新鹰眼,bRefreshAll为True则刷新鹰眼全图
        /// </summary>
        public void EagleMapRefresh(bool bRefreshAll)
        {
            if (EagleMapEvent != null && ShowEagleMap)
            {
                UIEventArgs.EagleMapEventArgs e = new GIS.TreeIndex.UIEventArgs.EagleMapEventArgs(m_ViewExtents, bRefreshAll);
                EagleMapEvent(this, e);
            }
        }
       
    }
}
