using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GISEditor.EditTool.Command;
namespace GISEditor.EditorTool.BasicClass
{
   public class MapManager
    {

        #region 初始化

        public MapManager()
        {

        }

        #endregion

        #region 变量定义

        public static Form ToolPlatForm = null;
        private static IEngineEditor _engineEditor;
        public static IEngineEditor EngineEditor
        {
            get { return MapManager._engineEditor; }
            set { MapManager._engineEditor = value; }
        }

        #endregion

        #region 获取颜色
        public static IRgbColor GetRgbColor(int intR, int intG, int intB)
        {
            IRgbColor pRgbColor = null;
            pRgbColor = new RgbColorClass();
            if (intR < 0) pRgbColor.Red = 0;
            else pRgbColor.Red = intR;

            if (intG < 0) pRgbColor.Green = 0;
            else pRgbColor.Green = intG;

            if (intB < 0) pRgbColor.Blue = 0;
            else pRgbColor.Blue = intB;

            return pRgbColor;
        }
        #endregion

        #region 计算两点之间X轴方向和Y轴方向上的距离
        public static bool CalDistance(IPoint lastpoint, IPoint firstpoint, out double deltaX, out double deltaY)
        {
            deltaX = 0; deltaY = 0;
            if (lastpoint == null || firstpoint == null)
                return false;
            deltaX = lastpoint.X - firstpoint.X;
            deltaY = lastpoint.Y - firstpoint.Y;
            return true;
        }
        #endregion

        #region 单位转换
        public static double ConvertPixelToMapUnits(IActiveView activeView, double pixelUnits)
        { 
            int pixelExtent=activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right-activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;
            double realWorldDisplayExtent=activeView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel=realWorldDisplayExtent/pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }
        #endregion
       
        #region 获取选择要素
        public static IFeatureCursor GetSelectedFeatures(IFeatureLayer pFeatLyr)
        { 
            ICursor pCursor=null;
            IFeatureCursor pFeatCur=null;
            if (pFeatLyr==null) 
                return null;
            IFeatureSelection pFeatSel =pFeatLyr as IFeatureSelection;
            ISelectionSet pSelSet=pFeatSel.SelectionSet;
            if (pSelSet.Count==0) 
                return null;
            pSelSet.Search(null,false,out pCursor);
            pFeatCur=pCursor as IFeatureCursor;
            return pFeatCur;
        }
        #endregion

        //#region 获取当前地图文档所有地图集合
        //public static List<ILayer> GetLayers(IMap pMap)
        //{
        //    ILayer pLyr = null;
        //    List<ILayer> pLstLayers = null;
        //    try
        //    {
        //        pLstLayers = new List<ILayer>();
        //        for (int i = 0; i < pMap.LayerCount; i++)
        //        {
        //            pLyr = pMap.get_Layer(i);
        //            if (!pLstLayers.Contains(pLyr))
        //            {
        //                pLstLayers.Add(pLyr);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { }
        //    return pLstLayers;
        //}
        //#endregion

        //#region 根据图层名获取图层
        //public static ILayer GetLayerByName(IMap pMap, string sLyrName)
        //{
        //    ILayer pLyr = null;
        //    ILayer pLayer = null;
        //    try
        //    {
        //      for (int i=0;i<pMap.LayerCount;i++)
        //       {
        //        pLyr=pMap.get_Layer(i);
        //        if (pLyr.Name.ToUpper()==sLyrName.ToUpper())
        //        {
        //        pLayer=pLyr;
        //            break;
        //        }
        //       }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    return pLayer;
        //}


        //#endregion
    }
}
