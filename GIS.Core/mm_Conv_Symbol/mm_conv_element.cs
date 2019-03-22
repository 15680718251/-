using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using GIS.Geometries;
using GIS.GeoData;
using System.Windows.Forms;
using System.Collections.Specialized;


namespace GIS.mm_Conv_Symbol
{
    public enum SymbolType { POINTTYPE, POINTTYPEEX, LINETYPE, REGIONTYPE };
    
    public class mm_conv_geometry
    {

        #region private members

        private List<Int32> m_sPointFeatID, m_sPtEleFeatID, m_sLineFeatID, m_sLineEleFeatID, m_sRegionFeatID;   //store  featureid
        private List<string> m_sPointName, m_sPtEleName, m_sLineName, m_sLineEleName, m_sRegionName;   //store  Name
        private List<string> m_sPointPara, m_sPtElePara, m_sLinePara, m_sLineElePara, m_sRegionPara;

        private Int32 CurrentFeatID;

        private List<string> m_PointEleStyles;
        private List<List<double>> m_PointDrawingPara;
        private List<List<double>> m_LineDrawingPara;
        private List<List<double>> m_RegionDrawingPara;
        private List<string> m_LineEleStyles;
        private List<string> m_RegionEleStyles;
        public bool buseCharMap = false;
        public string strcurcharmap = null;
        public Dictionary<int, int> m_codetable = null;
        #endregion

        public List<string> regionelestyles
        {
            get { return m_RegionEleStyles; }
            set { m_RegionEleStyles = value; }
        }

        public List<List<double>> regiondrawingpara
        {
            get { return m_RegionDrawingPara; }
            set { m_RegionDrawingPara = value; }
        }
        public List<string> pointelestyles
        {
            get { return m_PointEleStyles; }
            set { m_PointEleStyles = value; }
        }

        public List<List<double>> pointdrawingpara
        {
            get { return m_PointDrawingPara; }
            set { m_PointDrawingPara = value; }
        }

        public List<string> pointname
        {
            get { return m_sPointName; }
            set { m_sPointName = value; }
        }

        public List<string> pointpara
        {
            get { return m_sPointPara; }
            set { m_sPointPara = value; }
        }

        public List<Int32> pointfeatid
        {
            get { return m_sPointFeatID; }
            set { m_sPointFeatID = value; }
        }

        public List<string> ptelename
        {
            get { return m_sPtEleName; }
            set { m_sPtEleName = value; }
        }

        public List<string> ptelepara
        {
            get { return m_sPtElePara; }
            set { m_sPtElePara = value; }
        }

        public List<Int32> ptelefeatid
        {
            get { return m_sPtEleFeatID; }
            set { m_sPtEleFeatID = value; }
        }

        public List<Int32> lineelefeatid
        {
            get { return m_sLineEleFeatID; }
            set { m_sLineEleFeatID = value; }
        }

        public List<string> lineelepara
        {
            get { return m_sLineElePara; }
            set { m_sLineElePara = value; }
        }

        public List<string> lineelename
        {
            get { return m_sLineEleName; }
            set { m_sLineEleName = value; }
        }

        public List<Int32> linefeatid
        {
            get { return m_sLineFeatID; }
            set { m_sLineFeatID = value; }
        }

        public List<string> linepara
        {
            get { return m_sLinePara; }
            set { m_sLinePara = value; }
        }

        public List<string> linename
        {
            get { return m_sLineName; }
            set { m_sLineName = value; }
        }

        public mm_conv_geometry()
        {
            m_sPointFeatID = new List<Int32>();
            m_sPtEleFeatID = new List<Int32>();
            m_sLineFeatID = new List<Int32>();
            m_sRegionFeatID = new List<Int32>();
            m_sLineEleFeatID = new List<Int32>();

            m_sPointName = new List<string>();
            m_sPtEleName = new List<string>();
            m_sLineName = new List<string>();
            m_sRegionName = new List<string>();
            m_sLineEleName = new List<string>();

            m_sPointPara = new List<string>();
            m_sPtElePara = new List<string>();
            m_sLinePara = new List<string>();
            m_sRegionPara = new List<string>();
            m_sLineElePara = new List<string>();

            m_PointDrawingPara = new List<List<double>>();
            m_LineDrawingPara = new List<List<double>>();
            m_RegionDrawingPara = new List<List<double>>();
            m_LineEleStyles = new List<string>();
            m_PointEleStyles = new List<string>();
            m_codetable = new Dictionary<int, int>();
            m_RegionEleStyles = new List<string>();

            ReadSymbolCodeCharMap();
            ReadSymbolFromDisk();
            
        }

