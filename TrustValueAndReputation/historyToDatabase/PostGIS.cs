using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using GIS.GeoData;
using GIS.Geometries;
using GIS.UI.WellKnownText;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    /// <summary>
    /// 该类主要用于数据的检索
    /// </summary>
    /// <summary>
    /// PostGreSQL / PostGIS dataprovider
    /// </summary>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///	string ConnStr = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.PostGIS(ConnStr, "myTable");
    /// </code>
    /// </example>
    [Serializable]
    public class PostGIS : IDisposable
    {
        private string _ConnectionString;
        private string _defintionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private string _Schema = "public";
        private int _srid = -2;
        private string _Table;
        private string _keyColumn;
        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName,string keyColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = OID_ColumnName;
            KeyColumnName = keyColumnName;
        }
        /// <summary>
        /// 主键
        /// </summary>
        public string KeyColumnName 
        {
            get { return _keyColumn; }
            set { _keyColumn = value; }
        }
        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set
            {
                _Table = value;
                qualifyTable();
            }
        }

        /// <summary>
        /// Schema Name
        /// </summary>
        public string Schema
        {
            get { return _Schema; }
            set { _Schema = value; }
        }

        /// <summary>
        /// Qualified Table Name
        /// </summary>
        public string QualifiedTable
        {
            get { return string.Format("{0}",  _Table); }
            set { _Table = value; }
        }

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _GeometryColumn; }
            set { _GeometryColumn = value; }
        }

        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        #region IProvider Members

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. npgsql's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. npgsql's ConnectionPooling takes over here
            _IsOpen = false;
        }


        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public List<Geometry> GetGeometriesInView(GeoBound bbox)
        {
            List<Geometry> features = new List<Geometry>();
            using ( OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.LeftBottomPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.LeftBottomPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + "," +
                                 bbox.RightUpPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.RightUpPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(new CultureInfo("en-US", false).NumberFormat) + ")";

                string strSQL = "SELECT AsText(\"" + GeometryColumn + "\") AS Geom ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;

#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometriesInView: executing sql:", strSQL));
#endif
                using ( OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using ( OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                Geometry geom = GeometryFromWKT.Parse((string)dr[0]);
                                if (geom != null)
                                    features.Add(geom);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return features;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT sdo_geometry.get_wkt(" + GeometryColumn + ") AS Geom FROM " + QualifiedTable +
                                " WHERE " + ObjectIdColumn + "='" + oid + "'";
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometryByID: executing sql:", strSQL));
#endif
                conn.Open();
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKT.Parse((string)dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }
        /// <summary>
        /// 根据主键获得geo
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Geometry GetGeometryByKey(String key)
        {
            Geometry geom = null;
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT sdo_geometry.get_wkt(" + GeometryColumn + ") AS Geom FROM " + QualifiedTable +
                                " WHERE " + KeyColumnName + "= '" + key + "'";
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetGeometryByID: executing sql:", strSQL));
#endif
                conn.Open();
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            geom = GeometryFromWKT.Parse((string)dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }
        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public List<uint> GetObjectIDsInView(GeoBound bbox)
        {
            List<uint> objectlist = new List<uint>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.LeftBottomPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.LeftBottomPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + "," +
                                 bbox.RightUpPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.RightUpPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(new CultureInfo("en-US", false).NumberFormat) + ")";

                string strSQL = "SELECT " + ObjectIdColumn ;
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "GetObjectIDsInView: executing sql:", strSQL));
#endif

                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint)(int)dr[0];
                                objectlist.Add(ID);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        /// <summary>
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">GeoDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, GeoDataSet ds)
        {
            List<Geometry> features = new List<Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + GeometryToWKT.Write(geom) + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID + ")";

                string strSQL = "SELECT * , sdo_geometry.get_wkt(" + GeometryColumn + ") As sharpmap_tempgeometry FROM " +
                                QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL +=  GeometryColumn + " && " + strGeom + " AND distance(" + GeometryColumn + ", " +
                          strGeom + ")<0";

#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}", "ExecuteIntersectionQuery: executing sql:", strSQL));
#endif

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        GeoDataTable fdt = new GeoDataTable();
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            GeoDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    count = Convert.ToInt32(command.ExecuteScalar());
                    conn.Close();
                }
            }
            return count;
        }

        /// <summary>
        /// Spacial Reference ID
        /// </summary>
        public int SRID
        {
            get
            {
                if (_srid == -2)
                {
                    string strSQL = "select srid from geometry_columns WHERE f_table_schema='" + _Schema +
                                    "' AND f_table_name='" + _Table + "'";

                    using (OracleConnection conn = new OracleConnection(_ConnectionString))
                    {
                        using (OracleCommand command = new OracleCommand(strSQL, conn))
                        {
                            try
                            {
                                conn.Open();
                                _srid = (int)command.ExecuteScalar();
                                conn.Close();
                            }
                            catch
                            {
                                _srid = -1;
                            }
                        }
                    }
                }
                return _srid;
            }
            set { throw (new ApplicationException("Spatial Reference ID cannot by set on a PostGIS table")); }
        }


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public GeoDataRow GetFeature(uint rowId)
        {
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "select * , sdo_geometry.get_wkt(" + GeometryColumn + ") As sharpmap_tempgeometry from " +
                                QualifiedTable + " WHERE " + ObjectIdColumn + "='" + rowId + "'";
                using ( OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        GeoDataTable fdt = new GeoDataTable();
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            GeoDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            return fdr;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// GeoBound of dataset
        /// </summary>
        /// <returns>GeoBound</returns>
        public GeoBound GetExtents()
        {
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strSQL = "SELECT EXTENT(" + GeometryColumn + ") FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (OracleCommand command = new OracleCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    string strBox = (string)result;
                    if (strBox.StartsWith("BOX("))
                    {
                        string[] vals = strBox.Substring(4, strBox.IndexOf(")") - 4).Split(new char[2] { ',', ' ' });
                        return new GeoBound(
                            double.Parse(vals[0], new CultureInfo("en-US", false).NumberFormat),
                            double.Parse(vals[1], new CultureInfo("en-US", false).NumberFormat),
                            double.Parse(vals[2], new CultureInfo("en-US", false).NumberFormat),
                            double.Parse(vals[3], new CultureInfo("en-US", false).NumberFormat));
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionID
        {
            get { return _ConnectionString; }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">GeoDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(GeoBound bbox, GeoDataSet ds)
        {
            //List<Geometry> features = new List<Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.LeftBottomPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.LeftBottomPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + "," +
                                 bbox.RightUpPt.X.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                 bbox.RightUpPt.Y.ToString(new CultureInfo("en-US", false).NumberFormat) + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString(new CultureInfo("en-US", false).NumberFormat) + ")";

                string strSQL = "SELECT *, sdo_geometry.get_wkt(" + GeometryColumn + ") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL +=  GeometryColumn + " && " + strBbox;
#if DEBUG
                Debug.WriteLine(string.Format("{0}\n{1}\n", "ExecuteIntersectionQuery: executing sql:", strSQL));
#endif
                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        GeoDataTable fdt = new GeoDataTable();
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            GeoDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        #endregion

        #region Disposers and finalizers

        private bool disposed;

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //Close();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~PostGIS()
        {
            Dispose();
        }

        #endregion

        private void qualifyTable()
        {
            int dotPos = _Table.IndexOf(".");
            if (dotPos == -1)
            {
                _Schema = "public";
            }
            else
            {
                _Schema = _Table.Substring(0, dotPos);
                _Schema = _Schema.Replace('"', ' ').Trim();
            }
            _Table = _Table.Substring(dotPos + 1);
            _Table = _Table.Replace('"', ' ').Trim();
        }

        /// <summary>
        /// Returns all objects within a distance of a geometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        //[Obsolete("Use ExecuteIntersectionQuery instead")]
        public GeoDataTable QueryFeatures(Geometry geom, double distance)
        {
            //Collection<Geometries.Geometry> features = new Collection<SharpMap.Geometries.Geometry>();
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            {
                string strGeom = "GeomFromText('" + GeometryToWKT.Write(geom) + "')";
                if (SRID > 0)
                    strGeom = "setSRID(" + strGeom + "," + SRID + ")";

                string strSQL = "SELECT * , sdo_geometry.get_wkt(" + GeometryColumn + ") As sharpmap_tempgeometry FROM " +
                                QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL +=  GeometryColumn + " && " + "buffer(" + strGeom + "," +
                          distance.ToString(new CultureInfo("en-US", false).NumberFormat) + ")";
                strSQL += " AND distance(" + GeometryColumn + ", " + strGeom + ")<" +
                          distance.ToString(new CultureInfo("en-US", false).NumberFormat);

                using (OracleDataAdapter adapter = new OracleDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        GeoDataTable fdt = new GeoDataTable();
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            GeoDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKT.Parse((string)dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        return fdt;
                    }
                    else return null;
                }
            }
        }

        /// <summary>
        /// Convert WellKnownText to linestrings
        /// </summary>
        /// <param name="WKT"></param>
        /// <returns></returns>
        private GeoLineString WktToLineString(string WKT)
        {
            GeoLineString line = new GeoLineString();
            WKT = WKT.Substring(WKT.LastIndexOf('(') + 1).Split(')')[0];
            string[] strPoints = WKT.Split(',');
            foreach (string strPoint in strPoints)
            {
                string[] coord = strPoint.Split(' ');
                line.Vertices.Add(new GeoPoint(double.Parse(coord[0], new CultureInfo("en-US", false).NumberFormat),
                                            double.Parse(coord[1], new CultureInfo("en-US", false).NumberFormat)));
            }
            return line;
        }

        /// <summary>
        /// Queries the PostGIS database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "SELECT f_geometry_column from geometry_columns WHERE f_table_schema='" + _Schema +
                            "' and f_table_name='" + _Table + "'";
            using (OracleConnection conn = new OracleConnection(_ConnectionString))
            using (OracleCommand command = new OracleCommand(strSQL, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string)columnname;
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">GeoDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery")]
        public void GetFeaturesInView(GeoBound bbox, GeoDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }
    }
}
