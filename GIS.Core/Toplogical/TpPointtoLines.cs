using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
    public static class TpPointtoLines
    {
        /// <summary>
        /// pt与line（折线）的关系,只有inside（在线内），disjiont，touch（在端点），unknow四种
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="line"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPoint pt, GeoLineString line, bool bReport)
        {

            GeoBound bound = line.GetBoundingBox();
            ///pt在线的外包矩形内            
            if (TpPLPstruct.TpPointinBound(bound, pt))
            {
                int a = 0;
                int i = 0;
                ///a=1,3,2就跳出，说明点在线上
                for (i = 0; i < line.Vertices.Count - 1; i++)
                {
                    a = TpPLPstruct.TpPointToLine(pt, line.Vertices[i], line.Vertices[i + 1]);
                    if (a == 1 || a == 2 || a == 3)
                    {
                        break;
                    }

                }
                if (a == 0)
                {
                   
                    if (bReport)
                    {
                        MessageBox.Show("点与线相离","GIS");///"点线相离"
                    } 
                    return TpRelateConstants.tpDisjoint;

                }

                else if (i != 0 && a == 1)
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点在线非端点的节点上", "GIS");///“点在线的结点上”
                    }
                    return TpRelateConstants.tpInside;
                }
                else if (i == 0 && a == 1)
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点在线的端点上", "GIS");///"点在线的端点上"
                    }
                    return TpRelateConstants.tpTouch;
                }
                else if (i == line.Vertices.Count - 2 && a == 2)
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点在线的端点上", "GIS");///"点在线的端点上"
                    }
                    return TpRelateConstants.tpTouch;
                }

                else if (i != line.Vertices.Count - 2 && a == 2)
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点在线非端点的节点上", "GIS");///"点在线的节点上"
                    }
                    return TpRelateConstants.tpInside;
                }

                else if (a == 3)
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点在线上，但并不在端点和节点上", "GIS");///"点在线段上，但不在节点和端点上"
                    }
                    return TpRelateConstants.tpInside;
                }
                else
                {
                    
                    if (bReport)
                    {
                        MessageBox.Show("点线关系未知", "GIS");///"点线关系未知"
                    }
                    return TpRelateConstants.tpUnknow;

                }

            }
            else
            {
                
                if (bReport)
                {
                    MessageBox.Show("点线相离", "GIS");///"点线相离"
                }
                return TpRelateConstants.tpDisjoint;

            }




        }


        /// <summary>
        /// 线line与点pt的关系，只有contains（包含），disjiont，touch（在端点），unknow四种
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="line"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoLineString line, GeoPoint pt, bool bReport)
        {
            TpRelateConstants tp = TpRelateConstants.tpUnknow;
            tp = IsTpWith(pt, line, false);
            if (tp == TpRelateConstants.tpInside)
            {
                if (bReport)
                {
                    MessageBox.Show("点在线的内部","GIS");
                }
                return TpRelateConstants.tpContains;
            }
            else if (tp == TpRelateConstants.tpTouch)
            {
                if (bReport)
                {
                    MessageBox.Show("点在线的端点上", "GIS");
                }
                return TpRelateConstants.tpContains;
            }
            else if (tp == TpRelateConstants.tpDisjoint)
            {
                if (bReport)
                {
                    MessageBox.Show("点与线相离", "GIS");
                }
                return TpRelateConstants.tpDisjoint;
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点线关系未知", "GIS");///"点线关系未知"
                }
                return TpRelateConstants.tpUnknow;
            }



        }




        /// <summary>
        /// pt与折线集合lines的关系，只有inside（在线内），disjiont，touch（在端点），unknow四种
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="lines"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPoint pt, GeoMultiLineString lines, bool bReport)
        {

            GeoBound bound = lines.GetBoundingBox();
            if (bound.IsPointIn(pt))
            {
                int i = 0;
                int sum = 0;
                sum = lines.LineStrings.Count;
                TpRelateConstants tpp = TpRelateConstants.tpUnknow;
                bool disjiont = true;
                for (i = 0; i < sum; i++)
                {
                    tpp = IsTpWith(pt, lines.LineStrings[i], false);

                    if (tpp == TpRelateConstants.tpDisjoint||tpp==TpRelateConstants.tpUnknow)
                    {
                        disjiont = true;
                        
                    }
                    else
                    {
                        disjiont = false;
                        break;
                    }
                   

                }

                if (disjiont)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点与线集相离", "GIS");///点与线集相离
                    }
                    return TpRelateConstants.tpDisjoint;
                    
                }
                else
                {
                    if (tpp == TpRelateConstants.tpInside)
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在线集合的内部", "GIS");///点在线集合的内部
                        }
                        return TpRelateConstants.tpInside;
                        
                    }
                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在线集合的端点上", "GIS");///点与线集合相接
                        }
                        return TpRelateConstants.tpTouch;
                        
                    }
                }
                
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点与线集相离", "GIS");///点与线集相离
                }
                return TpRelateConstants.tpDisjoint;
                
            }


        }



        /// <summary>
        /// pt与折线集合lines的关系，只有contains（在线内），disjiont，touch（在端点），unknow四种
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="lines"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith( GeoMultiLineString lines,GeoPoint pt, bool bReport)
        {

            GeoBound bound = lines.GetBoundingBox();
            if (bound.IsPointIn(pt))
            {
                int i = 0;
                int sum = 0;
                sum = lines.LineStrings.Count;
                TpRelateConstants tpp = TpRelateConstants.tpUnknow;
                bool disjiont = true;
                for (i = 0; i < sum; i++)
                {
                    tpp = IsTpWith(pt, lines.LineStrings[i], false);

                    if (tpp == TpRelateConstants.tpDisjoint || tpp == TpRelateConstants.tpUnknow)
                    {
                        disjiont = true;

                    }
                    else
                    {
                        disjiont = false;
                        break;
                    }


                }

                if (disjiont)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点与线集相离", "GIS");///点与线集相离
                    }
                    return TpRelateConstants.tpDisjoint;

                }
                else
                {
                    if (tpp == TpRelateConstants.tpInside)
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在线集合的内部", "GIS");///点在线集合的内部
                        }
                        return TpRelateConstants.tpContains;

                    }
                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在线集合的端点上", "GIS");///点与线集合相接
                        }
                        return TpRelateConstants.tpTouch;

                    }
                }

            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点与线集相离", "GIS");///点与线集相离
                }
                return TpRelateConstants.tpDisjoint;

            }


        }


    }
}