        public void ReadSymbolCodeCharMap()
        {
            if (!buseCharMap)
                return;
            if (strcurcharmap == null || !File.Exists(strcurcharmap))
            {
                MessageBox.Show("请设置符号映射表的位置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!strcurcharmap.ToLower().EndsWith(".txt"))
            {
                MessageBox.Show("符号映射表文件类型不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FileStream symbfile = new FileStream(strcurcharmap, FileMode.OpenOrCreate, FileAccess.Read);

            StreamReader tmpstream = new StreamReader(strcurcharmap, Encoding.Default);

            while (!tmpstream.EndOfStream)
            {
                string strline = tmpstream.ReadLine();
                string[] strparts = strline.Split(' ');
                int it0 = 0;
                int it1 = 0;
                try
                {
                    int.TryParse(strparts[0], out it0);
                    int.TryParse(strparts[1], out it1);
                }
                catch
                {

                }
                m_codetable.Add(it0, it1);
            }

        }


        public void WriteSymbol2Disk()
        {
            string cur_path = Application.StartupPath;
            string str_path = cur_path + "\\10000.txt";

            FileStream symbfile = new FileStream(str_path, FileMode.Create, FileAccess.Write);
          
            StreamWriter writestream = new StreamWriter(symbfile, Encoding.Default);

            writestream.WriteLine("/***********************************************************");
            writestream.Write(";;  Symbol Definition file\r\n;;  Specifications for cartographic symbols\r\n;;  1:500 1:1000 1:2000 topographic maps\r\n;;  Copyright 2011  by JiangQi\r\n;;  Version 1.0\r\n");
            writestream.WriteLine("/***********************************************************\r\n");
            
            DateTime now = System.DateTime.Now;
            writestream.Write("Last Update:\r\r");
            writestream.WriteLine(now);
            #region -------------------------write point--------------------------------------------------
            //
            //write point
            //
            writestream.WriteLine("<Point>");
            for (int i = 0; i < m_sPointFeatID.Count; i++)
            {
                string packedline = "";
                packedline += m_sPointFeatID[i] + ";";
                packedline += m_sPointPara[i] + "#";
                packedline += m_sPointName[i];
                writestream.WriteLine(packedline);
            }
           
            writestream.WriteLine("</Point>");
            #endregion


            writestream.Write("--------------------------------------------------------------------\r\n");

            #region -----------------------write line-----------------------------------------------------
            //
            //write line
            //
            writestream.WriteLine("<Line>");
            for (int i = 0; i < m_sLineFeatID.Count; i++)
            {
                string packedline = "";
                packedline += m_sLineFeatID[i] + ";";
                packedline += m_sLinePara[i] + "#";
                packedline += m_sLineName[i];
                writestream.WriteLine(packedline);
            }
            writestream.WriteLine("</Line>");
            #endregion

            writestream.Write("--------------------------------------------------------------------\r\n");

            #region ------------------------write region---------------------------------------------------
            //
            // write region
            //
            writestream.WriteLine("<Region>");

            for (int i = 0; i < m_sRegionFeatID.Count; i++)
            {
                string packedline = "";
                packedline += m_sRegionFeatID[i] + ";";
                packedline += m_sRegionPara[i] + "#";
                packedline += m_sRegionName[i];
                writestream.WriteLine(packedline);
            }

            writestream.WriteLine("</Region>");
           
            #endregion

             writestream.Write("--------------------------------------------------------------------\r\n");

            #region -------------------------write point element------------------------------------------
            //
             //write point element
            //
            writestream.WriteLine("<Point Element>");
            for (int i = 0; i < m_sPtEleFeatID.Count; i++)
            {
                string packedline = "";
                packedline += m_sPtEleFeatID[i] + ";";
                packedline += m_sPtElePara[i] + "#";
                packedline += m_sPtEleName[i];
                writestream.WriteLine(packedline);
            }
            writestream.WriteLine("</Point Element>");
            #endregion

            writestream.Write("--------------------------------------------------------------------\r\n");
            #region ----------------------------write line element---------------------------------------
            //
            //write line element
            //
            writestream.WriteLine("<Line Element>");
            for (int i = 0; i < m_sLineEleFeatID.Count; i++)
            {
                string packedline = "";
                packedline += m_sLineEleFeatID[i] + ";";
                packedline += m_sLineElePara[i] + "#";
                packedline += m_sLineEleName[i];
                writestream.WriteLine(packedline);
            }

            writestream.WriteLine("</Line Element>");
            #endregion

            writestream.Close();
            symbfile.Close();
            
        }

        public void ReadSymbolFromDisk()
        {
            string cur_path = Application.StartupPath;
            string str_path = cur_path + "\\10000.txt";

            if (!File.Exists(str_path))
                throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", str_path));
            if (!str_path.ToLower().EndsWith(".txt"))
                throw (new Exception("Invalid filename: " + str_path));

            FileStream symbfile = new FileStream(str_path, FileMode.OpenOrCreate, FileAccess.Read);

            StreamReader tmpstream = new StreamReader(str_path, Encoding.Default);

            string sFileContents = tmpstream.ReadToEnd();
            if (sFileContents == null)
                return;

            if (!sFileContents.IsNormalized())
            {
                sFileContents = sFileContents.Normalize(NormalizationForm.FormKD);
            }
            tmpstream.Close();
            symbfile.Close();
            //
            //--------------------points--------------------------------------------------------
            //
            int nIndexBegin = sFileContents.IndexOf("<Point>");
            int nIndexEnd = sFileContents.IndexOf("</Point>");

            if (-1 == nIndexBegin || -1 == nIndexEnd)
            {

                MessageBox.Show("符号参数文件格式不匹配，点对象构建失败");
                //throw (new Exception("符号参数文件格式不匹配，点对象构建失败 " + sSymbolfile));
                return;
            }
            string strPoints = sFileContents.Substring(nIndexBegin + 9, nIndexEnd - nIndexBegin - 9);
            ReadPoints(strPoints);
            //
            //------------------point element-----------------------------------------------------
            //
            nIndexBegin = sFileContents.IndexOf("<Point Element>");
            nIndexEnd = sFileContents.IndexOf("</Point Element>");

            if (-1 == nIndexBegin || -1 == nIndexEnd)
            {

                MessageBox.Show("符号参数文件格式不匹配，点图元构建失败");
                //throw (new Exception("符号参数文件格式不匹配，点对象构建失败 " + sSymbolfile));
                return;
            }
            string strPtElement = sFileContents.Substring(nIndexBegin + 17, nIndexEnd - nIndexBegin - 17);
            ReadPtElement(strPtElement);


            //
            //--------------------lines-----------------------------------------------------------
            //
            nIndexBegin = sFileContents.IndexOf("<Line>");
            nIndexEnd = sFileContents.IndexOf("</Line>");
            if (-1 == nIndexBegin || -1 == nIndexEnd)
            {
                MessageBox.Show("符号参数文件格式不匹配，线对象构建失败");
                //throw (new Exception("符号参数文件格式不匹配，线对象构建失败 " + sSymbolfile));
                return;
            }
            string strLines = sFileContents.Substring(nIndexBegin + 8, nIndexEnd - nIndexBegin - 8);
            ReadLines(strLines);
            //
            //--------------------lines element------------------------------------------------------
            //
            nIndexBegin = sFileContents.IndexOf("<Line Element>");
            nIndexEnd = sFileContents.IndexOf("</Line Element>");
            if (-1 == nIndexBegin || -1 == nIndexEnd)
            {
                MessageBox.Show("符号参数文件格式不匹配，线图元对象构建失败");
                //throw (new Exception("符号参数文件格式不匹配，线对象构建失败 " + sSymbolfile));
                return;
            }
            string strLineElement = sFileContents.Substring(nIndexBegin + 16, nIndexEnd - nIndexBegin - 16);
            ReadLineElement(strLineElement);

            //
            //---------------------polygon--------------------------------------------------------------------

            //

            nIndexBegin = sFileContents.IndexOf("<Region>");
            nIndexEnd = sFileContents.IndexOf("</Region>");
            if (-1 == nIndexBegin || -1 == nIndexEnd)
            {
                MessageBox.Show("符号参数文件格式不匹配，面图元对象构建失败");
                //throw (new Exception("符号参数文件格式不匹配，线对象构建失败 " + sSymbolfile));
                return;
            }
            string strRegion = sFileContents.Substring(nIndexBegin + 10, nIndexEnd - nIndexBegin - 10);
            ReadRegion(strRegion);
        }

        private void ReadRegion(string strRegion)
        {
            if (strRegion == null)
                return;
            int nIndexEndLine = strRegion.IndexOf("\r\n");
            int nIndex = strRegion.IndexOf("#");
            int nPrevPos = 0;
            string sSymbFeatidTemp, sSymbParaTemp, sSymbNameTemp;
            int nIndexf = 0;
            while (-1 != nIndexEndLine && -1 != nIndex)
            {
                if (nIndexEndLine > nIndex)
                {
                    nIndexf = strRegion.IndexOf(';', nPrevPos);
                    sSymbFeatidTemp = strRegion.Substring(nPrevPos, nIndexf - nPrevPos);
                    sSymbParaTemp = strRegion.Substring(nIndexf + 1, nIndex - nIndexf - 1);
                    sSymbNameTemp = strRegion.Substring(nIndex + 1, nIndexEndLine - nIndex - 1);

                    m_sRegionFeatID.Add(System.Convert.ToInt32(sSymbFeatidTemp));
                    m_sRegionPara.Add(sSymbParaTemp);
                    m_sRegionName.Add(sSymbNameTemp);
                }
                else
                {//防止读取空行
                    nPrevPos = nIndexEndLine + 2;
                    nIndexEndLine = strRegion.IndexOf("\r\n", nIndexEndLine + 2);
                    continue;
                }
                nPrevPos = nIndexEndLine + 2;
                nIndexEndLine = strRegion.IndexOf("\r\n", nPrevPos);
                nIndex = strRegion.IndexOf("#", nPrevPos);
            }

        }


        private void ReadPtElement(string strptele)
        {
            if (strptele == null)
                return;

            int nIndexEndLine = strptele.IndexOf("\r\n");
            int nIndex = strptele.IndexOf("#");
            int nPrevPos = 0;
            string sSymbFeatidTemp, sSymbParaTemp, sSymbNameTemp;
            int nIndexf = 0;
            while (-1 != nIndexEndLine && -1 != nIndex)
            {
                if (nIndexEndLine > nIndex)
                {
                    nIndexf = strptele.IndexOf(';', nPrevPos);
                    sSymbFeatidTemp = strptele.Substring(nPrevPos, nIndexf - nPrevPos);
                    sSymbParaTemp = strptele.Substring(nIndexf + 1, nIndex  - nIndexf  - 1);
                    sSymbNameTemp = strptele.Substring(nIndex + 1, nIndexEndLine - nIndex - 1);

                    m_sPtEleFeatID.Add(System.Convert.ToInt32(sSymbFeatidTemp));
                    m_sPtElePara.Add(sSymbParaTemp);
                    m_sPtEleName.Add(sSymbNameTemp);
                }
                else
                {//防止读取空行
                    nPrevPos = nIndexEndLine + 2;
                    nIndexEndLine = strptele.IndexOf("\r\n", nIndexEndLine + 2);
                    continue;
                }
                nPrevPos = nIndexEndLine + 2;
                nIndexEndLine = strptele.IndexOf("\r\n", nPrevPos);
                nIndex = strptele.IndexOf("#", nPrevPos);
            }
        }

        private void ReadPoints(string strpts)
        {
            if (strpts == null)
                return;

            int nIndexEndLine = strpts.IndexOf("\r\n");
            int nIndex = strpts.IndexOf("#");
            int nPrevPos = 0;
            int nIndexf = 0;
            string sSymbFeatidTemp, sSymbParaTemp, sSymbNameTemp;
            while (-1 != nIndexEndLine && -1 != nIndex)
            {
                if (nIndexEndLine > nIndex)
                {
                    nIndexf = strpts.IndexOf(';', nPrevPos);
                    sSymbFeatidTemp = strpts.Substring(nPrevPos, nIndexf - nPrevPos);
                    sSymbParaTemp = strpts.Substring(nIndexf + 1, nIndex - nIndexf - 1);
                    sSymbNameTemp = strpts.Substring(nIndex + 1, nIndexEndLine - nIndex - 1);

                    m_sPointFeatID.Add(System.Convert.ToInt32( sSymbFeatidTemp));
                    m_sPointPara.Add(sSymbParaTemp);
                    m_sPointName.Add(sSymbNameTemp);
                }
                else
                {//防止读取空行
                    nPrevPos = nIndexEndLine + 2;
                    nIndexEndLine = strpts.IndexOf("\r\n", nIndexEndLine + 2);
                    continue;
                }
                nPrevPos = nIndexEndLine + 2;
                nIndexEndLine = strpts.IndexOf("\r\n", nPrevPos);
                nIndex = strpts.IndexOf("#", nPrevPos);
            
            }
        }
        private void ReadLineElement(string strlineele)
        {

            if (strlineele == null)
                return;
            int nIndexEndLine = strlineele.IndexOf("\r\n");
            int nIndex = strlineele.IndexOf("#");
            int nPrevPos = 0;
            int nIndexf = 0;
            string sSymbFeatidTemp, sSymbParaTemp, sSymbNameTemp;
            while (-1 != nIndexEndLine && -1 != nIndex)
            {
                if (nIndexEndLine > nIndex)
                {
                    nIndexf = strlineele.IndexOf(';', nPrevPos);
                    sSymbFeatidTemp = strlineele.Substring(nPrevPos, nIndexf - nPrevPos);
                    sSymbParaTemp = strlineele.Substring(nIndexf + 1, nIndex - nIndexf - 1);
                    sSymbNameTemp = strlineele.Substring(nIndex + 1, nIndexEndLine - nIndex - 1);

                    m_sLineEleFeatID.Add(System.Convert.ToInt32(sSymbFeatidTemp));
                    m_sLineElePara.Add(sSymbParaTemp);
                    m_sLineEleName.Add(sSymbNameTemp);
                }
                else
                {//防止读取空行
                    nPrevPos = nIndexEndLine + 2;
                    nIndexEndLine = strlineele.IndexOf("\r\n", nIndexEndLine + 2);
                    continue;
                }
                nPrevPos = nIndexEndLine + 2;
                nIndexEndLine = strlineele.IndexOf("\r\n", nPrevPos);
                nIndex = strlineele.IndexOf("#", nPrevPos);

            }
        }

        private void ReadLines(string strlines)
        {
            if (strlines == null)
                return;
            int nIndexEndLine = strlines.IndexOf("\r\n");
            int nIndex = strlines.IndexOf("#");
            int nPrevPos = 0;
            int nIndexf = 0;
            string sSymbFeatidTemp, sSymbParaTemp, sSymbNameTemp;
            while (-1 != nIndexEndLine && -1 != nIndex)
            {
                if (nIndexEndLine > nIndex)
                {
                    nIndexf = strlines.IndexOf(';', nPrevPos);
                    sSymbFeatidTemp = strlines.Substring(nPrevPos, nIndexf - nPrevPos);
                    sSymbParaTemp = strlines.Substring(nIndexf + 1, nIndex - nIndexf - 1);
                    sSymbNameTemp = strlines.Substring(nIndex + 1, nIndexEndLine - nIndex - 1);

                    m_sLineFeatID.Add(System.Convert.ToInt32(sSymbFeatidTemp));
                    m_sLinePara.Add(sSymbParaTemp);
                    m_sLineName.Add(sSymbNameTemp);
                }
                else
                {//防止读取空行
                    nPrevPos = nIndexEndLine + 2;
                    nIndexEndLine = strlines.IndexOf("\r\n", nIndexEndLine + 2);
                    continue;
                }
                nPrevPos = nIndexEndLine + 2;
                nIndexEndLine = strlines.IndexOf("\r\n", nPrevPos);
                nIndex = strlines.IndexOf("#", nPrevPos);

            }
        }

        public bool Extract_LineSymbol(int index)
        {
            Collection<string> sSymbParaEachLay = new Collection<string>();
            string sTemp = "";
            string sDrawingPara = m_sLinePara[index];  //存放符号要素编码对应的点符号的描述信息


            int endindex = sDrawingPara.IndexOf(';');
            if (endindex == -1)
            {
                MessageBox.Show("点图元参数错误");
                return false;
            }
            int startindex = 0;

            while (-1 != endindex)
            {
                sSymbParaEachLay.Add(sDrawingPara.Substring(startindex, endindex - startindex));
                startindex = endindex + 1;
                endindex = sDrawingPara.IndexOf(';', startindex);

            }

            int nLaySize = sSymbParaEachLay.Count;  //根据;分成层的个数

            m_LineDrawingPara = new List<List<double>>();
            for (int i = 0; i < nLaySize; i++)
            {
                m_LineDrawingPara.Add(new List<double>());
            }

            m_LineEleStyles = new List<string>();

            for (int j = 0; j < nLaySize; ++j)
            {
                int eindex = sSymbParaEachLay[j].IndexOf(',');
                int sindex = 0;

                if (-1 != eindex)
                {
                    m_LineEleStyles.Add(sSymbParaEachLay[j].Substring(sindex, eindex - sindex));  //第一个为类型描述符，eg:c,s,....
                    sindex = eindex + 1;
                }
                else
                {
                    continue;
                }

                while (-1 != eindex)
                {
                    eindex = sSymbParaEachLay[j].IndexOf(',', sindex);

                    if (-1 != eindex)
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex, eindex - sindex);

                       m_LineDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                      
                        sindex = eindex + 1;
                    }
                    else
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex);
                        m_LineDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                    }
                }
            }
            return true;
        }

