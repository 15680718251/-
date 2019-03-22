using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using GIS.Geometries;
using System.IO.Ports;
using System.IO;
 
namespace GIS.TreeIndex.QZY
{
    //全站仪的仪器类型
    public enum StationType
    {
        TOPCON = 0,
        南方NTS660 = 1,
        LEICA300_700 = 2,
    }
    public class QZY
    {

        public StationType m_DeviceType;
        //初始方向的零方位角
        public double m_zerofw;
        SerialPort _serialPort;
        //仪器高
        public double m_HeightOfDevice;
        //毡标高
        public double m_HeightOfPole;
       
        public GeoPoint3D m_StationPt = new GeoPoint3D();
        public GeoPoint3D m_BackSightPt = new GeoPoint3D();
        public string _portName = "COM3";
        public int _baudRate = 1200;
        public Parity _parity = Parity.None;
        public int _dataBits = 8;
        public StopBits _stopBits = StopBits.One;
        public Handshake _handShake = Handshake.None;
        public QZY()
        {
            _serialPort = new SerialPort(); 
            m_DeviceType = StationType.LEICA300_700;
            m_zerofw = 0.0;
            m_HeightOfDevice = 1;
            m_HeightOfPole = 1.5;
        }
        public bool Close()
        {
            try
            {
                _serialPort.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ConnectToDevice(string portName, int baudRade, Parity parity, int databits, StopBits stopbits, Handshake handshake)
        {
            _serialPort.Close();

            try
            {
                _portName = portName;
                _baudRate = baudRade;
                _parity = parity;
                _dataBits = databits;
                _stopBits = stopbits;
                _handShake = handshake;

                _serialPort.PortName = _portName;
                _serialPort.BaudRate = _baudRate;
                _serialPort.Parity = _parity;
                _serialPort.DataBits = _dataBits;
                _serialPort.StopBits = _stopBits;
                _serialPort.Handshake = _handShake;

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 50;
                _serialPort.ReadBufferSize = 512;
                _serialPort.WriteBufferSize = 512;
                _serialPort.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
        public string Read()
        {
            byte[] readBuffer = new byte[_serialPort.ReadBufferSize + 1];
            try
            {
                int count = _serialPort.Read(readBuffer, 0, _serialPort.ReadBufferSize);
                String SerialIn = System.Text.Encoding.ASCII.GetString(readBuffer, 0, count);
                return SerialIn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool GetHVS(ref double H, ref double V, ref double S)
        {
            switch (this.m_DeviceType)
            {
                case StationType.南方NTS660:
                    return this.Get_NTS660_HVS(ref H, ref V, ref S);
                case StationType.TOPCON:
                    return this.Get_TOPCON311S_HVS(ref H, ref V, ref S);
                case StationType.LEICA300_700:
                    return this.Get_LEICA300_700_HVS(ref H, ref V, ref S);
            }
            return false;
        }
        public string Parse(string source, string[] spilt)
        {
            if (source == null)
                return null;
            int index1st = 0;
            if (spilt[0] != null)
            {
                index1st = source.IndexOf(spilt[0]);
            }

            if (index1st == -1)
            {
                index1st = 0;
            }
            int index2sd = source.IndexOf(spilt[1], index1st);

            if (index2sd == -1)
                return null;
            return source.Substring(index1st, index2sd - index1st);
        }
        public bool Get_LEICA300_700_HVS(ref double H, ref double V, ref double S)
        {
            if (!_serialPort.IsOpen)
                return false;
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Write("GET/M/WI21/WI22/WI31\r\n");
            Thread.Sleep(5000);

            string preResult = Read();
            if (preResult == null ||Convert.ToChar(preResult[0]) =='@')
                return false;
             
            string[] spilt = new string[2];
            spilt[0] = null;
            spilt[1] = "\r\n";
            string strTmp = Parse(preResult, spilt);
            if (strTmp == null || strTmp.Length < 20)
                return false;

            string DS, VS, HS;
            int nPos = strTmp.IndexOf("21.");
            HS = strTmp.Substring(nPos + 15, 8);
            VS = strTmp.Substring(nPos + 39, 8);
            DS = strTmp.Substring(nPos + 63, 8);

            DS = DS.Insert(5, ".");
            VS = VS.Insert(3, ".");
            HS = HS.Insert(3, ".");
            H = double.Parse(HS);
            V = double.Parse(VS);
            S = double.Parse(DS);
            return true;
        }
        public bool Get_NTS660_HVS(ref double H, ref double V, ref double S)
        {
            if (!_serialPort.IsOpen)
                return false;
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Write("C067\x03\r\n");
            Thread.Sleep(5000);



            string preResult = Read();
            if (preResult == null)
                return false;
            string[] spilt = new string[2];
            spilt[0] = "?";
            spilt[1] = "\r\n";
            string strTmp = Parse(preResult, spilt);
            if (strTmp == null || strTmp.Length < 20)
                return false;

            string DS, VS, HS;
            DS = strTmp.Substring(2, 8);
            VS = strTmp.Substring(11, 7);
            HS = strTmp.Substring(19, 7);

            DS = DS.Insert(5, ".");
            VS = VS.Insert(3, ".");
            HS = HS.Insert(3, ".");
            H = double.Parse(HS);
            V = double.Parse(VS);
            S = double.Parse(DS);
            return true;
        }
        public bool IsOpen
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }
        public bool Get_TOPCON311S_HVS(ref double H, ref double V, ref double S)
        {
            if (!_serialPort.IsOpen)
                return false;
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Write("C067\x03\x0D\x0A");
            Thread.Sleep(5000);

            string preResult = Read();
            if (preResult == null)
                return false;

            string[] spilt = new string[2];
            spilt[0] = "?";
            spilt[1] = "\x03";
            string strTmp = Parse(preResult, spilt);
            if (strTmp == null || strTmp.Length < 20)
                return false;

            string DS, VS, HS;
            DS = strTmp.Substring(2, 8);
            VS = strTmp.Substring(11, 7);
            HS = strTmp.Substring(19, 7);

            DS = DS.Insert(5, ".");
            VS = VS.Insert(3, ".");
            HS = HS.Insert(3, ".");
            H = double.Parse(HS);
            V = double.Parse(VS);
            S = double.Parse(DS);
            return true;
        }
        public bool HasStationSet
        {
            get
            {
                if (m_StationPt.X == 0 || m_StationPt.Y == 0
                    || m_BackSightPt.X == 0 || m_BackSightPt.Y == 0)
                    return false;
                return true;
            }

        }
        public void CalcuXYZ(double H, double V, double Dist, bool bSlopDist, ref double X, ref double Y, ref double Z)
        {

            double pi = 3.1415926535;

            //求取平距
            if (bSlopDist)
                Dist = Dist * Math.Sin(V);

            double ang = GIS.SpatialRelation.GeoAlgorithm.CalcAzimuth(m_StationPt.X, m_StationPt.Y, m_BackSightPt.X, m_BackSightPt.Y);

            ang = ang - H;

            X = m_StationPt.X + Dist * Math.Cos(ang);
            Y = m_StationPt.Y + Dist * Math.Sin(ang);

            if (m_HeightOfPole < 99999)
                Z = m_StationPt.Z + m_HeightOfDevice - m_HeightOfPole + Dist * Math.Tan(pi / 2.0 - V);
            else
                Z = 99999;

        }
    }
}
