using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using System.Runtime.InteropServices;
using GIS.SpatialRelation;
 
namespace GIS.TreeIndex.Tool
{

    public partial class MapGenerateLineTool : MapTool
    {
        internal string strLength;
        public void FinishCommandByNumber(double len)
        {
            strLength = null;
            if (m_CurEditType == EditType.Extend)
            {
                Point ScrPt = m_MapUI.PointToClient(Cursor.Position);
                GeoPoint pt1 = m_PtList[m_PtList.Count - 1];
                GeoPoint pt2 = (m_MapUI.m_SnapPoint == null) ?
                m_MapUI.TransFromMapToWorld( ScrPt) : m_MapUI.m_SnapPoint;//点2 是否为 捕捉到的点
                GeoPoint ptNew = GeoAlgorithm.ExtentLine(pt1, pt2, len);
               
                m_PtList.Add(ptNew); 
                RecDataRollBack(m_PtList.Count - 1, 1);//记录单点的操作回退记录
                m_MapUI.OutPutTextInfo(strLevelOne);
                m_CurEditType = EditType.AddOnePoint;
                m_KeyType = KeyType.ALL;
            }
            else if (m_CurEditType == EditType.Turn)
            {
                m_TDistList.Add(len);
                if (m_TDistList.Count < 2)
                {
                    m_MapUI.OutPutTextInfo("提示：请再输入拐长纵向距离\r\n");
                }
                else
                {
                    m_MapUI.OutPutTextInfo("提示：请选择拐线方向！\r\n");
                    m_KeyType = KeyType.MOUSEDOWN_ONLY;
                }
            }
        
            
        }
        //处理输入数字的情况
        public void NumberHandle(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            { 
                IDataObject dataobj = Clipboard.GetDataObject();
                if (dataobj == null) return;
                object obj = dataobj.GetData(DataFormats.Text);
                if (obj == null) return;
                string numbInClipbord = obj.ToString();

                double numb;
                if (double.TryParse(numbInClipbord, out numb))
                {
                    m_MapUI.OutPutTextInfo(numbInClipbord);
                    strLength += numbInClipbord;

                }
            }
            if (e.KeyValue >= 48 && e.KeyValue <= 57) //输入的是数字
            { 
                string temp = ((char)e.KeyValue).ToString();
                strLength += temp;
                m_MapUI.OutPutTextInfo(temp );  
            }
            else if (e.KeyValue == 190)   //输入的是点号
            { 
                strLength += ".";
                m_MapUI.OutPutTextInfo(".");  
            }
            else if (e.KeyValue == 13)      //输入的是回车
            {
                m_MapUI.OutPutTextInfo("\r\n");
                double len;
                if (double.TryParse(strLength, out len))
                {
                    FinishCommandByNumber(len);
                }
                else
                {                   
                    strLength = null;
                    MessageBox.Show("数字格式不正确，请重新输入");
                    return;
                } 
                return;

            }
            else if (e.KeyValue == 8)//输入的是退格
            {
                strLength = strLength.Remove(strLength.Length - 1);
                m_MapUI.OutPutNumberModify(null);
                return;
            }
            else 
                return;         
         
        }
        

        //处理输入字母命令的情况
        public void CommandHandle(KeyEventArgs e)
        {
            if (e.KeyValue >= 48 && e.KeyValue <= 57)
                return;

            m_MapUI.OutPutTextInfo(e.KeyData.ToString() + "\r\n");
            if (e.KeyCode == Keys.A)              //画弧线命令
            {
                m_CurEditType = EditType.ThreePointArc;
                m_MapUI.OutPutTextInfo("三点画弧，指定下一点：\r\n");
                m_PtTemp.Add(m_PtList[m_PtList.Count - 1]);
                m_KeyType = KeyType.MOUSEDOWN_ONLY;              
            }
            else if (e.KeyCode == Keys.C)        //闭合命令
            {
                if (m_PtList.Count < 3)
                {
                    m_MapUI.OutPutTextInfo("线段点数少于3个，无法闭合：\r\n");
                }
                else
                {
                    if (!m_PtList[0].IsEqual(m_PtList[m_PtList.Count - 1]))
                    {
                        m_PtList.Add((GeoPoint)m_PtList[0].Clone());
                    }                   
                    m_MapUI.OutPutTextInfo("线段闭合，画线工具结束：\r\n");
                    m_MapUI.MapTool.Cancel();
                    m_MapUI.Refresh();
                   
                }
            }
            else if (e.KeyCode == Keys.E)       //延长命令
            {
                m_MapUI.OutPutTextInfo("输入线长[单位米]：\r\n");
                m_CurEditType = EditType.Extend;
                m_KeyType = KeyType.NUMBER_ONLY;
            }
            else if (e.KeyCode == Keys.X)
            {
                m_MapUI.OutPutTextInfo("提示：相交至线体，请选择要相交至的线\r\n");
                m_CurEditType = EditType.Cross;
                m_KeyType = KeyType.MOUSEDOWN_ONLY;
                
            }
            else if (e.KeyCode == Keys.V)
            {
                if (m_PtList.Count > 0)
                {
                    m_MapUI.OutPutTextInfo("请将鼠标垂足所在直线的方向！\r\n");
                    m_CurEditType = EditType.Vertical;
                    m_KeyType = KeyType.MOUSEDOWN_ONLY;
                }
                else
                    m_MapUI.OutPutTextInfo("已有点数少于1个，请添加起始点后再执行垂线命令！\r\n");
            }
            else if (e.KeyCode == Keys.T)
            {
                if (m_PtList.Count >= 2)
                {
                    m_TDistList.Clear();
                    m_MapUI.OutPutTextInfo("提示：请输入拐长横向距离,或者指定第一个目标点!\r\n");
                    m_CurEditType = EditType.Turn;
                    m_KeyType = KeyType.MOUSE_NUMBER;
                }
                else
                    m_MapUI.OutPutTextInfo("已有点数少于2个，请添加起始点后再执行拐线命令！\r\n");
            }
            else if (e.KeyCode == Keys.J)
            {
                if (m_PtList.Count != 3)
                {
                    m_MapUI.OutPutTextInfo("提示：构成三点矩形需要三个点，多了或者少了都不行的~\r\n");
                }
                else
                {
                    m_PtList = GeoAlgorithm.ThreePointRect(m_PtList);
                    m_MapUI.OutPutTextInfo("线段闭合，画线工具结束：\r\n");
                    m_MapUI.MapTool.Cancel();
                    m_MapUI.Refresh();
                    //base.Cancel();
                }
            }
            else if (e.KeyCode == Keys.U)
            {
                m_KeyType = KeyType.ALL;
                if (m_RollBackList.Count == 0)
                {
                    m_MapUI.OutPutTextInfo("已经回退至顶\r\n");
                    return;
                }
                RollBack bk = m_RollBackList[m_RollBackList.Count - 1];
                m_PtList.RemoveRange(bk.begin, bk.count);
                m_RollBackList.Remove(bk);
                m_MapUI.Refresh();
                m_MapUI.OutPutTextInfo(strLevelOne);
            }
            else
            {
                m_MapUI.OutPutTextInfo("无效命令\r\n");
                m_MapUI.OutPutTextInfo(this.strLevelOne);
            }
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            switch (m_KeyType)
            {
                case KeyType.ALL:
                case KeyType.COMMAND_ONLY:
                    CommandHandle(e);
                    break;
                case KeyType.MOUSE_NUMBER:
                case KeyType.NUMBER_ONLY:
                    NumberHandle(e);
                    break;

            }   
        }
    }
}
