using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using GIS.Map;
using GIS.Geometries;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.Layer;
using System.IO;
using System.Threading;
using GIS.Render;
using GIS.Increment;
 
using System.Runtime.InteropServices;
using GIS.GdiAPI;
using GIS.TreeIndex.Index;

namespace GIS.TreeIndex
{
    public partial class MapUI:PictureBox,ITransForm
    {
        public MapUI()
        {      
            ImportFeatureCodes();
            this.DoubleBuffered = true;
            base.MouseDown += new System.Windows.Forms.MouseEventHandler(MapUI_MouseDown);
            base.MouseMove += new System.Windows.Forms.MouseEventHandler(MapUI_MouseMove);
            base.MouseUp += new System.Windows.Forms.MouseEventHandler(MapUI_MouseUp);
            base.MouseWheel += new System.Windows.Forms.MouseEventHandler(MapUI_MouseWheel);
            base.MouseEnter += new EventHandler(MapUI_MouseEnter);
            CreateNewProject(true);

            TimerMouseWheel = new System.Timers.Timer(350);
            TimerMouseWheel.Elapsed += new System.Timers.ElapsedEventHandler(DrawMouseWheel);
            TimerMouseWheel.AutoReset = false;

            //if (m_conv_gtr == null)
            //    m_conv_gtr = new GIS.mm_Conv_Symbol.mm_conv_geometry();符号初始化过程放到mainform初始化

            InitializationSymbol();

        }

        #region PrivateMember
        private System.Timers.Timer TimerMouseWheel;
        private System.Timers.Timer TimerDrawLayer;
        private GeoBound m_ViewExtents ;             //地图控件中显示的地图的地理坐标范围
        private GeoMap m_Map;                        //地图控件中的地图对象       
        private double m_dBlc = 1;                   //缩放比例尺 
        private MapTool m_Tool;                      //绘图工具    
        internal MapTool m_EditToolBack;             //备份工具，用于编辑
        private MapTool m_TempTool;                  //备用工具，例如鼠标中键拖动屏幕
        private Point m_MouseDragPt;                 //鼠标拖拽前按下的屏幕点坐标  
        private bool m_MouseDowning;                 //是否按下左键   
        private bool m_IsCtrlPressed;                //是否按下ctrl
        internal Image m_ImgBackUp;                  //已经绘制好内存中的位图,用于编辑绘制                  
        internal GeoPoint m_SnapPoint = null;        //当前屏幕的捕捉点地理坐标
        internal GeoPoint3D m_SurveyPt = null;       //
        internal double m_direction = 0.0f;          //gps点的方向
        internal IncManager m_IncManager;            //增量信息管理器
        internal OprtRollBack.OprtManager m_OprtManager;//操作回退管理器
        private static Dictionary<long, string> m_FeatureCodes;//地物分类要素编码   

        public bool VecSymbolize = false;
        public Dictionary<string, Color> dic = new Dictionary<string, Color>();
        public string symField = "PlgAttr";

        private QuadTreeIndex m_QuadIndex = null;

        public static Dictionary<long, string> FeatureCodes
        {
            get { return MapUI.m_FeatureCodes; }            
        }
        public static Int64 FeatID  =  50000 ;        //初始化的FEATID\
        public static Int64 ClasID  = 988988;
        public static string BeginTime  ;             //初始化的开始时间
      
        #endregion

        #region Properties

        public QuadTreeIndex QuadIndex
        {
            get
            {
                return m_QuadIndex;
            }
            set
            {
                m_QuadIndex = value;
            }
        }
        /// <summary>
        /// 显示为图纸坐标
        /// </summary>
        public bool ShowAsPaper
        {
            get
            {
                return m_Map.ShowAsPaper;
            }
            set
            {
                m_Map.ShowAsPaper = value;
            }
        }

     
       
        public bool MouseDowning
        {
            get { return m_MouseDowning; }
            set { m_MouseDowning = value; }
            
        }

        public Point MouseDragPt
        {
            get { return m_MouseDragPt; }
        }
   
        /// <summary>
        /// 获取显示比例尺，屏幕一像素 = 实际距离
        /// </summary>
        public double DBlc
        {
            get { return m_dBlc; }
            set
            {
                if (value > 0.0000005)//加了三个0
                {
                    m_dBlc = value;
                    GIS.SpatialRelation.GeoAlgorithm.Tolerance = value * SnapPixels;
                    GeoPoint pt = GetCenterGeoPoint();
                    if (pt != null)
                        SetViewExtents(pt.X - Width * m_dBlc / 2, pt.Y - Height * m_dBlc / 2,
                           pt.X + Width * m_dBlc / 2, pt.Y + Height * m_dBlc / 2);
                }
            }
        }
        /// <summary>
        /// 获取该地图控件的屏幕尺寸
        /// </summary>
        public new Size Size
        {
            get{ return base.Size; }
            set 
            { 
                base.Size = value;                 
            }
        } 
        #endregion     

