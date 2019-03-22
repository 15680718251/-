using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using System.Runtime.InteropServices;
using GIS.SpatialRelation ;
//using System.Threading;
//using System.Timers;

namespace GIS.TreeIndex.Tool
{
    public partial class MapGenerateLineTool : MapTool
    {
        #region WIN32DLL
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("Kernel32.dll", EntryPoint = "GetTickCount", CharSet = CharSet.Auto)]
        internal static extern int GetTickCount();
        public enum MouseEventFlags
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Wheel = 0x0800,
            Absolute = 0x8000
        } 
        #endregion
        protected string strLevelOne = "指定下一个点或 三点圆弧(A)/闭合(C)/延长(E)/做垂线(V)/拐线(T)/相交至(X)/三点矩形(J)/放弃(U) ：\r\n";
        public struct RollBack
        {
            public RollBack(int b, int c)
            {
                begin = b;
                count = c;
            }
            public int begin;
            public int count;
        }

        public enum EditType
        {
            AddOnePoint,      //添加单点
            ThreePointArc,    //三点弧
            Extend ,          //延长、
            Vertical,         //做垂线
            Turn,             //做拐线
            Cross //相交
          
        }
        public enum KeyType
        {
            MOUSEDOWN_ONLY,   //只能输入鼠标
            COMMAND_ONLY, //只能输入命令字母
            NUMBER_ONLY,  //只能输入数字
            MOUSE_NUMBER, //只能数字或者鼠标
            ALL           //全部都可以

        }
 
        protected MapGenerateLineTool()
        {
        }

        public MapGenerateLineTool(MapUI ui) : base(ui)
        {
            m_Cursor = Cursors.Cross;            
            m_PtList = new List<GeoPoint>();
            m_PtTemp = new List<GeoPoint>(); 
            m_RollBackList = new List<RollBack>(); 
        }

        protected List<GeoPoint> m_PtList;      //最终点
        protected List<GeoPoint> m_PtTemp;      //临时骨架点
        protected KeyType m_KeyType = KeyType.MOUSEDOWN_ONLY;      //键盘类型
        protected EditType m_CurEditType = EditType.AddOnePoint; //当前画线模式为添加单点        
        protected List<RollBack> m_RollBackList;       //回退记录

        public override void Cancel()
        {
            if (m_CurEditType == EditType.Turn)
            {
                m_CurEditType = EditType.AddOnePoint;
                m_KeyType = KeyType.ALL;
                m_TDistList.Clear();
                m_MapUI.OutPutTextInfo("提示：拐线功能结束。\r\n");
                m_MapUI.OutPutTextInfo(strLevelOne);
            }
            else if (m_CurEditType == EditType.Cross)
            {
                m_CurEditType = EditType.AddOnePoint;
                m_KeyType = KeyType.ALL; 
                m_MapUI.OutPutTextInfo("提示：相交功能结束。\r\n");
                m_MapUI.OutPutTextInfo(strLevelOne);
            }
            else if (m_CurEditType == EditType.Extend)
            {
                m_CurEditType = EditType.AddOnePoint;
                m_KeyType = KeyType.ALL;
                m_MapUI.OutPutTextInfo("提示：线延长功能结束。\r\n");
                m_MapUI.OutPutTextInfo(strLevelOne);

            }
            else if (m_CurEditType == EditType.Vertical)
            {
                m_CurEditType = EditType.AddOnePoint;
                m_KeyType = KeyType.ALL;
                m_MapUI.OutPutTextInfo("提示：做垂线功能结束。\r\n");
                m_MapUI.OutPutTextInfo(strLevelOne);

            }
            else
                base.Cancel();
        }

        public override void initial()
        {
            m_Cursor = Cursors.Cross;
            m_PtList = new List<GeoPoint>();
            m_PtTemp.Clear();
            m_KeyType = KeyType.MOUSEDOWN_ONLY;
            m_CurEditType = EditType.AddOnePoint;
            m_RollBackList.Clear();
        }

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m_KeyType == KeyType.COMMAND_ONLY|| m_KeyType == KeyType.NUMBER_ONLY)
                return;

            if (m_PtList.Count == 0)
            {
                m_MapUI.OutPutTextInfo(strLevelOne);
                m_KeyType = KeyType.ALL;
            }
            GeoPoint curPt = m_MapUI.TransFromMapToWorld(e.Location);
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                                                         curPt : m_MapUI.m_SnapPoint;

            switch (m_CurEditType)
            {
                case EditType.AddOnePoint:
                    {
                        m_PtList.Add(pt.Clone() as GeoPoint);
                        RecDataRollBack(m_PtList.Count-1,1);//记录单点的操作回退记录
                    }
                    return;
                case EditType.ThreePointArc:
                    {
                        GenerateArc(pt.Clone() as GeoPoint);
                    }
                    return;
                case EditType.Vertical:
                    {
                        GenerateVertical(curPt);
                        return;
                    }
                case EditType.Turn:
                    {
                        GenerateTurnCommand(curPt);
                        return;
                    }
                case EditType.Cross:
                    {
                        GenerateCrossPt(curPt);
                        return;
                    }
                default:
                    return;
            }
        }
    
        protected void RecDataRollBack(int begin, int count)
        {
            RollBack rb = new RollBack(begin, count);
            m_RollBackList.Add(rb);//记录单点的操作回退记录
        }
    }
}
