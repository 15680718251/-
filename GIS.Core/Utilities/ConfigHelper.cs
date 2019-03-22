using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;

namespace GIS.Utilities
{
    public static class ConfigHelper
    {
        ///<summary>
        ///读取app.config文件中appSettings配置节的value项
        ///</summary>
        public static string GetAppConfig(string strKey)
        {
            foreach (string key in System.Configuration.ConfigurationManager.AppSettings)
            {
                if (key == strKey)
                {
                    return ConfigurationManager.AppSettings[strKey];
                }
            }
            return null;
        }

        //获取参数
        public static Dictionary<string, string> GetAppConfigs()
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            NameValueCollection apps = System.Configuration.ConfigurationManager.AppSettings;
            string[] keys = apps.AllKeys;
            int count = keys.Length;
            for (int i = 0; i < count; i++)
            {
                dics.Add(keys[i], System.Configuration.ConfigurationManager.AppSettings[keys[i]]);
            }
            return dics;
        }

        //更新参数
        public static void UpdateAppConfig(string newKey, string newValue)
        {
            bool flag = false;
            foreach (string a in ConfigurationManager.AppSettings)
            {
                if (a == newKey)
                {
                    flag = true;
                }
            }

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (flag)
            {
                configuration.AppSettings.Settings.Remove(newKey);
            }

            configuration.AppSettings.Settings.Add(newKey, newValue);
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
