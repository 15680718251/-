using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using GIS.Geometries;
using GIS.Layer;
using GIS.Style;

namespace GIS.GeoData.DataProviders
{
    public class LQFile:IProvider
    {
        private string m_FileName;                  //文件名
        private FileStream fsLQFile;
        private StreamReader srLQFile;              //读取文件流
        private GeoDataTable m_DataTable;
        private int m_LayerType;                    //图层类型
        private Geometries.GeoBound m_Bound;        //地理范围
        private int m_GeoCount;                     //空间目标个数
        private bool m_IsOpen = false;                      //判断文件是否打开 
        private string m_LayerName;
        private VectorStyle m_Style;
        private Int64 m_ClasID;
        public LQFile(string filename)
        {
            m_FileName = filename;
        }

        public bool ExecuteAllAttributeQuery(GeoDataTable table)
        {
            try
            {

                fsLQFile = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);
                srLQFile = new StreamReader(fsLQFile, Encoding.Default);
                fsLQFile.Seek(0, SeekOrigin.Begin);
                string strTemp = srLQFile.ReadLine();
                while (strTemp != "AttributeBegin")
                {
                    strTemp = srLQFile.ReadLine();
                }
                string[] strArray =null;
                for (int i = 0; i < table.Count; i++)
                {
                    GeoDataRow row = table[i];
                    strTemp = srLQFile.ReadLine();
                    strArray = strTemp.Split(',');
                    
                    for (int j = 0; j < strArray.Length; j++)
                    {
                        if (strArray[j].ToString() == "")
                            row[j] = DBNull.Value;
                        else
                            row[j] = strArray[j].ToString();
                    }      
                }
                strTemp = srLQFile.ReadLine();
                if (strTemp != "AttributeEnd")
                {
                    throw new Exception("EVC文件属性数据错误");
                }
                srLQFile.Close();
                fsLQFile.Close(); 
            }

            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            return true;
        }
              //只读几何数据
        public GeoDataTable ExecuteAllGeomQuery()
        {
            try
            {
               
                fsLQFile = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);
                srLQFile = new StreamReader(fsLQFile, Encoding.Default);
                srLQFile.BaseStream.Seek(0, SeekOrigin.Begin);
                string strTemp = srLQFile.ReadLine();
                while (strTemp != "GeometryBegin")
                {
                    strTemp = srLQFile.ReadLine();
                }

                for (uint id = 0; id < m_GeoCount; ++id)
                {
                    strTemp = srLQFile.ReadLine();
                    GeoDataRow dr = m_DataTable.NewRow();
                    Geometry geom = null;
                    switch (strTemp)
                    {
                        case "GeoPoint":
                            geom = new GeoPoint();
                            break;
                        case "GeoLineString":
                            geom = new GeoLineString();
                            break;
                        case "GeoPolygon":
                            geom = new GeoPolygon();
                            break;
                        case "GeoArc":
                            geom = new GeoArc();
                            break;
                        case "GeoSpline":
                            geom = new GeoSpline();
                            break;
                        case "GeoMultiPoint":
                            geom = new GeoMultiPoint();
                            break;
                        case "GeoMultiLineString":
                            geom = new GeoMultiLineString();
                            break;
                        case "GeoMultiPolygon":
                            geom = new GeoMultiPolygon();
                            break;
                        case "GeoLabel":
                            geom = new GeoLabel();
                            break;
                    }

                    geom.ReadFromLQFile(srLQFile);
                    dr.Geometry = geom;
                    m_DataTable.AddRow(dr);
                }
                srLQFile.Close();  
                fsLQFile.Close();
              
                return m_DataTable;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
        public void Dispose()
        { 
            GC.SuppressFinalize(this);
        }
        public bool Open()
        {
            try
            {
                fsLQFile = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);
                srLQFile = new StreamReader(fsLQFile, Encoding.Default);


                fsLQFile.Seek(-50, SeekOrigin.End);

                string strTemp = null;
                string[] strArray = null;
                strTemp = srLQFile.ReadToEnd();
                strArray = strTemp.Split(new char[] { '\r', '\n' });
                for (int i = strArray.Length - 1; i >= 0; i--)
                {
                    if (strArray[i].Length > 2)
                    {
                        strTemp = strArray[i];
                        break;
                    }
                }
                strArray = strTemp.Split(',');
                if (strArray[0].Trim() != strArray[1].Trim())
                {
                    Close();
                    throw new Exception("属性和几何个数不一致");
                }
                else
                    m_GeoCount = int.Parse(strArray[0].Trim());


                fsLQFile.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < 3; i++)
                {
                    srLQFile.ReadLine();
                }


                double[] extents = new double[4];

                for (int i = 0; i < 4; i++)
                {
                    strTemp = srLQFile.ReadLine();
                    strArray = strTemp.Split(':');
                    extents[i] = double.Parse(strArray[1].Trim());
                }
                m_Bound = new GeoBound(extents[0], extents[1], extents[2], extents[3]);


                while (strTemp != "TableStructureBegin")
                {
                    strTemp = srLQFile.ReadLine();
                }

                strTemp = srLQFile.ReadLine();

                strArray = strTemp.Split(',');
                m_LayerName = strArray[0].Trim();
                m_LayerType = int.Parse(strArray[2].Trim());
                int nColumnCount = int.Parse(strArray[1].Trim());

                strTemp = srLQFile.ReadLine();

                strArray = strTemp.Split(',');

                m_ClasID = Int64.Parse(strArray[0].Trim());
                float symbolSize = float.Parse(strArray[1].Trim());
                float lineSize = float.Parse(strArray[2].Trim());
                int[] argb = new int[4];
                for (int i = 3; i < 7; i++)
                {
                    argb[i - 3] = int.Parse(strArray[i].Trim());
                }
                Color lineclr = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);

                for (int i = 7; i < 11; i++)
                {
                    argb[i - 7] = int.Parse(strArray[i].Trim());
                }
                Color fillclr = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);


                m_Style = new VectorStyle(symbolSize, lineSize, lineclr, fillclr, false);


                m_DataTable = new GeoDataTable();
                for (int i = 0; i < nColumnCount; i++)
                {
                    strTemp = srLQFile.ReadLine();
                    strArray = strTemp.Split(',');
                    m_DataTable.Columns.Add(strArray[0].Trim(), Type.GetType(strArray[1].Trim()));
                }

                srLQFile.Close();
                fsLQFile.Close();
                return true;
            }
            catch (Exception e)
            {
                if (srLQFile != null)
                {
                    srLQFile.Close();
                    fsLQFile.Close();
                }
                throw new ApplicationException(e.Message);
            } 
        
        }
   
        public void Close()
        {
            if (IsOpen())
            {
                srLQFile.Close();
                fsLQFile.Close();
            }
        }

        public GeoBound GetExtents()
        {
            return m_Bound;
        }
        public bool IsOpen()
        {
            return m_IsOpen;
        }
        public int GetGeomCount()
        {
            return 0;
        }
        public Style.VectorStyle GetLayerStype()
        {
            return m_Style;
        }
        public Collection<uint> GetObjectIDsInView(GeoBound bound)
        {
            throw new Exception("未定义");
        }
        public GeoDataTable ExecuteAllQuery()
        {
            return null;
        }
        
     
        public int GetLayerType()
        {
            return m_LayerType;
        }
    }
}
