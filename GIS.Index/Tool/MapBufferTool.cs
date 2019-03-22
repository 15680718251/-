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
    public class MapBufferTool : MapAddLineStringTool
    {
        public double radius;        //设定缓冲区距离的窗口和设定条件的窗口分开 --
        public string InputCoords;   //缓冲区输入坐标串
        public string OutputCoords;  //计算得到的缓冲区边界坐标串
        //protected List<GeoPoint> m_BufferPtList;//缓冲区边界骨架点
        BufferQueryForm form;        //缓冲区剔除条件设定窗口

        public GIS.Geometries.Geometry srcGeometry;
        //public GIS.Geometries.Geometry bufGeometry;
        public OSGeo.OGR.Geometry bufGeometry;

        protected MapBufferTool(){}

        public MapBufferTool(MapUI ui) : base(ui)
        {
            radius = 0;
            //m_BufferPtList = new List<GeoPoint>();
        }

        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            GeoLayer gl = m_MapUI.GetActiveVectorLayer();
            if (gl == null)
            {
                MessageBox.Show("当前没有活动图层","提示");
                return;
            }
            if (gl.LayerTypeDetail == LAYERTYPE_DETAIL.PointLayer || gl.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer
                || gl.LayerTypeDetail == LAYERTYPE_DETAIL.PolygonLayer)
            {
                if (e.Button == MouseButtons.Left)
                {
                    base.OnMouseDown(sender, e);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (m_PtList.Count < 2)
                    {
                        MessageBox.Show("线段端点数不够", "提示");
                        return;
                    }
                    srcGeometry = new GIS.Geometries.GeoLineString(m_PtList);
                    BufferRemove(); //缓冲区剔除
                    m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                }
            }
            else
            {
                MessageBox.Show("当前活动图层不符合", "提示");
                return;
            }
        }

        public void BufferRemove()
        {
            form = new BufferQueryForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    radius = Convert.ToDouble(form.distance);
                    if (radius <= 0)
                    {
                        MessageBox.Show("缓冲半径输入错误", "提示");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("缓冲半径输入错误", "提示");
                    return;
                }
            }
            else
                return;

            //CalPolylineBuffer();

            //bufGeometry = 
            GetBufferFromGeometry(srcGeometry, radius);
            if (bufGeometry == null)
            {
                m_MapUI.ClearAllSlt();
                return;
            }
            
            DrawBuffer();  //这里将Cancel函数放在第一行，也可以将缓冲区的坐标点写入图层？原因？
            m_MapUI.OutPutTextInfo("缓冲区剔除开始：\r\n"); //m_MapUI 的值就是ui;
            BufferQuery();                               //缓冲区查询---可弹出一对话框，显示查询到的要素
            m_MapUI.OutPutTextInfo("缓冲区查询结束：\r\n");

            Cancel();
        }

        public override void Cancel()
        {
            try
            {
                if (m_CurEditType == EditType.AddOnePoint)
                {
                    GeoVectorLayer actLayer = m_MapUI.GetDraftMixLayer() as GeoVectorLayer;

                    if(actLayer!=null&&actLayer.VectorType==VectorLayerType.DraftLayer)
                    {
                        //if (m_PtList.Count >= 2)
                        //{
                            //m_Geom = actLayer.AddGeometry(new GeoLineString(m_PtList));
                            //m_Geom = actLayer.AddGeometry(new GeoLineString(m_BufferPtList));
                            //m_MapUI.OutPutTextInfo("画缓冲区直线成功\r\n");
                            //m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                            //m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                            
                        //}
                        //else
                        //{
                        //    m_MapUI.OutPutTextInfo("提示：  画线失败，退出画线命令\r\n");
                        //    m_MapUI.OutPutTextInfo("提示：  构线点数少于2个，无法完成线的绘制\r\n");
                        //}
                    }
                }
                else
                    base.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        //重写计算缓冲的方法
        public void GetBufferFromGeometry(GIS.Geometries.Geometry g,double r)
        {
            //GIS.Geometries.Geometry gr = null;
            try
            {
                string wkt = GIS.Converters.WellKnownText.GeometryToWKT.Write(g);
                OSGeo.OGR.Geometry og = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                OSGeo.OGR.wkbGeometryType gtype = og.GetGeometryType();

                bufGeometry = og.Buffer(r, 30);  //缓冲
                //OSGeo.OGR.wkbGeometryType gbtype = gb.GetGeometryType();
                //string rwkt;
                //gb.ExportToWkt(out rwkt);
                //gr = GIS.Converters.WellKnownText.GeometryFromWKT.Parse(rwkt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"提示");
                bufGeometry = null;
            }
        }

        public void DrawBuffer()
        {
            //Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
            //Graphics g = Graphics.FromImage(imgTemp);
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //GIS.Style.VectorStyle styleSelect = new GIS.Style.VectorStyle(2, 2, Color.Blue, Color.Blue, true);
            //GIS.Render.RenderAPI.DrawGeometry(g, bufGeometry, styleSelect, m_MapUI);

            //m_MapUI.RePaint(g);
            //m_MapUI.Image.Dispose();
            //m_MapUI.Image = imgTemp;
            //g.Dispose();
            //m_MapUI.BaseRefresh();
        }

        public void BufferQuery()
        {
            List<GeoData.GeoDataRow> rows = m_MapUI.SelectByBuffer(bufGeometry, form.TpType);
            //if (rows.Count > 0)
            //{
            //    int n = rows.Count;
            //    for (int i = 0; i < n; i++)
            //    {
            //         rows[i]["ChangeType"] = EditState.Disappear;
            //    }
            //}

            System.Threading.Thread.Sleep(500);
            m_MapUI.ToolDeleteObj();
            m_MapUI.MapTool = new MapPointSelectTool(m_MapUI);
        }

        # region 废弃

        //public void CalPolylineBuffer()
        //{
        //    Point[] pts = null;
        //    pts = new Point[m_PtList.Count];
        //    m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);
        //    for (int i = 0; i < m_PtList.Count; i++)
        //    {
        //        if (i != (m_PtList.Count - 1))
        //            InputCoords += pts[i].X + "," + pts[i].Y + ";";
        //        else
        //            InputCoords += pts[i].X + "," + pts[i].Y;
        //    }
        //    OutputCoords = PolylineBuffer.GetBufferEdgeCoords(InputCoords, radius);
        //}
        //public bool DrawBuffer()
        //{
            //if (OutputCoords.Trim().Length < 1)
            //    return false;

            //string[] strCoords = OutputCoords.Split(new char[] { ';' });
            //List<Coordinate> coords = new List<Coordinate>();
            //foreach (string coord in strCoords)
            //{
            //    coords.Add(new Coordinate(coord));
            //}

            //Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
            //Graphics g = Graphics.FromImage(imgTemp);
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //m_MapUI.RePaint(g);

            //Point[] pts1 = null;
            //pts1 = new Point[coords.Count];
            //int j = 0;
            //foreach (Coordinate coord in coords)
            //{
            //    pts1[j].X = (int)coord.X;
            //    pts1[j].Y = (int)coord.Y;
            //    GeoPoint pt = m_MapUI.TransFromMapToWorld(pts1[j]);
            //    m_BufferPtList.Add(pt.Clone() as GeoPoint);   //缓冲区坐标存入m_BufferPtList
            //    j++;
            //}
            //Pen pen = new Pen(Color.MediumVioletRed, 1f);                //画笔颜色  
            //g.DrawLines(pen, pts1);
            //pen.Dispose();

        //    m_MapUI.Image.Dispose();
        //    m_MapUI.Image = imgTemp;
        //    g.Dispose();
        //    m_MapUI.BaseRefresh();
        //    return true;
        //}

        //public void BufferQuery()
        //{
            //if (m_BufferPtList.Count > 3)
            //{
            //    //默认是否闭合
            //    if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(m_BufferPtList[0], m_BufferPtList[m_BufferPtList.Count - 1]))
            //    {
            //        GeoLinearRing ring = new GeoLinearRing(m_BufferPtList);
            //        List<GeoData.GeoDataRow> rows = m_MapUI.SelectByBuffer(new GeoPolygon(ring), ref form);
            //        if (rows.Count > 0)
            //        {
            //            m_MapUI.Refresh();
            //        }
            //    }
            //}

        //}
        #endregion
    }
}