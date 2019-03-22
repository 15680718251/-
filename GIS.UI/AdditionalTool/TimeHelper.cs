using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.UI.AdditionalTool
{
   public  class TimeHelper
    {
        /// <summary>
        /// 获得还剩时间的字符串
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetLeftTimeStr(DateTime beginTime, int index, int len)
        {
            DateTime curruntTime = DateTime.Now;
            TimeSpan difTime = curruntTime - beginTime;
            int leftSeconds = (int)(difTime.TotalSeconds * (len - index) / (index + 0.0));
            return string.Format("大约还需{0}分{1}秒", leftSeconds / 60, leftSeconds % 60);

        }
    }
}