        #region ViewFunction
        //新建工程
        public bool CreateNewProject(bool initial)
        {
            if (!initial)
            {
                if (MessageBox.Show("确定重新出创建新工程？", "提示!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
                    != DialogResult.OK)
                    return false;
            }
            m_Map = new GeoMap();
            m_Map.MapBoundChange += new GeoMap.MapBoundChangeEventHandler(EagleMapRefresh);
            m_Map.LayerIncrease += new GeoMap.LayerIncreaseEventHandle(LayerIncreaseHandle);
            m_Map.LayerDecrease += new GeoMap.LayerDecreaseEventHandle(LayerDecreaseHandle);
            m_Map.LayerGroupDecrease += new GeoMap.LayerGroupDecreaseEventHandle(LayerGroupDecreaseHandle);
            m_Map.LayerGroupIncrease += new GeoMap.LayerGroupIncreaseEventHandle(LayerGroupIncreaseHandle);
            MapTool = new MapPointSelectTool(this);
            m_IncManager = new IncManager(m_Map);
            m_OprtManager = new GIS.TreeIndex.OprtRollBack.OprtManager();
            m_EditToolBack = null;
            m_ImgBackUp = null;
            m_dBlc = 1;
            m_SnapPoint = null;
            m_SnapAngle = 90;//捕捉角度
            m_AngleToler = 3;//角度容差
            m_SnapPixels = 8;               //屏幕捕捉的像素大小 
            m_ViewExtents = new GeoBound(0, 0, 100, 100);
            InitialDrawContext();
            ComputeStartPtAndBlc();
            Refresh();
            return true;

        }
        /// <summary>
        /// 调用输出文本信息的事件
        /// </summary>
        /// <param name="strText"></param>
        public void OutPutTextInfo(string strText)
        {
            if (OutputText != null)
            {
                UIEventArgs.OutPutEventArgs e = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs(strText);
                OutputText(this, e);
            }
        }

        public void OutPutNumberModify(string strText)
        {
            if (OutPutNumber != null)
            {
                UIEventArgs.OutPutEventArgs e = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs(strText);
                OutPutNumber(this, e);
            }
        }

        public GeoBound GetMapBound()
        {
            return m_Map.MapBound;
        }
        /// <summary>
        /// 获取屏幕视图范围坐标
        /// </summary>
        /// <returns></returns>
        public GeoBound GetViewExtents()
        {
            return m_ViewExtents;
        }
        //设置视图范围，同时触发鹰眼
        public void SetViewExtents(GeoBound bound)
        {
            if (!m_ViewExtents.isEqual(bound))
            {
                m_ViewExtents.SetExtents(bound);
                EagleMapRefresh(false);
            }
        }
        public void SetViewExtents(double l, double b, double r, double t)
        {
            GeoBound bound = new GeoBound(l, b, r, t);
            SetViewExtents(bound);
        }
        //获取屏幕的中心点的地理坐标
        public GeoPoint GetCenterGeoPoint()
        {
            return m_ViewExtents.GetCentroid();
        }
        /// <summary>
        /// 在设置中心点的地理坐标
        /// </summary>
        /// <param name="pt"></param>
        public void SetCenterGeoPoint(GeoPoint pt)
        {
            if (pt != null)
            {
                if (m_ViewExtents == null)
                    m_ViewExtents = new GeoBound();
                
                SetViewExtents(pt.X - Size.Width * m_dBlc / 2, pt.Y - Size.Height * m_dBlc / 2,
                  pt.X + Size.Width * m_dBlc / 2, pt.Y + Size.Height * m_dBlc / 2);
            }
        }
        //
        // 摘要:
        //     将地图初始化至全屏状态。
        public void ComputeStartPtAndBlc()
        {
            ZoomToBox(m_Map.MapBound);
        }
      
        //
        // 摘要:
        //     全屏显示完整地图内容，并刷新控件。
        public void ZoomToFullExtent()
        {
            ComputeStartPtAndBlc();           
            this.Refresh();
            EagleMapRefresh(true);
        }

        /// <summary>
        /// 将当前视图地理范围设置为BOX地理范围
        /// </summary>
        public void ZoomToBox(GeoBound box)
        {
            if (box != null)
            {
                if (box.Left == box.Right || box.Top == box.Bottom)
                {
                    SetCenterGeoPoint(new GeoPoint(box.Left, box.Bottom));
                }
                else
                {
                    double cx = box.Width;
                    double cy = box.Height;
                    double blcH, blcV;
                    blcH = cx / (Size.Width);
                    blcV = cy / (Size.Height);
                    blcH = blcH > blcV ? blcH : blcV;

                    //if (blcH > 0.0005) //如果比例尺小于0.05 就按照0.5进行视图范围设置
                    //{
                    SetViewExtents(box);
                    DBlc = blcH;

                    //}
                    //else
                    //{
                    //    DBlc = 0.0005;
                    SetCenterGeoPoint(box.GetCentroid());
                    //}
                }
            }

        }       
         
        #endregion

        #region KeyDownHandle
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)//如果是ESC键，则将所有选中的反选
            {
                m_Map.ClearAllSelect();
                Refresh();
            }
            else if (e.KeyCode == Keys.F8)
            {
                RecSurveyPoint();
            }
            else
            {
                m_IsCtrlPressed = e.Control;
                MapTool.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            m_IsCtrlPressed = e.Control;
           
        } 
        #endregion

