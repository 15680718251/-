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

namespace GIS.TreeIndex.Tool
{
    public class MapAddContourPtTool:MapTool
    {
        /// <summary>
        /// ///添加高程点，dat，txt格式
        /// </summary>
        /// <param name="ui"></param>
        public MapAddContourPtTool(MapUI ui)
            : base(ui)
        {
           GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
           if (actLayer != null &&
                   (actLayer.VectorType == VectorLayerType.PointLayer
                   || actLayer.VectorType == VectorLayerType.MixLayer))
           {
               OpenFileDialog dlg = new OpenFileDialog();
               dlg.Filter = "文本文件|*.dat";
               if (dlg.ShowDialog() == DialogResult.OK)
               {
                   System.IO.FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                   StreamReader sw = new StreamReader(fs);
                   GeoVectorLayer lyr = actLayer;
                   try
                   {
                       string data = sw.ReadLine();
                       while (data != null)
                       {
                           string[] datas = data.Split(',');
                           double x = double.Parse(datas[1].Trim());
                           double y = double.Parse(datas[2].Trim());
                           double z = double.Parse(datas[3].Trim());

                           GeoPoint3D pt = new GeoPoint3D(x, y, z);
                           lyr.AddGeometry(pt);
                           data = sw.ReadLine();
                       }
                       //ZoomToFullExtent();
                       m_MapUI.OutPutTextInfo("提示：高程点导入成功。。。。。。。\r\n");
                       
                   }
                   catch
                   {
                       MessageBox.Show("文件格式不对");
                   }
                   finally
                   {
                       sw.Close();
                       fs.Close();
                   }

               }
           }
           else
           {
               m_MapUI.OutPutTextInfo("提示：当前活动图层不是点图层。。。。。。。\r\n");
           }

        }

    }
}
