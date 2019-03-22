using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.IO;
using GIS.GeoData;
using GIS.Layer;
using GIS.Geometries;
namespace GIS.GeoData.DataProviders
{
    public class DxfFile
    {
        public class DXFInsert
        {
            public GeoPoint Point0 = new GeoPoint();
            public double XScale =1;
            public double YScale =1 ;
            public double ZScale = 1;
            public double RotationAngle =0.0;
            public string BlockHeaderName;
        }

        class DXFBlockHeader
        {
            public string Name;				// 块名
            public string Flags;					// 块类型注记
            public GeoPoint BasePoint = new GeoPoint();				// 基点
            //public string LayerName;			// 图层名
            public List<Geometries.Geometry> block_geo= new List<Geometry>();          //图形
            public List<GeoLabel> block_lable=  new List<GeoLabel>();
        } 
        private string m_PathName;
        private string m_LayerName;
        private GeoBound m_Extents;
        private short m_GCode;
        private string m_Value;
        private List<Geometries.Geometry> block_geo = new List<Geometry>();
        private List<GeoLabel> block_label = new List<GeoLabel>();
        private List<DXFBlockHeader> m_BlockHeaders = new List<DXFBlockHeader>();
        private List<GeoVectorLayer> m_LayerList = new List<GeoVectorLayer>();
        private GeoVectorLayer m_LabelLayer;

        public List<GeoVectorLayer> LayerList
        {
            get { return m_LayerList; } 
        }

        DXFBlockHeader GetBlock(string name)
        {
            int numb = m_BlockHeaders.Count;
            for (int i = 0; i < numb; i++)
            {
                if (m_BlockHeaders[i].Name== name)
                return m_BlockHeaders[i];
            }
            return null;
        }
        private void ReadParam(StreamReader sr)
        {
            m_GCode = short.Parse(sr.ReadLine().Trim());
            m_Value = sr.ReadLine().Trim(); 
        }
        public void AddLabelLayer()
        {
            GeoData.GeoDataTable table = null;
            string LayerPath = Path.GetDirectoryName(m_PathName) + "\\" + Path.GetFileNameWithoutExtension(m_PathName) + "(注记)" + ".dxf";
            m_LabelLayer = new GeoVectorLayer(LayerPath);
            m_LabelLayer.VectorType = VectorLayerType.LabelLayer; 
            m_LabelLayer.DataTable = new GIS.GeoData.GeoDataTable();
            table = m_LabelLayer.DataTable;

            table.FillData = true; ///空图层，属性信息已经填充，因为是空的
            table.Columns.Add("FID", typeof(int));
            table.Columns.Add("FeatID", typeof(string));
            table.Columns.Add("ClasID", typeof(Int64));
            table.Columns.Add("BeginTime", typeof(string));
            table.Columns.Add("ChangeType", typeof(string));
            m_LayerList.Add(m_LabelLayer);
        }


