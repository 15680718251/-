using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace GIS.TreeIndex.Tool
{
    public enum ZoomType
    {
        ZoomIn,     //放大
        ZoomOut     //缩小
    }
    public class MapTool
    {
        protected MapTool()
        {
        }
        public MapTool(MapUI ui)
        {
            m_MapUI = ui;          
        }
        protected MapUI m_MapUI;
        protected Cursor m_Cursor;


        public Cursor ToolCursor
        {
            get { return m_Cursor; }
            set { m_Cursor = value; }
        }
        public virtual void OnMouseDown(object sender, MouseEventArgs e)
        {
        }
        public virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
        }
        public virtual void OnMouseUp(object sender, MouseEventArgs e)
        {
        }
        public virtual void Finish()
        {

        }
        public virtual void Cancel()
        {
            m_MapUI.m_EditToolBack = m_MapUI.MapTool;
            m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);     
        }
        public virtual void OnKeyDown(KeyEventArgs e)
        { 
        }
        public virtual void initial()
        {
        }
    }
}
