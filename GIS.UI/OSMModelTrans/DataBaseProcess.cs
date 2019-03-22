using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.UI.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;

namespace GIS.UI.OSMModelTrans
{
    class DataBaseProcess
    {
         /// <summary>
        /// 获取数据库中的数据表
        /// </summary>
        /// <param name="conString"></param>
        public static List<string> TablesQuery()
        {            
            List<string> result = new List<string>();
            string server = OSMDataBaseLinkForm.Server_;
            string user = OSMDataBaseLinkForm.User_;
            string password = OSMDataBaseLinkForm.Password_;
            string database = OSMDataBaseLinkForm.DataBase_;
            IWorkspace workspace;

            IPropertySet pPropset = new PropertySet();//创建一个属性设置对象
            IWorkspaceFactory pWorkspaceFact = new SdeWorkspaceFactory();//创建一个空间数据引擎工作空间工厂
            pPropset.SetProperty("server", server);
            // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
            pPropset.SetProperty("INSTANCE", "sde:oracle11g:" + server + "/" + database);// by 丁洋修改
            pPropset.SetProperty("database", database);
            pPropset.SetProperty("user", user);
            pPropset.SetProperty("password", password);
            pPropset.SetProperty("version", "SDE.DEFAULT");
            workspace = pWorkspaceFact.Open(pPropset, 0);//使用属性集来打开地理数据库
            //MessageBox.Show("连接成功");
            //原理就是根据空间数据引擎工作空间工厂进行属性对象设置，返回一个工作空间，正确就通过
            //有个问题：属性集不正确的话就直接抛出异常而不是叫我们自己重新输入


           
            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象
            //获取矢量数据集
           
            //获取图层名  
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                string temp;
                temp = datasetName.Name;//用于接收要素类的名字，赋给temp进行处理
                int i = temp.IndexOf('.');//找到.的下标
                temp = temp.Substring(i + 1);//从.开始截取，substring是从i+1开始截取的，也包括i+1
                result.Add(temp);
                datasetName = enumDatasetName.Next();
            }

            return result;
        }

        
    
    }
}
