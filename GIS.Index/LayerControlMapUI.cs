using System;
using System.Collections.Generic;
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
namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox  
    {
        public void LayerIncreaseHandle(string lyrName, LAYERTYPE_DETAIL type)
        {
            if (LayerIncrease != null)
                LayerIncrease(lyrName, type);
        }
        public void LayerDecreaseHandle(string lyrName)
        {
            if (LayerDecrease != null)
                LayerDecrease(lyrName);
        }
        public void LayerGroupDecreaseHandle(string lyrGroupName)
        {
            if (LayerGroupDecrease != null)
                LayerGroupDecrease(lyrGroupName);
        }
        public void LayerGroupIncreaseHandle(string lyrGroupName)
        {
            if (LayerGroupIncrease != null)
                LayerGroupIncrease(lyrGroupName);
        }
        public delegate void  LayerIncreaseEventHandle(string lyrName,LAYERTYPE_DETAIL type);
        public event LayerIncreaseEventHandle LayerIncrease;

        public delegate void LayerGroupIncreaseEventHandle(String strGroupName);
        public event LayerGroupIncreaseEventHandle LayerGroupIncrease;

        public delegate void LayerDecreaseEventHandle(string lyrName);
        public event LayerDecreaseEventHandle LayerDecrease;

        public delegate void LayerGroupDecreaseEventHandle(string lyrGroupName);
        public event LayerGroupDecreaseEventHandle LayerGroupDecrease;


        private bool CheckLayerNameExist(string strLayerName)
        {
            return m_Map.LayerExist(strLayerName);
        }
        public void AddLayer(GeoLayer layer)
        {
            m_Map.AddLayer(layer);         
        }

        //创建新的空图层，lyrType = 1代表 点层，2代表线，3 代表面，4代表注记
        public GeoLayer CreateLayer(LAYERTYPE_DETAIL lyrType)
        {
            string nameType = null;
            switch (lyrType)
            {
                case LAYERTYPE_DETAIL.PointLayer:
                    nameType = "点图层";
                    break;
                case LAYERTYPE_DETAIL .LineLayer:
                    nameType = "线图层";
                    break;
                case  LAYERTYPE_DETAIL.PolygonLayer:
                    nameType = "面图层";
                    break;
                case LAYERTYPE_DETAIL .LabelLayer:
                    nameType = "注记层";
                    break;
                case  LAYERTYPE_DETAIL.MixLayer:
                    nameType = "混合层";
                    break;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "新建空"+nameType;
            dlg.Filter = nameType+"|*.shp";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string strFilePath = dlg.FileName;
                string strLayerName = Path.GetFileNameWithoutExtension(strFilePath);
                if (CheckLayerNameExist(strLayerName))
                {
                    MessageBox.Show("图层名已经存在!", "提示");
                    return null;
                }

                GeoLayer layer = null;
                GeoData.GeoDataTable table = null;

                GeoVectorLayer ptLayer = new GeoVectorLayer(strFilePath);
                ptLayer.DataTable = new GIS.GeoData.GeoDataTable();
                table = ptLayer.DataTable;
                layer = ptLayer;
                ptLayer.VectorType = (VectorLayerType)((int)lyrType);


                table.FillData = true; ///空图层，属性信息已经填充，因为是空的
                table.Columns.Add("FID", typeof(int));
                table.Columns.Add("FeatID", typeof(string));
                table.Columns.Add("ClasID", typeof(Int64));
                table.Columns.Add("BeginTime", typeof(string));
                table.Columns.Add("ChangeType", typeof(string));
                AddLayer(layer);
                return layer;
            }

            return null;
        }
        public struct LayerInfo
        {
            public string GroupName;
            public string LayerName;
        }

        public List<LayerInfo> GetAllLabelLayer()
        {
            List<LayerInfo> list = new List<LayerInfo>();
            for (int i = 0; i < GroupCounts; i++)
            {
                LayerGroup group = GetGroupByIndex(i);
                for(int j =0 ; j<group.Counts;j++)
                {
                    GeoVectorLayer lyr = group.Layers[j] as GeoVectorLayer;
                    if (lyr != null && 
                        (lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LabelLayer
                    || lyr.LayerTypeDetail == LAYERTYPE_DETAIL.MixLayer
                    || lyr.LayerTypeDetail == LAYERTYPE_DETAIL.DraftLayer))
                    {
                        LayerInfo info = new LayerInfo();
                        info.GroupName = group.LayerGroupName;
                        info.LayerName = lyr.LayerName;
                        list.Add(info);
                    }
                }
            }
            return list;
        }
        public int GroupCounts
        {
            get { return m_Map.GroupCounts; }
        }
        public int LayerCounts
        {
            get
            {
                return m_Map.LayerCounts;
            }
        }
        //工作组重命名，strOldName代表旧的名称，strNewName代表新的名称
        public bool LayerGroupReName(string strOldName, string strNewName)
        {
            return m_Map.LayerGroupReName(strOldName, strNewName);
        }
        //工作组重命名，index代表工作组在列表中的索引号，strNewName代表新的名称 
        public bool LayerGroupReName(int index, string strNewName)
        {
            return m_Map.LayerGroupReName(index, strNewName);
        }

        public bool RemoveLayerGroup(string strGroupName)
        {
            if (strGroupName == "系统工作区")
            {
                MessageBox.Show("系统工作区不能删除！","提示");
                return false;
            }
            if (m_Map.RemoveLayerGroup(strGroupName))
            {
                ZoomToFullExtent();
                return true;
            }
            return false;
        }
        //删除图层
        public bool RemoveLayer(string strGroupName, string strLayerName)
        {
            if (strGroupName == "系统工作区")
            {
                MessageBox.Show("系统工作区不能删除！", "提示");
                return false;
            }
            if (m_Map.RemoveLayer(strGroupName, strLayerName))
            {
                ZoomToFullExtent();
                return true;
            }
            return false;
        }
        //获取地图中的图层组
        public LayerGroup GetGroupByIndex(int index)
        {
            return m_Map.GetGroupAt(index);
        }
        public GeoLayer GetActiveVectorLayer()
        {
            return m_Map.GetActiveVectorLayer();
        }
        public GeoLayer GetDraftMixLayer()
        {
            return m_Map.GetDraftMixLayer();
        }
        public GeoLayer GetActiveLabelLayer()
        {
            return m_Map.GetActiveLabelLayer();
        }
        public GeoLayer GetLayerByName(String strLayerName)
        {
            return m_Map.GetLayerByName(strLayerName);
        }
        public GeoLayer GetLayerAt(int index)
        {
            return m_Map.GetLayerAt(index);
        }
        /// <summary>
        /// 设置活动图层
        /// </summary>
        /// <returns></returns>
        public bool SetActiveLayer(String strGroupName, String strLayerName)
        {
            if (!m_Map.SetActiveLayer(strGroupName, strLayerName))
            {
                MessageBox.Show("图层不存在，不能设为活动图层");
                return false;
            }
            return true;
        }

        //添加工作组，如有同名的GROUP对象,提示用户修改
        public bool AddGroup(String strGroupName)
        {
            return m_Map.AddGroup(strGroupName);
        }

        //设置图层组里的所有图层对象是否可见
        public bool SetGroupEnable(string strGroupName, bool bEnable)
        {
            return m_Map.SetGroupEnable(strGroupName, bEnable);
        }

        //通过图层集的名称找到该图层集
        public LayerGroup GetGroupByName(string strGroupName)
        {
            return m_Map.GetGroupByName(strGroupName);
        }
        public LayerGroup GetGroupByLayer(GeoLayer lyr)
        {
            return m_Map.GetGroupByLayer(lyr);
        }
        //将图层组设置为当前活动图层集，文件将添加至该图层集
        public string ActiveLyrGroupName
        {
            get
            {
                return m_Map.ActiveLyrGroup;
            }
            set
            {
                m_Map.ActiveLyrGroup = value;
            }
        }
        //设置图层是否可见
        public bool SetLayerEnable(string strGroupName, string strLayerName, bool bEnable)
        {
            if (m_Map.SetLayerEnable(strGroupName, strLayerName, bEnable))
            {
                EagleMapRefresh(true);
                return true;
            }
            return false;
            
        }
        //通过表找到矢量图层，返回的是基类图层
        public GeoLayer GetLayerByTable(GeoData.GeoDataTable table)
        {
            return m_Map.GetLayerByTable(table);
        }

        /// <summary>
        /// 通过图层所在的图层组名称和图层名，找到该图层的属性表
        /// </summary>
        /// <param name="strGroupName"></param>
        /// <param name="strLayerName"></param>
        /// <returns></returns>
        public GeoData.GeoDataTable GetLayerTableByName(string strGroupName, string strLayerName)
        {
            GeoLayer lyr = m_Map.GetLayerByName(strGroupName, strLayerName);
            if (lyr != null ) 
            {
                if (lyr.LayerType == LAYERTYPE.VectorLayer && ((GeoVectorLayer)lyr).DataTable.FillData)
                {
                    return ((GeoVectorLayer)lyr).DataTable;
                }              
            }
            return null;
        }
    }
}
