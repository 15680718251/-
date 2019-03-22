using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{


    public static class TpPointtoPoint
    {
        
        /// <summary>
        /// 返回tpEqual，tpDisjoint
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPoint pt1, GeoPoint pt2, bool bReport)
        {


            if (pt1.IsEqual(pt2))
            {
                if (bReport)
                {
                    MessageBox.Show("选择的两点相等", "GIS");
                    ///消息框“两点相等”
                }

                return TpRelateConstants.tpEqual;
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("选择的两点不相等", "GIS");
                    ///“两点相离”
                }
                return TpRelateConstants.tpDisjoint;
            }
        }
    }
}
