using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GIS.Utilities
{
    //记录矢量日志
    public class DebugLog
    {
        //private static string APPLOGINFO = "<Vectorization Start>";
        //private static string APPLOGCLOSE = "Vectorization End>";
        private static string APPLOGSEPARATOR = "----------";
        private static string APPLOGDEBUG = " <BUG> ";
        //private static string APPLOGFOOTER = "--------------------------------------------------";

        private string _fileName;
        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();

        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
            }
        }

        public DebugLog()
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            this._fileName = fileName;
        }

        public void Create()
        {
            string fileName = this.FileName;
            if (!File.Exists(fileName))
            {
                using (FileStream fileStream = File.Create(fileName))
                {
                    fileStream.Close();
                }
            }
        }

        private void Write(string content, string newLine)
        {
            if (string.IsNullOrEmpty(this._fileName))
            {
                throw new Exception("FileName不能为空！");
            }
            using (FileStream fileStream = new FileStream(this._fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
            {
                byte[] bytes = Encoding.Default.GetBytes(content + newLine);
                bool flag = true;
                long num = (long)bytes.Length;
                long num2 = 0L;
                while (flag)
                {
                    try
                    {
                        if (num2 >= fileStream.Length)
                        {
                            fileStream.Lock(num2, num);
                            DebugLog.lockDic[num2] = num;
                            flag = false;
                        }
                        else
                        {
                            num2 = fileStream.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        while (!DebugLog.lockDic.ContainsKey(num2))
                        {
                            num2 += DebugLog.lockDic[num2];
                        }
                    }
                }
                fileStream.Seek(num2, SeekOrigin.Begin);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }
        }
        private void WriteLine(string content)
        {
            this.Write(content, Environment.NewLine);
        }
        private void Write(string content)
        {
            this.Write(content, "");
        }

        public void WriteLog(object obj)
        {
            string text = obj.ToString();
            this.WriteLine(string.Concat(new object[]
			{
				"[",
				DateTime.Now,
				"]",
				DebugLog.APPLOGSEPARATOR,
				text
			}));
        }
        public void WriteLog(string str)
        {
            this.WriteLine(string.Concat(new object[]
			{
				"[",
				DateTime.Now,
				"]",
				DebugLog.APPLOGSEPARATOR,
				str
			}));
        }

        public void WriteDebuglog(string str)
        {
            this.WriteLine(string.Concat(new object[]
			{
				"[",
				DateTime.Now,
				"]",
				DebugLog.APPLOGDEBUG,
				str
			}));
        }
        public void WriteDebuglog(Exception e)
        {
            string message = e.Message;
            this.WriteLine(string.Concat(new object[]
			{
				"[",
				DateTime.Now,
				"]",
				DebugLog.APPLOGDEBUG,
				message
			}));
        }
    }
}
