using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using GIS.TreeIndex;
using GIS.Buffer;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.Forms;
using GIS.RaToVe.ObjectVecRLE;
using System.IO;
namespace GIS.TreeIndex.Tool
{
    public class MapRasterToVectorTool : MapTool
    {
        //public RunLengthEncodingVec VectorRle; //矢量化对象
        public GIS.RaToVe.ObjectVecRLE.ObjectOrientedVec VectorObj;

        public MapRasterToVectorTool(MapUI ui) : base(ui)
        {
            RasterToVector();
        }
        public void RasterToVector()
        {
            int FileCount = 0; //记录不可被矢量化的文件数目
            RasToVecForm Form = new RasToVecForm();
            if (Form.ShowDialog() == DialogResult.OK)
            {
                //2016-7-24测试时间
                DateTime startTime = DateTime.Now;

                //2016-7-24测试时间




                foreach (string strRasFilePathName in Form.strRasterPathNames)
                {
                    //VectorRle = new RunLengthEncodingVec(Form.strRasterPathName, Form.strDEMFilePathName, Form.strExportfolderName,Form.Tolerance);          //矢量化对象
                    //VectorRle.RaToVe();
                    VectorObj = new GIS.RaToVe.ObjectVecRLE.ObjectOrientedVec(strRasFilePathName, Form.strDEMFilePathName, Form.strExportfolderName, Form.Tolerance,Form.dics);          //矢量化对象
                    VectorObj.RaToVe(ref FileCount);
                    //VectorObj1 = new GIS.RaToVe.ObjectVecRLEAndRas.ObjectOrientedVec(Form.strRasterPathName, Form.strDEMFilePathName, Form.strExportfolderName, Form.Tolerance);
                    //VectorObj1.RaToVe();
                }
                string MoveToPath = Path.GetDirectoryName(Form.strRasterPathNames[0]) + "\\Temp";
                int FinishCount = Form.strRasterPathNames.Count() - FileCount;


                //2016-7-24测试时间
                DateTime stopTime = DateTime.Now;
                TimeSpan elapsedTime = stopTime - startTime;

                //2016-7-24测试时间


                if (FileCount > 0)
                    m_MapUI.OutPutTextInfo(string.Format("完成栅格矢量化文件{0}个，未完成{1}个（因遇到多边形内圈数量大于3万个的问题），\r\n未完成文件已移入{2}目录下：\r\n", FinishCount, FileCount, MoveToPath));
                else if (FileCount == 0)
                {
                    m_MapUI.OutPutTextInfo(string.Format("所有{0}个文件栅格矢量化完成:\r\n", Form.strRasterPathNames.Count()));
                    m_MapUI.OutPutTextInfo(string.Format("elapsed:  {0} \r\n", elapsedTime));
                    m_MapUI.OutPutTextInfo(string.Format("in hours:  {0} \r\n", elapsedTime.TotalHours));
                    m_MapUI.OutPutTextInfo(string.Format("in minutes: {0} \r\n", elapsedTime.TotalMinutes));
                    m_MapUI.OutPutTextInfo(string.Format("in second: {0} \r\n", elapsedTime.TotalSeconds));
                    m_MapUI.OutPutTextInfo(string.Format("in milliseconds: {0} \r\n", elapsedTime.TotalMilliseconds));
                }

                //2016-7-24测试时间
                //outputtext("elapsed: " + elapsedtime);
                //outputtext("in hours: " + elapsedtime.totalhours);
                //outputtext("in minutes: " + elapsedtime.totalminutes);
                //outputtext("in seconds: " + elapsedtime.totalseconds);
                //outputtext("in milliseconds: " + elapsedtime.totalmilliseconds);
                //2016-7-24测试时间



            }
        }
    }
}