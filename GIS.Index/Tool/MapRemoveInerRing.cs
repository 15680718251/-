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
using GIS.TreeIndex.Index;
using GIS.GeoData;
using GIS.SpatialRelation;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        //
        // 摘要:一些多边形中存在很多空洞（空洞内可能也存在其他对象），本方法可以删除所有空洞（包括内环及内环中的多边形）
        //
        // 
        public bool RemoveInerRing()
        {
            OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("开始删除内环.............................\r\n");
            this.Invoke(evt, null, e1);

            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return false;
            }
            for (int i = 0; i < actlyr.DataTable.Count; i++)
            {
                if (actlyr.DataTable[i].Geometry == null)
                    continue;
                for (int j = 0; j < actlyr.DataTable.Count; j++)
                {
                    if (actlyr.DataTable[j].Geometry == null)
                        continue;
                    if (i == j)
                        continue;
                    GeoPolygon gi = actlyr.DataTable[i].Geometry as GeoPolygon;
                    GeoPolygon gj = actlyr.DataTable[j].Geometry as GeoPolygon;
                    for (int k = 0; k < gi.InteriorRings.Count(); k++)
                    {
                        if (gj.Bound.IsIntersectWith(gi.InteriorRings[k].Bound))
                        {
                            if (GeoAlgorithm.IsInLinearRing(gj.ExteriorRing.Vertices[0], gi.InteriorRings[k]) >= 1)
                            { 
                                actlyr.DataTable[j].Geometry = null;
                            }
                        }
                    }

                }
            }
            for (int i = 0; i < actlyr.DataTable.Count; i++)
            {
                if (actlyr.DataTable[i].Geometry == null)
                {
                    actlyr.DataTable.Rows.RemoveAt(i);
                    i--;
                }
                GeoPolygon g = actlyr.DataTable[i].Geometry as GeoPolygon;
                for (int j = g.InteriorRings.Count - 1; j >= 0; j--)
                {
                    g.InteriorRings.RemoveAt(j);
                }
            }
            UIEventArgs.OutPutEventArgs e2 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("内环删除完成..................................\r\n");
            this.Invoke(evt, null, e2);

            return true;

        }


    }
}
