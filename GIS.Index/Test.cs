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
using GIS.TreeIndex.VectorUpdate;
using GIS.Layer;
using System.IO;
using System.Threading;
using GIS.TreeIndex.Index;
using GIS.GeoData;
using GIS.SpatialRelation;
using OSGeo.OGR;
using OSGeo.OSR;
using GIS.Converters.GeoConverter;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        public void Test()
        {

            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
            }

            OSGeo.OGR.Geometry g1 = GeometryConverter.GeometricToOGR(actlyr.DataTable[278].Geometry);
            OSGeo.OGR.Geometry g2 = GeometryConverter.GeometricToOGR(actlyr.DataTable[265].Geometry);
            OSGeo.OGR.Geometry g3 = g1.Intersection(g2);
            wkbGeometryType t1 = g3.GetGeometryType();
            bool b1 = g1.Touches(g2);
            bool b2 = g1.Intersect(g2);
            bool b3 = g1.Disjoint(g2);
            bool b4 = g1.Overlaps(g2);
            double d = g1.Distance(g2);
            double d2 = g3.GetArea();
            string s1 = null;
            string s2 = null;
            string s3 = null;
            if (b1)
                s1 = "Touches";
            if (b2)
                s2 = "Intersect";
            if (b3)
                s3 = "Disjoint";
            MessageBox.Show(s1 + "  " + s2 + "  " + s3+"  "+d.ToString());
                

        }


        public void ClearRepeatPoints()//清除连续重复的点、清除线上多余点（某结点在其前后结点的线上），目的是为了进行环内间的判等
        {
            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return ;
            }
            OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("开始清除连续重复的点.............................\r\n");
            this.Invoke(evt, null, e1);
            int srcFeatureCount = actlyr.DataTable.Count;
            for (int ik = 0; ik < srcFeatureCount; ik++)
            {
                GeoPolygon gp = actlyr.DataTable[ik].Geometry as GeoPolygon;
                gp.ExteriorRing.ClearRepeatPoints();
                for (int i = 0; i < gp.InteriorRings.Count; i++)
                    gp.InteriorRings[i].ClearRepeatPoints();
            }
            UIEventArgs.OutPutEventArgs e2 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("清除连续重复的点完成.............................\r\n");
            this.Invoke(evt, null, e2);
            UIEventArgs.OutPutEventArgs e3 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("开始清除线上多余点.............................\r\n");
            this.Invoke(evt, null, e3);

            for (int ik = 0; ik < srcFeatureCount; ik++)
            {
                GeoPolygon gp = actlyr.DataTable[ik].Geometry as GeoPolygon;
                for (int i = 1; i < gp.ExteriorRing.Vertices.Count-1; i++)
                {
                    if (GeoAlgorithm.PtToLine(gp.ExteriorRing.Vertices[i],
                                              gp.ExteriorRing.Vertices[(i - 1 + gp.ExteriorRing.Vertices.Count) % gp.ExteriorRing.Vertices.Count],
                                              gp.ExteriorRing.Vertices[(i + 1 + gp.ExteriorRing.Vertices.Count) % gp.ExteriorRing.Vertices.Count]) > 0)
                        gp.ExteriorRing.RemoveVertex(gp.ExteriorRing.Vertices[i]);
                }
                for (int i = 0; i < gp.InteriorRings.Count; i++)
                {
                    for (int j = 1; j < gp.InteriorRings[i].Vertices.Count-1; j++)
                    {
                        if (GeoAlgorithm.PtToLine(gp.InteriorRings[i].Vertices[j],
                                                  gp.InteriorRings[i].Vertices[(j - 1 + gp.InteriorRings[i].Vertices.Count) % gp.InteriorRings[i].Vertices.Count],
                                                  gp.InteriorRings[i].Vertices[(j + 1 + gp.InteriorRings[i].Vertices.Count) % gp.InteriorRings[i].Vertices.Count]) > 0)
                            gp.InteriorRings[i].RemoveVertex(gp.InteriorRings[i].Vertices[j]);

                    }
                }
            }

            UIEventArgs.OutPutEventArgs e4 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("清除线上多余点完成.............................\r\n");
            this.Invoke(evt, null, e4);
            UIEventArgs.OutPutEventArgs e5 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("清除完成..................................\r\n");
            this.Invoke(evt, null, e5);
        }

        #region RemoveDuplicatePoint包括几个函数        
        /// <summary>
        /// 消除边界上有小方块空洞的情况，只消除了4个点的小方块(隔4个点有重复)
        /// </summary>
        public bool RemoveDuplicatePoint()
        {
            OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("消除4个结点重复一次的情形.............................\r\n");
            this.Invoke(evt, null, e1);

            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return false;
            }
            int srcFeatureCount = actlyr.DataTable.Count;
            for (int ik = 0; ik < srcFeatureCount; ik++)
            {
                GeoPolygon gp = actlyr.DataTable[ik].Geometry as GeoPolygon;
                List<GeoPoint> a = gp.ExteriorRing.Vertices;
                if (a.Count < 8)
                    continue;
                int interval = 4;
                for (int repeat = 0; repeat < 3; repeat++)//重复三次,消除连续重复
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (IsDuplicateInlist(a, i, interval))
                        {
                            try
                            {
                                RemoveElementInList(a, i + 1, interval);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("Something is wrong!   " + e.Message);
                            }
                        }
                    }
                }
            }
            UIEventArgs.OutPutEventArgs e2 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs(""  + "\r\n");
            this.Invoke(evt, null, e2);
            UIEventArgs.OutPutEventArgs e3 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("测试完成..................................\r\n");
            this.Invoke(evt, null, e3);

            return true;
        }
        /// <summary>
        /// 判断a中第k个位置与后第k+n个位置的点是否相同
        /// </summary>
        /// <param name="a">点数组</param>
        /// <param name="k">起始位置</param>
        /// <param name="n">间隔数</param>
        /// <returns></returns>
        private bool IsDuplicateInlist(List<GeoPoint> a, int k, int n)
        {
            int length = a.Count;
            if (a[k].IsEqual(a[(k + n) % length]))
                return true;
            else
                return false;
        }
        /// <summary>
        /// 在数组a中，删除元素（从k位置开始，连续删除n个）
        /// </summary>
        /// <param name="a">数组</param>
        /// <param name="k">起始位置</param>
        /// <param name="n">删除个数</param>
        /// <returns></returns>
        private bool RemoveElementInList(List<GeoPoint> a, int k, int n)
        {
            if (a.Count < n)
                return false;
             for (int i = 0; i < n; i++)
            {
               if (k == a.Count)//如果当前位置是最后一个点，则先删除当前点，下一个位置从头开始
                {
                    a.RemoveAt(k);
                    k = 0;
                }
               else if (k > a.Count)//如果当前位置是最后一个点的后面，则从头开始删除
                {
                    k = 0;
                    a.RemoveAt(k);
                }
                else
                    a.RemoveAt(k);
            }
            return true;
        }
        #endregion        
        public bool Test1()//测试内环
        {
            OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);
            UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("测试内环数量及ID.............................\r\n");
            this.Invoke(evt, null, e1);

            GeoVectorLayer actlyr = GetActiveVectorLayer() as GeoVectorLayer;
            if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
            {
                MessageBox.Show("请先设置面要素图层为活动图层！");
                return false;
            }

            //测试最大互环数
            int srcFeatureCount = actlyr.DataTable.Count;

            int numberOfRing = 0;
            int IDwithMostRing = 0;
            for (int ik = 0; ik < srcFeatureCount; ik++)
            {
                GeoPolygon gp = actlyr.DataTable[ik].Geometry as GeoPolygon;
                if (gp.InteriorRings.Count > numberOfRing)
                {
                    numberOfRing = gp.InteriorRings.Count;
                    IDwithMostRing = System.Convert.ToInt32(actlyr.DataTable[ik]["PlgID"]);
                }
            }
            UIEventArgs.OutPutEventArgs e2 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("最大内环数为：" + numberOfRing.ToString()  + "         该多边形PlgID为" + IDwithMostRing.ToString() + "\r\n");
            this.Invoke(evt, null, e2);
            UIEventArgs.OutPutEventArgs e3 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("测试完成..................................\r\n");
            this.Invoke(evt, null, e3);

            return true;
        }


    }
}
