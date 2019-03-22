using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GIS.Geometries;
using GIS.mm_Conv_Symbol; 

namespace GIS.GeoData
{
    public class GeoDataRow:DataRow 
    {
        internal GeoDataRow(DataRowBuilder rb) : base(rb) { }
        #region PrivateMemeber
        private Geometry m_Geometry = null;  //几何
        private bool m_bSelectState = false;   //是否选中
        private EditState m_EditState = EditState.Original; //该条记录的编辑状态 
      //  private DateTime m_EditTime;    //编辑时间

        private bool m_bsymbolupdate = true;
        private Symbol_Base m_symbol = null;
        private bool m_bsymrender = true;

        #endregion

        #region Properties
        public bool bsymrender
        {
            get { return m_bsymrender; }
            set { m_bsymrender = value; }
        }

        public bool bsymbolupdate
        {
            get { return m_bsymbolupdate; }
            set
            {
                m_bsymbolupdate = value;
            }
        }

        public EditState EditState
        {
            get { return m_EditState; }
            set 
            { 
                m_EditState = value;
                this["ChangeType"] = value.ToString();
            }
        }
        public bool SelectState
        {
            get { return m_bSelectState; }
            set { m_bSelectState = value; }
        }

        public Geometry Geometry
        {
            get { return m_Geometry; }
            set { m_Geometry = value; }
        }

        //public Symbol_Cache Symbol
        //{
        //    get { return m_symbol; }
        //    set { m_symbol = value; }
        //}

        public Symbol_Base symbol
        {
            get { return m_symbol; }
            set { m_symbol = value; }
        }

        #endregion
        public GeoDataRow Clone()
        {
            GeoDataRow row =  ((GeoDataTable)Table).NewRow();
            row.Geometry = Geometry.Clone();
            foreach (DataColumn column in Table.Columns)
            {
                row[column] = this[column];
            }
            row[0] = Table.Rows.Count;
            return row;
        }

        public GeoDataRow Clone_rb()
        {
            GeoDataRow row = ((GeoDataTable)Table).NewRow();
            row.Geometry = Geometry.Clone();
            foreach (DataColumn column in Table.Columns)
            {
                row[column] = this[column];
            }
            return row;
        }

        public bool IsGeometryNull()
        {
            return m_Geometry == null;
        }

 
    }
}
