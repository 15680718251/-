using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using GIS.Map;
using GIS.Geometries;
using GIS.Layer;
using GIS.Toplogical;
using GIS.Utilities;
using GIS.TreeIndex.Forms;
using GIS.TreeIndex.Tool;
using GIS.GeoData;
using GIS.SpatialRelation;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        //Quality Control

        #region//打断自身相交
        public bool bSelfIntersection()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;
                        SpiltSelfIntersect(vectorlyr);
                    }

                }

            }
            return true;
        }

        class pt
        {
            public pt(bool bvertice, GeoPoint point)
            {
                _bvertice = bvertice;
                _point = (GeoPoint)point.Clone();
            }

            public bool _bvertice;
            public GeoPoint _point;
        }

        public bool SpiltSelfIntersect(GeoVectorLayer lyr)
        {
            for (int a = 0; a < lyr.DataTable.Count; a++)
            {
                GeoDataRow row = lyr.DataTable[a];
                if (row.EditState == EditState.Invalid || row.EditState == EditState.Disappear|| 
                    row.EditState == EditState.AttributeBef || row.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line = row.Geometry as GeoLineString;

                GeoLineString newline1 = new GeoLineString();
                GeoLineString newline2 = new GeoLineString();

                GeoPoint pt0, pt1, pt2, pt3;
                bool bbreak = false;

                double cx = 0.0, cy = 0.0;

                Dictionary<int, List<GeoPoint>> CrossptPool = new Dictionary<int, List<GeoPoint>>();

                for (int i = 0; i < line.Vertices.Count - 1; i++)
                {
                    CrossptPool.Add(i, new List<GeoPoint>());
                }
                bool bhascrosspt = false;
                for (int i = 0; i < line.Vertices.Count - 3; i++)
                {
                    pt0 = line.Vertices[i];
                    pt1 = line.Vertices[i + 1];
                    for (int j = i + 2; j < line.Vertices.Count - 1; j++)
                    {
                        pt2 = line.Vertices[j];
                        pt3 = line.Vertices[j + 1];
                        if (IntersectTwoLine(pt0, pt1, pt2, pt3) && 
                            GeoAlgorithm.CrossPtOfTwoLine(pt0.X, pt0.Y, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, out cx, out cy))  //相交且不平行
                        {
                            bhascrosspt = true;
                            int t = 0;
                            for (; t < CrossptPool[i].Count; t++)
                            {
                                double cplength = line.Vertices[i].DistanceTo(new GeoPoint(cx, cy));
                                double tlength = CrossptPool[i][t].DistanceTo(line.Vertices[i]);
                                if (cplength < tlength)
                                {
                                    CrossptPool[i].Insert(t, (new GeoPoint(cx, cy)));
                                    break;
                                }
                                if (cplength == tlength)
                                    continue;
                            }
                            if (t == CrossptPool[i].Count)
                                CrossptPool[i].Add(new GeoPoint(cx, cy));


                            t = 0;

                            for (; t < CrossptPool[j].Count; t++)
                            {
                                double cplength = line.Vertices[i].DistanceTo(new GeoPoint(cx, cy));
                                double tlength = CrossptPool[i][t].DistanceTo(line.Vertices[j]);
                                if (cplength < tlength)
                                {
                                    CrossptPool[j].Insert(t, (new GeoPoint(cx, cy)));
                                    break;
                                }
                                if (cplength == tlength)
                                    continue;
                            }
                            if (t == CrossptPool[j].Count)
                                CrossptPool[j].Add(new GeoPoint(cx, cy));

                        }
                    }
                }

                if (bhascrosspt == false)
                    continue;


                List<pt> respt = new List<pt>();

                for (int i = 0; i < line.Vertices.Count - 1; i += 1)
                {
                    //第一点看成交点
                    if (i == 0)
                    {
                        respt.Add(new pt(false, (GeoPoint)line.Vertices[i].Clone()));
                    }
                    bool bv = true;
                    for (int j = 0; j < CrossptPool[i].Count; j++)
                    {
                        if (j == 0 && CrossptPool[i][j].DistanceTo(line.Vertices[i]) < 0.005)
                        {
                            respt[respt.Count - 1]._bvertice = false;
                            continue;
                        }
                        if (j == CrossptPool[i].Count - 1 && CrossptPool[i][j].DistanceTo(line.Vertices[i + 1]) < 0.005)
                        {
                            bv = false;
                            continue;
                        }
                        respt.Add(new pt(false, (GeoPoint)CrossptPool[i][j].Clone()));

                    }
                    //最后一点看成交点
                    if (i == line.Vertices.Count - 1)
                    {
                        respt.Add(new pt(false, (GeoPoint)line.Vertices[i + 1].Clone()));
                    }
                    else
                    {
                        respt.Add(new pt(true, (GeoPoint)line.Vertices[i + 1].Clone()));
                    }
                }

                List<GeoDataRow> lllll = new List<GeoDataRow>();
                for (int i = 0; i < respt.Count; i++)
                {
                    GeoDataRow rrr = row.Clone();
                    GeoLineString lline = new GeoLineString();
                    int cnt = 0;
                    for (; i < respt.Count; )
                    {
                        if (cnt < 2)
                        {
                            if (respt[i]._bvertice == false)
                                cnt++;
                            lline.AddPoint(new GeoPoint(respt[i]._point.X, respt[i]._point.Y));
                            i++;
                        }
                        else
                        {
                            i -= 2;
                            break;
                        }
                    }
                    rrr.Geometry = lline;
                    lllll.Add(rrr);
                }

                lyr.DataTable.RemoveRow(row);
                for (int i = 0; i < lllll.Count; i++)
                {
                    lyr.DataTable.Rows.InsertAt(lllll[i], a);
                }

                a += lllll.Count - 1;

            }
            return true;
        }
        public bool brepeat(GeoLineString line, GeoPoint pt)
        {
            if (GeoAlgorithm.DistanceOfTwoPt(line.Vertices[0], pt) < 0.005
                || GeoAlgorithm.DistanceOfTwoPt(line.Vertices[line.Vertices.Count - 1], pt) < 0.005)
            {
                return true;
            }
            else
                return false;
            //for (int i = 0; i < line.Vertices.Count; i++)
            //{
            //    if (GeoAlgorithm.DistanceOfTwoPt(line.Vertices[i], pt) < 0.05)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }
        #endregion

        #region//判断两条线是否相交
        public bool IntersectTwoLine(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4)
        {

            return System.Convert.ToBoolean(((CCW(p1, p2, p3) * CCW(p1, p2, p4)) <= 0) && ((CCW(p3, p4, p1) * CCW(p3, p4, p2) <= 0)));

        }

        public int CCW(GeoPoint p0, GeoPoint p1, GeoPoint p2)
        {

            double dx1, dx2;
            double dy1, dy2;

            dx1 = p1.X - p0.X; dx2 = p2.X - p0.X;
            dy1 = p1.Y - p0.Y; dy2 = p2.Y - p0.Y;

            /* This is basically a slope comparison: we don't do divisions because

             * of divide by zero possibilities with pure horizontal and pure
             * vertical lines.
             */

            return ((dx1 * dy2 > dy1 * dx2) ? 1 : -1);

        }
        #endregion

        #region//打断相交线体
        public bool SpiltLinesIntersection()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;
                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer layer = lyr as GeoVectorLayer;
                        //SpiltSelfIntersect(layer);
                        SpiltTwoLinesIntersection(layer);
                    }
                }
            }
            return true;
        }

        public bool SpiltTwoLinesIntersection(GeoVectorLayer layer)
        {
            for (int a = 0; a < layer.DataTable.Count; a++)
            {
                GeoDataRow row1 = layer.DataTable[a];
                if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                    || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line1 = row1.Geometry as GeoLineString;
                if (line1.StartPoint.IsEqual(line1.EndPoint) && line1.Length < 0.001)
                {
                    continue;
                }
                for (int b = 0; b < layer.DataTable.Count; b++)
                {
                    if (a == b)
                    {
                        continue;
                    }

                    GeoDataRow row2 = layer.DataTable[b];
                    if (!(row2.Geometry.Bound.IsIntersectWith(row1.Geometry.Bound)))
                    {
                        continue;
                    }
                    if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                       || row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                        continue;
                    GeoLineString line2 = row2.Geometry as GeoLineString;
                    if (line2.StartPoint.IsEqual(line2.EndPoint) && line2.Length < 0.005)
                    {
                        continue;
                    }

                    GeoLineString newline11 = new GeoLineString();
                    GeoLineString newline12 = new GeoLineString();
                    GeoLineString newline22 = new GeoLineString();
                    GeoLineString newline21 = new GeoLineString();

                    double cx = 0, cy = 0;
                    GeoPoint pt0, pt1, pt2, pt3;
                    int i = 0;
                    int j = 0;
                    bool bbreak = false;

                    for (; i < line1.Vertices.Count - 1; i++)
                    {
                        pt0 = line1.Vertices[i];
                        pt1 = line1.Vertices[i + 1];
                        GeoLineString m_line1 = new GeoLineString();
                        m_line1.AddPoint(pt0);
                        m_line1.AddPoint(pt1);

                        j = 0;
                        for (; j < line2.Vertices.Count - 1; j++)
                        {
                            pt2 = line2.Vertices[j];
                            pt3 = line2.Vertices[j + 1];
                            GeoLineString m_line2 = new GeoLineString();
                            m_line2.AddPoint(pt2);
                            m_line2.AddPoint(pt3);

                            if (GeoAlgorithm.IntersectTwoLine(pt0, pt1, pt2, pt3))
                            {
                                GeoAlgorithm.CrossPtOfTwoLine(pt0.X, pt0.Y, pt1.X, pt1.Y, pt2.X, pt2.Y,
                                    pt3.X, pt3.Y, out cx, out cy);


                                if (brepeat(m_line1, new GeoPoint(cx, cy)) && brepeat(m_line2, new GeoPoint(cx, cy)))
                                {
                                    continue;
                                }
                                if (cx == 1e20 && cy == 1e20)
                                {
                                    continue;
                                }


                                bbreak = true;
                                break;

                            }
                        }
                        if (bbreak)
                            break;
                    }

                    if (bbreak)
                    {
                        for (int m = 0; m <= i; m++)
                        {
                            newline11.AddPoint(new GeoPoint(line1.Vertices[m].X, line1.Vertices[m].Y));
                        }
                        newline11.AddPoint(new GeoPoint(cx, cy));

                        newline12.AddPoint(new GeoPoint(cx, cy));
                        for (int n = i + 1; n < line1.Vertices.Count; n++)
                        {
                            newline12.AddPoint(new GeoPoint(line1.Vertices[n].X, line1.Vertices[n].Y));
                        }

                        for (int p = 0; p <= j; p++)
                        {
                            newline21.AddPoint(new GeoPoint(line2.Vertices[p].X, line2.Vertices[p].Y));
                        }
                        newline21.AddPoint(new GeoPoint(cx, cy));

                        newline22.AddPoint(new GeoPoint(cx, cy));
                        for (int q = j + 1; q < line2.Vertices.Count; q++)
                        {
                            newline22.AddPoint(new GeoPoint(line2.Vertices[q].X, line2.Vertices[q].Y));
                        }

                        GeoDataRow row11 = row1.Clone();
                        GeoDataRow row12 = row1.Clone();
                        GeoDataRow row21 = row2.Clone();
                        GeoDataRow row22 = row2.Clone();

                        row11.Geometry = newline11;
                        row12.Geometry = newline12;
                        row21.Geometry = newline21;
                        row22.Geometry = newline22;

                        if (!(newline11.StartPoint.IsEqual(newline11.EndPoint) && newline11.Length < 0.005))
                        {
                            layer.DataTable.AddRow(row11);
                        }
                        if (!(newline12.StartPoint.IsEqual(newline12.EndPoint) && newline12.Length < 0.005))
                        {
                            layer.DataTable.AddRow(row12);
                        }
                        if (!(newline21.StartPoint.IsEqual(newline21.EndPoint) && newline21.Length < 0.005))
                        {
                            layer.DataTable.AddRow(row21);
                        }
                        if (!(newline22.StartPoint.IsEqual(newline22.EndPoint) && newline22.Length < 0.005))
                        {
                            layer.DataTable.AddRow(row22);
                        }

                        layer.DataTable.Rows.Remove(row1);
                        layer.DataTable.Rows.Remove(row2);

                        a--;
                        break;
                    }
                }
            }
            return true;
        }
        #endregion

        #region//同层重复线显示
        public bool RepetitiveLine()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;

                        for (int k = 0; k < vectorlyr.DataTable.Count - 1; k++)
                        {
                            GeoDataRow row1 = vectorlyr.DataTable[k];
                            if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear ||
                                row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                                continue;
                            GeoLineString line1 = row1.Geometry as GeoLineString;
                            for (int m = k + 1; m < vectorlyr.DataTable.Count; m++)
                            {
                                GeoDataRow row2 = vectorlyr.DataTable[m];
                                if (!(row2.Geometry.Bound.IsIntersectWith(row1.Geometry.Bound)))
                                {
                                    continue;
                                }
                                if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear ||
                                    row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                                    continue;
                                GeoLineString line2 = row2.Geometry as GeoLineString;
                                if (IsRepetitiveLine(line1, line2))
                                {
                                    //还需判断属性是否相同
                                    int nn = 0;
                                    if (row1.Table.Columns.Count == row2.Table.Columns.Count)
                                    {
                                        for (int n = 2; n < row1.Table.Columns.Count; n++)
                                        {
                                            if (row1.Table.Columns[n] == row2.Table.Columns[n])
                                            {
                                                nn = n;
                                            }
                                        }
                                        if (nn == row1.Table.Columns.Count - 1)
                                        {
                                            row1.SelectState = true;
                                            row2.SelectState = true;
                                            m_Map.AddSltObj(row1);
                                            m_Map.AddSltObj(row2);
                                            //vectorlyr.DataTable.Rows.Remove(row2);

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }
            return true;
        }

        public bool IsRepetitiveLine(GeoLineString line1, GeoLineString line2)
        {
            int mm = 0;
            bool bbreak = false;
            if (line1.Vertices.Count == line2.Vertices.Count && line1.Length == line2.Length)
            {
                for (int i = 0; i < line1.Vertices.Count; i++)
                {
                    if (line1.Vertices[i].IsEqual(line2.Vertices[i]))
                    {
                        mm = i;
                        continue;
                    }
                    if (line1.Vertices[line1.Vertices.Count - 1 - i].IsEqual(line2.Vertices[i]))
                    {
                        mm = i;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (mm == line1.Vertices.Count - 1)
                {
                    bbreak = true;
                }
            }

            if (bbreak)
                return true;
            else
                return false;
        }
        #endregion

        #region//伪点自动对接
        public bool FalseNodeLinked()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;
                        LinkLines(vectorlyr);

                        //if (IsFalseNode(line1,line2))
                        //{
                        //    //MessageBox.Show("OK");
                        //    row1.SelectState = true;
                        //    row2.SelectState = true;
                        //    m_Map.AddSltObj(row1);
                        //    m_Map.AddSltObj(row2);                                        
                        //}

                    }

                }

            }
            return true;
        }

        public bool LinkLines(GeoVectorLayer vectorlyr)
        {
            int index1, index2, num1, num2;
            int id1 = 0, id2 = 0;
            bool eNode, sNode, start;
            GeoPoint startPt, endPt;
            for (index1 = 0; index1 < vectorlyr.DataTable.Count - 1; index1++)
            {
                GeoDataRow row1 = vectorlyr.DataTable[index1];
                if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                    || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line1 = row1.Geometry as GeoLineString;

                GeoLineString line2 = new GeoLineString();
                startPt = line1.StartPoint;
                endPt = line1.EndPoint;
                if (startPt.IsEqual(endPt))//线的起点等于终点
                {
                    continue;
                }

                start = false;
                #region //1.判断线line1起点情况
                num1 = 0;
                for (index2 = 0; index2 < vectorlyr.DataTable.Count; index2++)
                {
                    if (index1 == index2)
                    {
                        continue;
                    }
                    GeoDataRow row2 = vectorlyr.DataTable[index2];
                    if (!(row2.Geometry.Bound.IsIntersectWith(row1.Geometry.Bound)))
                    {
                        continue;
                    }
                    if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                        || row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                        continue;
                    line2 = row2.Geometry as GeoLineString;

                    eNode = sNode = false;
                    if (row1.Table.Columns[2] == row2.Table.Columns[2])
                    {
                        if (startPt.IsEqual(line2.StartPoint))
                            sNode = true;
                        if (startPt.IsEqual(line2.EndPoint))
                            eNode = true;
                        if (sNode && eNode)
                        {
                            num1 = 2;//真结点
                            break;
                        }
                        if (sNode || eNode)
                        {
                            num1++;
                            id1 = index2;//记录与该起点相等的线索引
                            if (sNode)
                            {
                                start = true;
                            }
                            else
                            {
                                start = false;
                            }
                            if (num1 >= 2)
                            {
                                break;
                            }
                        }
                        else
                            continue;
                    }
                }

                if (num1 == 1) //line起点是伪结点
                {
                    GeoDataRow DeletRow = vectorlyr.DataTable[id1];
                    GeoLineString DeletLine = DeletRow.Geometry as GeoLineString;
                    if (start) //起点等于某线起点
                    {
                        for (index2 = 1; index2 < DeletLine.Vertices.Count; index2++)//从1位置开始赋值
                        {
                            line1.Vertices.Insert(0, DeletLine.Vertices[index2]);
                        }
                    }
                    else  //起点等于某线终点
                    {
                        for (index2 = DeletLine.Vertices.Count - 2; index2 >= 0; index2--)
                        {
                            line1.Vertices.Insert(0, DeletLine.Vertices[index2]);
                        }
                    }

                    vectorlyr.DataTable.Rows.RemoveAt(id1);
                }
                #endregion

                start = false;
                #region //2.判断线line终点情况
                num2 = 0;
                GeoLineString line3 = new GeoLineString();
                for (index2 = 0; index2 < vectorlyr.DataTable.Count; index2++)
                {
                    if (index1 == index2)
                    {
                        continue;
                    }
                    GeoDataRow row3 = vectorlyr.DataTable[index2];
                    if (!(row3.Geometry.Bound.IsIntersectWith(row1.Geometry.Bound)))
                    {
                        continue;
                    }
                    if (row3.EditState == EditState.Invalid || row3.EditState == EditState.Disappear
                        || row3.EditState == EditState.AttributeBef || row3.EditState == EditState.GeometryBef)
                        continue;
                    line3 = row3.Geometry as GeoLineString;

                    eNode = sNode = false;
                    if (row1.Table.Columns[2] == row3.Table.Columns[2])
                    {
                        if (endPt.IsEqual(line3.StartPoint))
                            sNode = true;
                        if (endPt.IsEqual(line3.EndPoint))
                            eNode = true;
                        if (sNode && eNode)
                        {
                            num2 = 2;//真结点
                            break;
                        }
                        if (sNode || eNode)
                        {
                            num2++;
                            id2 = index2;//记录与该起点相等的线索引
                            if (sNode)
                            {
                                start = true;
                            }
                            else
                            {
                                start = false;
                            }
                            if (num2 >= 2)
                            {
                                break;
                            }
                        }
                        else
                            continue;
                    }
                }

                if (num2 == 1) //line终点是伪结点
                {
                    GeoDataRow DeletRow = vectorlyr.DataTable[id2];
                    GeoLineString DeletLine = DeletRow.Geometry as GeoLineString;
                    if (start) //终点等于某线起点
                    {
                        for (index2 = 1; index2 < DeletLine.Vertices.Count; index2++)//从1位置开始赋值
                        {
                            line1.Vertices.Add(DeletLine.Vertices[index2]);
                        }
                    }
                    else  //终点等于某线终点
                    {
                        for (index2 = DeletLine.Vertices.Count - 2; index2 >= 0; index2--)
                        {
                            line1.Vertices.Add(DeletLine.Vertices[index2]);
                        }
                    }

                    vectorlyr.DataTable.Rows.RemoveAt(id2);
                }
                #endregion

            }
            return true;
        }
        #endregion 

        #region//标识零长度对象
        public bool ZeroLengthLine()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;
                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;

                        for (int k = 0; k < vectorlyr.DataTable.Count; k++)
                        {
                            GeoDataRow row1 = vectorlyr.DataTable[k];
                            if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                                || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                                continue;
                            GeoLineString line1 = row1.Geometry as GeoLineString;
                            if (line1.StartPoint.IsEqual(line1.EndPoint) && line1.Length < 0.005) //0.005
                            {
                                row1.SelectState = true;
                                m_Map.AddSltObj(row1);
                                //vectorlyr.DataTable.Rows.RemoveAt(k);
                            }

                        }

                    }

                }

            }
            return true;
        }
        #endregion 

        #region //线段稀释
        public bool Simplify_Line(float tt)
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;
                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;

                        for (int k = 0; k < vectorlyr.DataTable.Count; k++)
                        {
                            GeoDataRow row1 = vectorlyr.DataTable[k];
                            if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                                || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                                continue;
                            GeoLineString line1 = row1.Geometry as GeoLineString;
                            if (line1.Length < tt)
                            {
                                vectorlyr.DataTable.Rows.Remove(row1);
                                continue;
                            }

                            if (line1.Vertices.Count >= 3 && line1.Length >= tt)
                            {
                                for (int m = 0; m < line1.Vertices.Count - 1; m++)
                                {
                                    int n = m + 1;
                                    float len = (float)line1.Vertices[m].DistanceTo(line1.Vertices[m + 1]);
                                    if (len < tt)
                                    {
                                        line1.Vertices.RemoveAt(n);
                                        m--;
                                        continue;
                                    }
                                }
                                if (line1.Vertices.Count == 1)
                                    vectorlyr.DataTable.Rows.Remove(row1);

                            }

                        }

                    }

                }

            }
            return true;
        }
        #endregion 

        //悬点>>:延伸未及、删除悬挂、捕捉节点簇
        #region//删除悬挂线
        public bool DeletHangingLine(float tt)
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;
                        //SpiltSelfIntersect(vectorlyr);
                        //SpiltTwoLinesIntersection(vectorlyr);
                        ComputerDeletHangLine(vectorlyr, tt);
                    }

                }

            }
            return true;
        }

        public bool ComputerDeletHangLine(GeoVectorLayer layer, float tt)
        {
            bool startNode, endNode;
            for (int i = 0; i < layer.DataTable.Count; i++)
            {
                endNode = startNode = false;
                GeoDataRow row1 = layer.DataTable[i];
                if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                    || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line1 = row1.Geometry as GeoLineString;
                if (line1.StartPoint.IsEqual(line1.EndPoint))
                {
                    continue;
                }
                for (int j = 0; j < layer.DataTable.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    GeoDataRow row2 = layer.DataTable[j];
                    if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                        || row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                        continue;
                    GeoLineString line2 = row2.Geometry as GeoLineString;

                    for (int m = 0; m < line2.Vertices.Count; m++)
                    {
                        if (line1.StartPoint.IsEqual(line2.Vertices[m]))
                        {
                            startNode = true;
                            break;
                        }
                    }

                    for (int n = 0; n < line2.Vertices.Count; n++)
                    {
                        if (line1.EndPoint.IsEqual(line2.Vertices[n]))
                        {
                            endNode = true;
                            break;
                        }
                    }

                    if (startNode && endNode)
                    {
                        break;
                    }
                }

                if (!startNode && !endNode)  //zhh++ 说明是单独的线
                {
                    continue;
                }

                if (!startNode || !endNode)  //如果起点或终点是度为1的点，则此线是悬挂线
                {
                    //row1.SelectState = true;
                    //m_Map.AddSltObj(row1);
                    if (line1.Length <= tt)
                    {
                        layer.DataTable.Rows.RemoveAt(i);
                        i--;
                    }
                    else
                        continue;
                }

            }

            return true;
        }
        #endregion

        #region//延伸未及点
        public bool Extend_PointToLine(float tt)
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer vectorlyr = lyr as GeoVectorLayer;
                        Do_Extend_PointToLine(vectorlyr, tt);
                        //SpiltSelfIntersect(vectorlyr);
                        //SpiltTwoLinesIntersection(vectorlyr);
                    }
                }
            }
            return true;
        }

        public bool Do_Extend_PointToLine(GeoVectorLayer layer, float tt)
        {
            for (int i = 0; i < layer.DataTable.Count; i++)
            {
                GeoDataRow row1 = layer.DataTable[i];
                if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                    || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line1 = row1.Geometry as GeoLineString;
                if (line1.Length <= 0.005)
                    continue;
                GeoPoint startPoint = line1.StartPoint;
                GeoPoint endPoint = line1.EndPoint;
                for (int j = 0; j < layer.DataTable.Count; j++)
                {
                    GeoDataRow row2 = layer.DataTable[j];
                    if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                        || row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                        continue;
                    GeoLineString line2 = row2.Geometry as GeoLineString;
                    if (line2.Length <= 0.005)
                        continue;
                    int ptIndex = 0;

                    GeoPoint ptVertical;
                    int index;
                    //startpoint
                    if (GeoAlgorithm.VerticalPtofPtToLineString(startPoint, line2, out ptVertical, out index))
                    {
                        GeoPoint pt1 = line2.Vertices[index - 1];
                        GeoPoint pt2 = line2.Vertices[index];
                        GeoPoint pt3 = line1.Vertices[1];
                        GeoPoint pt4 = line1.Vertices[0];
                        if (GeoAlgorithm.DistanceOfPtToLine(startPoint, pt1, pt2) < (tt * tt))
                        {
                            double crossX, crossY;
                            if (GeoAlgorithm.CrossPtOfTwoLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y, out crossX, out crossY))
                            {
                                GeoLineString line = new GeoLineString();
                                line.AddPoint(pt3);
                                line.AddPoint(new GeoPoint(crossX, crossY));
                                int CrossPtIndex = 0;
                                if (IsPtInLine(line2, new GeoPoint(crossX, crossY), ref CrossPtIndex) && IsPtInLine(line, startPoint, ref ptIndex))
                                {
                                    line1.Vertices.Remove(startPoint);
                                    line1.Vertices.Insert(0, new GeoPoint(crossX, crossY));
                                    line2.Vertices.Insert(CrossPtIndex + 1, new GeoPoint(crossX, crossY));
                                }
                            }
                        }
                    }

                    //endpoint
                    if (GeoAlgorithm.VerticalPtofPtToLineString(endPoint, line2, out ptVertical, out index))
                    {
                        GeoPoint pt1 = line2.Vertices[index - 1];
                        GeoPoint pt2 = line2.Vertices[index];
                        GeoPoint pt3 = line1.Vertices[line1.Vertices.Count - 2];
                        GeoPoint pt4 = line1.Vertices[line1.Vertices.Count - 1];
                        if (GeoAlgorithm.DistanceOfPtToLine(endPoint, pt1, pt2) < (tt * tt))
                        {
                            double crossX, crossY;
                            if (GeoAlgorithm.CrossPtOfTwoLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y, out crossX, out crossY))
                            {
                                GeoLineString line = new GeoLineString();
                                line.AddPoint(pt3);
                                line.AddPoint(new GeoPoint(crossX, crossY));
                                int CrossPtIndex = 0;
                                if (IsPtInLine(line2, new GeoPoint(crossX, crossY), ref CrossPtIndex) && IsPtInLine(line, endPoint, ref ptIndex))
                                {
                                    line1.Vertices.Remove(endPoint);
                                    line1.AddPoint(new GeoPoint(crossX, crossY));
                                    line2.Vertices.Insert(CrossPtIndex + 1, new GeoPoint(crossX, crossY));
                                }
                            }
                        }
                    }

                    DeleteRepeatpt(line1);
                    DeleteRepeatpt(line2);

                }

            }

            return true;
        }

        //判断点在线上，保证沿着线的方向
        private bool IsPtInLine(GeoLineString line, GeoPoint pt, ref int ptindex)
        {
            for (int i = 0; i < line.Vertices.Count - 1; i++)
            {
                GeoPoint pt1 = line.Vertices[i];
                GeoPoint pt2 = line.Vertices[i + 1];
                if (GeoAlgorithm.PtToLine(pt, pt1, pt2) == 3 || GeoAlgorithm.PtToLine(pt, pt1, pt2) == 1 || GeoAlgorithm.PtToLine(pt, pt1, pt2) == 2)
                {
                    ptindex = i;
                    return true;
                }
            }
            return false;
        }

        //删除线中重复点
        private bool DeleteRepeatpt(GeoLineString line)
        {
            for (int i = 0; i < line.Vertices.Count - 1; i++)
            {
                GeoPoint pt1 = line.Vertices[i];
                GeoPoint pt2 = line.Vertices[i + 1];
                if (pt1.IsEqual(pt2))
                {
                    line.Vertices.Remove(pt2);
                }
            }

            return true;
        }

        #endregion

        #region //捕捉节点簇
        public bool CatchNodes(float tt)
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer
                        && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer layer = lyr as GeoVectorLayer;
                        Nodes_Fit(layer, tt);
                    }

                }

            }
            return true;
        }

        //构造函数
        public class Node
        {
            public Node(GeoLineString line, GeoPoint point)
            {
                _line = line;
                _point = point;
            }
            public GeoLineString _line;
            public GeoPoint _point;

        }

        public bool Nodes_Fit(GeoVectorLayer layer, float tt)
        {
            List<Node> NodeList = new List<Node>();
            for (int i = 0; i < layer.DataTable.Count; i++)
            {
                GeoDataRow row1 = layer.DataTable[i];
                if (row1.EditState == EditState.Invalid || row1.EditState == EditState.Disappear
                    || row1.EditState == EditState.AttributeBef || row1.EditState == EditState.GeometryBef)
                    continue;
                GeoLineString line1 = row1.Geometry as GeoLineString;
                if (line1.Length <= tt)
                    continue;
                GeoPoint pt1 = line1.StartPoint;
                GeoPoint pt2 = line1.EndPoint;
                GeoPoint Fit_Point = new GeoPoint(); //拟合的点

                //起点
                for (int j = 0; j < layer.DataTable.Count; j++)
                {
                    GeoDataRow row2 = layer.DataTable[j];
                    if (row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                        || row2.EditState == EditState.AttributeBef || row2.EditState == EditState.GeometryBef)
                        continue;
                    GeoLineString line2 = row2.Geometry as GeoLineString;
                    GeoPoint pt3 = line2.StartPoint;
                    GeoPoint pt4 = line2.EndPoint;
                    if (GeoAlgorithm.DistanceOfTwoPt(pt1, pt3) <= (tt * tt) && !(pt1.IsEqual(pt3)))
                    {
                        NodeList.Add(new Node(line2, line2.Vertices[0]));
                    }

                    if (GeoAlgorithm.DistanceOfTwoPt(pt1, pt4) <= (tt * tt) && !(pt1.IsEqual(pt4)))
                    {
                        NodeList.Add(new Node(line2, line2.Vertices[line2.Vertices.Count - 1]));
                    }
                }


                if (NodeList.Count != 0)
                {
                    Fit_Point.X = pt1.X;
                    Fit_Point.Y = pt1.Y;
                    for (int m = 0; m < NodeList.Count; m++)
                    {
                        Fit_Point.X += NodeList[m]._point.X;
                        Fit_Point.Y += NodeList[m]._point.Y;
                    }
                    Fit_Point.X /= (NodeList.Count + 1);
                    Fit_Point.Y /= (NodeList.Count + 1);

                    line1.Vertices.Remove(pt1);
                    line1.Vertices.Insert(0, new GeoPoint(Fit_Point.X, Fit_Point.Y));

                    for (int n = 0; n < NodeList.Count; n++)
                    {
                        GeoLineString sline = NodeList[n]._line;
                        GeoPoint spt = NodeList[n]._point;
                        if (spt.IsEqual(sline.StartPoint))
                        {
                            sline.Vertices.Remove(spt);
                            sline.Vertices.Insert(0, new GeoPoint(Fit_Point.X, Fit_Point.Y));
                        }
                        if (spt.IsEqual(sline.EndPoint))
                        {
                            sline.Vertices.Remove(spt);
                            sline.AddPoint(new GeoPoint(Fit_Point.X, Fit_Point.Y));
                        }
                    }

                }
                NodeList.Clear();

                //终点
                pt2 = line1.EndPoint;
                for (int k = 0; k < layer.DataTable.Count; k++)
                {
                    GeoDataRow row3 = layer.DataTable[k];
                    if (row3.EditState == EditState.Invalid || row3.EditState == EditState.Disappear
                       || row3.EditState == EditState.AttributeBef || row3.EditState == EditState.GeometryBef)
                        continue;
                    GeoLineString line3 = row3.Geometry as GeoLineString;
                    GeoPoint pt3 = line3.StartPoint;
                    GeoPoint pt4 = line3.EndPoint;

                    if (GeoAlgorithm.DistanceOfTwoPt(pt2, pt3) <= (tt * tt) && !(pt2.IsEqual(pt3)))
                    {
                        NodeList.Add(new Node(line3, line3.Vertices[0]));
                    }

                    if (GeoAlgorithm.DistanceOfTwoPt(pt2, pt4) <= (tt * tt) && !(pt2.IsEqual(pt4)))
                    {
                        NodeList.Add(new Node(line3, line3.Vertices[line3.Vertices.Count - 1]));
                    }

                }

                if (NodeList.Count != 0)
                {
                    Fit_Point.X = pt2.X;
                    Fit_Point.Y = pt2.Y;
                    for (int p = 0; p < NodeList.Count; p++)
                    {
                        Fit_Point.X += NodeList[p]._point.X;
                        Fit_Point.Y += NodeList[p]._point.Y;
                    }
                    Fit_Point.X /= (NodeList.Count + 1);
                    Fit_Point.Y /= (NodeList.Count + 1);

                    line1.Vertices.Remove(pt2);
                    line1.AddPoint(new GeoPoint(Fit_Point.X, Fit_Point.Y));

                    for (int q = 0; q < NodeList.Count; q++)
                    {
                        GeoLineString eline = NodeList[q]._line;
                        GeoPoint ept = NodeList[q]._point;
                        if (ept.IsEqual(eline.StartPoint))
                        {
                            eline.Vertices.Remove(ept);
                            eline.Vertices.Insert(0, new GeoPoint(Fit_Point.X, Fit_Point.Y));
                        }
                        if (ept.IsEqual(eline.EndPoint))
                        {
                            eline.Vertices.Remove(ept);
                            eline.AddPoint(new GeoPoint(Fit_Point.X, Fit_Point.Y));
                        }
                    }
                }
                NodeList.Clear();


            }
            return true;
        }

        #endregion 

        #region//增量质量控制
        public void IncreTP()
        {
            for (int i = 0; i < m_Map.GroupCounts; i++)
            {
                LayerGroup lyrgrp = m_Map.GetGroupAt(i);
                if (lyrgrp.LayerGroupName == "系统工作区")
                    continue;

                for (int j = 0; j < lyrgrp.Counts; j++)
                {
                    GeoLayer lyr = lyrgrp[j];
                    if (lyr.LayerType == LAYERTYPE.VectorLayer && lyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                    {
                        GeoVectorLayer layer = lyr as GeoVectorLayer;
                        for (int k = 0; k < layer.DataTable.Count; k++)
                        {
                            GeoDataRow row = layer.DataTable[k];
                            if (row.EditState == EditState.Appear ||row.EditState == EditState.GeometryAft)
                            {
                                double toler = 2.0;
                                if (TTTTPPPPP(row, layer, toler))
                                {
                                    //k = k - 1;
                                    //k = 0;
                                    k = k - 3;
                                    if (k < 0)
                                        k = 0;
                                }
                            }


                        }

                    }

                }

            }

        }

        public bool TTTTPPPPP(GeoDataRow row, GeoVectorLayer lyr, double toler)
        {
            //相交存到can里面
            List<GeoDataRow> can = new List<GeoDataRow>();
            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoDataRow row2 = lyr.DataTable[i];
                if (row2.Equals(row) || row2.EditState == EditState.Invalid || row2.EditState == EditState.Disappear
                    || row2.EditState == EditState.GeometryBef || row2.EditState == EditState.AttributeBef)
                    continue;
                if (row.Geometry.Bound.IsIntersectWith(row2.Geometry.Bound))
                {
                    //进行质量控制，否则没必要
                    can.Add(row2);
                }
            }

            //然后再用row与can里面的row一个个做质量控制就行了撒
            //先用row里面的线的二个端点分别求到要判断线的首尾端点距离和到每一段的距离，这点到直线距离算法可以在geoalgorithm.cs里面找到，
            //如果距离小于某个值，就把端点设成交点就行或者设成两端点中点就行，这样悬点与伪点都求了
            GeoLineString line = row.Geometry as GeoLineString;
            for (int i = 0; i < can.Count; i++)
            {
                GeoDataRow row2 = can[i];
                GeoLineString line1 = can[i].Geometry as GeoLineString;
                if (line.StartPoint.DistanceTo(line1.StartPoint) < toler)
                {
                    line.Vertices[0] = new GeoPoint((line.StartPoint.X + line1.StartPoint.X) / 2,
                         (line.StartPoint.Y + line1.StartPoint.Y) / 2);
                }
                if (line.StartPoint.DistanceTo(line1.EndPoint) < toler)
                {
                    line.Vertices[0] = new GeoPoint((line.StartPoint.X + line1.EndPoint.X) / 2,
                         (line.StartPoint.Y + line1.EndPoint.Y) / 2);
                }
                if (line.EndPoint.DistanceTo(line1.StartPoint) < toler)
                {
                    line.Vertices[line.NumPoints - 1] = new GeoPoint((line.EndPoint.X + line1.StartPoint.X) / 2,
                         (line.EndPoint.Y + line1.StartPoint.Y) / 2);
                }
                if (line.EndPoint.DistanceTo(line1.EndPoint) < toler)
                {
                    line.Vertices[line.NumPoints - 1] = new GeoPoint((line.EndPoint.X + line1.EndPoint.X) / 2,
                         (line.EndPoint.Y + line1.EndPoint.Y) / 2);
                }


                int j = 0;
                double interx = 0, intery = 0;
                for (; j < line1.Vertices.Count - 1; j++)
                {
                    //startpoint 到各段的距离
                    if (line.StartPoint.IsEqual(line1.Vertices[j]) || line.StartPoint.IsEqual(line1.Vertices[j + 1]))
                        break;
                    double dist = GeoAlgorithm.DistanceOfPtToLine(line.StartPoint, line1.Vertices[j], line1.Vertices[j + 1]);

                    if (dist < toler * toler && dist > 0.5)
                    {
                        //求交点

                        GeoAlgorithm.CrossPtOfTwoLine(line.Vertices[0].X, line.Vertices[0].Y, line.Vertices[1].X, line.Vertices[1].Y,
                            line1.Vertices[j].X, line1.Vertices[j].Y, line1.Vertices[j + 1].X, line1.Vertices[j + 1].Y,
                            out interx, out intery);
                        line.Vertices[0].X = interx;
                        line.Vertices[0].Y = intery;
                        break;
                    }
                }

                j = 0;
                for (; j < line1.Vertices.Count - 1; j++)
                {
                    //startpoint 到各段的距离
                    double dist = GeoAlgorithm.DistanceOfPtToLine(line.EndPoint, line1.Vertices[j], line1.Vertices[j + 1]);
                    if (dist < toler * toler && dist > 0.5)
                    {
                        interx = 0;
                        intery = 0;
                        int cnt = line.Vertices.Count;
                        GeoAlgorithm.CrossPtOfTwoLine(line.Vertices[cnt - 1].X, line.Vertices[cnt - 1].Y, line.Vertices[cnt - 2].X, line.Vertices[cnt - 2].Y,
                            line1.Vertices[j].X, line1.Vertices[j].Y, line1.Vertices[j + 1].X, line1.Vertices[j + 1].Y,
                            out interx, out intery);
                        line.Vertices[cnt - 1].X = interx;
                        line.Vertices[cnt - 1].Y = intery;
                        break;
                    }
                }

                //最后做判断线线是否相交
                if (SpiltTwoLinesInter(row, row2, lyr))
                    return true;
            }
            return false;
        }

        public bool SpiltTwoLinesInter(GeoDataRow row1, GeoDataRow row2, GeoVectorLayer lyr)
        {
            GeoLineString line1 = row1.Geometry as GeoLineString;
            if (line1.StartPoint.IsEqual(line1.EndPoint) && line1.Length < 0.001)
            {
                return false;
            }

            GeoLineString line2 = row2.Geometry as GeoLineString;
            if (line2.StartPoint.IsEqual(line2.EndPoint) && line2.Length < 0.005)
            {
                return false;
            }

            GeoLineString newline11 = new GeoLineString();
            GeoLineString newline12 = new GeoLineString();
            GeoLineString newline22 = new GeoLineString();
            GeoLineString newline21 = new GeoLineString();

            double cx = 0, cy = 0;
            GeoPoint pt0, pt1, pt2, pt3;
            int i = 0;
            int j = 0;
            bool bbreak = false;

            for (; i < line1.Vertices.Count - 1; i++)
            {
                pt0 = line1.Vertices[i];
                pt1 = line1.Vertices[i + 1];
                GeoLineString m_line1 = new GeoLineString();
                m_line1.AddPoint(pt0);
                m_line1.AddPoint(pt1);

                j = 0;
                for (; j < line2.Vertices.Count - 1; j++)
                {
                    pt2 = line2.Vertices[j];
                    pt3 = line2.Vertices[j + 1];
                    GeoLineString m_line2 = new GeoLineString();
                    m_line2.AddPoint(pt2);
                    m_line2.AddPoint(pt3);

                    if (GeoAlgorithm.IntersectTwoLine(pt0, pt1, pt2, pt3))
                    {
                        GeoAlgorithm.CrossPtOfTwoLine(pt0.X, pt0.Y, pt1.X, pt1.Y, pt2.X, pt2.Y,
                            pt3.X, pt3.Y, out cx, out cy);


                        if (brepeat(m_line1, new GeoPoint(cx, cy)) && brepeat(m_line2, new GeoPoint(cx, cy)))
                        {
                            continue;
                        }
                        if (cx == 1e20 && cy == 1e20)
                        {
                            continue;
                        }


                        bbreak = true;
                        break;

                    }
                }
                if (bbreak)
                    break;
            }

            if (bbreak)
            {
                for (int m = 0; m <= i; m++)
                {
                    newline11.AddPoint(new GeoPoint(line1.Vertices[m].X, line1.Vertices[m].Y));
                }
                newline11.AddPoint(new GeoPoint(cx, cy));

                newline12.AddPoint(new GeoPoint(cx, cy));
                for (int n = i + 1; n < line1.Vertices.Count; n++)
                {
                    newline12.AddPoint(new GeoPoint(line1.Vertices[n].X, line1.Vertices[n].Y));
                }

                for (int p = 0; p <= j; p++)
                {
                    newline21.AddPoint(new GeoPoint(line2.Vertices[p].X, line2.Vertices[p].Y));
                }
                newline21.AddPoint(new GeoPoint(cx, cy));

                newline22.AddPoint(new GeoPoint(cx, cy));
                for (int q = j + 1; q < line2.Vertices.Count; q++)
                {
                    newline22.AddPoint(new GeoPoint(line2.Vertices[q].X, line2.Vertices[q].Y));
                }

                GeoDataRow row11 = row1.Clone_rb();
                GeoDataRow row12 = row1.Clone_rb();
                GeoDataRow row21 = row2.Clone_rb();
                GeoDataRow row22 = row2.Clone_rb();

                row11.Geometry = newline11;
                row12.Geometry = newline12;
                row21.Geometry = newline21;
                row22.Geometry = newline22;

                row11.EditState = row1.EditState;
                row12.EditState = row1.EditState;
                row21.EditState = row2.EditState;
                row22.EditState = row2.EditState;


                row11[0] = lyr.DataTable.Count;
                row12[0] = lyr.DataTable.Count + 1;
                row21[0] = lyr.DataTable.Count + 2;
                row22[0] = lyr.DataTable.Count + 3;


                if (!(newline11.StartPoint.IsEqual(newline11.EndPoint) && newline11.Length < 0.005))
                {
                    lyr.DataTable.AddRow(row11);
                }
                if (!(newline12.StartPoint.IsEqual(newline12.EndPoint) && newline12.Length < 0.005))
                {
                    lyr.DataTable.AddRow(row12);
                }
                if (!(newline21.StartPoint.IsEqual(newline21.EndPoint) && newline21.Length < 0.005))
                {
                    lyr.DataTable.AddRow(row21);
                }
                if (!(newline22.StartPoint.IsEqual(newline22.EndPoint) && newline22.Length < 0.005))
                {
                    lyr.DataTable.AddRow(row22);
                }

                lyr.DataTable.Rows.Remove(row1);
                lyr.DataTable.Rows.Remove(row2);
                return true;
            }
            return false;










        }
        #endregion

    }
}
