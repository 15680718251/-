using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;
using System.Globalization;

namespace GIS.UI.Forms
{
    public partial class txt2DB : Form
    {
        public txt2DB()
        {
            InitializeComponent();
        }
        OracleDBHelper helper = new OracleDBHelper();
        public enum ChangeType
        {
            CHANGE_APPEAR = 1,		//出现
            CHANGE_DISAPPEAR = 2,		//消失
            CHANGE_GEOMETRY = 3,     //几何变化
            CHANGE_ATTRIBUTE = 4,		//稳定
            CHANGE_UNION = 5,     //合并
            CHANGE_SPILT = 6    //分割
        }
        public enum GeoType
        {
            point = 1,
            line = 2,
            polygon = 3
        }
        //浏览
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.CheckFileExists = true;
            pOpenFileDialog.Title = "打开txt文件";
            pOpenFileDialog.Filter = "inc文件（*.inc）|*.inc|txt文件（*.txt）|*.txt";
            pOpenFileDialog.Multiselect = true;
            pOpenFileDialog.ShowDialog();
            textBox1.Text = pOpenFileDialog.FileName;
        }
        //入库
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                //将txt内容分解为行数组
                string[] txt = File.ReadAllLines(textBox1.Text);
                ArrayList arrar = new ArrayList(txt);
                //string start = "RecordBegin";
                int startIndex = arrar.IndexOf("RecordBegin");
                string[] txtValue = txt.Skip(startIndex + 1).ToArray();//截取有用的