        public DxfFile(string strFileName)
        {
            m_PathName = strFileName;
            m_LayerName = Path.GetFileNameWithoutExtension(strFileName);
            AddLabelLayer();
            StreamReader sr = new StreamReader(m_PathName,Encoding.Default);
            try
            {
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                if (FindText(sr, "HEADER"))
                {
                    ReadRange(sr);
                }
                if (FindText(sr, "BLOCKS"))
                {
                    // ReadBlockHeaderData(sr);
                }
                if (FindText(sr, "ENTITIES"))
                {
                    ReadEntities(sr);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                sr.Close();
            }
             
        }
        private GeoVectorLayer GetLayer(string lyrName)
        {
            for (int i = 0; i < m_LayerList.Count; i++)
            {
                if (m_LayerList[i].LayerName == lyrName)
                {
                    return m_LayerList[i];
                }
            }

            GeoVectorLayer layer = null;
            GeoData.GeoDataTable table = null;
            string LayerPath = Path.GetDirectoryName(m_PathName) +"\\"+ lyrName+".dxf";
            layer = new GeoVectorLayer(LayerPath);
            layer.DataTable = new GIS.GeoData.GeoDataTable();
            table = layer.DataTable;
            layer.VectorType = VectorLayerType.MixLayer; 

            table.FillData = true; ///空图层，属性信息已经填充，因为是空的
            table.Columns.Add("FID", typeof(int));
            table.Columns.Add("FeatID", typeof(string));
            table.Columns.Add("ClasID", typeof(Int64));
            table.Columns.Add("BeginTime", typeof(string));
            table.Columns.Add("ChangeType", typeof(string));
            m_LayerList.Add(layer);
            return layer;
        }
        private bool FindText(StreamReader sr, string text)
        {
            string strTemp = sr.ReadLine();
            while (strTemp != null)
            {
                if (strTemp == text)
                {
                    return true;
                }
                strTemp = sr.ReadLine();
            }
            return false;
        }
        public void ReadRange(StreamReader sr)
        {
            m_Extents = new GeoBound();
            if (FindText(sr, "$EXTMIN"))
            {
                ReadParam(sr);
                m_Extents.Left = double.Parse(m_Value);
                ReadParam(sr);
                m_Extents.Bottom = double.Parse(m_Value);
                ReadParam(sr);


            }
            if (FindText(sr, "$EXTMAX"))
            {
                ReadParam(sr);
                m_Extents.Right = double.Parse(m_Value);
                ReadParam(sr);
                m_Extents.Top = double.Parse(m_Value);
                ReadParam(sr);
            }
        }

        private bool ReadEntities(StreamReader sr)
        {
            ReadParam(sr);
            if (m_GCode != 0) return false;

            do
            {
                if (m_Value == "POINT")
                {
                    ReadPointData(sr, block_geo, false);
                }
                if (m_Value == "LINE")
                {
                    ReadLineData(sr, block_geo, false);

                }
                else if (m_Value == "LWPOLYLINE")
                {
                    ReadLWPolyline(sr, block_geo, false);

                }
                else if (m_Value == "POLYLINE")
                {
                    ReadPolyline(sr, block_geo, false);

                }
                else  if (m_Value == "TEXT")
                {
                    ReadTextData(sr, block_label, false);
                }
                //else if (m_Value == "INSERT")
                //{
                //    ReadInsertData(sr);
                //}
                else if (m_Value == "CIRCLE")
                {
                    ReadCircleData(sr, block_geo, false);
                }
                else
                { //读取其他实体
                    do
                    {
                        ReadParam(sr);
                    } while (m_GCode != 0);
                }
            } while ((m_Value != null) && m_Value != "ENDSEC");
            return true;
        }

        private void ReadInsertData(StreamReader sr)
        {

            DXFInsert pInsert = new DXFInsert();
            ReadParam(sr);
            GeoVectorLayer plyr = null;
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 2:
                        pInsert.BlockHeaderName = m_Value;
                        break;
                    case 10:
                        pInsert.Point0.X = double.Parse(m_Value);
                        break;
                    case 20:
                        pInsert.Point0.Y = double.Parse(m_Value);
                        break;
                    case 41:
                        pInsert.XScale = double.Parse(m_Value);
                        break;
                    case 42:
                        pInsert.YScale = double.Parse(m_Value);
                        break;
                    case 43:
                        pInsert.ZScale = double.Parse(m_Value);
                        break;
                    case 50:
                        pInsert.RotationAngle = double.Parse(m_Value);
                        break;
                    case 8:
                        plyr = GetLayer(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            DXFBlockHeader block = GetBlock(pInsert.BlockHeaderName);


            if (block != null)
            {
                int textNumb = block.block_lable.Count;
                for (int i = 0; i < textNumb; ++i)
                {
                    GeoLabel lable = block.block_lable[i].Clone() as GeoLabel;
                    try
                    {
                       
                        lable.Move(pInsert.Point0.X - block.BasePoint.X, pInsert.Point0.Y - block.BasePoint.Y);
                        lable.Angle = -pInsert.RotationAngle;
                        m_LabelLayer.AddGeometry(lable);
                    }
                    catch (Exception e)
                    { 
                        throw new Exception(e.Message);
                    }
                }
                int geoNumb = block.block_geo.Count;
                for (int i = 0; i < geoNumb; ++i)
                {
                    Geometry geo = block.block_geo[i].Clone();
                    geo.Move(pInsert.Point0.X - block.BasePoint.X, pInsert.Point0.Y - block.BasePoint.Y);
                  //  geo.RotateAt(-(pInsert.RotationAngle * Math.PI / 180), new GeoPoint(pInsert.Point0.X, pInsert.Point0.Y));
                    plyr.AddGeometry(geo);
                }
            }

        }
        private void ReadCircleData(StreamReader sr, List<Geometry> block_geo, bool Block)
        {
            ReadParam(sr);
            double x=0, y=0, r=0;

            GeoVectorLayer layer = null;
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {

                    case 10:
                        x = double.Parse(m_Value);
                        break;
                    case 20:
                        y = double.Parse(m_Value);
                        break;

                    case 40:
                        r = double.Parse(m_Value);
                        break;
                    case 8:
                        layer = GetLayer(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            GeoPoint pt0 = new GeoPoint(x - r, y);
            GeoPoint pt1 = new GeoPoint(x, y - r);
            GeoPoint pt2 = new GeoPoint(x + r, y);

            GeoArc pArc = new GeoArc();
            pArc.ArcType = ArcType.Circle;
            pArc.SkeletonPtList.Add(pt0);
            pArc.SkeletonPtList.Add(pt1);
            pArc.SkeletonPtList.Add(pt2);

            pArc.Interpolation();
            
            if (Block)
            {
                block_geo.Add(pArc);
                return;
            }
            if (!pArc.Bound.IsIntersectWith(m_Extents))
                return;
            layer.AddGeometry(pArc);
        }

        private void ReadTextData(StreamReader sr, List<GeoLabel> block_label, bool Block)
        {
            GeoLabel lable = new GeoLabel();
            ReadParam(sr);
           
            double x=0, y=0;
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 10:
                        x = double.Parse(m_Value);
                        break;
                    case 20:
                        y = double.Parse(m_Value);
                        break;
                    case 40:
                        lable.TextSize = double.Parse(m_Value);
                        break;
                    case 50:
                        lable.Angle = double.Parse(m_Value);
                        break;
                    case 1:
                        lable.Text = m_Value;
                        break;
                }
                ReadParam(sr);
            };
            lable.StartPt = new GeoPoint(x, y);
            if (Block)
            {
                block_label.Add(lable);
                return;
            }
            m_LabelLayer.AddGeometry(lable); 
            	 
        }

        private void ReadPolyline(StreamReader sr, List<Geometry> block_geo, bool Block)
        {
            bool bclosed = false;
            GeoLineString pline = null;
            ReadParam(sr);
            GeoVectorLayer plyr = null;
           
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 70:	// 多线是否是闭合曲线（默认值0） 1为闭合
                        bclosed = ((m_Value == "1") || (m_Value == "129")) ? true : false;
                        break;
                    case 8:
                        plyr = GetLayer(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            if (bclosed)
                pline = new GeoLinearRing();
            else pline = new GeoLineString();
            double x = 0, y = 0; 
            GeoPoint pt = null; 

            while ( m_GCode == 0  &&   m_Value == "VERTEX") 
            {
                bool valid = true;
                ReadParam(sr);
                while (m_GCode != 0)
                {
                    switch (m_GCode)
                    {
                        case 10:
                            x = double.Parse(m_Value);
                            break;
                        case 20:
                            y = double.Parse(m_Value);
                            break;

                        case 70:
                            if (m_Value == "16")
                                valid = false ;
                            break;
                    }
                    ReadParam(sr);
                }
                if (valid)
                {
                    pt = new GeoPoint(x, y);
                    pline.AddPoint(pt);
                }
            }
            if (bclosed  )
            {
                if (!pline.IsClosed)
                    pline.AddPoint(pline.StartPoint.Clone() as GeoPoint);
            }
       
            if (Block)
            {
                block_geo.Add(pline);
                return;
            }
            if (!pline.Bound.IsIntersectWith(m_Extents))
                return;
            plyr.AddGeometry(pline);	
        }
        private void ReadLWPolyline(StreamReader sr, List<Geometry> block_geo, bool Block)
        {
            bool bclosed = false;
            GeoLineString pline = null;
            ReadParam(sr);
            GeoVectorLayer plyr = null;
            while (m_GCode != 10 && m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 70:	// 多线是否是闭合曲线（默认值0） 1为闭合
                        bclosed = ((m_Value == "1") || (m_Value == "129")) ? true : false;
                        break;
                    case 8:
                        plyr = GetLayer(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            if (bclosed)
                pline = new GeoLinearRing();
            else pline = new GeoLineString();
            double x =0 , y =0;
            float bugle = 0;
            bool isArc = false;
            GeoPoint m_InsertPts = null;

            GeoPoint pt = null;
            while (m_GCode !=0)
            {
                switch (m_GCode)
                {
                    case 10:
                        x = double.Parse(m_Value);                       
                        break;
                    case 20:
                        y = double.Parse(m_Value);
                        if (isArc)
                        {
                            pt = new GeoPoint(x, y);
                            isArc = false; 
                            GeoPoint pt3rd = SpatialRelation.GeoAlgorithm.g_Calu_thirdPoint(m_InsertPts, pt, bugle);
                            List<GeoPoint> pts = SpatialRelation.GeoAlgorithm.ThreePointsArc(m_InsertPts, pt3rd, pt, ArcType.Arc);
                            pline.Vertices.AddRange(pts);
                            pline.ClearRepeatPoints();

                        }
                        break;
                    case 42:
                        if (bugle == double.Parse(m_Value))
                        {
                            isArc = true;
                            m_InsertPts = new GeoPoint(x,y);
                        }
                        break;

                }
                ReadParam(sr);
                if (m_GCode == 10)
                {
                    pt = new GeoPoint(x, y);
                    if (pline.NumPoints == 0 || !(pt.IsEqual(pline.EndPoint)))
                    {
                        pline.AddPoint(pt);
                    }
                }
            }
            pt = new GeoPoint(x, y);
            if (pline.NumPoints == 0 || !(pt.IsEqual(pline.EndPoint)))
            {
                pline.AddPoint(pt);
            }

            if (bclosed)
            {
                if (!pline.IsClosed)
                    pline.AddPoint(pline.StartPoint.Clone() as GeoPoint);
            }
            if (Block)
            {
                block_geo.Add(pline);
                return;
            }
            if (!pline.Bound.IsIntersectWith(m_Extents))
                return;
            plyr.AddGeometry(pline);
        }

        private void ReadLineData(StreamReader sr, List<Geometry> block_geo, bool Block)
        {
            double x = 0;
            double y = 0;
            
            double x1 = 0;
            double y1 = 0;
           
            GeoVectorLayer plyr = null;
            ReadParam(sr);
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 10:
                        x = double.Parse(m_Value);
                        break;
                    case 20:
                        y = double.Parse(m_Value);
                        break;

                    case 11:
                        x1 = double.Parse(m_Value);
                        break;
                    case 21:
                        y1 = double.Parse(m_Value);
                        break;

                    case 8:
                        plyr = GetLayer(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            GeoPoint pt = new GeoPoint(x, y);
            GeoPoint pt1 = new GeoPoint(x1, y1);
            GeoLineString line = new GeoLineString();
            line.AddPoint(pt);
            line.AddPoint(pt1);
            if (Block)
            {
                block_geo.Add(line);
                return;
            }
            if (!line.Bound.IsIntersectWith(m_Extents))
                return;
            plyr.AddGeometry(line);
        }

        private void ReadPointData(StreamReader sr, List<Geometry> block_geo, bool Block)
        {
            double x = 0, y = 0;
            ReadParam(sr);
            GeoVectorLayer plyr = null;
            while (m_GCode != 0)
            {
                switch (m_GCode)
                {
                    case 8:
                        plyr = GetLayer(m_Value);

                        break;
                    case 10:
                        x = double.Parse(m_Value);
                        break;
                    case 20:
                        y = double.Parse(m_Value);
                        break;
                }
                ReadParam(sr);
            }
            GeoPoint pt = new GeoPoint(x, y);
            if (Block)
            {
                block_geo.Add(pt);
                return;
            }
            if (!pt.Bound.IsIntersectWith(m_Extents))
                return;
            plyr.AddGeometry(pt);

        }

        private void ReadBlockHeaderData(StreamReader sr)
        {
            ReadParam(sr);
            do
            {
                ReadParam(sr);
                DXFBlockHeader pBlockHeader = new DXFBlockHeader();
                while (m_GCode != 0 && m_Value != "BLOCK")
                {
                    ReadParam(sr);
                    switch (m_GCode)
                    {
                        case 2:
                            pBlockHeader.Name = m_Value;
                            break;
                        case 70:
                            pBlockHeader.Flags = m_Value;
                            break;
                        case 10:
                            pBlockHeader.BasePoint.X = double.Parse(m_Value);
                            break;
                        case 20:
                            pBlockHeader.BasePoint.Y = double.Parse(m_Value);
                            break;
                        case 3:
                            pBlockHeader.Name = m_Value;
                            break;
                    }
                    while (m_GCode == 0 &&
                        (m_Value == "LWPOLYLINE" || m_Value == "POINT" || m_Value == "LINE" || m_Value == "POLYLINE" || m_Value == "ARC" || m_Value == "CIRCLE"
                        || m_Value == "TEXT" || m_Value == "SOLID" || m_Value == "DIMENSION"))
                    {
                       // if (m_Value == "LWPOLYLINE") ReadLWPolyline(sr, pBlockHeader.block_geo, true);
                      //  if (m_Value == "POINT") ReadPointData(sr, pBlockHeader.block_geo, true);
                      //   if (m_Value == "LINE") ReadLineData(sr, pBlockHeader.block_geo, true);
                      //  else if (m_Value == "POLYLINE") ReadPolyline(sr, pBlockHeader.block_geo, true);
                       if (m_Value == "CIRCLE") ReadCircleData(sr, pBlockHeader.block_geo, true);
                      //  else if (m_Value == "TEXT") ReadTextData(sr, pBlockHeader.block_lable, true);
                        else ReadParam(sr);
                    }
                    if (m_GCode == 0 && m_Value == "ENDBLK")
                    {
                        do
                        {
                            ReadParam(sr);
                        } while ((m_GCode != 0 && m_Value != "BLOCK") || (m_GCode != 0 && m_Value != "ENDSEC"));
                    }
                }
                m_BlockHeaders.Add(pBlockHeader);
            } while (m_Value != "ENDSEC");
        }

    }
}
