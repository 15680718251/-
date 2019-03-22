using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
    public static  class TpPointtoPolygon
    {
        
        
        
        /// <summary>
        /// 点与面的关系，只有inside（在内部），touch(在边界)，disjiont三种情况
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pg"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPoint pt, GeoPolygon pg,bool bReport)
        {
            GeoBound bound=pg.GetBoundingBox();
            if (TpPLPstruct.TpPointinBound(bound, pt))
            {
                int pp=0;
                pp = TpPLPstruct.TpPointToLineRing(pt, pg.ExteriorRing);
                if (pp == 0)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的外部","GIS");///点与面相离
                    }
                    return TpRelateConstants.tpDisjoint;
                    
                }
                else if (pp == 2)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的外边界上", "GIS");///点与面相接，点在面的外边界上
                    }
                    return TpRelateConstants.tpTouch;
                    
                }
                else
                {
                    int pi=0;
                    for (int i = 0; i < pg.InteriorRings.Count;i++ )
                    {

                        pi = TpPLPstruct.TpPointToLineRing(pt, pg.InteriorRings[i]);
                        if (pi > 0)
                        {
                            break;
                        }
                    }
                    if (pi == 0)
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在面的内部", "GIS");///点在面的内部
                        }
                        return TpRelateConstants.tpInside;
                        
                    }
                    else if (pi == 2)
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点在面的内边界上", "GIS");///点在面的内边界上
                        }
                        return TpRelateConstants.tpTouch;
                        
                    }
                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("点与面相离，点在线的空洞内", "GIS");///点与面相离，点在线的空洞内
                        }
                        return TpRelateConstants.tpDisjoint;
                        
                    }
                }
            
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点在面的外部", "GIS");///点与面相离
                }
                return TpRelateConstants.tpDisjoint;
                
            }
        }



        /// <summary>
        /// 面pl与点pt的拓扑关系，只有返回contains（包含），touch(在边界)，disjiont三种情况
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pt"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPolygon pg, GeoPoint pt, bool bReport)
        {
            TpRelateConstants tp = TpRelateConstants.tpUnknow;
            tp = IsTpWith(pt, pg, false);
            if (tp == TpRelateConstants.tpInside)
            {
                if (bReport)
                {
                    MessageBox.Show("点在面的内部", "GIS");
                }
                return TpRelateConstants.tpContains;
            }
            else if (tp == TpRelateConstants.tpTouch)
            {
                if (bReport)
                {
                    MessageBox.Show("点在面的端点上", "GIS");
                }
                return TpRelateConstants.tpContains;
            }
            else 
            {
                if (bReport)
                {
                    MessageBox.Show("点与面相离", "GIS");
                }
                return TpRelateConstants.tpDisjoint;
            }
            
        }



        /// <summary>
        /// 点与面集的关系，只有inside（在内部），touch(在边界)，disjiont三种情况
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPoint pt, GeoMultiPolygon pl, bool bReport)
        {

            GeoBound bound = pl.GetBoundingBox();

            if (bound.IsPointIn(pt))
            {

                TpRelateConstants tp = TpRelateConstants.tpUnknow;

                for (int i = 0; i < pl.NumGeometries; i++)
                {
                    tp = IsTpWith(pt, pl.Polygons[i], false);
                    if (tp == TpRelateConstants.tpDisjoint)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (tp == TpRelateConstants.tpDisjoint)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点与面相离", "GIS");///点与面关系未知
                    }
                    return tp;
                }
                else if (tp == TpRelateConstants.tpInside)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的内部", "GIS");///点与面关系未知
                    }
                    return tp;
                }
                else
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的边界上", "GIS");///点与面关系未知
                    }
                    return tp;
                }
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点与面相离", "GIS");///点与面关系未知
                }
                return TpRelateConstants.tpDisjoint;
            }
            
            
            
            
        }



        /// <summary>
        /// 点与面集的关系，只有contains（在内部），touch(在边界)，disjiont三种情况
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiPolygon pl,GeoPoint pt,  bool bReport)
        {

            GeoBound bound = pl.GetBoundingBox();

            if (bound.IsPointIn(pt))
            {

                TpRelateConstants tp = TpRelateConstants.tpUnknow;

                for (int i = 0; i < pl.NumGeometries; i++)
                {
                    tp = IsTpWith(pt, pl.Polygons[i], false);
                    if (tp == TpRelateConstants.tpDisjoint)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (tp == TpRelateConstants.tpDisjoint)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点与面相离", "GIS");///点与面关系未知
                    }
                    return tp;
                }
                else if (tp == TpRelateConstants.tpInside)
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的内部", "GIS");///点与面关系未知
                    }
                    return TpRelateConstants.tpContains;
                }
                else
                {
                    if (bReport)
                    {
                        MessageBox.Show("点在面的边界上", "GIS");///点与面关系未知
                    }
                    return tp;
                }
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("点与面相离", "GIS");///点与面关系未知
                }
                return TpRelateConstants.tpDisjoint;
            }




        }


    }
}
