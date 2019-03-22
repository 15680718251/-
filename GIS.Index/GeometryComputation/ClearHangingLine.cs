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
    public class ClearHangingLine
    {
        /// <summary>
        /// 删除线组src中的悬挂线，并保存到des中
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        
        
        public static void ClearHangingLines(List<GeoLineString> src, List<GeoLineString> des)
        {

            bool startNode = false;
            bool endNode = false;
            bool over = false;


            des.Clear();
            for(int i=0;i<src.Count;i++)
            {
                GeoLineString pl=new GeoLineString(src[i].Vertices);
                des.Add(pl);
            }


            for (int sum = 0; sum < des.Count&&over==false; sum++)
            {//往返检查sumLine次，这是有用途的，可以防止在剔除一条悬挂线后又带来新的悬挂线
                over = true;
                for (int count = 0; count < des.Count; count++)
                {
                    endNode=false;
                    startNode = false;

                    GeoLineString line=new GeoLineString(des[count].Vertices);



                    if(des[count].StartPoint.IsEqual(des[count].EndPoint))
                    {//该线的起点和终点相等
                        continue;
                    }

                    for (int index = 0; index < des.Count; index++)
                    {
                        if (index == count)
                        {
                            continue;
                        }



                        if((des[count].StartPoint.IsEqual(des[index].StartPoint))||(des[count].StartPoint.IsEqual(des[index].EndPoint)))

                        {
                            startNode=true;
                        }


                        if((des[count].EndPoint.IsEqual(des[index].StartPoint))||(des[count].EndPoint.IsEqual(des[index].EndPoint)))
                        {
                            endNode=true;
                        }


                        if (startNode && endNode)
                        {
                            break;
                        }
                    }

                    if (!startNode && !endNode)
                    {//zhh++ 说明是单独的线
                        continue;
                    }

                    if (!startNode || !endNode)
                    {//如果起点或终点是度为1的点，则此线是悬挂线
                        src.Add(line);

                        des.RemoveAt(count);
                        count--;
                        over = false;
                    }
                }
            }

 
        }



        /// <summary>
        /// 先打断线，在删除悬挂线
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public static void ClearHanging(List<GeoLineString> src, List<GeoLineString> des)
        {

            List<GeoLineString> src1 = new List<GeoLineString>();

                       

            GeometryComputation.LinesSplitLines.SplitLines(src,src1,false);

            ClearHangingLines(src1,des);



        }



    }
}
