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
namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        #region PrivateMember
        private double m_SnapAngle = 90;//捕捉角度
        private double m_AngleToler = 3;//角度容差
        private int m_SnapPixels = 8;               //屏幕捕捉的像素大小 
        private MouseCatchType m_CatchType = MouseCatchType.Both; 
        private bool m_ShowSurveyPoint = true;
        private bool m_ShowOnlineNotes = false;
        private bool m_LineAngleSnapEnable = true; 
        private bool m_ShowEagleMap = true; 
        private int m_FileSaveInterval = 5;
        private SelectType m_SelectType = SelectType.All;
        private System.Drawing.Drawing2D.SmoothingMode m_SmoothingMode= System.Drawing.Drawing2D.SmoothingMode.Default;


        #endregion

        #region Properties
        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get { return m_SmoothingMode; }
            set { m_SmoothingMode = value; }
        }
        public bool ShowEagleMap
        {
            get { return m_ShowEagleMap; }
            set { m_ShowEagleMap = value; }
        }
        public bool LineAngleSnapEnable
        {
            get { return m_LineAngleSnapEnable; }
            set { m_LineAngleSnapEnable = value; }
        }
        public SelectType SelectType
        {
            get { return m_SelectType; }
            set { m_SelectType = value; }
        }
        public int FileSaveInterval
        {
            get { return m_FileSaveInterval; }
            set { m_FileSaveInterval = value; }
        }
        public bool ShowOnlineNotes
        {
            get { return m_ShowOnlineNotes; }
            set { m_ShowOnlineNotes = value; }
        }
        public MouseCatchType SnapType
        {
            get { return m_CatchType; }
            set { m_CatchType = value; }
        }
        public bool ShowSurveyPoint
        {
            get { return m_ShowSurveyPoint; }
            set { m_ShowSurveyPoint = value; }
        }
        public double AngleToler
        {
            get { return m_AngleToler; }
            set { m_AngleToler = value; }
        }
        public double SnapAngle
        {
            get { return m_SnapAngle; }
            set { m_SnapAngle = value; }
        }
        //屏幕捕捉的像素大小
        public int SnapPixels
        {
            get { return m_SnapPixels; }
            set { m_SnapPixels = value; }
        } 
        #endregion
        

        public void SetLayerParamater(string lyrName)
        {
            Forms.LayerSettingForm form = new LayerSettingForm(this, lyrName);
            form.ShowDialog();
        }

        public void DrawLineSetting()
        {
            DrawLineSettringForm form = new DrawLineSettringForm(this);
            form.ShowDialog();
        }
        public void SystemSetting()
        {
            SysParameterForm form = new SysParameterForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                m_CatchType = form.CatchType;
                m_FileSaveInterval = form.FileSaveInterval;
                m_SnapPixels = form.VertexSnapTolerance;
                m_ShowSurveyPoint = form.ShowSurveyPt;
                GetLayerByName("测点层").Enable = m_ShowSurveyPoint;
                m_ShowOnlineNotes = form.ShowOnlineNotes;
                BackColor = form.SysBackColor;
                SmoothingMode = form.SmoothingMode;
                Refresh();
            }

        }

    }
}
