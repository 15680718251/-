using System;
using System.Collections.Generic;
using System.Collections;

namespace GIS.TreeIndex.Vectorize
{
    public class Run
    {
        public int LeftFlag;   //分别标志游程的左边和右边各自用于哪个多边形 ； -1为初始值
        public int RightFlag;
        public short Attr;
        public int Start;      //游程起始列  游程是栅格数据，不可用矢量坐标

        public void SetLeftFlag(int value)
        {
            this.LeftFlag = value;
        }

        public void SetRightFlag(int value)
        {
            this.RightFlag = value;
        }
    }
}
