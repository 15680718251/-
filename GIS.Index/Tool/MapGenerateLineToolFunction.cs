using System.Collections.Generic;
using GIS.Geometries;
using GIS.SpatialRelation;
using System.Drawing;
using GIS.Layer;

namespace GIS.TreeIndex.Tool
{

    public partial class MapGenerateLineTool : MapTool
    {
        //生成弧线
        public void GenerateArc(GeoPoint LastPt)
        {
            if (m_PtTemp.Count == 1)
            {
                m_PtTemp.Add(LastPt);
            }
            else if (m_PtTemp.Count == 2)
            {
                List<GeoPoint> ArcPts = SpatialRelation.GeoAlgorithm.ThreePointsArc(m_PtTemp[0], m_PtTemp[1], LastPt, ArcType.Arc);

                if (ArcPts != null)
                {
                    RecDataRollBack(m_PtList.Count, ArcPts.Count);
                    m_PtList.AddRange(ArcPts);
                    SpatialRelation.GeoAlgorithm.ClearRepeatPoints(m_PtList);
                    m_PtTemp.Clear();
                    m_CurEditType = EditType.AddOnePoint;
                    m_MapUI.OutPutTextInfo(strLevelOne);
                    m_KeyType = KeyType.ALL;
                }
            }
        }
        //计算垂足点坐标
        public GeoPoint CalVerticalPt(GeoPoint LastPt)
        {
            GeoPoint pt1 = m_PtList[m_PtList.Count - 1];
            GeoPoint Pt2 = GeoAlgorithm.ExtentLine(pt1, LastPt, 1000);
            GeoLineString line = new GeoLineString();
            line.Vertices.Add(pt1);
            line.Vertices.Add(Pt2);
            double minDistOfVerticalPt = double.MaxValue;
            GeoPoint ptTemp;
            GeoPoint ptVertical = null;
            int indexIntersect;
            int indexPt;

            for (int j = 0; j < m_MapUI.LayerCounts; j++)
            {
                GeoVectorLayer layer = m_MapUI.GetLayerAt(j) as GeoVectorLayer;
                if (layer == null)
                    continue;
                GeoData.GeoDataTable table = layer.DataTable;
                for (int k = 0; k < table.Count; k++)
                {
                    GeoData.GeoDataRow row = table[k];
                    Geometry geom = row.Geometry;
                     
                    if (geom == null
                        || !geom.Bound.IsIntersectWith(m_MapUI.GetViewExtents()))
                        continue;

                    GeoLineString lineIntersect = GeoAlgorithm.IntersectTwoLine(pt1, Pt2, geom, out indexIntersect);
                    if (lineIntersect == null)
                        continue;
                    if (GeoAlgorithm.PtToLine(pt1, lineIntersect.Vertices[indexIntersect], lineIntersect.Vertices[indexIntersect + 1]) == 3)
                        continue;

                    GeoLineString lineTemp = new GeoLineString();
                    lineTemp.Vertices.Add(lineIntersect.Vertices[indexIntersect]);
                    lineTemp.Vertices.Add(lineIntersect.Vertices[indexIntersect + 1]);
                    if (!GeoAlgorithm.VerticalPtofPtToLineString(pt1, lineTemp, out ptTemp, out indexPt))//没有垂足
                        continue;
                    double dist = ptTemp.DistanceTo(LastPt);
                    if (dist < minDistOfVerticalPt)
                    {
                        minDistOfVerticalPt = dist;
                        ptVertical = ptTemp;
                    }
                }

            }
            return ptVertical;
        }
        //做垂足
        public void GenerateVertical(GeoPoint LastPt)
        {
            GeoPoint ptVertical =  CalVerticalPt(LastPt);//垂足坐标           
            if (ptVertical != null)
            {
                m_PtList.Add(ptVertical);
                RecDataRollBack(m_PtList.Count - 1, 1);                
            }
            m_CurEditType = EditType.AddOnePoint;
            m_KeyType = KeyType.ALL;
            m_MapUI.OutPutTextInfo(strLevelOne);
        }
        private void GenerateCrossPt(GeoPoint curPt)
        {
            Point pt = m_MapUI.TransFromWorldToMap(curPt);

           GeoData.GeoDataRow row =  m_MapUI.SelectByPt(pt, SelectType.Geomtry);
           if (row != null)
           {
               m_MapUI.RemoveSltObj(row);
               GeoPoint ptVertical;
               int index;
              GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(curPt,row.Geometry,out ptVertical,out index);
              if (line != null)
              {
                  GeoPoint  pt1 = line.Vertices[index-1];
                  GeoPoint pt2 = line.Vertices[index];
                  GeoPoint pt3 = m_PtList[m_PtList.Count - 1];
                  GeoPoint pt4 = m_PtList[m_PtList.Count - 2];
                  double crossX,crossY;
                  if(GeoAlgorithm.CrossPtOfTwoLine(pt1.X,pt1.Y,pt2.X,pt2.Y,pt3.X,pt3.Y,pt4.X,pt4.Y,out crossX,out crossY))
                  {
                      m_PtList.Add(new GeoPoint(crossX, crossY)); 
                      RecDataRollBack(m_PtList.Count - 1, 1);             
                      m_CurEditType = EditType.AddOnePoint;
                      m_KeyType = KeyType.ALL;
                      m_MapUI.OutPutTextInfo(strLevelOne); 
                  } 
              }
              m_MapUI.Refresh();
           }
        }
        
    }
}
