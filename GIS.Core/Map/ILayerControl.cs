using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Layer;
namespace GIS.Map
{
    //控制图层集里的图层的操作
    public interface ILayerControl
    {
        /// <summary>
        /// 设置单个图层是否可见
        /// </summary>
        /// <param name="strGroupName">图层所在的图层组的名称</param>
        /// <param name="strLayerName">图层名</param>
        /// <param name="bIsEnable">是否可见</param>
        /// <returns></returns>
        bool SetLayerEnable(String strGroupName, string strLayerName, bool bIsEnable);
        /// <summary>
        /// 通过图层名和组名找到该图层对象
        /// </summary>
        /// <param name="stGroupName">工作组名称</param>
        /// <param name="strLayerName">图层名称</param>
        /// <returns></returns>
        GeoLayer GetLayerByName(String strGroupName,String strLayerName);
        GeoLayer GetLayerByTable(GeoData.GeoDataTable table );
        /// <summary>
        /// 从工作组 strGroupName中删除图层 strLayerName
        /// </summary>
        /// <param name="strGroupName">工作组名称</param>
        /// <param name="strLayerName">图层名称</param>
        /// <returns></returns>
        bool RemoveLayer(String strGroupName, String strLayerName);
        /// <summary>
        /// 判断图层在当前活动组里是否存在
        /// </summary>
        /// <param name="strLayerName"></param>
        /// <returns></returns>
        bool LayerExist(string strLayerName);
        /// <summary>
        /// 获取当前活动图层
        /// </summary>
        /// <returns></returns>
        GeoLayer GetActiveVectorLayer();
        /// <summary>
        /// 设置活动图层
        /// </summary>
        /// <returns></returns>
        bool SetActiveLayer(String strGroupName, String strLayerName);
    }

    //控制图层集的操作
    public interface ILayerGroupControl
    {
        int GroupCounts { get; }
        //如有同名的GROUP对象,提示用户修改
        bool AddGroup(String strGroupName);

        //设置图层组里的所有图层对象是否可见
        bool SetGroupEnable(string strGroupName, bool bEnable);

        //通过图层集的名称或引用找到该图层集
        LayerGroup GetGroupByName(string strGroupName);
        LayerGroup GetGroupByLayer(GeoLayer lyr);
        
        //对图层集重命名
        bool LayerGroupReName(int index, string strNewName);
        bool LayerGroupReName(string strOldName, string strNewName);
    }
}
