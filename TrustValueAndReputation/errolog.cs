using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TrustValueAndReputation
{
    class errolog
    {
        //当前程序的日志文件目录
        static string strAppLofDir = AppDomain.CurrentDomain.BaseDirectory + "log/";

        /// <summary>
        ///  写日志到EduApp.log文件中；
        /// </summary>
        /// <param name="logInfo"></param>
        public static void WriteEduAppLog(string ErrorReason, string StackTrace)
        {
            WriteLog(ErrorReason, StackTrace, "EduApp.log");
        }
        private static void WriteLog(string ErrorReason, string StackTrace, string logFileName)
        {
            //判断有没有日志目录，没有就创建
            DirectoryInfo directoryInfo = new DirectoryInfo(strAppLofDir);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            StringBuilder logInfo = new StringBuilder("");
            string currentTime = System.DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
            logInfo.Append("\n").Append(currentTime).Append("：").Append(ErrorReason).Append("\n").Append(StackTrace);
            System.IO.File.AppendAllText(strAppLofDir + logFileName, logInfo.ToString());
        }
    }
}
