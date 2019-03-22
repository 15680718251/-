using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GIS.GeoData;
using GIS.Layer;
using System.Runtime.Serialization;

namespace GIS.GeoData
{
    [Serializable]
    public class GeoDataTable:DataTable
    {
        public GeoDataTable(): base()
        {
        }

        #region 修改
        private bool bFillData = false;//是否将属性数据导入
        #endregion
        private GeoLayer m_BelongLayer = null;

        public GeoLayer BelongLayer
        {
            get { return m_BelongLayer; }
            set { m_BelongLayer = value; }
        }

        /// <summary>
        /// 是否将属性数据导入
        /// </summary>
        public bool FillData
        {
            get { return bFillData; }
            set { bFillData = value; }
        }
        public int Count
        {
            get { return base.Rows.Count; }
        }
        /// <summary>
        /// 创建和表拥有相同结构属性的属性行
        /// </summary>
        /// <returns></returns>
        public new GeoDataRow NewRow()
        {
            return (GeoDataRow)base.NewRow();
        }
        public new GeoDataTable Clone()
        {
            return (GeoDataTable)base.Clone();
        }
        /// <summary>
        /// 返回属性表的第INDEX行
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GeoDataRow this[int index]
        {
            get { return (GeoDataRow)base.Rows[index]; }
        }
        //添加行
        public void AddRow(GeoDataRow row)
        {
            base.Rows.Add(row);
        }
        //删除行
        public void RemoveRow(GeoDataRow row)
        {
            base.Rows.Remove(row);
        }
     
        /// <summary>
        /// 创建与属性表拥有相同属性的行，根据DataRowBuilder
        /// </summary> 必须有该函数，不然NEWROW 出错
        /// <param name="builder"></param>
        /// <returns></returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new GeoDataRow(builder);
        }

    }
}