        public bool Extract_LineEleSymbol(int index)
        {
            Collection<string> sSymbParaEachLay = new Collection<string>();
            string sTemp = "";
            string sDrawingPara = m_sLineElePara[index];  //存放符号要素编码对应的点符号的描述信息


            int endindex = sDrawingPara.IndexOf(';');
            if (endindex == -1)
            {
                MessageBox.Show("点图元参数错误");
                return false;
            }
            int startindex = 0;

            while (-1 != endindex)
            {
                sSymbParaEachLay.Add(sDrawingPara.Substring(startindex, endindex - startindex));
                startindex = endindex + 1;
                endindex = sDrawingPara.IndexOf(';', startindex);

            }

            int nLaySize = sSymbParaEachLay.Count;  //根据;分成层的个数

            m_LineDrawingPara = new List<List<double>>();
            for (int i = 0; i < nLaySize; i++)
            {
                m_LineDrawingPara.Add(new List<double>());
            }

            m_LineEleStyles = new List<string>();

            for (int j = 0; j < nLaySize; ++j)
            {
                int eindex = sSymbParaEachLay[j].IndexOf(',');
                int sindex = 0;

                if (-1 != eindex)
                {
                    m_LineEleStyles.Add(sSymbParaEachLay[j].Substring(sindex, eindex - sindex));  //第一个为类型描述符，eg:c,s,....
                    sindex = eindex + 1;
                }
                else
                {
                    continue;
                }

                while (-1 != eindex)
                {
                    eindex = sSymbParaEachLay[j].IndexOf(',', sindex);

                    if (-1 != eindex)
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex, eindex - sindex);
                        m_LineDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                        sindex = eindex + 1;
                    }
                    else
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex);
                        m_LineDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                    }
                }
            }
            return true;
        }

        //
        // unsafe!!!!!!! only used for listview
        //
        public bool Extract_PtElement(int index)
        {  
            
            Collection<string> sSymbParaEachLay = new Collection<string>();
            string sTemp = "";
            string sDrawingPara = m_sPtElePara[index];  //存放符号要素编码对应的点符号的描述信息


            int endindex = sDrawingPara.IndexOf(';');
            if (endindex == -1)
            {
                MessageBox.Show("点图元参数错误");
                return false;
            }
            int startindex = 0;

            while (-1 != endindex)
            {
                sSymbParaEachLay.Add(sDrawingPara.Substring(startindex, endindex - startindex));
                startindex = endindex + 1;
                endindex = sDrawingPara.IndexOf(';', startindex);

            }

            int nLaySize = sSymbParaEachLay.Count;  //根据;分成层的个数

            m_PointDrawingPara = new List<List<double>>();
            for (int i = 0; i < nLaySize; i++)
            {
                m_PointDrawingPara.Add(new List<double>());
            }

            m_PointEleStyles = new List<string>();

            for (int j = 0; j < nLaySize; ++j)
            {
                int eindex = sSymbParaEachLay[j].IndexOf(',');
                int sindex = 0;

                if (-1 != eindex)
                {
                    m_PointEleStyles.Add(sSymbParaEachLay[j].Substring(sindex, eindex - sindex));  //第一个为类型描述符，eg:c,s,....
                    sindex = eindex + 1;
                }
                else
                {
                    continue;
                }

                while (-1 != eindex)
                {
                    eindex = sSymbParaEachLay[j].IndexOf(',', sindex);

                    if (-1 != eindex)
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex, eindex - sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                        sindex = eindex + 1;
                    }
                    else
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                    }
                }
            }
            return true;

        }

        public bool Extract_LineElementbyID(int id)
        {
            int index = -1;
            for (int i = 0; i < m_sLineElePara.Count; i++)
            {
                if (m_sLineEleFeatID[i] == id)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return false;

            string sDrawingPara = m_sLineElePara[index];  //存放符号要素编码对应的点符号的描述信息
            sDrawingPara = sDrawingPara.Remove(sDrawingPara.Length - 1);
            try
            {
                string[] sSymbParaEachLay = sDrawingPara.Split(';');
       
                int nLaySize = sSymbParaEachLay.Length;  //根据;分成层的个数

                m_LineDrawingPara = new List<List<double>>();
                m_LineEleStyles = new List<string>();
                for (int i = 0; i < nLaySize; i++)
                {
                    m_LineDrawingPara.Add(new List<double>());
                    string tmp = sSymbParaEachLay[i];
                    string[] ttt = tmp.Split(',');
                    m_LineEleStyles.Add(ttt[0]);

                    for (int j = 1; j < ttt.Length; j++)
                    {
                        m_LineDrawingPara[i].Add(System.Convert.ToDouble(ttt[j]));
                    }

                }
                return true;
            }
            catch
            {
                MessageBox.Show("面参数错误");
                return false;
            }

        }

        public bool Extract_PtElementbyID(int id)
        {
            Collection<string> sSymbParaEachLay = new Collection<string>();
            string sTemp = "";

            int index = -1;
            for (int i = 0; i < m_sPtElePara.Count; i++)
            {
                if (m_sPtEleFeatID[i] == id)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return false ;
            string sDrawingPara = m_sPtElePara[index];  //存放符号要素编码对应的点符号的描述信息


            int endindex = sDrawingPara.IndexOf(';');
            if (endindex == -1)
            {
                MessageBox.Show("点图元参数错误");
            }
            int startindex = 0;

            while (-1 != endindex)
            {
                sSymbParaEachLay.Add(sDrawingPara.Substring(startindex, endindex - startindex));
                startindex = endindex + 1;
                endindex = sDrawingPara.IndexOf(';', startindex);

            }

            int nLaySize = sSymbParaEachLay.Count;  //根据;分成层的个数

            m_PointDrawingPara = new List<List<double>>();
            for (int i = 0; i < nLaySize; i++)
            {
                m_PointDrawingPara.Add(new List<double>());
            }

            m_PointEleStyles = new List<string>();

            for (int j = 0; j < nLaySize; ++j)
            {
                int eindex = sSymbParaEachLay[j].IndexOf(',');
                int sindex = 0;

                if (-1 != eindex)
                {
                    m_PointEleStyles.Add(sSymbParaEachLay[j].Substring(sindex, eindex - sindex));  //第一个为类型描述符，eg:c,s,....
                    sindex = eindex + 1;
                }
                else
                {
                    continue;
                }

                while (-1 != eindex)
                {
                    eindex = sSymbParaEachLay[j].IndexOf(',', sindex);

                    if (-1 != eindex)
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex, eindex - sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                        sindex = eindex + 1;
                    }
                    else
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                    }
                }
            }

            return true;
        }

        public bool Extract_RegionSymbol(int index)
        {
            string sDrawingPara = m_sRegionPara[index];  //存放符号要素编码对应的点符号的描述信息
            sDrawingPara = sDrawingPara.Remove(sDrawingPara.Length - 1);
            try
            {
                string[] sSymbParaEachLay = sDrawingPara.Split(';');

                int nLaySize = sSymbParaEachLay.Length;  //根据;分成层的个数

                m_RegionDrawingPara = new List<List<double>>();
                m_RegionEleStyles = new List<string>();
                for (int i = 0; i < nLaySize; i++)
                {
                    m_RegionDrawingPara.Add(new List<double>());
                    string tmp = sSymbParaEachLay[i];
                    string[] ttt = tmp.Split(',');
                    m_RegionEleStyles.Add(ttt[0]);

                    for (int j = 1; j < ttt.Length; j++)
                    {
                        m_RegionDrawingPara[i].Add(System.Convert.ToDouble(ttt[j]));
                    }

                }
                return true; 
            }
            catch
            {
                MessageBox.Show("面参数错误");
                return false;
            }

        }


        public bool Extract_PointSymbol(int index)
        {
            Collection<string> sSymbParaEachLay = new Collection<string>();
            string sTemp = "";
            string sDrawingPara = m_sPointPara[index];  //存放符号要素编码对应的点符号的描述信息


            int endindex = sDrawingPara.IndexOf(';');
            if (endindex == -1)
            {
                MessageBox.Show("点参数错误");
                return false;
            }
            int startindex = 0;

            while (-1 != endindex)
            {
                sSymbParaEachLay.Add(sDrawingPara.Substring(startindex, endindex - startindex));
                startindex = endindex + 1;
                endindex = sDrawingPara.IndexOf(';', startindex);

            }

            int nLaySize = sSymbParaEachLay.Count;  //根据;分成层的个数

            m_PointDrawingPara = new List<List<double>>();
            for (int i = 0; i < nLaySize; i++)
            {
                m_PointDrawingPara.Add(new List<double>());
            }

            m_PointEleStyles = new List<string>();

            for (int j = 0; j < nLaySize; ++j)
            {
                int eindex = sSymbParaEachLay[j].IndexOf(',');
                int sindex = 0;

                if (-1 != eindex)
                {
                    m_PointEleStyles.Add(sSymbParaEachLay[j].Substring(sindex, eindex - sindex));  //第一个为类型描述符，eg:c,s,....
                    sindex = eindex + 1;
                }
                else
                {
                    continue;
                }

                while (-1 != eindex)
                {
                    eindex = sSymbParaEachLay[j].IndexOf(',', sindex);

                    if (-1 != eindex)
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex, eindex - sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                        sindex = eindex + 1;
                    }
                    else
                    {
                        sTemp = sSymbParaEachLay[j].Substring(sindex);
                        m_PointDrawingPara[j].Add(System.Convert.ToDouble(sTemp));
                    }
                }
            }
            return true;
        }


        public LineSymbol conv_geolinestring(GeoLineString line, int code)
        {
            //调用各种函数进行运算

            //search feature id
            int featid = code;

            if (buseCharMap != false && strcurcharmap != null)
            {
                int result = 0;
                try
                {
                    result = m_codetable[featid];
                }
                catch
                {

                }
                featid = result;
            }

            bool bfind = false;
            int i = 0;
            for (; i < m_sLineFeatID.Count; i++)
            {
                if (System.Convert.ToInt32(m_sLineFeatID[i]) == featid)
                {
                    bfind = true;
                    break;
                }
            }

            if (!bfind)
            {
                MessageBox.Show("ClassID未找到");
                return null;
            }

            CurrentFeatID = featid;

            //------------------------------------

            Extract_LineSymbol(i);

            PointF[] pts = new PointF[line.Vertices.Count];
            for (int j = 0; j < line.Vertices.Count; j++)
            {
                pts[j].X = (float)line.Vertices[j].X;
                pts[j].Y = (float)line.Vertices[j].Y;
            }

            return (generate_linesymbol(pts));

            //--------------------------------------


        }
        public void conv_geometry(GeoDataRow datarow)
        {
            if (datarow.Geometry is GeoPoint)
            {
                conv_geopoint(datarow);
            }
            else if (datarow.Geometry is GeoLineString)
            {
                conv_geolinestring(datarow);
            }
            else if (datarow.Geometry is GeoPolygon)
            {
                conv_geopolygon(datarow);
            }
        }

        public void conv_geopolygon(GeoDataRow datarow)
        {
            int featid;
            try
            {
                featid = System.Convert.ToInt32(datarow["ClasID"]);
            }
            catch (Exception e)
            {
                //ClasID == null
                datarow.bsymrender = false;
                return;
            }

            if (buseCharMap != false && strcurcharmap != null)
            {
                int result = 0;
                try
                {
                    result = m_codetable[featid];
                }
                catch
                {

                }
                featid = result;
            }

            bool bfind = false;
            int i = 0;
            for (; i < m_sRegionFeatID.Count; i++)
            {
                if (System.Convert.ToInt32(m_sRegionFeatID[i]) == featid)
                {
                    bfind = true;
                    break;
                }
            }

            if (!bfind)
            {
                //MessageBox.Show("ClassID未找到");
                datarow.bsymrender = false;
                return;
            }

            CurrentFeatID = featid;

            GeoPolygon plg = datarow.Geometry as GeoPolygon;

            Extract_RegionSymbol(i);

            PointF[] pts = new PointF[plg.ExteriorRing.Vertices.Count];
            for (int j = 0; j < pts.Length; j++)
            {
                pts[j].X = (float)plg.ExteriorRing.Vertices[j].X;
                pts[j].Y = (float)plg.ExteriorRing.Vertices[j].Y;
            }



            datarow.symbol = generate_regionsymbol(pts);

            //--------------------------------------
            datarow.bsymrender = true;

        }

        public void conv_geolinestring(GeoDataRow datarow)
        {
            //调用各种函数进行运算
          
            //search feature id
            int featid;
            try
            {
                featid = System.Convert.ToInt32(datarow["ClasID"]);
            }
            catch (Exception e)
            {
                //ClasID == null
                datarow.bsymrender = false;
                return;
            }

            if (buseCharMap != false && strcurcharmap != null)
            {
                int result = 0;
                try
                {
                    result = m_codetable[featid];
                }
                catch
                {

                }
                featid = result;
            }

            bool bfind = false;
            int i = 0;
            for (; i < m_sLineFeatID.Count; i++)
            {
                if (System.Convert.ToInt32(m_sLineFeatID[i]) == featid)
                {
                    bfind = true;
                    break;
                }
            }

            if (!bfind)
            {
                //MessageBox.Show("ClassID未找到");
                datarow.bsymrender = false;
                return;
            }

            CurrentFeatID = featid;

            GeoLineString line = datarow.Geometry as GeoLineString;
            //------------------------------------

            Extract_LineSymbol(i);
      
            PointF []pts = new PointF[line.Vertices.Count];
            for (int j = 0; j < line.Vertices.Count; j++)
            {
                pts[j].X = (float)line.Vertices[j].X;
                pts[j].Y = (float)line.Vertices[j].Y;
            }

            datarow.symbol = generate_linesymbol(pts);
            
            //--------------------------------------
            datarow.bsymrender = true;

        }

        public void conv_geopoint(GeoDataRow datarow)
        {
            //调用各种函数进行运算
            GeoPoint pt = datarow.Geometry as GeoPoint;

            //search feature id
            int featid;
            try
            {
                featid = System.Convert.ToInt32(datarow["ClasID"]);
            }
            catch (Exception e)
            {
                //ClasID == null
                datarow.bsymrender = false;
                return;
            }

            if (buseCharMap != false && strcurcharmap != null)
            {
                int result = 0;
                try
                {
                    result = m_codetable[featid];
                }
                catch
                {

                }
                featid = result;
            }
            

            bool bfind = false;
            int i = 0;
            for (; i < m_sPointFeatID.Count; i++)
            {        
                if (System.Convert.ToInt32(m_sPointFeatID[i]) == featid)
                {
                    bfind = true;
                    break;
                }
            }

            if (!bfind)
            {
                //MessageBox.Show("ClassID未找到");
                datarow.bsymrender = false;
                return;
            }

            CurrentFeatID = featid;


                //------------------------------------

                Extract_PointSymbol(i);
                generate_pointsymbol(datarow);
            //--------------------------------------
                datarow.bsymrender = true;

        }




        public PointSymbol generate_pointsymbol(PointF pt)
        {
            PointSymbol ptsym = new PointSymbol();

            for (int i = 0; i < m_PointEleStyles.Count; i++)
            {
                string tmp = m_PointEleStyles[i];
                switch (tmp)
                {
                    case "C":
                        //circle C,X,Y,a,b,angle,filled,penwidth,r,填充模式

                        ptsym.AddAtom(new Atom_Circle(0, 0, (float)m_PointDrawingPara[i][0], (float)m_PointDrawingPara[i][1],
                            (float)m_PointDrawingPara[i][2], (float)m_PointDrawingPara[i][3], (float)m_PointDrawingPara[i][4], (int)m_PointDrawingPara[i][5] == 1 ? true : false,
                            (float)m_PointDrawingPara[i][6], (float)m_PointDrawingPara[i][7]));
                        //需要检测个数是否一致

                        break;
                    case "A":
                        //arc
                        Atom_Arc arc = new Atom_Arc(0, 0, (float)m_PointDrawingPara[i][0], (float)m_PointDrawingPara[i][1],
                           (float)m_PointDrawingPara[i][2], (float)m_PointDrawingPara[i][3], (float)m_PointDrawingPara[i][4], (int)m_PointDrawingPara[i][5] == 1 ? true : false, (float)m_PointDrawingPara[i][6],
                            (float)m_PointDrawingPara[i][7], (float)m_PointDrawingPara[i][8], (float)m_PointDrawingPara[i][9], (int)m_PointDrawingPara[i][10] == 1 ? true : false);

                        ptsym.AddAtom(arc);

                        break;
                    case "L":
                        //continuous line
                        {
                            List<float> list = new List<float>();
                            for (int j = 0; j < m_PointDrawingPara[i].Count; j++)
                            {
                                list.Add((float)m_PointDrawingPara[i][j]);
                            }
                                ptsym.AddAtom(new Atom_Line4p(0, 0, list));

                            break;
                        }

                    default:
                        break;

                }
            }
            return ptsym;
        }

        public RegionSymbol generate_regionsymbol(PointF[] pts)
        {
            RegionSymbol resym = new RegionSymbol();
            for (int i = 0; i < m_RegionEleStyles.Count; i++)
            {
                string tmp = m_RegionEleStyles[i];
                switch (tmp)
                {
                    case "o":   //outline
                        //conv_geolineEle(resym, pts, (int)m_RegionDrawingPara[i][0]);

                        if (Extract_LineElementbyID((int)m_RegionDrawingPara[i][0]))
                        {
                            LineSymbol ptele = generate_linesymbol(pts);
                            resym.outline = ptele;
                        }
                        break;
                    case "p":    //填充点图元
                        if (Extract_PtElementbyID(System.Convert.ToInt32(m_RegionDrawingPara[i][0])))
                        {
                            PointSymbol ptele = generate_pointsymbol(new PointF(0, 0));
                            resym.mothersymbol.Add(ptele);
                            resym.m_udgap = (float)m_RegionDrawingPara[i][1];
                            resym.m_lrgap = (float)m_RegionDrawingPara[i][2];
                        }
                        break;
                    case "l":    //填充线图元
                        break;
                    default:
                        break;
                }

            }
            resym.initialself(pts);
            return resym;
        }



        public LineSymbol generate_linesymbol(PointF[] pts)
        {
            LineSymbol linesym = new LineSymbol();

            float[] verlen = new float[pts.Length - 1];
            float[] rad = new float[pts.Length - 1];
            float dy = 0f, dx = 0f;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                dy = pts[i + 1].Y - pts[i].Y;
                dx = pts[i + 1].X - pts[i].X;

                verlen[i] = System.Convert.ToSingle(Math.Sqrt(Math.Pow((double)(dx), 2) + Math.Pow((double)(dy), 2)));
                rad[i] = (float)Math.Atan((double)(dy / dx));
            }

            for (int i = 0; i < m_LineEleStyles.Count; i++)
            {
                string tmp = m_LineEleStyles[i];
                switch (tmp)
                {
                    case "P":
                        //点图元                   
                        if (Extract_PtElementbyID(System.Convert.ToInt32(m_LineDrawingPara[i][0])))
                        {
                            PointSymbol ptele = generate_pointsymbol(new PointF(0, 0));
                            linesym.AddPointSymbol(ptele, pts, (float)m_LineDrawingPara[i][1], (float)m_LineDrawingPara[i][2], (float)m_LineDrawingPara[i][3], verlen, rad);
                        }

                        break;
                    case "D":
                        //dash line
                        Atom_DashLine dashline = new Atom_DashLine((float)m_LineDrawingPara[i][0], (float)m_LineDrawingPara[i][1],
                           (float)m_LineDrawingPara[i][2], (float)m_LineDrawingPara[i][4], (float)m_LineDrawingPara[i][5], pts, verlen, rad);
                        linesym.AddAtom(dashline);
                        break;
                    case "L":
                        //solid line
                        Atom_SolidLine solidline = new Atom_SolidLine((float)m_LineDrawingPara[i][0], (float)m_LineDrawingPara[i][1], pts, verlen, rad);
                        linesym.AddAtom(solidline);
                        break;
                    default:
                        break;
                }
            }
            return linesym;
        }

        public LineSymbol generate_lineelement(PointF[] pts)
        {
            LineSymbol linesym = new LineSymbol();

            float[] verlen = new float[pts.Length - 1];
            float[] rad = new float[pts.Length - 1];
            float dy = 0f, dx = 0f;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                dy = pts[i + 1].Y - pts[i].Y;
                dx = pts[i + 1].X - pts[i].X;

                verlen[i] = System.Convert.ToSingle(Math.Sqrt(Math.Pow((double)(dx), 2) + Math.Pow((double)(dy), 2)));
                rad[i] = (float)Math.Atan((double)(dy / dx));
            }

            for (int i = 0; i < m_LineEleStyles.Count; i++)
            {
                string tmp = m_LineEleStyles[i];
                switch (tmp)
                {
                    case "P":
                        //点图元                   
                        if (Extract_PtElementbyID(System.Convert.ToInt32(m_LineDrawingPara[i][0])))
                        {
                            PointSymbol ptele = generate_pointsymbol(new PointF(0, 0));
                            linesym.AddPointSymbol(ptele, pts, (float)m_LineDrawingPara[i][1], (float)m_LineDrawingPara[i][2], (float)m_LineDrawingPara[i][3], verlen, rad);
                        }

                        break;
                    case "D":
                        //dash line

                        break;
                    case "S":
                        //solid line
                        break;
                    default:
                        break;
                }
            }
            return linesym;
        }


        private void generate_pointsymbol(GeoDataRow row)
        {

            GeoPoint pt = row.Geometry as GeoPoint;
            PointSymbol ptsym = new PointSymbol();

            for (int i = 0; i < m_PointEleStyles.Count; i++)
            {
                string tmp = m_PointEleStyles[i];
                switch (tmp)
                {
                    case "C":
                        //circle C,X,Y,a,b,angle,filled,penwidth,r,填充模式

                        ptsym.AddAtom(new Atom_Circle((float)pt.X, (float)pt.Y, (float)m_PointDrawingPara[i][0], (float)m_PointDrawingPara[i][1],
                            (float)m_PointDrawingPara[i][2], (float)m_PointDrawingPara[i][3], (float)m_PointDrawingPara[i][4], (int)m_PointDrawingPara[i][5] == 1 ? true : false,
                            (float)m_PointDrawingPara[i][6], (float)m_PointDrawingPara[i][7]));
                         //需要检测个数是否一致

                        break;
                    case "A":
                        //arc
                        ptsym.AddAtom(new Atom_Arc((float)pt.X, (float)pt.Y, (float)m_PointDrawingPara[i][0], (float)m_PointDrawingPara[i][1],
                            (float)m_PointDrawingPara[i][2], (float)m_PointDrawingPara[i][3], (float)m_PointDrawingPara[i][4], (int)m_PointDrawingPara[i][5] == 1 ? true : false, (float)m_PointDrawingPara[i][6],
                            (float)m_PointDrawingPara[i][7], (float)m_PointDrawingPara[i][8], (float)m_PointDrawingPara[i][9], (int)m_PointDrawingPara[i][10] == 1 ? true : false));

                        break;
                    case "L":
                        //continuous line
                        {
                            List<float> list = new List<float>();
                            for (int j = 0; j < m_PointDrawingPara[i].Count; j++)
                            {
                                list.Add((float)m_PointDrawingPara[i][j]);
                            }
                            ptsym.AddAtom(new Atom_Line4p((float)pt.X, (float)pt.Y, list));

                            break;
                        }

                    default:
                        break;
                   
                }
            }
            row.symbol = ptsym;

        }

       

        

       

        





      
    }

}
