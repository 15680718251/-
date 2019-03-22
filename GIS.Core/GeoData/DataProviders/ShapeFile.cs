using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using GIS.Geometries;

namespace GIS.GeoData.DataProviders
{
    /// <summary>
    /// Shapefile geometry type.
    /// </summary>
    public enum ShapeType : int
    {
        /// <summary>
        /// Null shape with no geometric data
        /// </summary>
        Null = 0,
        /// <summary>
        /// A point consists of a pair of double-precision coordinates.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        Point = 1,
        /// <summary>
        /// PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
        /// connected sequence of two or more points. Parts may or may not be connected to one
        ///	another. Parts may or may not intersect one another.
        /// GIS interpretes this as either <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLine = 3,
        /// <summary>
        /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
        /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
        /// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
        /// is the interior of the polygon. The neighborhood to the right of an observer walking along
        /// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
        /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
        /// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
        /// as its parts.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        Polygon = 5,
        /// <summary>
        /// A MultiPoint represents a set of points.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        Multipoint = 8,
        /// <summary>
        /// A PointZ consists of a triplet of double-precision coordinates plus a measure.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        PointZ = 11,
        /// <summary>
        /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
        /// more points. Parts may or may not be connected to one another. Parts may or may not
        /// intersect one another.
        /// GIS interpretes this as <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineZ = 13,
        /// <summary>
        /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
        /// its parts.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonZ = 15,
        /// <summary>
        /// A MultiPointZ represents a set of <see cref="PointZ"/>s.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointZ = 18,
        /// <summary>
        /// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
        /// GIS interpretes this as <see cref="GIS.Geometries.Point"/>
        /// </summary>
        PointM = 21,
        /// <summary>
        /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
        /// two or more points. Parts may or may not be connected to one another. Parts may or may
        /// not intersect one another.
        /// GIS interpretes this as <see cref="GIS.Geometries.LineString"/> or <see cref="GIS.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineM = 23,
        /// <summary>
        /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// GIS interpretes this as either <see cref="GIS.Geometries.Polygon"/> or <see cref="GIS.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonM = 25,
        /// <summary>
        /// A MultiPointM represents a set of <see cref="PointM"/>s.
        /// GIS interpretes this as <see cref="GIS.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointM = 28,
        /// <summary>
        /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
        /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
        /// part controls how the order of vertices of an MultiPatch part is interpreted.
        /// GIS doesn't support this feature type.
        /// </summary>
        MultiPatch = 31
    } ;
    public class ShapeFile : IProvider, IDisposable
    {
        /// <summary>
        /// SHAPEFILE的构造函数
        /// </summary>
        /// <param name="filename"></param>
        public ShapeFile(string filename)
        {
            try
            {
                m_FileName = filename;
                if (!InitializeShape())
                    throw new ApplicationException("索引文件不存在"); ;
                string dbFileName = Path.ChangeExtension(filename, ".dbf");
                if (File.Exists(dbFileName))
                    dbaseFile = new DbaseReader(dbFileName);
                ParseHeader();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #region PrivateMember
        private DbaseReader dbaseFile;
        private FileStream fsShapeFile;
        private FileStream fsShapeIndex;
        private BinaryReader brShapeFile;           //读取空间数据
        private BinaryReader brShapeIndex;          //读取索引信息
        private Geometries.GeoBound m_Bound;        //地理范围
        private int m_GeoCount;                     //空间目标个数
        private string m_FileName;                  //文件名
        private ShapeType m_ShapeType;              //文件类型       
        private bool m_IsOpen;                      //判断文件是否打开 
        #endregion

        #region Properties
        public ShapeType ShapeType
        {
            get { return m_ShapeType; }
        }
        #endregion

        #region Disposers and finalizers
        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Close();
                    this.m_Bound = null;
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Finalizes the object
        /// </summary>
        ~ShapeFile()
        {
            Dispose();
        }
        #endregion

        #region PrivateFunction

        private int GetShapeIndex(uint n)
        {
            brShapeIndex.BaseStream.Seek(100 + n * 8, 0);
            return 2 * SwapByteOrder(brShapeIndex.ReadInt32());
        }
        private bool InitializeShape()
        {

            if (!File.Exists(Path.ChangeExtension(m_FileName, ".shx")))
                return false;
            if (!File.Exists(Path.ChangeExtension(m_FileName, ".shp")))
                return false;
            return true;
        }
        private void ParseHeader()
        {
            fsShapeIndex = new FileStream(Path.ChangeExtension(m_FileName, ".shx"), FileMode.Open, FileAccess.Read);
            brShapeIndex = new BinaryReader(fsShapeIndex, Encoding.Unicode);

            fsShapeIndex.Seek(24, 0);
            int indexFileSize = SwapByteOrder(brShapeIndex.ReadInt32());
            m_GeoCount = (2 * indexFileSize - 100) / 8;
            fsShapeIndex.Seek(32, 0);
            m_ShapeType = (ShapeType)brShapeIndex.ReadInt32();
            m_Bound = new GeoBound(brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble());
            brShapeIndex.Close();
            fsShapeIndex.Close();

        }
        ///<summary>
        ///交换INT32类型的位序
        ///</summary>
        /// <param name="i">Integer to swap</param>
        /// <returns>Byte Order swapped int32</returns>
        private int SwapByteOrder(int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            Array.Reverse(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }
        /// <summary>
        /// 返回ID为OID的几何数据
        /// </summary>
        /// <param name="oid">几何对象的ID 为OID</param>
        /// <returns>几何数据</returns>
        private Geometry ReadGeometry(uint oid)
        {
            if (oid < 0 || oid > this.m_GeoCount)
                throw new ArgumentOutOfRangeException("读取内容超出范围"); ;
            brShapeFile.BaseStream.Seek(GetShapeIndex(oid) + 8, 0); //跳过记录编号和长度，8个字节
            ShapeType type = (ShapeType)brShapeFile.ReadInt32();// 获取几何类型
            if (type == ShapeType.Null)
                throw new Exception(string.Format("读取第{0}个目标空间数据类型未知", oid));

            if (m_ShapeType == ShapeType.Point || m_ShapeType == ShapeType.PointZ || m_ShapeType == ShapeType.PointM)
            {

                GeoPoint pt = new GeoPoint(brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
                return pt;
            }
            else if (m_ShapeType == ShapeType.Multipoint || m_ShapeType == ShapeType.MultiPointM || m_ShapeType == ShapeType.MultiPointZ)
            {
                //brShapeFile.BaseStream.Seek(32 + brShapeFile.BaseStream.Position, 0); //skip min/max box
                GeoBound bound = new GeoBound(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(),
                    brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
                GeoMultiPoint pts = new GeoMultiPoint();
                pts.Bound = bound;
                int nPoints = brShapeFile.ReadInt32(); // get the number of points
                if (nPoints == 0)
                {
                    return null;        ////////、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、
                } 
                //throw new Exception(string.Format("读取第{0}个点目标空间数据错误", oid));
                for (int i = 0; i < nPoints; i++)
                    pts.Points.Add(new GeoPoint(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));

                return pts;
            }
            //如果类型为线，多边形，或者多线
            else if (m_ShapeType == ShapeType.PolyLine || m_ShapeType == ShapeType.Polygon ||
                     m_ShapeType == ShapeType.PolyLineM || m_ShapeType == ShapeType.PolygonM ||
                    m_ShapeType == ShapeType.PolyLineZ || m_ShapeType == ShapeType.PolygonZ)
            {
                GeoBound bound = new GeoBound(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(),
                    brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
                int nParts = brShapeFile.ReadInt32(); // get number of parts (segments)
                int nPoints = brShapeFile.ReadInt32(); // get number of points
                if (nParts == 0)
                // throw new Exception(string.Format("读取第{0}个线或者面的目标空间数据错误", oid));
                {
                    return null;        ////////、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、
                }
                int[] segments = new int[nParts + 1];
                //读取每一部分的起始位置
                for (int b = 0; b < nParts; b++)
                    segments[b] = brShapeFile.ReadInt32();
                //add end point
                segments[nParts] = nPoints;
                /////////////////////////////////////////////////////////////////////////////
                #region 代表为线或者多线
                if ((int)m_ShapeType % 10 == 3) //
                {                     
                    GeoMultiLineString mline = new GeoMultiLineString();
                    for (int LineID = 0; LineID < nParts; LineID++)
                    {
                        GeoLineString line = new GeoLineString();
                        for (int i = segments[LineID]; i < segments[LineID + 1]; i++)
                            line.Vertices.Add(new GeoPoint(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
                         
                         if (line.Vertices.Count > 1)
                             mline.LineStrings.Add(line);
                       
                    }
                    if (mline.LineStrings.Count == 1)
                    {
                        mline[0].Bound = bound;
                        return mline[0];
                    }
                    mline.Bound = bound;
                    return mline;
                }
                #endregion
                /////////////////////////////////////////////////////////////////////////////
                #region 类型为多边形
                else
                {
                    if (nPoints < 3 * nParts)
                        throw new Exception(string.Format("读取第{0}个面目标空间数据错误，端点个数小于3", oid));
                    //读取所有环
                    List<GeoLinearRing> rings = new List<GeoLinearRing>();
                    for (int RingID = 0; RingID < nParts; RingID++)
                    {
                        GeoLinearRing ring = new GeoLinearRing();
                        for (int i = segments[RingID]; i < segments[RingID + 1]; i++)
                            ring.Vertices.Add(new GeoPoint(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
                        ring.ClearRepeatPoints();
                        rings.Add(ring);
                    }
                    bool[] IsCounterClockWise = new bool[rings.Count];
                    int PolygonCount = 0;
                    for (int i = 0; i < rings.Count; i++)
                    {
                        IsCounterClockWise[i] = rings[i].IsCCW();
                        if (!IsCounterClockWise[i])
                            PolygonCount++;
                    }
                    if (PolygonCount == 1) //We only have one polygon
                    {
                        GeoPolygon poly = new GeoPolygon();
                        for (int i = 0; i < rings.Count; i++)
                        {
                            if (!rings[i].IsCCW())
                            { 
                                poly.ExteriorRing = rings[i]; 
                            }
                            else
                                poly.InteriorRings.Add(rings[i]);
                        } 
 
                        poly.Bound = bound;
                        return poly;
                    }
                    else
                    {
                        GeoMultiPolygon mpoly = new GeoMultiPolygon();
                        GeoPolygon poly = new GeoPolygon();
                        poly.ExteriorRing = rings[0];
                        for (int i = 1; i < rings.Count; i++)
                        {
                            if (!IsCounterClockWise[i])
                            {
                                mpoly.Polygons.Add(poly);
                                poly = new GeoPolygon(rings[i]);
                            }
                            else
                                poly.InteriorRings.Add(rings[i]);
                        }
                        mpoly.Polygons.Add(poly);
                        return mpoly;
                    }
                }
                #endregion
            }
            else
                throw (new ApplicationException("Shapefile type " + m_ShapeType.ToString() + " not supported"));
        }
        #endregion

        #region IProvider Members
        /// <summary>
        /// 带开数据源
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            if (!m_IsOpen)
            {
                try
                {

                    if (dbaseFile != null)
                    {
                        dbaseFile.Open();
                        if (dbaseFile.GetRecordNumber() != GetGeomCount())
                        {
                            throw new ApplicationException("属性记录个数和SHP个数不一致"); 
                        }
                    }
                    else
                    {
                        //属性表不存在，不能进行增量更新
                        throw new ApplicationException("属性表不存在，不能进行增量更新");
                    }


                    fsShapeIndex = new FileStream(Path.ChangeExtension(m_FileName, ".shx"), FileMode.Open, FileAccess.Read);

                    brShapeIndex = new BinaryReader(fsShapeIndex, Encoding.Unicode);

                    fsShapeFile = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);
                    brShapeFile = new BinaryReader(fsShapeFile);
                    m_IsOpen = true;
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            throw new ApplicationException("SHAPE文件已经打开");
        }
        /// <summary>
        /// 关闭数据源
        /// </summary>
        public void Close()
        {
            if (!disposed)
            {
                if (m_IsOpen)
                {
                    brShapeFile.Close();
                    fsShapeFile.Close();
                    brShapeIndex.Close();
                    fsShapeIndex.Close();
                    if (dbaseFile != null)
                        dbaseFile.Close();
                    m_IsOpen = false;
                }
            }
        }
        /// <summary>
        /// 返回SHAPEFILE 文件是否打开
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            return m_IsOpen;
        }
        /// <summary>
        /// 返回几何对象的个数
        /// </summary>
        /// <returns></returns>
        public int GetGeomCount()
        {
            return m_GeoCount;
        }
        public GeoBound GetExtents()
        {
            return m_Bound;
        }
        public int GetLayerType()
        {
            if (m_ShapeType == ShapeType.Point || m_ShapeType == ShapeType.PointZ ||
                m_ShapeType == ShapeType.PointM || m_ShapeType == ShapeType.Multipoint ||
                m_ShapeType == ShapeType.MultiPointM || m_ShapeType == ShapeType.MultiPointZ)
            {
                return 1;
            }

            else if ((int)m_ShapeType % 10 == 3) //返回线层
            {
                return 3;
            }
            else 
            {
                return 5;
            }
        }
        public Collection<uint> GetObjectIDsInView(GeoBound bound)
        {
            return null;
        }
        public Style.VectorStyle GetLayerStype()
        {
            return null;
        }
        /// <summary>
        /// 执行全面查询，将查询结果输出到表TABLE中
        /// </summary>
        /// <param name="table"></param>
        public GeoDataTable ExecuteAllQuery()
        {
            try
            {
                if (!this.IsOpen() && dbaseFile == null) //如果文件没打开不能执行
                    throw new ApplicationException("文件已经打开");
                
                GeoDataTable table = dbaseFile.NewTable;//新建一个数据表
                for (uint id = 0; id < m_GeoCount; ++id)
                {
                    GeoDataRow dr = dbaseFile.GetDataRow(id, table);
                    dr.Geometry = ReadGeometry(id);
                    table.AddRow(dr);
                }
                return table;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
        

       /// <summary>
       /// 返回的GeoDataTable只包含几何数据
       /// </summary>
       /// <returns></returns>
        public GeoDataTable ExecuteAllGeomQuery()
        { 
            try
            {
                if (!this.IsOpen() && dbaseFile == null) //如果文件没打开不能执行
                    throw new ApplicationException("文件未打开");

                GeoDataTable table = dbaseFile.NewTable;
                for (uint id = 0; id < m_GeoCount; ++id)
                {
                    GeoDataRow dr = dbaseFile.GetDataRow(table);//创建的dr与table结构相同
                    dr.Geometry = ReadGeometry(id);
                    table.AddRow(dr);
                }
                return table;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
        public bool ExecuteAllAttributeQuery(GeoDataTable table)
        {
            if (!table.FillData)
            {
                try
                {
                    if (!this.IsOpen() && dbaseFile == null) //如果文件没打开不能执行
                        throw new ApplicationException("文件已经打开");
 
                    for (uint id = 0; id < m_GeoCount; ++id)
                    {
                         dbaseFile.FillDataRow(id,table);                          
                    }
                    return true;
                }
                catch (Exception e)
                {
                    throw new ApplicationException(e.Message);
                }
            }

            return false;
        }

       
        #endregion


        //public GeoDataRow GetGeoDataRow(uint RowID)
        //{

        //}
        //public GeoDataRow GetGeoDataRow(uint RowID, GeoDataTable table)
        //{
        //    if (dbaseFile != null && table != null)
        //    {
        //        GeoDataRow dr = dbaseFile.GetDataRow(RowID, table);
        //    }
        //}
    }
}