        #region MouseEventHandler
        /// <summary>
        /// 鼠标按下的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MapUI_MouseDown(object sender, MouseEventArgs e)
        {
            m_MouseDragPt = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint ptBack = m_SnapPoint;
                GeoPoint pt = TransFromMapToWorld(e.Location);
                MouseCatch(pt);
                m_SurveyPt = (m_SnapPoint != null) ? new GeoPoint3D(m_SnapPoint) :new GeoPoint3D( pt);
                RePaint();
                m_SnapPoint = ptBack;
            }
            if (e.Button == MouseButtons.Middle)
            {
                m_TempTool = m_Tool;
                MapTool = new MapPanTool(this);
            } 
            MapTool.OnMouseDown(sender, e);
            m_MouseDowning = true;// 放在Tool 后面是因为Tool可能不执行下一步  
        }

        private void MapUI_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMove != null)
            {
                GeoPoint pt = TransFromMapToWorld(e.Location);
                MouseMove(pt, e);
            }
              
            MapTool.OnMouseMove(sender, e);
        }

        private void MapUI_MouseUp(object sender, MouseEventArgs e)
        {           
            MapTool.OnMouseUp(sender, e);
            if (e.Button == MouseButtons.Middle)
            {
                MapTool = m_TempTool;
                m_TempTool = null; 
            }
            m_MouseDowning = false;
        }
       
        private  void DrawMouseWheel(object sender, System.Timers.ElapsedEventArgs e)
        {
            Refresh();
        } 

        private TimeSpan m_TsWheelBefore = new TimeSpan(DateTime.Now.Ticks);
        private GeoBound m_ScaleBound ;

        private void MapUI_MouseWheel(object sender, MouseEventArgs e)
        {
            double scale = (e.Delta / 120.0);
            double scaleBase = m_IsCtrlPressed ? 1.02 : 1.2; 
            scaleBase = m_dBlc * Math.Pow(scaleBase, scale);

            if (scaleBase > 0.0000005)
            {
                DBlc = scaleBase;

                ///超过0.6秒 就全部画，没超过则不画
                TimeSpan Ts = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts = Ts.Subtract(m_TsWheelBefore).Duration();
                if (ts.TotalMilliseconds < 350)
                {
                    TimerMouseWheel.Stop();
                }
                else
                    m_TsWheelBefore = Ts;

                TimerMouseWheel.Start();

                try
                {
                    Graphics g = Graphics.FromImage(Image);
                    g.Clear(this.BackColor);
                    Rectangle rect = TransFromWorldToMap(m_ScaleBound);
                    g.DrawImage(m_ImgBackUp, rect);
                    base.Refresh();
                }
                catch(Exception k)
                {
                    MessageBox.Show(k.Message, "mousewheel出问题啦!!!");
                }
            }
        }
 
        private void MapUI_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }
        #endregion

        #region Delegates
        public delegate void MouseEventHandler(GeoPoint WorldPos, MouseEventArgs e);
        public delegate void EagleMapRefreshEventHandler(object sender, UIEventArgs.EagleMapEventArgs e);
        public delegate void OutputTextEventHandler(object sender, UIEventArgs.OutPutEventArgs e);
        public delegate void OutputNumberEventHandler(object sender, UIEventArgs.OutPutEventArgs e);

        public new event MouseEventHandler MouseMove;
        /// <summary>
        /// 和鹰眼视图同步
        /// </summary>
        public event EagleMapRefreshEventHandler EagleMapEvent;
        /// <summary>
        /// 向文本框中输出提示信息
        /// </summary>
        public event OutputTextEventHandler OutputText;
        public event OutputNumberEventHandler OutPutNumber;
        //public new event MouseEventHandler MouseDown;
        //public new event MouseEventHandler MouseUp;
        #endregion
    }
}
