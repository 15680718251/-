using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.Increment;

using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.SpatialRelation;
using GIS.TreeIndex.GeometryComputation;
using System.IO;
using System.Threading;





namespace GIS.TreeIndex.GeometryComputation
{
    class LinkLine
    {


        public static void LinkLines1(List<GeoLineString> src, List<GeoLineString> des)
        {
            des.Clear();
            for (int i = 0; i < src.Count; i++)
            {
                des.Add(src[i]);
            }

            List<GeoLineString> LinesLinked = new List<GeoLineString>();

            int i1 = 0; int i2 = 0;
            int n1 = 0; int n2 = 0;
            int id1 = 0; int id2 = 0;
            bool start = false;
            bool sNode = false;
            bool eNode = false;


            for (i1 = 0; i1 < des.Count - 1; i1++)
            {

                if (des[i1].StartPoint.IsEqual(des[i1].EndPoint))
                    continue;

                n1 = 0;

                for (i2 = 0; i2 < des.Count; i2++)
                {
                    if (i1 == i2)
                        continue;

                    sNode = false;
                    eNode = false;
                    if (des[i1].StartPoint.IsEqual(des[i2].StartPoint))
                        sNode = true;
                    if (des[i1].StartPoint.IsEqual(des[i2].EndPoint))
                        eNode = true;

                    if(sNode && eNode)
                    {
                        n1 = 2;
                        break;
                    }

                    if (sNode || eNode)
                    {
                        n1++;
                        id1 = i2;

                        if (sNode)
                        {
                            start = true;

                        }
                        else
                        {
                            start = false;
                        }
                        if (n1 > 1)
                            break;


                    }

                }

                if (n1 == 1)
                {

                    GeoLineString pl = new GeoLineString(des[i1].Vertices);

                    if (start)
                    {
                        for (i2 = 1; i2 < des[id1].Vertices.Count; i2++)
                        {
                            pl.Vertices.Insert(0,des[id1].Vertices[i2]);
                        }
                    }
                    else
                    {
                        for (i2 = des[id1].Vertices.Count - 2; i2 > -1;i2-- )
                        {
                            pl.Vertices.Insert(0,des[id1].Vertices[i2]);
                            
                        }

                    }

                    GeoLineString pll = new GeoLineString(des[id1].Vertices);
                    LinesLinked.Add(pll);

                    if (src[i1].IsEqual(des[i1]))
                    {
                        GeoLineString plll = new GeoLineString(des[i1].Vertices);
                        LinesLinked.Add(plll);

                    }

                    src.RemoveAt(id1);


                    des.RemoveAt(i1);
                    des.Insert(i1,pl);

                    des.RemoveAt(id1);

                    i1--;
                    continue;

                }

                n2 = 0;
                for (i2 = 0; i2 < des.Count; i2++)
                {

                    if (i1 == i2)
                        continue;

                    sNode = false;
                    eNode = false;
                    if (des[i1].EndPoint.IsEqual(des[i2].StartPoint))
                        sNode = true;
                    if (des[i1].EndPoint.IsEqual(des[i2].EndPoint))
                        eNode = true;

                    if (sNode && eNode)
                    {
                        n2 = 2;
                        break;
                    }

                    if (sNode || eNode)
                    {
                        n2++;
                        id2 = i2;

                        if (sNode)
                        {
                            start = true;

                        }
                        else
                        {
                            start = false;
                        }
                        if (n2 > 1)
                            break;


                    }

                }
                if (n2 == 1)
                {

                    GeoLineString pl = new GeoLineString(des[i1].Vertices);

                    if (start)
                    {
                        for (i2 = 1; i2 < des[id2].Vertices.Count; i2++)
                        {
                            pl.Vertices.Add(des[id2].Vertices[i2]);
                        }
                    }
                    else
                    {
                        for (i2 = des[id2].Vertices.Count - 2; i2 > 0; i2--)
                        {
                            pl.Vertices.Add(des[id2].Vertices[i2]);
                        }
                        pl.Vertices.Add(des[id2].Vertices[0]);
                    }

                    GeoLineString pll = new GeoLineString(des[id2].Vertices);
                    LinesLinked.Add(pll);

                    if (src[i1].IsEqual(des[i1]))
                    {
                        GeoLineString plll = new GeoLineString(des[i1].Vertices);
                        LinesLinked.Add(plll);

                    }

                    src.RemoveAt(id2);


                    des.RemoveAt(i1);
                    des.Insert(i1, pl);

                    des.RemoveAt(id2);

                    i1--;
                    continue;


                }


            }

            src.Clear();
            for (int i = 0; i < LinesLinked.Count; i++)
            {
                src.Add(LinesLinked[i]);
            }



        }
        
        
        
        
        
        
   
    }
}
