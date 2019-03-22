using System;
using System.Collections;

namespace GIS.RaToVe.ObjectVecRLE
{
    public class Run //: CollectionBase
    {
        public int LeftFlag;   //分别标志游程的左边和右边各自用于哪个多边形 ； -1为初始值
        public int RightFlag;
        public short Attr;
        public int Start;      //游程起始列  游程是栅格数据，不可用矢量坐标
        public int ElevationMax;
        public int ElevationMin;

        //public Run this[int RunIndex]
        //{
        //    get
        //    {
        //        return (Run)List[RunIndex];
        //    }
        //    set
        //    {
        //        List[RunIndex] = value;
        //    }
        //}
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Drawing;
//using System.Drawing.Imaging;
//namespace GIS.RaToVe.ObjectVec
//{
//    public class Ras : CollectionBase
//    {
//        public int Flag;   //对每个栅格做标记（标记为具有唯一性的多边形序号）
//        public int Attr;   //栅格属性
//        //public int Start;   //游程起始列  游程是栅格数据，不可用矢量坐标
//        //public int ElevationMax;  //暂时不支持有高程数据的情况
//        //public int ElevationMin;
//        public Ras this[int RasIndex]
//        {
//            get
//            {
//                return (Ras)List[RasIndex];
//            }
//            set
//            {
//                List[RasIndex] = value;
//            }
//        }
//    }
//}