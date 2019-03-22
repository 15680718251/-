using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GIS.Geometries;
namespace GIS.GeoData.DataProviders
{
    public interface  IProvider:IDisposable
    {
        /// <summary>
        /// 打开数据源
        /// </summary>
        bool Open();
        /// <summary>
        /// 关闭数据源
        /// </summary>
        void Close();
        /// <summary>
        /// 获取地理范围
        /// </summary>
        /// <returns></returns>
        GeoBound GetExtents();
        /// <summary>
        /// 数据源是否打开
        /// </summary>
        /// <returns></returns>
        bool IsOpen();
        /// <summary>
        /// 获取地理要素的个数
        /// </summary>
        /// <returns></returns>
        int GetGeomCount();
        /// <summary>
        /// 通过索引找到边界BOUND 内所有几何ID
        /// </summary>
        /// <param name="bound">搜索的边界范围</param>
        /// <returns></returns>
        Collection<uint> GetObjectIDsInView(GeoBound bound);
        /// <summary>
        /// 获取全部对象，并存入表TABLE中
        /// </summary>
        /// <param name="table"></param>
        GeoDataTable   ExecuteAllQuery( );
        GeoDataTable ExecuteAllGeomQuery();
        //执行属性查询，将已经生成的几何数据图层，填充属性数据
        bool ExecuteAllAttributeQuery(GeoDataTable table);
        //GeoDataRow GetGeoDataRow(uint RowId);
        int GetLayerType();
        Style.VectorStyle GetLayerStype();
    }
}
