using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;
using System.Threading;

namespace GIS.UI.Forms
{
    public partial class CombineOSC : Form
    {
        string[] ChosenFiles = null;
        public CombineOSC()
        {
            InitializeComponent();
        }
        public void Addprogess()
        {
            oscPgBar.Value++;
        }
        public void Combine()
        {
            int m = 0, n = 0, z = 0,y=0;//m为清理面数，n为清理线数,z为清理点数
            oscPgBar.Maximum=10000;
            List<string> handleTable = new List<string>() { "oscarea", "oscline", "oscpoint" };            
            for (int i = 0; i < handleTable.Count; i++)
            {
                //找到osmid相同的数据
                string sql1 = string.Format("select osmid from {0} group by osmid having count(*)>1", handleTable[i]);
                //执行输入的语句
                string sqlName = "";
                OracleDBHelper odb = new OracleDBHelper();
                using (OracleDataReader rd = odb.queryReader(sql1))
                {
                    if (rd == null || !rd.HasRows)
                    {
                        continue;
                    }

                    while (rd.Read())//读取表的信息
                    {

                        object repeatdID = rd["osmid"];//读取osmid重复的项
                        string sql2 = string.Format("select OBJECTID,CHANGETYPE,STARTTIME from {0} where osmid='{1}' order by STARTTIME asc", handleTable[i], repeatdID);
                        List<string> changType = new List<string>();
                        List<string> startTime = new List<string>();
                        List<string> objectID = new List<string>();
                        using (OracleDataReader rd2 = odb.queryReader(sql2))
                        {
                            if (rd2 == null || !rd2.HasRows)
                            {
                                continue;
                            }
                            while (rd2.Read())
                            {
                               
                                changType.Add(rd2["CHANGETYPE"].ToString());//修改的类型
                                startTime.Add(rd2["STARTTIME"].ToString());//开始时间
                                objectID.Add(rd2["OBJECTID"].ToString());//osm入库的主键id

                            }
                            y++;
                            Addprogess();
                            //Thread.Sleep(50);
                            //-------------------------------------------------
                            //这里根据比较规则比较两种相同osmid的编辑类型，选择保留其中一种
                            if (changType.Count >= 2)//当集合中有两个值的时候进行比较，否则就跳过
                            {
                                string InitialValue = changType[0];
                                string sql3 = "", sql4 = "";
                                if (startTime[0] != startTime[startTime.Count - 1])
                                {
                                    //编辑时间不同
                                    if (InitialValue == "create" && changType[changType.Count - 1] == "delete")
                                    {
                                        //不保留该对象，将两条数据都删除
                                        sql3 = string.Format("delete  from {0} where osmid='{1}'", handleTable[i], repeatdID);
                                    }
                                    else if (InitialValue == "modify" && changType[changType.Count - 1] == "delete")
                                    {
                                        //先修改，最后删除，则保留删除数据，删除修改数据
                                        sql3 = string.Format("delete  from {0} where osmid='{1}' and STARTTIME<'{2}'", handleTable[i], repeatdID, startTime[startTime.Count - 1]);
                                    }
                                    else if (InitialValue == "create" && changType[changType.Count - 1] == "modify")
                                    {
                                        //先创建，最后修改，那么保留最后一项，并将修改类型变为创建
                                        sql3 = string.Format("delete  from {0} where osmid='{1}' and STARTTIME<'{2}'", handleTable[i], repeatdID, startTime[startTime.Count - 1]);
                                        sql4 = string.Format("update {0} set CHANGETYPE='create' where osmid='{1}'", handleTable[i], repeatdID);
                                    }
                                    else if (InitialValue == "modify" && changType[changType.Count - 1] == "modify")
                                    {
                                        //最先和最后一条都是修改，那么就保留最后一条，
                                        sql3 = string.Format("delete  from {0} where osmid='{1}' and STARTTIME<'{2}'", handleTable[i], repeatdID, startTime[startTime.Count - 1]);
                                    }
                                    else
                                    {
                                        sql3 = string.Format("delete  from {0} where osmid='{1}' and STARTTIME<'{2}'", handleTable[i], repeatdID, startTime[startTime.Count - 1]);
                                    }
                                    switch (i)//判断点线面增量图层中，每删除一条重复的数据其相应的计数就加1
                                    {
                                        case 0: m++;
                                            break;
                                        case 1: n++;
                                            break;
                                        case 2: z++;
                                            break;
                                    }
                                   
                                }

                                else if (InitialValue == changType[changType.Count - 1] && startTime[0] == startTime[startTime.Count - 1])
                                {
                                    //时间相同，并且改变类型相同，则保留其中一个
                                    sql3 = string.Format("delete  from {0} where osmid='{1}' and OBJECTID<'{2}'", handleTable[i], repeatdID, objectID[objectID.Count - 1]);
                                    //switch (i)
                                    //{
                                    //    case 0: m++;
                                    //        break;
                                    //    case 1: n++;
                                    //        break;
                                    //    case 2: z++;
                                    //        break;
                                    //}
                                   
                                }

                                if (sql3 != "")
                                {
                                    using (OracleDataReader rd3 = odb.queryReader(sql3))
                                    {

                                    }
                                }
                                if (sql4 != "")
                                {
                                    using (OracleDataReader rd4 = odb.queryReader(sql4))
                                    {

                                    }
                                }
                            }
                            //-------------------------------------------------
                        }

                    }

                }

            }
            if (y < oscPgBar.Maximum)
            {
                oscPgBar.Value = oscPgBar.Maximum;
            }
            polygenClearNum.Text = m.ToString();
            lineClearNum.Text = n.ToString();
            pointClearNum.Text = z.ToString();

            AddMap.Fun1();
            MessageBox.Show("整合完成" + y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Combine();
        }







        
       }
}
