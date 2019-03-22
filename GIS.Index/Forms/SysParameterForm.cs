using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class SysParameterForm : Form
    {
        public SysParameterForm(MapUI ui)
        {
            InitializeComponent();
            m_MapUI = ui;
            initial();
        }
        private void initial()
        {
            m_BackColor = m_MapUI.BackColor;
            m_VertexSnapTolerance = m_MapUI.SnapPixels;
            m_bShowSurveyPt = m_MapUI.ShowSurveyPoint;
            m_CatchType = m_MapUI.SnapType;
            m_bShowOnlineNotes = m_MapUI.ShowOnlineNotes;
            m_FileSaveInterval = m_MapUI.FileSaveInterval;

            checkBoxShowSurveyPt.Checked = m_bShowSurveyPt;
            checkBoxShowOnlineNotes.Checked = m_bShowOnlineNotes;
            button1.BackColor = m_BackColor;
            numericUpDownMouseCatchToler.Value = (decimal)m_VertexSnapTolerance;
      
            numericUpCheckedDown1.Value = (decimal)m_FileSaveInterval;
            if (m_CatchType == MouseCatchType.Both)
            {
                checkBoxSnapCenter.Checked = true;
                checkBoxSnapVertex.Checked = true;
            }
            else if (m_CatchType == MouseCatchType.Vertex)
                checkBoxSnapVertex.Checked = true;
            else if (m_CatchType == MouseCatchType.Center)
                checkBoxSnapCenter.Checked = true;



            if (m_MapUI.SmoothingMode == System.Drawing.Drawing2D.SmoothingMode.AntiAlias)
            {
               trackBarRenderMode.Value = 3;
            }
            else if (m_MapUI.SmoothingMode == System.Drawing.Drawing2D.SmoothingMode.HighSpeed)
            {
                trackBarRenderMode.Value = 1;
            }
            else if (m_MapUI.SmoothingMode == System.Drawing.Drawing2D.SmoothingMode.Default)
            {
                trackBarRenderMode.Value = 2;
            }

            this.中比例尺.Checked = true;
        }
        private MapUI m_MapUI;

        private int m_FileSaveInterval;

        public int FileSaveInterval
        {
            get { return m_FileSaveInterval; }
            set { m_FileSaveInterval = value; }
        }
        private Color m_BackColor;

        public Color SysBackColor
        {
            get { return m_BackColor; }
            set { m_BackColor = value; }
        }

        private int m_VertexSnapTolerance;

        public int VertexSnapTolerance
        {
            get { return m_VertexSnapTolerance; }
            set { m_VertexSnapTolerance = value; }
        }

        private bool m_bShowSurveyPt;

        public bool ShowSurveyPt
        {
            get { return m_bShowSurveyPt; }
            set { m_bShowSurveyPt = value; }
        }
        private System.Drawing.Drawing2D.SmoothingMode m_SmoothingMode;

        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get { return m_SmoothingMode; }
            set { m_SmoothingMode = value; }
        }
        private MouseCatchType m_CatchType;

        public MouseCatchType CatchType
        {
            get { return m_CatchType; }
            set { m_CatchType = value; }
        }
        private bool m_bShowOnlineNotes;

        public bool ShowOnlineNotes
        {
            get { return m_bShowOnlineNotes; }
            set { m_bShowOnlineNotes = value; }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            m_bShowOnlineNotes = checkBoxShowOnlineNotes.Checked;
            m_bShowSurveyPt = checkBoxShowSurveyPt.Checked;
            if (checkBoxSnapCenter.Checked && checkBoxSnapVertex.Checked)
            {
                m_CatchType = MouseCatchType.Both;
            }
            else if (checkBoxSnapVertex.Checked)
            {
                m_CatchType = MouseCatchType.Vertex;
            }
            else if (checkBoxSnapCenter.Checked)
            {
                m_CatchType = MouseCatchType.Center;
            }
            else 
                m_CatchType = MouseCatchType.None;

            if (trackBarRenderMode.Value == 3)
            {
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
            else if (trackBarRenderMode.Value == 2)
            {
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
            else if (trackBarRenderMode.Value == 1)
            {
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            m_FileSaveInterval = (int)numericUpCheckedDown1.Value ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_BackColor = dlg.Color;
                button1.BackColor = dlg.Color;
            }
        }

        private void numericUpDownMouseCatchToler_ValueChanged(object sender, EventArgs e)
        {
            m_VertexSnapTolerance = (int)numericUpDownMouseCatchToler.Value;
        }

        private void setcharmap_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "符号映射文件|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string path = dlg.FileName;
                m_MapUI.m_conv_gtr.buseCharMap = true;
                m_MapUI.m_conv_gtr.strcurcharmap = path;
                //MessageBox.Show(path);
                m_MapUI.m_conv_gtr.ReadSymbolCodeCharMap();
            }
        }


    }
}