                for (int i = 0; i < txtValue.Count(); i++)
                {
                    string changeType;//变化类型
                    string SQL="";
                    string geoType;//几何类型
                    switch (txtValue[i])
                    {
                        case "1"://CHANGE_APPEAR出现
                            changeType = Enum.GetName(typeof(ChangeType), 1);
                            geoType = txtValue[i + 7];
                            i = insertSQL(geoType, txtValue, i, SQL, "create");
                            break;

                        case "2"://CHANGE_DISAPPEAR消失
                            changeType = Enum.GetName(typeof(ChangeType), 2);
                            if (1 == 1)
                            {
                                string[] unit = txtValue.Skip(i).Take(6).ToArray();//根据changeType和pointNumber截取一个数据单元

                                int objectID = int.Parse(unit[1]);
                                int nationCode = int.Parse(unit[2]);
                                //string nationName = lines[ 2];
                                string areaChange = unit[4];
                                string lenChange = unit[5];
                                i += unit.Count();
                            }
                            break;

                        case "3"://CHANGE_GEOMETRY几何修改
                            changeType = Enum.GetName(typeof(ChangeType), 3);
                            geoType = txtValue[i + 7];
                            i = insertSQL(geoType, txtValue, i, SQL, "modify");
                            break;

                        case "4"://CHANGE_ATTRIBUTE稳定
                            changeType = Enum.GetName(typeof(ChangeType), 4);
                            geoType = txtValue[i + 7];
                            i = insertSQL(geoType, txtValue, i, SQL, "ATTRIBUTE");
                            break;

                        case "5"://CHANGE_UNION合并
                            changeType = Enum.GetName(typeof(ChangeType), 5);
                            geoType = txtValue[i + 7];
                            i = insertSQL(geoType, txtValue, i, SQL, "UNION");
                            break;

                        case "6"://CHANGE_SPILT分割
                            changeType = Enum.GetName(typeof(ChangeType), 6);
                            geoType = txtValue[i + 7];
                            i = insertSQL(geoType, txtValue, i, SQL, "SPILT");
                            break;

                    }

                }

            }
            else
            {
                MessageBox.Show("请选择要入库的文件！");
            }




        }
        public int insertSQL(string geoType, string[] txtValue, int i, string SQL, string changeType)
        {
            if (geoType.Contains("Line"))
            {
                int pointNumber = int.Parse(txtValue[i + 8]);
                string[] unit = txtValue.Skip(i).Take(pointNumber + 9).ToArray();//根据changeType和pointNumber截取一个数据单元

                int objectID = int.Parse(unit[1]);
                int nationCode = int.Parse(unit[2]);
                string nationName;
                string areaChange = unit[4];
                string lenChange = unit[5];
                string[] ChangeTimes = unit[6].Split(',');
                string startTime = "";
                foreach (string s in ChangeTimes)
                {
                    if (IsOnlyNumber(s))
                    {
                        startTime = s;
                    }
                }
                string pointString = "";
                for (int k = 0; k < pointNumber; k++)
                {
                    string pointUnit;
                    if (k != pointNumber - 1)
                    {
                        pointUnit = unit[9 + k].Replace(",", " ") + ",";
                    }
                    else
                    {
                        pointUnit = unit[9 + k].Replace(",", " ");
                    }
                    pointString = pointString.Insert(pointString.Length, pointUnit);
                }
                string tableName = getTableName(nationCode, geoType, out nationName);//sdo_geometry('POINT ({2} {3})',4326)
                if (tableName != null)
                {
                    SQL = string.Format("insert into {0} (NATIONCODE,NATIONELENAME,STARTTIME,SHAPE,CHANGETYPE,SOURCE) VALUES ({1},'{2}','{3}',sdo_cs.transform(sdo_geometry('LINESTRING ({4})',2436),4326),'{5}',1 )", tableName, nationCode, nationName, startTime, pointString, changeType);
                    helper.sqlExecuteUnClose(SQL);
                    Console.WriteLine(tableName + "中一行已插入");
                }
                i += unit.Count();
            }
            else if (geoType.Contains("Polygon"))
            {
                int ringNumber = int.Parse(txtValue[i + 8]);
                int pointNumber = int.Parse(txtValue[i + 9]);
                string[] unit = txtValue.Skip(i).Take(10 + pointNumber).ToArray();//根据changeType和pointNumber截取一个数据单元

                int objectID = int.Parse(unit[1]);
                int nationCode = int.Parse(unit[2]);
                string nationName;
                string areaChange = unit[4];
                string lenChange = unit[5];
                string[] ChangeTimes = unit[6].Split(',');
                string startTime="";
                foreach (string s in ChangeTimes)
                {
                    if (IsOnlyNumber(s))
                    {
                        startTime = s;
                    }
                }
              
                IFormatProvider ifp = new CultureInfo("zh-CN", true);
                DateTime dt1 = DateTime.ParseExact(startTime, "yyyyMMdd", ifp);
                startTime = dt1.ToString("yyyy-MM-dd");

                string pointString = "";
                string tableName = getTableName(nationCode, geoType, out nationName);
                if (ringNumber == 1)
                {
                    for (int k = 0; k < pointNumber; k++)
                    {
                        string pointUnit;
                        if (k != pointNumber - 1)
                        {
                            pointUnit = unit[10 + k].Replace(",", " ") + ",";
                        }
                        else
                        {
                            pointUnit = unit[10 + k].Replace(",", " ");
                        }
                        pointString = pointString.Insert(pointString.Length, pointUnit);
                    }

                    if (tableName != null)
                    {
                        SQL = string.Format("insert into {0} (NATIONCODE,NATIONELENAME,STARTTIME,SHAPE,CHANGETYPE,SOURCE) VALUES ({1},'{2}','{3}',sdo_cs.transform(sdo_geometry('POLYGON (({4}))',2436),4326),'{5}',1 )", tableName, nationCode, nationName, startTime, pointString, changeType);
                        helper.sqlExecuteUnClose(SQL);
                        Console.WriteLine(tableName + "中一行已插入");
                    }
                }
                else
                {
                    int lineNumber = 0; //行数
                    pointNumber = int.Parse(txtValue[i+9]);
                    lineNumber = i+9;
                    for (int k = 0; k < ringNumber; k++)
                    {
                        int pointNumbers = pointNumber;
                        pointNumber = int.Parse(txtValue[pointNumber + lineNumber+1]);
                        lineNumber = lineNumber + (pointNumbers + 1);
                    }
                    unit = txtValue.Skip(i).Take(lineNumber-i+1).ToArray();
                   
                    string pointUnit = "";
                    int Number = 9;
                    pointNumber = int.Parse(unit[9]);
                    for (int k = 0; k < ringNumber; k++)
                    {
                       
                        for (int m = 1; m < pointNumber+1; m++)
                        {
                            if (m != pointNumber)
                            {
                                pointUnit = unit[Number + m].Replace(",", " ") + ", ";
                            }
                            else
                            {
                                pointUnit = unit[Number + m].Replace(",", " ");
                            }
                            pointString = pointString+pointUnit;
                            
                        }
                        pointString = pointString + ")), ((";

                        int pointNumbers = pointNumber;
                        pointNumber = int.Parse(unit[pointNumber + Number + 1]);
                        Number = Number + (pointNumbers + 1);
                    }
                    pointString = "((" + pointString.Remove(pointString.Length-4, 4);//需要修改为删除最后一个
  
                    

                    if (tableName !=null)
                    {
                        SQL = string.Format("insert into {0} (NATIONCODE,NATIONELENAME,STARTTIME,SHAPE,CHANGETYPE,SOURCE) VALUES ({1},'{2}','{3}',sdo_cs.transform(sdo_geometry('MULTIPOLYGON ({4})',2436),4326),'{5}',1 )", tableName, nationCode, nationName, startTime, pointString, changeType);
                        helper.sqlExecuteUnClose(SQL);
                        Console.WriteLine(tableName + "中一行已插入");
                    }
                }
                i += unit.Count();
            }
            else if (geoType.Contains("Point"))
            {
                string[] unit = txtValue.Skip(i).Take(10).ToArray();//根据changeType和pointNumber截取一个数据单元

                int objectID = int.Parse(unit[1]);
                int nationCode = int.Parse(unit[2]);
                string nationName;
                string areaChange = unit[4];
                string lenChange = unit[5];
                string[] ChangeTimes = unit[6].Split(',');
                string startTime = "";
                foreach (string s in ChangeTimes)
                {
                    if (IsOnlyNumber(s))
                    {
                        startTime = s;
                    }
                }
                string pointString = unit[8].Replace(",", " ");
                
                string tableName = getTableName(nationCode, geoType, out nationName);
                if (tableName != null)
                {
                    SQL = string.Format("insert into {0} (NATIONCODE,NATIONELENAME,STARTTIME,SHAPE,CHANGETYPE,SOURCE) VALUES ({1},'{2}','{3}',sdo_cs.transform(sdo_geometry('POINT ({4})',2436),4326),'{5}',1 )", tableName, nationCode, nationName, startTime, pointString, changeType);
                    helper.sqlExecuteUnClose(SQL);
                    Console.WriteLine(tableName + "中一行已插入");
                }
                i += unit.Count();
            }
            return i;
        }
        public static bool IsOnlyNumber(string value)
        {
            Regex r = new Regex(@"\d{8}");//判断匹配8位的数字
            return r.Match(value).Success;
        }
        private string getTableName(int nationCode, string geoType, out string nationName)
        {
            string SQL = string.Format("select elementtype from land_type where elementtypeid=(select DISTINCT elementtype from type_rule where nationcode={0})", nationCode);
            string tableName = null;
            nationName = "";
            OracleDBHelper conHelper = new OracleDBHelper();
            using (OracleDataReader dr = conHelper.queryReader(SQL))
            {
                while (dr.Read())
                {
                    if (geoType.Contains("Line"))
                    {
                        switch (dr.GetString(0))
                        {
                            case "traffic":
                                tableName = "TRAFFIC_NEWLINE";
                                break;
                            case "water":
                                tableName = "WATER_NEWLINE";
                                break;
                            case "residential":
                                tableName = "RESIDENTIAL_NEWLINE";
                                break;
                            case "vegetation":
                                tableName = "VEGETATION_NEWLINE";
                                break;
                            case "soil":
                                tableName = "SOIL_NEWLINE";
                                break;
                        }
                    }
                    else if (geoType.Contains("Polygon"))
                    {
                        switch (dr.GetString(0))
                        {
                            case "traffic":
                                tableName = "TRAFFIC_NEWAREA";
                                break;
                            case "water":
                                tableName = "WATER_NEWAREA";
                                break;
                            case "residential":
                                tableName = "RESIDENTIAL_NEWAREA";
                                break;
                            case "vegetation":
                                tableName = "VEGETATION_NEWAREA";
                                break;
                            case "soil":
                                tableName = "SOIL_NEWAREA";
                                break;
                        }
                    }
                    else if (geoType.Contains("Point"))
                    {
                        switch (dr.GetString(0))
                        {
                            case "traffic":
                                tableName = "TRAFFIC_NEWPOINT";
                                break;
                            case "water":
                                tableName = "WATER_NEWPOINT";
                                break;
                            case "residential":
                                tableName = "RESIDENTIAL_NEWPOINT";
                                break;
                            case "vegetation":
                                tableName = "VEGETATION_NEWPOINT";
                                break;
                            case "soil":
                                tableName = "SOIL_NEWPOINT";
                                break;
                        }
                    }
                }
            }
            SQL = string.Format("select DISTINCT NATIONELENAME from type_rule where nationcode={0}", nationCode);
            using (OracleDataReader dr = conHelper.queryReader(SQL))
            {
                while (dr.Read())
                {
                    nationName = dr.GetString(0);
                }
            }

            return tableName;
        }
    }
}
