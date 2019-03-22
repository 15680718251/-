using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using GIS.UI.Forms;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using TrustValueAndReputation.historyToDatabase;

namespace TrustValueAndReputation
{
    public partial class FindSonVersion : Form
    {
        double w1 = 0;
        double w2 = 0;
        int paraC = 0;
        double initReputation = 0;

        public FindSonVersion()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        #region 公共变量
       
        /// <summary>
        /// 地球半径
        /// </summary>
        const double EarthRadio = 6371.004;
        /// <summary>
        /// 全局变量list，用于存储所有的原始记录
        /// </summary>
        List<polygon> listAll = new List<polygon>();
        /// <summary>
        /// 全局变量list，用于存储所有的原始记录
        /// </summary>
        List<polyline> listlineAll = new List<polyline>();
        /// <summary>
        /// 全局变量sublist，用于存储所有的子版本记录
        /// </summary>
        List<polygonsonversion> sublist = new List<polygonsonversion>();

        List<polylinesonversion> sublistline = new List<polylinesonversion>();
        /// <summary>
        /// 实体操作类
        /// </summary>
        Dal_polygon dal_polygon = new Dal_polygon();

        Dal_point dal_point = new Dal_point();

        Dal_polygonsonversion dal_polygonsonversion = new Dal_polygonsonversion();

        Dal_polyline dal_polyline = new Dal_polyline();

        Dal_polylinesonversion dal_polylinesonversion = new Dal_polylinesonversion();

        /// <summary>
        /// 公共变量容差值
        /// </summary>
        private double tolerance;

        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }

        public bool isValid = false;//是否有效
        public int count = 0;
        #endregion 公共变量

        #region 窗体事件
        /// <summary>
        /// 窗体载入时间，设置容差为100米
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindSonVersion_Load(object sender, EventArgs e)
        {
            tolerance = 10;

           

            string[] TableNames = { "POLYGONSONVERSION", "POLYLINESONVERSION", "RESIDENTIAL_AREA", "SOIL_AREA", "TRAFFIC_AREA", "VEGETATION_AREA", "WATER_AREA", "RESIDENTIAL_LINE", "SOIL_LINE", "TRAFFIC_LINE", "VEGETATION_LINE", "WATER_LINE", "RESIDENTIAL_NEWAREA", "SOIL_NEWAREA", "TRAFFIC_NEWAREA", "VEGETATION_NEWAREA", "WATER_NEWAREA", "RESIDENTIAL_NEWLINE", "SOIL_NEWLINE", "TRAFFIC_NEWLINE", "VEGETATION_NEWLINE", "WATER_NEWLINE" };
            this.comboBox1.Items.Clear();
            if (TableNames != null && TableNames.Count() > 0)
            {
                foreach (string name in TableNames)
                {
                    comboBox1.Items.Add(name);
                }
            }
            else
            {
                MessageBox.Show("对不起，您连接的数据库中无基态数据表！！！");
            }
        }
        #endregion 窗体事件

        #region 面处理



        #region 按钮事件操作
        /// <summary>
        /// 获取原始版本的面，存入datagridview中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!OSMDataBaseLinkForm.OSMLinkSuccess)
                {
                    MessageBox.Show("数据库连接失败！请重新连接数据库");
                }
                else
                {
                    //参数设置

                    try
                    {
                        tolerance = Math.Abs(Convert.ToDouble(this.textBox2.Text));
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("请输入正确的容差值！");
                    }
                    try
                    {
                        w1 = Math.Abs(Convert.ToDouble(this.textBox3.Text));
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("请输入正确的W1值！");
                    }
                    try
                    {
                        w2 = Math.Abs(Convert.ToDouble(this.textBox4.Text));
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("请输入正确的W2值！");
                    }
                    try
                    {
                        paraC = Math.Abs(Convert.ToInt32(this.textBox5.Text));
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("请输入正确的C值！");
                    }
                    try
                    {
                        initReputation = Math.Abs(Convert.ToDouble(this.textBox6.Text));
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("请输入正确的初始信誉值！");
                    }





                    Thread getur = new Thread(GetUserReputation);
                    getur.Start();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void GetUserReputation()
        {
            //表格不自动生成行
            //this.dataGridView1.AutoGenerateColumns = false;
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            string[] tableNames = { "polygonsonversion", "polylinesonversion" };
            foreach (string tableName in tableNames)
            {
                string sql = "select count(*)  from USER_TABLES where Table_Name = upper('" + tableName + "')";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand conmand = new OracleCommand(sql, con))
                {
                    using (OracleDataReader rd = conmand.ExecuteReader())
                    {
                        int count = 0;
                        while (rd.Read())
                        {
                            count += Int32.Parse(rd["count(*)"].ToString());
                        }
                        if (count > 0)
                        {
                            sql = "drop table " + tableName;
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }

                        SqlHelper_OSC.CreateTable(con, "Osm\\CreateTable" + tableName);
                            //SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePolygonsubversion");
                        
                    }
                }
            }
            
           
            
            Stopwatch sw = new Stopwatch();

            try
            {
                listAll = dal_polygon.GetList(); //获取表中的所有面记录
                listlineAll = dal_polyline.GetList();//获取表中的所有线记录
                //this.dataGridView1.DataSource = listAll;  //将面记录显示在表格中
                this.label1.Text = "共" + (listAll.Count + listlineAll.Count) + "个";

                //MessageBox.Show("点击确定，开始运行！");
                sw.Start();

                this.label5.Text = "正在获取面目标的子版本";
                Console.WriteLine("正在获取面目标的子版本....................................................................................................");
                //获得面子版本
                Getsubversion();
                dal_polygonsonversion.UpdateTableIsValid("polygonsonversion", "1");//0为线数据


                this.progressBar1.Value = 0;
                this.label5.Text = "正在删除用户ID相同的面版本";
                //删除id相同的连续子版本
                Console.WriteLine("正在删除用户ID相同的面版本....................................................................................................");
                DeletsameID();

                this.progressBar1.Value = 0;
                this.label5.Text = "正在计算面目标的面积相似度";
                //计算面积相似度
                Console.WriteLine("正在计算面目标的面积相似度....................................................................................................");
                Getareadiffsim();

                this.progressBar1.Value = 0;
                this.label5.Text = "正在计算面目标的形状相似度";
                //计算面形状相似度
                Console.WriteLine("正在计算面目标的形状相似度....................................................................................................");
                Getshapediffsim();




                this.progressBar1.Value = 0;
                this.label5.Text = "正在获取线目标的子版本";
                //获取线子版本
                Console.WriteLine("正在获取线目标的子版本....................................................................................................");
                Getsubversionline();
                dal_polygonsonversion.UpdateTableIsValid("polylinesonversion", "0");//0为线数据


                this.progressBar1.Value = 0;
                this.label5.Text = "正在删除用户ID相同的线版本";
                //清理用户相同的连续版本
                Console.WriteLine("正在删除用户ID相同的线版本....................................................................................................");
                DeletesameIDline();

                this.progressBar1.Value = 0;
                this.label5.Text = "正在计算线目标的长度相似度";
                //计算长度相似度
                Console.WriteLine("正在计算线目标的长度相似度....................................................................................................");
                Getlengthdiffsim();

                this.progressBar1.Value = 0;
                this.label5.Text = "正在计算线目标的形状相似度";
                //计算线形状相似度
                Console.WriteLine("正在计算线目标的形状相似度....................................................................................................");
                Getshapediffsimline();


                this.progressBar1.Value = 0;
                this.label5.Text = "正在计算用户信誉度";
                //计算用户信誉度
                Console.WriteLine("正在计算用户信誉度....................................................................................................");
                Getuserreputation(w1, w2, paraC, initReputation);

                sw.Stop();
                this.label5.Text += "计算完成!\r\n历时:" + sw.Elapsed;
                //MessageBox.Show("计算完成!历时:" + sw.Elapsed);

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("错误：" + ex.ToString());

            }
        }


        private void baseToHistory(string tablename1, string tablename2)
        {    //table1历史
            //table2基态或者增量
            //string sql="";
            //string sql1="";
            string sqlread = "select osmid,versionid from " + tablename2;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sqlread))//读取基态或者增量数据表中的osmid
            {
                while (dr.Read())
                {
                    //StringBuilder sqlInsert1 = new StringBuilder(sqlInsert);
                    //sqlInsert1.Append(" where osmid=" + dr["osmid"]);
                    //sqlInsert=String.Format(sqlInsert+" where oscid=" + dr["OSCID"]);
                    sqlread = String.Format("select count(*),trustvalue,userrepatation from {0}  where osmid={1}", tablename1, dr["osmid"]);
                    using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sqlread))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() != "0")//判断历史数据表中是否有相同是osmid
                            {
                                //如果有相同的osmid 判断其versionID是否相同
                                sqlread = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", tablename1, dr["osmid"], dr["versionid"]);
                                using (OracleDataReader dr2 = OracleDBHelper.ExecuteReader(sqlread))
                                {
                                    while (dr2.Read())
                                    {
                                        if (dr2["count(*)"].ToString() != "0")
                                        {//如果versionid相同 则直接更新
                                            sqlread = "update set " + tablename2 + " trustvalue=" + dr1["trustvalue"] + "userrepatation=" + dr1["userreputation"];
                                            OracleDBHelper.ExecuteSql(sqlread);
                                            Console.WriteLine("更新一行");
                                        }
                                    }

                                }

                            }
                          
                        }


                    }

                }

            }
        }


        private void incToHistory(string tablename1, string tablename2)
        {    //table1历史
            //table2基态或者增量
            //string sql = "";
            //string sql1 = "";
            string sqlread = "select OSCID,versionid from " + tablename2;
            
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sqlread))//读取基态或者增量数据表中的osmid
            {
                while (dr.Read())
                {

                    //StringBuilder sqlInsert1 = new StringBuilder(sqlInsert);
                    //sqlInsert1.Append(" where oscid=" + dr["OSCID"]);
                    //sqlInsert=String.Format(sqlInsert+" where oscid=" + dr["OSCID"]);
                    sqlread = String.Format("select count(*), trustvalue, userreputation from {0}  where osmid={1}", tablename1, dr["OSCID"]);//拿增量数据中的一个数据与历史数据中的每一个数据相比较
                    using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sqlread))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() != "0")//判断历史数据表中是否有相同是osmid
                            {
                                //如果有相同的osmid 判断其versionID是否相同
                                sqlread = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", tablename1, dr["OSCID"], dr["versionid"]);
                                using (OracleDataReader dr2 = OracleDBHelper.ExecuteReader(sqlread))
                                {
                                    while (dr2.Read())
                                    {
                                        if (dr2["count(*)"].ToString() != "0")
                                        {//如果versionid不同 则直接插入
                                            sqlread = "update set " + tablename2 + " trustvalue=" + dr1["trustvalue"] + "userrepatation=" + dr1["userreputation"];
                                            OracleDBHelper.ExecuteSql(sqlread);
                                            Console.WriteLine("更新一行");
                                        }
                                    }

                                }

                            }
                            
                        }


                    }

                }

            }
        }
        ///// <summary>
        ///// 清空textBox
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void button3_Click(object sender, EventArgs e)
        //{
        //    this.textBox1.Text = "";
        //}

        /// <summary>
        /// 显示子版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void button5_Click(object sender, EventArgs e)
        //{

        //    if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
        //    {
        //        MessageBox.Show("数据库连接失败！请重新连接数据库");
        //    }
        //    else
        //    {

        //        //表格不自动生成行
        //        this.dataGridView2.AutoGenerateColumns = false;
        //        try
        //        {
        //            sublist = dal_polygonsonversion.GetList();  //获取表中的所有面记录
        //            this.dataGridView2.DataSource = sublist;  //将面记录显示在表格中
        //            this.label2.Text = "共" + sublist.Count() + "个";
        //        }
        //        catch (System.Exception ex)
        //        {
        //            MessageBox.Show("错误：" + ex.ToString());
        //        }
        //    }
        //}

        #endregion 按钮事件操作

        #region 相关函数操作
        /// <summary>
        /// 生成面子版本
        /// </summary>
        /// <param name="list"></param>
        /// <param name="basenum"></param>
        public void generateSubPolygon(List<polygon> list, int basenum)
        {
            this.progressBar1.Maximum = listAll.Count - 1;
            this.progressBar1.Minimum = 0;
            try
            {
                string str = "";
                //分割器符号
                char[] split = { ',' };
                //使用公开枚举数合并非泛型字符串数组
                //将list的第一个元素的点集合赋给arrUnion
                
                IEnumerable<string> arrUnion = list[0].pointids.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
                //IEnumerable<string> arrIntersect = list[0].pointids.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
                //使用循环，将list列表的其它元素的点集合拼接到arrUnion
                for (int i = 1; i < list.Count; i++)
                {
                    string[] arr = list[i].pointids.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
                    arrUnion = arrUnion.Union(arr).OrderBy(c => c);
                }

                string[] arrU = arrUnion.ToArray();
                str = String.Join(",", arrU);

                List<point> allnodeList = new List<point>();
                allnodeList = dal_point.GetListByCond(str);

                //对所有的way进行遍历
                for (int i = 0; i < list.Count - 1; i++)
                {
                    this.progressBar1.Value = basenum + i;
                    try
                    {
                        polygon currentVersion = list[i]; //当前版本
                        polygon nextVersion = list[i + 1]; //下一个版本

                        //提示信息
                        //this.textBox1.Text += "\r\n正在处理" + currentVersion.objectid + "," + currentVersion.id + "," + currentVersion.version + ":";

                        //将点串分开，存储在nodeID中
                        string[] nodeID = currentVersion.pointids.Split(',');

                        //利用正则表达式计算一个面包含的点的个数
                        int nodeNum = Regex.Matches(currentVersion.pointids, ",").Count;

                        #region 判断是否是面，考虑容差
                        //Console.WriteLine(nodeID[0]);
                        //Console.WriteLine(Convert.ToInt64(nodeID[0]));
                        //Console.WriteLine(Convert.ToInt64(nodeID[0]).GetType());
                        point startPoint=null;
                        point endPoint=null;
                        try
                        {
                            startPoint = dal_point.GetModel(Convert.ToInt64(nodeID[0]), currentVersion.timestamp);
                            endPoint = dal_point.GetModel(Convert.ToInt64(nodeID[nodeNum - 1]), currentVersion.timestamp);
                        }
                        catch (System.FormatException ex)
                        {
                            Console.WriteLine(ex);
                        }
                        //point startPoint = dal_point.GetModel(Convert.ToInt64(nodeID[0]), currentVersion.timestamp);
                        //point endPoint = dal_point.GetModel(Convert.ToInt64(nodeID[nodeNum - 1]), currentVersion.timestamp);

                        if (startPoint == null || endPoint==null)
                        {
                            continue;
                        }

                        double startY = startPoint.lat;
                        double startX = startPoint.lon;
                        double endX = endPoint.lon;
                        double endY = endPoint.lat;


                        double distance = computeDistance(startX, startY, endX, endY);
                        //首尾点距离大于容差或当前版本无点序列，不处理
                        if (distance > tolerance || currentVersion.pointids == "")
                        {
                            //this.textBox1.Text += "\r\n  不是面，忽略。";
                            //this.textBox1.ScrollToCaret();//滚动到光标处
                            continue;
                        }
                        //首尾点不相同，但在容差范围内，补充点序列尾结点
                        else if (distance < tolerance && nodeID[0] != nodeID[nodeNum - 1])
                        {
                            currentVersion.pointids += nodeID[0] + ";";
                        }
                        #endregion 判断是否是面，考虑容差
                        //判断是否闭合，若闭合，则做面处理，继续进行；不闭合，不处理，跳出当次循环


                        //对比两个版本，获取所有需要点（即原始版本的点和用户更改的点）
                        int Num;

                        //Console.WriteLine("是这里出错了吗");
                        List<point> changenodeList = CompareFindChangeNode(currentVersion, nextVersion, allnodeList);
                        //Console.WriteLine("是这里出错了吗");
                        //对点按照时间进行排序
                        changenodeList.Sort(delegate(point info1, point info2)
                        {
                            Type t1 = info1.GetType();
                            Type t2 = info2.GetType();
                            PropertyInfo pro1 = t1.GetProperty("timestamp");
                            PropertyInfo pro2 = t2.GetProperty("timestamp");
                            int a = Convert.ToDateTime(pro1.GetValue(info1, null)).CompareTo(Convert.ToDateTime(pro2.GetValue(info2, null)));
                            return a;
                        });
                        //子版本计数
                        int subVersionNum = 0;

                        //首先生成原始版本
                        try
                        {
                            if (GenerateSubVersion(nodeID, currentVersion, subVersionNum, currentVersion.timestamp, currentVersion.username, currentVersion.userid))
                            {
                                //this.textBox1.Text += "\r\n  生成原始版本：" + subVersionNum++;
                                //this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                            else
                            {
                                this.textBox1.Text += "\r\n  生成失败！";
                                Console.WriteLine(" 生成失败啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                                //。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。
                                this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                        }
                        catch (System.Exception ex)
                        {
                            string erro = "生成面的子版本，当前处理到面ID：" + currentVersion.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                            errolog.WriteEduAppLog(erro, ex.StackTrace);
                            this.textBox1.Text += "\r\n  生成原始版本失败，失败原因：" + ex.ToString();
                            this.textBox1.ScrollToCaret();//滚动到光标处
                        }

                        //对比排序后的点list，计算用户改变次数，并生成子版本
                        for (int j = nodeNum - 1; j < changenodeList.Count - 1; j++)
                        {
                            point currentNode = changenodeList[j];
                            point nextNode = changenodeList[j + 1];
                            //判断点用户是否改变
                            bool change = CompareNode(currentNode, nextNode, "userid");

                            try
                            {
                                if (change)
                                {
                                    //设置时间为最后用户改变的点的时间
                                    DateTime deadTime = nextNode.timestamp;
                                    //生成子版本
                                    if (GenerateSubVersion(nodeID, currentVersion, subVersionNum, deadTime, nextNode.username, nextNode.userid))
                                    {
                                        //this.textBox1.Text += "\r\n  生成子版本：" + subVersionNum++;
                                        //this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                    else
                                    {
                                        this.textBox1.Text += "\r\n  生成失败！";
                                        this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                string erro = "生成面的子版本，当前处理到面ID：" + currentVersion.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                errolog.WriteEduAppLog(erro, ex.StackTrace);
                                this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                                this.textBox1.ScrollToCaret();//滚动到光标处
                                continue;
                            }

                        }
                    }
                    catch (System.Exception ex)
                    {
                        errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                        this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                        this.textBox1.ScrollToCaret();//滚动到光标处
                        continue;
                    }
                }

                //处理最后一个面     
                //for (int i = list.Count - 1; i < list.Count; i++)
                //{
                try
                {
                    this.progressBar1.Value = basenum + list.Count - 1;
                    polygon lastPolygon = list[list.Count - 1];
                    //提示信息
                    //this.textBox1.Text += "\r\n正在处理" + lastPolygon.objectid + "," + lastPolygon.id + "," + lastPolygon.version + ":";

                    //将点串分开，存储在nodeID中
                    string[] nodeID = lastPolygon.pointids.Split(',');
                    //将点串替换成以“'，'”为分隔符的点串，原来用“;”
                    string range = lastPolygon.pointids.Replace(";", "','");
                    range = range.Substring(0, range.Length - 2);
                    //利用正则表达式计算一个面包含的点的个数
                    int nodeNum = Regex.Matches(lastPolygon.pointids, ",").Count;

                    #region 判断是否是面，考虑容差
                    point startPoint = dal_point.GetModel(Convert.ToInt64(nodeID[0]), lastPolygon.timestamp);
                    point endPoint = dal_point.GetModel(Convert.ToInt64(nodeID[nodeNum - 1]), lastPolygon.timestamp);

                    //if (startPoint == null || endPoint == null)
                    //{
                    //    break;
                    //}
                    double startY = startPoint.lat;
                    double startX = startPoint.lon;
                    double endX = endPoint.lon;
                    double endY = endPoint.lat;

                    double distance = computeDistance(startX, startY, endX, endY);
                    //首尾点距离大于容差或当前版本无点序列，不处理
                    if (distance > tolerance || lastPolygon.pointids == "")
                    {
                        //this.textBox1.Text += "\r\n  不是面，忽略。";
                        //this.textBox1.ScrollToCaret();//滚动到光标处

                    }
                    else
                    {
                        //首尾点不相同，但在容差范围内，补充点序列尾结点
                        if (distance < tolerance && nodeID[0] != nodeID[nodeNum - 1])
                        {
                            lastPolygon.pointids += nodeID[0] + ";";
                        }
                    #endregion 判断是否是面，考虑容差


                        //最终版本的时间为最终版本点的时间，新建一个最终版本，把他的时间定为最终版本点的时间，让最后一个面与最终版本进行比较
                        polygon theLastPolygon = lastPolygon;
                        theLastPolygon.timestamp = dal_point.GetModel(range).timestamp;
                        //对比两个版本，获取所有需要点（即原始版本的点和用户更改的点）
                        List<point> changenodeList = CompareFindChangeNode(lastPolygon, theLastPolygon, allnodeList);
                        //子版本计数
                        int subVersionNum = 0;

                        //首先生成原始版本
                        try
                        {
                            if (GenerateSubVersion(nodeID, lastPolygon, subVersionNum, lastPolygon.timestamp, lastPolygon.username, lastPolygon.userid))
                            {
                                //this.textBox1.Text += "\r\n  生成原始版本：" + subVersionNum++;
                                //this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                            else
                            {
                                this.textBox1.Text += "\r\n  生成失败！";
                                this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                        }
                        catch (System.Exception ex)
                        {
                            string erro = "生成面的子版本，当前处理到面ID：" + lastPolygon.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                            errolog.WriteEduAppLog(erro, ex.StackTrace);
                            this.textBox1.Text += "\r\n  生成原始版本失败，失败原因：" + ex.ToString();
                            this.textBox1.ScrollToCaret();//滚动到光标处
                        }
                        //对比排序后的点list，计算用户改变次数，并生成子版本
                        for (int j = nodeNum - 1; j < changenodeList.Count - 1; j++)
                        {
                            point currentNode = changenodeList[j];
                            point nextNode = changenodeList[j + 1];
                            //判断点用户是否改变
                            bool change = CompareNode(currentNode, nextNode, "userid");

                            try
                            {
                                if (change)
                                {
                                    //设置时间为最后用户改变的点的时间
                                    DateTime deadTime = nextNode.timestamp;
                                    //生成子版本
                                    if (GenerateSubVersion(nodeID, lastPolygon, subVersionNum, deadTime, nextNode.username, nextNode.userid))
                                    {
                                        //this.textBox1.Text += "\r\n  生成子版本：" + subVersionNum++;
                                        //this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                    else
                                    {
                                        this.textBox1.Text += "\r\n  生成失败！";
                                        this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                string erro = "生成面的子版本，当前处理到面ID：" + lastPolygon.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                errolog.WriteEduAppLog(erro, ex.StackTrace);
                                this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                                this.textBox1.ScrollToCaret();//滚动到光标处
                                continue;
                            }

                        }
                    }
                }
                catch (System.Exception ex)
                {
                    errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                    //this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                    this.textBox1.ScrollToCaret();//滚动到光标处
                }
            //}
            }
            catch (System.Exception ex)
            {
                errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                //this.textBox1.Text += "\r\n  生成失败，进程中断！错误原因;" + ex.ToString();
                this.textBox1.ScrollToCaret();//滚动到光标处
            }
            OracleDBHelper.ExecuteSql("update polygonsonversion set objectid=rownum");
        }

        /// <summary>
        /// 对比两个版本，发现改变的点
        /// </summary>
        /// <param name="currentVersion">当前版本</param>
        /// <param name="nextVersion">下一个版本</param>
        /// <returns>改变的点的list</returns>
        public List<point> CompareFindChangeNode(polygon currentVersion, polygon nextVersion, List<point> OriginalNodeList2)
        {
            DateTime startTime = currentVersion.timestamp; //当前版本的时间
            DateTime endTime = nextVersion.timestamp; //下一个版本的时间
            //将点串替换成以“'，'”为分隔符的点串，原来用“;”
            string range = "'" + currentVersion.pointids.Replace(";", "','");
            range = range.Substring(0, range.Length - 2);
            //用字符数组存储点串
            string[] nodeID = currentVersion.pointids.Split(',');
            string[] test = range.Split(',');
            //用于存储点的list
            List<point> OriginalNodeList = new List<point>();

            //OriginalNodeList = dal_point.GetListByRange("id in (" + range + ")", startTime);

            OriginalNodeList = OriginalNodeList2;

            //从OriginalNodeList里面选出时间最靠近startTime的点存入nodeList中
            List<point> nodeList = new List<point>();

            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                //int flag = 0;
                long id = Convert.ToInt64(nodeID[i]);
                //for (int j = 0; j < OriginalNodeList.Count; j++)
                //{
                //    if (OriginalNodeList[j].id == id && startTime >= OriginalNodeList[j].timestamp)
                //    {
                //        flag = j;
                //    }
                //}
                //nodeList.Add(OriginalNodeList[flag]);
                var first = OriginalNodeList.LastOrDefault(item => item.id == id && item.timestamp <= startTime);
                //time = first.timestamp;
                nodeList.Add(first);

            }
            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                long id = Convert.ToInt64(nodeID[i]);
                var second = from item in OriginalNodeList
                             where (item.id == id && item.timestamp < endTime && item.timestamp > startTime)
                             orderby item.id, item.timestamp
                             select item;
                nodeList.AddRange(second);
            }

            //nodeList.Add(nodeList[0]);
            //为list添加当前版本与下一个版本之间的所有版本的点
            //nodeList.AddRange(dal_point.GetListSortByidandTime(range, startTime, endTime));
            //用于存储发生用户更改的点的list，changenodeList
            List<point> changenodeList = new List<point>();
            //首先把第一个点添加进去
            changenodeList.Add(nodeList[0]);
            //循环判断所有list中发生用户改变的点，存进changenodeList
            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                point currentNode = nodeList[i];
                point nextNode = nodeList[i + 1];
                //标识用户或者点是否改变
                if (currentNode == null || nextNode==null)
                {
                    continue;
                }
                bool change = CompareNode(currentNode, nextNode, "userid&nodeid");
                //改变了就存进去
                if (change)
                {
                    changenodeList.Add(nodeList[i + 1]);
                }
            }
            return changenodeList;
        }

        /// <summary>
        /// 对比两个点，判断用户或点ID是否改变
        /// </summary>
        /// <param name="currentNode">当前点</param>
        /// <param name="nextNode">下一个点</param>
        /// <returns></returns>
        public bool CompareNode(point currentNode, point nextNode, string field)
        {
            bool change = false;
            try {
                if (currentNode == null || nextNode==null)
                {
                    return change; 
                }
            long currentUserId = currentNode.userid;
            long nextUserId = nextNode.userid;
            long currentNodeId = currentNode.id;
            long nextNodeId = nextNode.id;
            switch (field)
            {
                case "userid":
                    if (currentUserId != nextUserId)
                    {
                        change = true;
                    }
                    break;
                case "nodeid":
                    if (currentNodeId != nextNodeId)
                    {
                        change = true;
                    }
                    break;
                case "userid&nodeid":
                    if (currentUserId != nextUserId || currentNodeId != nextNodeId)
                    {
                        change = true;
                    }
                    break;
                default:
                    break;
            }
            }catch(Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            
            return change;
        }
        /// <summary>
        /// 计算AB两点的距离
        /// </summary>
        /// <param name="lonA"></param>
        /// <param name="latA"></param>
        /// <param name="lonB"></param>
        /// <param name="latB"></param>
        /// <returns>单位米</returns>
        public double computeDistance(double startX, double startY, double endX, double endY)
        {
            double lonA = startX * Math.PI / 180;
            double latA = startY * Math.PI / 180;
            double lonB = endX * Math.PI / 180;
            double latB = endY * Math.PI / 180;
            double distanceAB;
            distanceAB = EarthRadio * Math.Acos(Math.Cos(latA) * Math.Cos(latB) * Math.Cos(lonB - lonA) + Math.Sin(latA) * Math.Sin(latB));
            return distanceAB * 1000;

        }

        /// <summary>
        /// 生成子版本
        /// </summary>
        /// <param name="nodeID">组成面的所有点的id</param>
        /// <param name="fatherVersion">原始版本</param>
        /// <param name="subVersion">子版本版本号</param>
        /// <param name="subTime">子版本时间</param>
        /// <param name="userName">子版本用户名，跟随用户改变点的用户名</param>
        /// <param name="userId">子版本用户ID，跟随用户改变点的用户ID</param>
        /// <returns>生成情况，成功true，失败false</returns>
        public bool GenerateSubVersion(string[] nodeID, polygon fatherVersion, int subVersion, DateTime subTime, string userName, long userId)
        {
            polygonsonversion subWay = new polygonsonversion();
            subWay.id = fatherVersion.id;
            subWay.username = userName;
            subWay.userid = userId;
            subWay.changeset = fatherVersion.changeset;
            subWay.version = fatherVersion.version;
            subWay.versionsub = subVersion;
            subWay.timestamp = subTime;
            //subWay.pointids = fatherVersion.pointids;
            subWay.tags = fatherVersion.tags;
            //生成shape字段
            string pointlist = "";
            string strNodeID = "";
            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                strNodeID += "'" + nodeID[i] + "'" + ",";
            }
            strNodeID = strNodeID.Substring(0, strNodeID.Length - 1);
            List<point> LstOsmNode = new List<point>();
            LstOsmNode = dal_point.GetListByCond(strNodeID);
            int flag = -1;
            try
            {

                for (int i = 0; i < nodeID.Length - 1; i++)
                {
                    long id = Convert.ToInt64(nodeID[i]);
                    for (int j = 0; j < LstOsmNode.Count; j++)
                    {
                        if (LstOsmNode[j].id == Convert.ToInt64(nodeID[i]) && (LstOsmNode[j].timestamp <= subTime))
                        {
                            flag = j;
                        }
                    }
                    if (flag != -1)
                    {
                        subWay.pointids += LstOsmNode[flag].id + "," + LstOsmNode[flag].version + ";";

                        double[] aa = Gaoss(LstOsmNode[flag].lon, LstOsmNode[flag].lat);
                        double x = aa[0];
                        double y = aa[1];
                        pointlist += x + " " + y + ",";
                        // pointlist += LstOsmNode[flag].lon + " " + LstOsmNode[flag].lat + ",";
                    }
                }
            }
            catch (System.Exception ex)
            {
                string erro = "生成面的子版本，当前处理到面ID：" + subWay.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                errolog.WriteEduAppLog(erro, ex.StackTrace);
                //MessageBox.Show(ex.ToString());
            }



            //for (int i = 0; i < nodeID.Length - 1; i++)
            //{
            //    long id = Convert.ToInt64(nodeID[i]);
            //    //根据点id，用户id，面生成时间去确定具体的点
            //    point osmNode = dal_point.GetModel(id, subTime);
            //    //点数据不全，避免返回的空值
            //    if (osmNode != null)
            //    {
            //        //子版本的点ID和点版本号，构成了该子版本的内容，可用于清理重复子版本
            //        subWay.pointids += osmNode.id + "," + osmNode.version + ";";
            //        //将点的经纬度赋予xy，用于shp字符串的拼接
            //        pointlist += osmNode.lon + " " + osmNode.lat + ",";
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            
            //subWay.points = pointlist.Substring(0, pointlist.Length - 1);
            //subWay.geom = "POLYGON((" + subWay.points + "))";
            try
            {
                if (pointlist.Length != 0)
                {
                    subWay.points = pointlist.Substring(0, pointlist.Length - 1);
                    subWay.geom = "POLYGON((" + subWay.points + "))";
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //Console.WriteLine("长度等于零啦。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                    //数据库插入子版本
                    dal_polygonsonversion.Add(subWay);
                    //dal_polygonsonversion.update1(subWay);
                   
                }
                return true;
            }
            catch (System.Exception ex)
            {
                string erro = "生成面的子版本，当前处理到面ID：" + subWay.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                errolog.WriteEduAppLog(erro, ex.StackTrace);
                return false;
            }
        }
        /// </summary>
        /// 经纬度转换成高斯坐标
        /// <param name="Lon"></param>
        /// <param name="Lat"></param>
        /// <returns></returns>
        public static double[] Gaoss(double Lon, double Lat)
        {

            double reelNo = (int)(Lon / 6) + 1;//对(L+3)/6的值四舍五入取整数,求经度所在6度带带号，Lon为当地经度
            double lx = 6 * reelNo - 3;//中央子午线经度

            double a = 6378137;
            double b = 6356752.3;

            double Pi = 3.1415926535898;
            double p = 180 / Pi * 3600;

            double dLon = (Lon - lx) * 3600;     //求经度L0与当前中央子午线的差值，单位为秒
            double B0 = Lat * Pi / 180;//将纬度的度分秒转换为弧度



            double e = Math.Sqrt(a * a - b * b) / a;
            double e1 = Math.Sqrt(a * a - b * b) / b;
            double Eta = e1 * Math.Cos(B0);
            double t = Math.Tan(B0);
            double N = a / Math.Sqrt(1 - e * e * Math.Sin(B0) * Math.Sin(B0));


            double A0 = 1 + 3.0 / 4.0 * Math.Pow(e, 2) + 45.0 / 64.0 * Math.Pow(e, 4) + 350.0 / 512.0 * Math.Pow(e, 6) + 11025.0 / 16384.0 * Math.Pow(e, 8);
            double A2 = -0.5 * (3.0 / 4.0 * Math.Pow(e, 2) + 60.0 / 64.0 * Math.Pow(e, 4) + 525.0 / 512.0 * Math.Pow(e, 6) + 17640.0 / 16384.0 * Math.Pow(e, 8));
            double A4 = 0.25 * (15.0 / 64.0 * Math.Pow(e, 4) + 210.0 / 512.0 * Math.Pow(e, 6) + 8820.0 / 16384.0 * Math.Pow(e, 8));
            double A6 = -1 * 1.0 / 6.0 * (35.0 / 512.0 * Math.Pow(e, 6) + 2520.0 / 16384.0 * Math.Pow(e, 8));
            double A8 = 1.0 / 8.0 * (315.0 / 16384.0 * Math.Pow(e, 8));
            double X = a * (1 - e * e) * (A0 * B0 + A2 * Math.Sin(2 * B0) + A4 * Math.Sin(4 * B0) + A6 * Math.Sin(6 * B0) + A8 * Math.Sin(8 * B0));

            double x, y;
            x = X + N / (2 * p * p) * Math.Sin(B0) * Math.Cos(B0) * dLon * dLon
                + N / (24 * Math.Pow(p, 4)) * Math.Sin(B0) * Math.Pow(Math.Cos(B0), 3) * (5 - t * t + 9 * Eta * Eta + 4 * Math.Pow(Eta, 4)) * Math.Pow(dLon, 4)
                + N / (720 * Math.Pow(p, 6)) * Math.Sin(B0) * Math.Pow(Math.Cos(B0), 5) * (61 - 58 * Math.Pow(t, 2) + Math.Pow(t, 4)) * Math.Pow(dLon, 6);


            y = N / p * Math.Cos(B0) * dLon
                + N / (6 * Math.Pow(p, 3)) * Math.Pow(Math.Cos(B0), 3) * (1 - t * t + Eta * Eta) * Math.Pow(dLon, 3)
                + N / (120 * Math.Pow(p, 5)) * Math.Pow(Math.Cos(B0), 5) * (5 - 18 * Math.Pow(t, 2) + Math.Pow(t, 4) + 14 * Eta * Eta - 58 * Eta * Eta * t * t) * Math.Pow(dLon, 5);
            y = 500000 + y; //加常数500000

            double del = 1;
            while (y / del >= 1)
            {
                del *= 10;
            }
            y = y + reelNo * del;

            double[] xy = { 0, 0 };
            xy[0] = x;
            xy[1] = y;
            return xy;

        }




        /// <summary>
        /// 判断点在点list中的位置
        /// </summary>
        /// <param name="nodeIDList"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public int nodeIdtoNum(string[] nodeIDList, string nodeID)
        {
            int num = nodeIDList.Length;
            int no = -1;
            for (int i = 0; i < num - 1; i++)
            {
                if (nodeIDList[i] == nodeID)
                {
                    no = i;
                }
            }
            return no;
        }
        /// <summary>
        /// 获取子版本，并存入数据库中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getsubversion()
        {
            DateTime start = System.DateTime.Now;

            List<polygon> list = new List<polygon>();
            int totalnum = 0;
            int allnum = listAll.Count();
            while (totalnum < allnum)
            {
                list.Clear();
                int tempnum = 0;
                while (tempnum < 1000 && totalnum < allnum)
                {
                    list.Add(listAll[totalnum]);
                    totalnum++;
                    tempnum++;
                }
                generateSubPolygon(list, totalnum - tempnum);
            }
            
        }


        /// <summary>
        /// 删除连续重复版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeletsameID()
        {

            sublist = dal_polygonsonversion.GetList();
            this.progressBar1.Maximum = sublist.Count - 1;
            this.progressBar1.Minimum = 0;
            polygonsonversion currentWay = new polygonsonversion();
            polygonsonversion nextWay = new polygonsonversion();
            for (int i = 0; i < sublist.Count - 1; i++)
            {
                this.progressBar1.Value = i;
                currentWay = sublist[i];
                nextWay = sublist[i + 1];
                //比较当前版本和下一个版本，如果id相同，userid也相同的话，删除
                if (currentWay.id == nextWay.id && currentWay.userid == nextWay.userid)
                {
                    dal_polygonsonversion.Delete(currentWay.objectid);
                }
            }
            // MessageBox.Show("执行成功！");
        }

        #region 面积相似度
        /// <summary>
        /// 面积相似度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getareadiffsim()
        {
            Dal_IntersectArea dal_intersect = new Dal_IntersectArea();

            List<polygonsonversion> versionList = new List<polygonsonversion>();
            //sublist = dal_polygonsonversion.GetList();
            polygonsonversion currentWay = new polygonsonversion();
            polygonsonversion nextWay = new polygonsonversion();
            List<long> idnum = new List<long>();
            idnum = dal_polygonsonversion.GetIdList();
            this.progressBar1.Maximum = idnum.Count- 1;
            this.progressBar1.Minimum = 0;
            //开始进行面积相似度比较，第一重循环用来遍历所有不同的面，根据ID
            for (int frist = 0; frist < idnum.Count; frist++)
            {
                this.progressBar1.Value = frist;
                //获取当次循环需要比较的所有面
                sublist = dal_polygonsonversion.GetList(idnum[frist]);
                //this.textBox1.Text += "当前评价id为" + idnum[frist] + "的面：\r\n";

                //第二重循环用来遍历当前ID的所有版本，每次循环评价一个版本，从第0个版本到最后一个版本
                for (int second = 0; second < sublist.Count; second++)
                {
                    //当前评价版本
                    currentWay = sublist[second];
                    InterstArea currentArea = dal_intersect.GetArea(currentWay.objectid);
                    List<long> userid = new List<long>();

                    //第三重循环用来比较当前版本与其他版本，并进行计算
                    for (int third = second - 1; third >= 0; third--)
                    {
                        nextWay = sublist[third];
                        //不评价自己的贡献，不评价已经评价过的贡献
                        if (currentWay.userid == nextWay.userid)
                            break;
                        else
                        {
                            //只评价最接近评价者的一个版本，
                            if (userid.Exists(delegate(long p)
                            {
                                if (p == nextWay.userid)
                                    return true;
                                else
                                    return false;
                            }))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {

                                    InterstArea intersectArea = new InterstArea();

                                    intersectArea = dal_intersect.GetIntersectArea(nextWay.objectid, currentWay.objectid);
                                    InterstArea nextArea = dal_intersect.GetArea(nextWay.objectid);
                                    double trustValue = intersectArea.IntersectArea / (Math.Max(currentArea.IntersectArea, nextArea.IntersectArea));
                                    nextWay.areaV += trustValue;
                                    if (nextWay.areaG==null)
                                    { nextWay.areaG = 0; }
                                    nextWay.areaG = nextWay.areaG + 1;
                                    nextWay.areadiffsim = Convert.ToDecimal(nextWay.areaV / nextWay.areaG);
                                    userid.Add(nextWay.userid);
                                    dal_polygonsonversion.Update(nextWay);

                                    //if (second == sublist.Count - 1)
                                    //{

                                    //    this.textBox1.Text += "当前被评价的版本为：" + third + ",其面积相似度为：" + nextWay.areadiffsim + "\r\n";
                                    //}

                                }
                                catch (Exception ex)
                                {
                                    string a = "计算面积相似度，当前处理到评价版本objectid：" + second + ",被评价版本objectid：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                    errolog.WriteEduAppLog(a, ex.StackTrace);
                                    //this.textBox1.Text += "执行失败，当前处理到评价版本：" + second + ",被评价版本：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                }
                            }
                            //}
                        }
                    }
                }

            }
        }
        #endregion 面积相似度

        /// <summary>
        /// 计算形状相似度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getshapediffsim()
        {


            //读取一个面的两个版本
            //把面做成点串数组
            //遍历List<>,取出相邻的两个点，计算对应边旋转角，方法：turnAngle（x1,y1,x2,y2）
            //线段，
            //形状相似度
            //Dal_IntersectArea dal_intersect = new Dal_IntersectArea();
            Dal_Shapedifsim dal_shapedifsim = new Dal_Shapedifsim();
            Dal_polygonsonversion dal_polygonsonversion = new Dal_polygonsonversion();
            List<polygonsonversion> versionList = new List<polygonsonversion>();
            //sublist = dal_polygonsonversion.GetList();
            polygonsonversion currentWay = new polygonsonversion();
            polygonsonversion nextWay = new polygonsonversion();
            List<long> idnum = new List<long>();
            idnum = dal_polygonsonversion.GetIdList();
            this.progressBar1.Maximum = idnum.Count - 1;
            this.progressBar1.Minimum = 0;
            //开始进行面积相似度比较，第一重循环用来遍历所有不同的面，根据ID
            for (int frist = 0; frist < idnum.Count; frist++)
            {
                this.progressBar1.Value = frist;
                //获取当次循环需要比较的所有面
                sublist = dal_polygonsonversion.GetList(idnum[frist]);
                //this.textBox1.Text += "当前评价id为" + idnum[frist] + "的面：\r\n";

                //第二重循环用来遍历当前ID的所有版本，每次循环评价一个版本，从第0个版本到最后一个版本
                for (int second = 0; second < sublist.Count; second++)
                {
                    //当前评价版本
                    currentWay = sublist[second];
                    //InterstArea currentArea = dal_intersect.GetArea(currentWay.objectid);
                    List<long> userid = new List<long>();
                    //第三重循环用来比较当前版本与其他版本，并进行计算
                    for (int third = second - 1; third >= 0; third--)
                    {
                        nextWay = sublist[third];
                        //不评价自己的贡献，不评价已经评价过的贡献
                        if (currentWay.userid == nextWay.userid)
                            break;
                        else
                        {
                            //只评价最接近评价者的一个版本，
                            if (userid.Exists(delegate(long p)
                            {
                                if (p == nextWay.userid)
                                    return true;
                                else
                                    return false;
                            }))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    string p = currentWay.points;
                                    string[] sp = currentWay.points.Split(',');
                                    int n = sp.Length;//sp表示split之后得到的点串
                                    double[,] points = new double[n, 2];
                                    double points2array = Dal_Shapedifsim.points2Array(p, points);


                                    double[,] turn = new double[n, 2];

                                    double[,] turnAngleDist = Dal_Shapedifsim.turnFunction(p, turn, false);
                                    string p1 = currentWay.points;
                                    string p2 = nextWay.points;
                                    double similarity = Dal_Shapedifsim.turnFunctionSim(p1, p2);
                                    double trustValue = similarity;
                                    nextWay.centroidX += trustValue;
                                    if (nextWay.centroidY == null)
                                    { nextWay.centroidY = 0; }
                                    nextWay.centroidY = nextWay.centroidY + 1;
                                    nextWay.shapediffsim = Convert.ToDecimal(nextWay.centroidX / nextWay.centroidY);
                                    userid.Add(nextWay.userid);
                                    dal_polygonsonversion.update(nextWay);


                                    //if (second == sublist.Count - 1)
                                    //{
                                    //    this.textBox1.Text += "当前被评价的版本为：" + third + ",其形状相似度为：" + nextWay.shapediffsim + "\r\n";
                                    //}

                                }
                                catch (Exception ex)
                                {
                                    string a = "计算面形状相似度，当前处理到评价版本objectid：" + second + ",被评价版本objectid：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                    errolog.WriteEduAppLog(a, ex.StackTrace);
                                    //this.textBox1.Text += "执行失败，当前处理到评价版本：" + second + ",被评价版本：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                }
                            }
                        }
                    }
                }
            }
        }
        #region


        /// <summary>
        /// 计算用户信誉度和目标可信度值
        /// </summary>
        /// <param name="w1">形状相似度所占权重</param>
        /// <param name="w2">面积相似度所占权重</param>
        /// <param name="c">c为调节用户信誉的影响程度</param>
        /// <param name="initReputation">初始信誉，默认值0.6</param>
        private void Getuserreputation(double w1, double w2, int c, double initReputation)
        {
            dal_polygonsonversion.UpdateTablereputationAndtrustvalueNull("polygonsonversion");
            dal_polygonsonversion.UpdateTablereputationAndtrustvalueNull("polylinesonversion");
            List<int> useridlist = new List<int>();
            List<polygonsonversion> listU = new List<polygonsonversion>();
            useridlist = dal_polygonsonversion.GetAlluserIdList();
            polygonsonversion currentWay = new polygonsonversion();
            polygonsonversion nextWay = new polygonsonversion();
            int userid = 0;
            this.progressBar1.Maximum = useridlist.Count - 1;
            this.progressBar1.Minimum = 0;
            for (int i = 0; i < useridlist.Count; i++)
            {
                this.progressBar1.Value = i;
                userid = useridlist[i];
                listU = dal_polygonsonversion.GetAllListbyuserid(userid);
                decimal sum1 = 0;
                decimal sum2 = 0;
                int n = 0;
                int userReviedCount = 0;
                double aveReputation = 0;
                double trustvalue = 0;
                double reputationDefault = initReputation;//初始信誉默认值0.6
                for (int j = 0; j < listU.Count; j++)
                {
                    currentWay = listU[j];
                    //nextWay = listU[i + 1];
                    if (userReviedCount <= 0)
                    {//第一次评价的化，直接将其初始信誉的默认值作为其信誉度值；
                        aveReputation = reputationDefault;
                    }
                    sum1 += currentWay.areadiffsim;
                    sum2 += currentWay.shapediffsim;
                    n = n + 1;
                    double similarity = (((double)sum1 * w1 + (double)sum2 * w2) / n);
                    //double similarity = ((double)(sum1 + sum2) / (n * 2));
                    if (similarity <= 0 )//如果相似度为0则用户信誉为上次评价信誉可信度值为用户信誉值
                    //if (similarity <= 0 || currentWay.areadiffsim <= 0 || currentWay.shapediffsim<=0)//如果相似度为0则用户信誉为上次评价信誉可信度值为用户信誉值
                    {
                        trustvalue = aveReputation;

                    }
                    else
                    {
                        if (similarity >= 1)
                        {
                            similarity = 1;
                        }
                        if (Double.IsNaN(similarity))
                        {
                            similarity = 0.5f; // similarity不能计算，则编辑部分和非编辑比例分别算一半
                        }
                        //得到单次评价的评价值
                        similarity = (float)(similarity * Math.Pow(aveReputation, c)); // 评价者对被评价者的评价值，c为调节用户信誉的影响程度,默认为零的话，此步无作用
                        if (userReviedCount <= 0)
                        {
                            aveReputation = similarity;
                            userReviedCount++;
                        }
                        else
                        {
                            aveReputation = (aveReputation + similarity) / 2;//每次信誉值计算结果都与上次取平均值
                        }

                        double editDegree = 1 - similarity;

                        trustvalue = editDegree * aveReputation + (1 - editDegree) * Math.Max(similarity, aveReputation);//取被评价对象的贡献者的信誉度值与评价对象的可信度值的最大值
                    }

                    if (currentWay.isArea == 0)//polyline
                    {
                        dal_polygonsonversion.UpdateReputationAndTrustvalue(currentWay.objectid, "polylinesonversion", aveReputation, trustvalue);
                    }
                    else if (currentWay.isArea == 1)//polygon
                    {
                        dal_polygonsonversion.UpdateReputationAndTrustvalue(currentWay.objectid, "polygonsonversion", aveReputation, trustvalue);
                    }
                }
            }
        }
 #endregion

        #endregion 相关函数操作
        #endregion 面处理

        #region 线处理
        #region 按钮操作
        /// <summary>
        /// 获取线对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            #region(该部分暂停使用20180608_hz)
            //try
            //{
            //    if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            //    {
            //        MessageBox.Show("数据库连接失败！请重新连接数据库");
            //    }
            //    else
            //    {
            //        Thread gettruvalue = new Thread(GetTrustvalue);
            //        gettruvalue.Start();
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
            //}
            #endregion
        }
        #region(该部分暂停使用20180608_hz)
        //private void GetTrustvalue()
        //{
        //    Stopwatch sw = new Stopwatch();

        //    //表格不自动生成行
        //    //this.dataGridView1.AutoGenerateColumns = false;
        //    try
        //    {
        //        listAll = dal_polygon.GetList();
        //        listlineAll = dal_polyline.GetList();  //获取表中的所有面记录
        //        // this.dataGridView1.DataSource = listlineAll;  //将面记录显示在表格中
        //        this.label1.Text = "共" + (listAll.Count + listlineAll.Count) + "个";
        //        //this.progressBar1.Maximum = listlineAll.Count - 1;
        //        //this.progressBar1.Minimum = 0;
        //        MessageBox.Show("点击确定，开始运行！");
        //        sw.Start();
        //        //dal_polylinesonversion.UpdateUserReputation();
        //        this.label5.Text = "正在计算面目标的可信度值";
        //        //计算面目标的信誉值
        //        Gettrustvalue();
        //        this.label5.Text = "正在计算线目标的可信度值";
        //        //计算线目标的信誉值
        //        GettrustvalueLine();
        //        sw.Stop();
        //        this.textBox1.Text += "\r\n历时:" + sw.Elapsed;
        //        MessageBox.Show("Job Done!历时:" + sw.Elapsed);

        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show("错误：" + ex.ToString());
        //    }
        //}
        #endregion

        /// <summary>
        /// 计算用户信誉度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void button16_Click(object sender, EventArgs e)
        //{
        //    dal_polylinesonversion.UpdateUserReputation();
        //    MessageBox.Show("计算完成！");
        //}
        private void button2_Click(object sender, EventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("数据库连接失败！请重新连接数据库");
            }
            else
            {
                this.dataGridView1.AutoGenerateColumns = false;
                try
                {
                    sublistline = dal_polylinesonversion.GetList();  //获取表中的所有面记录
                    this.dataGridView1.DataSource = sublistline;  //将面记录显示在表格中
                    this.label2.Text = "共" + sublistline.Count() + "个";
                }
                catch (System.Exception ex)
                {

                    errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                    //MessageBox.Show("错误：" + ex.ToString());
                }
            }
        }
        #endregion 按钮操作

        #region 相关函数操作
        /// <summary>
        /// 对比两个版本，发现改变的点
        /// </summary>
        /// <param name="currentVersion">当前版本</param>
        /// <param name="nextVersion">下一个版本</param>
        /// <returns>改变的点的list</returns>
        public List<point> CompareFindChangeNodeLine(polyline currentVersion, polyline nextVersion, List<point> OriginalNodeList2)
        {
            DateTime startTime = currentVersion.timestamp; //当前版本的时间
            DateTime endTime = nextVersion.timestamp; //下一个版本的时间
            //将点串替换成以“'，'”为分隔符的点串，原来用“;”
            string range = currentVersion.pointids.Replace(";", "','");
            range = range.Substring(0, range.Length - 2);
            //用字符数组存储点串
            string[] nodeID = currentVersion.pointids.Split(',');
            string[] test = range.Split(',');
            //用于存储点的list
            List<point> OriginalNodeList = new List<point>();

            //OriginalNodeList = dal_point.GetListByRange("id in (" + range + ")", startTime);

            OriginalNodeList = OriginalNodeList2;

            //OriginalNodeList = dal_point.GetListByRange("id in (" + range + ")", startTime);


            //从OriginalNodeList里面选出时间最靠近startTime的点存入nodeList中
            List<point> nodeList = new List<point>();

            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                //int flag = 0;
                long id = Convert.ToInt64(nodeID[i]);
                //for (int j = 0; j < OriginalNodeList.Count; j++)
                //{
                //    if (OriginalNodeList[j].id == id && startTime >= OriginalNodeList[j].timestamp)
                //    {
                //        flag = j;
                //    }
                //}
                //nodeList.Add(OriginalNodeList[flag]);
                var first = OriginalNodeList.LastOrDefault(item => item.id == id && item.timestamp <= startTime);
                //time = first.timestamp;
                if (first==null)
                {
                    continue;
                
                }
                nodeList.Add(first);

            }
            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                long id = Convert.ToInt64(nodeID[i]);
                var second = from item in OriginalNodeList
                             where (item.id == id && item.timestamp < endTime && item.timestamp > startTime)
                             orderby item.id, item.timestamp
                             select item;
                nodeList.AddRange(second);
            }
            //nodeList.Add(nodeList[0]);
            //为list添加当前版本与下一个版本之间的所有版本的点
            //nodeList.AddRange(dal_point.GetListSortByidandTime(range, startTime, endTime));
            //用于存储发生用户更改的点的list，changenodeList
            List<point> changenodeList = new List<point>();
            //首先把第一个点添加进去
            if (nodeList.Count != 0)
            {
                changenodeList.Add(nodeList[0]);
            }
            
            //循环判断所有list中发生用户改变的点，存进changenodeList
            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                point currentNode = nodeList[i];
                point nextNode = nodeList[i + 1];
                //标识用户或者点是否改变
                bool change = CompareNode(currentNode, nextNode, "userid&nodeid");
                //改变了就存进去
                if (change)
                {
                    changenodeList.Add(nodeList[i + 1]);
                }
            }
            return changenodeList;
        }

        /// <summary>
        /// 生成子版本
        /// </summary>
        /// <param name="nodeID">组成面的所有点的id</param>
        /// <param name="fatherVersion">原始版本</param>
        /// <param name="subVersion">子版本版本号</param>
        /// <param name="subTime">子版本时间</param>
        /// <param name="userName">子版本用户名，跟随用户改变点的用户名</param>
        /// <param name="userId">子版本用户ID，跟随用户改变点的用户ID</param>
        /// <returns>生成情况，成功true，失败false</returns>
        public bool GenerateSubVersionLine(string[] nodeID, polyline fatherVersion, int subVersion, DateTime subTime, string userName, long userId)
        {
            polylinesonversion subWay = new polylinesonversion();
            subWay.id = fatherVersion.id;
            subWay.username = userName;
            subWay.userid = userId;
            subWay.changeset = fatherVersion.changeset;
            subWay.version = fatherVersion.version;
            subWay.versionsub = subVersion;
            subWay.timestamp = subTime;
            //subWay.pointids = fatherVersion.pointids;
            subWay.tags = fatherVersion.tags;
            //生成shape字段
            string pointlist = "";
            string strNodeID = "";
            for (int i = 0; i < nodeID.Length - 1; i++)
            {
                strNodeID += "'" + nodeID[i] + "'" + ",";
            }
            strNodeID = strNodeID.Substring(0, strNodeID.Length - 1);
            List<point> LstOsmNode = new List<point>();
            LstOsmNode = dal_point.GetListByCond(strNodeID);
            int flag = -1;
            try
            {
                for (int i = 0; i < nodeID.Length - 1; i++)
                {
                    long id = Convert.ToInt64(nodeID[i]);

                    for (int j = 0; j < LstOsmNode.Count; j++)
                    {
                        if (LstOsmNode[j].id == Convert.ToInt64(nodeID[i]) && (LstOsmNode[j].timestamp <= subTime))
                        {

                            flag = j;
                        }
                    }
                    if (flag != -1)
                    {
                        subWay.pointids += LstOsmNode[flag].id + "," + LstOsmNode[flag].version + ";";
                        double[] aa = Gaoss(LstOsmNode[flag].lon, LstOsmNode[flag].lat);
                        double x = aa[0];
                        double y = aa[1];
                        try
                        {
                            pointlist += x + " " + y + ",";
                            subWay.points = pointlist.Substring(0, pointlist.Length - 1);
                            subWay.geomline = "LINESTRING(" + subWay.points + ")";
                        }
                        catch (System.Exception ex)
                        {
                            string erro = "生成线的子版本，处理的线ID为：" + subWay.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                            errolog.WriteEduAppLog(erro, ex.StackTrace);
                        }
                        //pointlist += LstOsmNode[flag].lon + " " + LstOsmNode[flag].lat + ",";
                    }
                }
            }
            catch (System.Exception ex)
            {
                string erro = "生成线的子版本，处理的线ID为：" + subWay.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                errolog.WriteEduAppLog(erro, ex.StackTrace);
                //MessageBox.Show(ex.ToString());
            }



            //for (int i = 0; i < nodeID.Length - 1; i++)
            //{
            //    long id = Convert.ToInt64(nodeID[i]);
            //    //根据点id，用户id，面生成时间去确定具体的点
            //    point osmNode = dal_point.GetModel(id, subTime);
            //    //点数据不全，避免返回的空值
            //    if (osmNode != null)
            //    {
            //        //子版本的点ID和点版本号，构成了该子版本的内容，可用于清理重复子版本
            //        subWay.pointids += osmNode.id + "," + osmNode.version + ";";
            //        //将点的经纬度赋予xy，用于shp字符串的拼接
            //        pointlist += osmNode.lon + " " + osmNode.lat + ",";
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
          
            
            if (subWay.geomline.Length<50)//排除只有一个点的错误
            {
                return false;
            }
            try
            {
                //数据库插入子版本
                dal_polylinesonversion.Add(subWay);
                return true;
            }
            catch (System.Exception ex)
            {
                string erro = "生成线的子版本，处理的线ID为：" + subWay.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                errolog.WriteEduAppLog(erro, ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 生成线子版本
        /// </summary>
        /// <param name="listline"></param>
        /// <param name="basenum"></param>
        public void generate(List<polyline> listline, int basenum)
        {
            this.progressBar1.Minimum = listlineAll.Count - 1;
            this.progressBar1.Minimum = 0;
            try
            {
                string str = "";
                List<int> test = new List<int>();
                //分割器符号
                char[] split = { ',' };
                List<point> allnodeList = new List<point>();
                //使用公开枚举数合并非泛型字符串数组
                //将list的第一个元素的点集合赋给arrUnion
                IEnumerable<string> arrUnion = listline[0].pointids.Split(split, System.StringSplitOptions.RemoveEmptyEntries);

                //使用循环，将list列表的其它元素的点集合拼接到arrUnion
                for (int i = 1; i < listline.Count; i++)
                {
                    string[] arr = listline[i].pointids.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
                    arrUnion = arrUnion.Union(arr).OrderBy(c => c);
                }
                string[] arrt = arrUnion.ToArray();
                str = String.Join(",", arrt);

                allnodeList = dal_point.GetListByCond(str);
                allnodeList.Count();
                allnodeList = allnodeList.Distinct().ToList();
                //对所有的way进行遍历
                for (int i = 0; i < listline.Count - 1; i++)
                {
                    this.progressBar1.Value = basenum + i;
                    try
                    {
                        polyline currentVersion = listline[i]; //当前版本
                        polyline nextVersion = listline[i + 1]; //下一个版本

                        //提示信息
                        //this.textBox1.Text += "\r\n正在处理" + currentVersion.objectid + "," + currentVersion.id + "," + currentVersion.version + ":";

                        //将点串分开，存储在nodeID中
                        string[] nodeID = currentVersion.pointids.Split(',');

                        ////利用正则表达式计算一个面包含的点的个数
                        int nodeNum = Regex.Matches(currentVersion.pointids, ",").Count;

                        //对比两个版本，获取所有需要点（即原始版本的点和用户更改的点）
                        int Num;

                        List<point> changenodeList = CompareFindChangeNodeLine(currentVersion, nextVersion, allnodeList);

                        //对点按照时间进行排序
                        changenodeList.Sort(delegate(point info1, point info2)
                        {
                            Type t1 = info1.GetType();
                            Type t2 = info2.GetType();
                            PropertyInfo pro1 = t1.GetProperty("timestamp");
                            PropertyInfo pro2 = t2.GetProperty("timestamp");
                            int a = Convert.ToDateTime(pro1.GetValue(info1, null)).CompareTo(Convert.ToDateTime(pro2.GetValue(info2, null)));
                            return a;
                        });
                        //子版本计数
                        int subVersionNum = 0;

                        //首先生成原始版本
                        try
                        {
                            if (GenerateSubVersionLine(nodeID, currentVersion, subVersionNum, currentVersion.timestamp, currentVersion.username, currentVersion.userid))
                            {
                                //this.textBox1.Text += "\r\n  生成原始版本：" + subVersionNum++;
                                //this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                            else
                            {
                                //this.textBox1.Text += "\r\n  生成失败！";
                                //this.textBox1.ScrollToCaret();//滚动到光标处
                            }
                        }
                        catch (System.Exception ex)
                        {
                            string erro = "生成线的子版本，处理的线ID为：" + currentVersion.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                            errolog.WriteEduAppLog(erro, ex.StackTrace);
                            //this.textBox1.Text += "\r\n  生成原始版本失败，失败原因：" + ex.ToString();
                            this.textBox1.ScrollToCaret();//滚动到光标处
                        }

                        //对比排序后的点list，计算用户改变次数，并生成子版本
                        for (int j = nodeNum - 1; j < changenodeList.Count - 1; j++)
                        {
                            point currentNode = changenodeList[j];
                            point nextNode = changenodeList[j + 1];
                            //判断点用户是否改变
                            bool change = CompareNode(currentNode, nextNode, "userid");

                            try
                            {
                                if (change)
                                {
                                    //设置时间为最后用户改变的点的时间
                                    DateTime deadTime = nextNode.timestamp;
                                    //生成子版本
                                    if (GenerateSubVersionLine(nodeID, currentVersion, subVersionNum, deadTime, nextNode.username, nextNode.userid))
                                    {
                                        //this.textBox1.Text += "\r\n  生成子版本：" + subVersionNum++;
                                        //this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                    else
                                    {
                                        this.textBox1.Text += "\r\n  生成失败！";
                                        this.textBox1.ScrollToCaret();//滚动到光标处
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                string erro = "生成线的子版本，处理的线ID为：" + currentVersion.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                errolog.WriteEduAppLog(erro, ex.StackTrace);
                                //this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                                this.textBox1.ScrollToCaret();//滚动到光标处
                                continue;
                            }

                        }
                    }
                    catch (System.Exception ex)
                    {

                        errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                        //this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                        this.textBox1.ScrollToCaret();//滚动到光标处
                        continue;
                    }
                }
                //处理最后一个线                
                try
                {
                    this.progressBar1.Value =basenum + listline.Count - 1;
                    polyline lastPolyline = listline[listline.Count - 1];
                    //提示信息
                    //this.textBox1.Text += "\r\n正在处理" + lastPolyline.objectid + "," + lastPolyline.id + "," + lastPolyline.version + ":";

                    //将点串分开，存储在nodeID中
                    string[] nodeID = lastPolyline.pointids.Split(',');
                    //将点串替换成以“'，'”为分隔符的点串，原来用“;”
                    string range =lastPolyline.pointids.Replace(";", "','");
                    range = range.Substring(0, range.Length - 2);
                    //利用正则表达式计算一个面包含的点的个数
                    int nodeNum = Regex.Matches(lastPolyline.pointids, ",").Count;

                    //判断是否闭合，若闭合，则做面处理，继续进行；不闭合，不处理，跳出当次循环
                    //if (nodeID[0] != nodeID[nodeNum - 1] || lastPolyline.pointids == "")
                    //{
                    //    this.textBox1.Text += "\r\n  不是面，忽略。";
                    //    this.textBox1.ScrollToCaret();//滚动到光标处
                    //}
                    //else
                    //{
                    //最终版本的时间为最终版本点的时间，新建一个最终版本，把他的时间定为最终版本点的时间，让最后一个面与最终版本进行比较
                    polyline theLastPolyline = lastPolyline;
                    theLastPolyline.timestamp = dal_point.GetModel(range).timestamp;
                    //对比两个版本，获取所有需要点（即原始版本的点和用户更改的点）
                    List<point> changenodeList = CompareFindChangeNodeLine(lastPolyline, theLastPolyline, allnodeList);
                    //子版本计数
                    int subVersionNum = 0;


                    //首先生成原始版本
                    try
                    {
                        if (GenerateSubVersionLine(nodeID, lastPolyline, subVersionNum, lastPolyline.timestamp, lastPolyline.username, lastPolyline.userid))
                        {
                            //this.textBox1.Text += "\r\n  生成原始版本：" + subVersionNum++;
                            //this.textBox1.ScrollToCaret();//滚动到光标处
                        }
                        else
                        {
                            this.textBox1.Text += "\r\n  生成失败！";
                            this.textBox1.ScrollToCaret();//滚动到光标处
                        }
                    }
                    catch (System.Exception ex)
                    {
                        string erro = "生成线的子版本，处理的线ID为：" + lastPolyline.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                        errolog.WriteEduAppLog(erro, ex.StackTrace);
                        //this.textBox1.Text += "\r\n  生成原始版本失败，失败原因：" + ex.ToString();
                        this.textBox1.ScrollToCaret();//滚动到光标处
                    }
                    //对比排序后的点list，计算用户改变次数，并生成子版本
                    for (int j = nodeNum - 1; j < changenodeList.Count - 1; j++)
                    {
                        point currentNode = changenodeList[j];
                        point nextNode = changenodeList[j + 1];
                        //判断点用户是否改变
                        bool change = CompareNode(currentNode, nextNode, "userid");

                        try
                        {
                            if (change)
                            {
                                //设置时间为最后用户改变的点的时间
                                DateTime deadTime = nextNode.timestamp;
                                //生成子版本
                                if (GenerateSubVersionLine(nodeID, lastPolyline, subVersionNum, deadTime, nextNode.username, nextNode.userid))
                                {
                                    //this.textBox1.Text += "\r\n  生成子版本：" + subVersionNum++;
                                    //this.textBox1.ScrollToCaret();//滚动到光标处
                                }
                                else
                                {
                                    this.textBox1.Text += "\r\n  生成失败！";
                                    this.textBox1.ScrollToCaret();//滚动到光标处
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            string erro = "生成线的子版本，处理的线ID为：" + lastPolyline.id + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                            errolog.WriteEduAppLog(erro, ex.StackTrace);
                            //this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                            this.textBox1.ScrollToCaret();//滚动到光标处
                            continue;
                        }

                    }
                }

                catch (System.Exception ex)
                {

                    errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                    //this.textBox1.Text += "\r\n  生成失败，跳过！失败原因：" + ex.ToString();
                    this.textBox1.ScrollToCaret();//滚动到光标处
                }



            }
            catch (System.Exception ex)
            {

                errolog.WriteEduAppLog(ex.Message, ex.StackTrace);
                //this.textBox1.Text += "\r\n  生成失败，进程中断！错误原因;" + ex.ToString();
                this.textBox1.ScrollToCaret();//滚动到光标处
            }

        }
        /// <summary>
        /// 获取子版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getsubversionline()
        {

            List<polyline> listline = new List<polyline>();
            int totalnum = 0;
            int allnum = listlineAll.Count();
            while (totalnum < allnum)
            {
                listline.Clear();
                int tempnum = 0;
                while (tempnum < 1000 && totalnum < allnum)
                {
                    listline.Add(listlineAll[totalnum]);
                    totalnum++;
                    tempnum++;
                }
                generate(listline, totalnum - tempnum);
            }
            //MessageBox.Show("Job Done!");
            OracleDBHelper.ExecuteSql("update polylinesonversion set objectid=rownum");

        }


        /// <summary>
        /// 清理用户相同的连续版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletesameIDline()
        {
            sublistline = dal_polylinesonversion.GetList();
            this.progressBar1.Maximum = sublistline.Count - 1;
            this.progressBar1.Minimum = 0;
            polylinesonversion currentWay = new polylinesonversion();
            polylinesonversion nextWay = new polylinesonversion();
            for (int i = 0; i < sublistline.Count - 1; i++)
            {
                this.progressBar1.Value = i;
                currentWay = sublistline[i];
                nextWay = sublistline[i + 1];
                //比较当前版本和下一个版本，如果id相同，userid也相同的话，删除
                if (currentWay.id == nextWay.id && currentWay.userid == nextWay.userid)
                {
                    dal_polylinesonversion.Delete(currentWay.objectid);
                }
            }
            //MessageBox.Show("执行成功！");
        }

        /// <summary>
        /// 计算线的长度相似度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getlengthdiffsim()
        {
            Dal_IntersectArea dal_intersect = new Dal_IntersectArea();

            List<polylinesonversion> versionList = new List<polylinesonversion>();
            //sublist = dal_polylinesonversion.GetList();
            polylinesonversion currentWay = new polylinesonversion();
            polylinesonversion nextWay = new polylinesonversion();

            List<long> idnum = new List<long>();
            idnum = dal_polylinesonversion.GetIdList();
            this.progressBar1.Maximum = idnum.Count - 1;
            this.progressBar1.Minimum = 0;
            //开始进行面积相似度比较，第一重循环用来遍历所有不同的面，根据ID
            for (int frist = 0; frist < idnum.Count; frist++)
            {
                Console.WriteLine("正在计算线的长度相似度。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                this.progressBar1.Value = frist;
                //获取当次循环需要比较的所有面
                sublistline = dal_polylinesonversion.GetList(idnum[frist]);
                //this.textBox1.Text += "当前评价id为" + idnum[frist] + "的线：\r\n";

                //第二重循环用来遍历当前ID的所有版本，每次循环评价一个版本，从第0个版本到最后一个版本
                for (int second = 0; second < sublistline.Count; second++)
                {


                    //当前评价版本
                    currentWay = sublistline[second];
                    Console.WriteLine("Getlength" + currentWay.objectid);
                    InterstArea currentLength = dal_intersect.Getlength(currentWay.objectid);
                    Console.WriteLine("GetBuffer" + currentWay.objectid);
                    InterstArea currentBuffer = dal_intersect.GetBuffer(currentWay.objectid);
                    Console.WriteLine("frist=" + frist + "second=" + second);
                    List<long> userid = new List<long>();

                    //第三重循环用来比较当前版本与其他版本，并进行计算
                    for (int third = second - 1; third >= 0; third--)
                    {
                        nextWay = sublistline[third];
                        //不评价自己的贡献，不评价已经评价过的贡献
                        if (currentWay.userid == nextWay.userid)
                            break;
                        else
                        {
                            //只评价最接近评价者的一个版本，
                            if (userid.Exists(delegate(long p)
                            {
                                if (p == nextWay.userid)
                                    return true;
                                else
                                    return false;
                            }))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {

                                    InterstArea intersectLength = new InterstArea();
                                    InterstArea intersectBuffer = new InterstArea();
                                    //object Length = dal_intersect.GetIntersectLength(nextWay.objectid, currentWay.objectid);
                                    //object Buffer= dal_intersect.GetIntersectBuffer(nextWay.objectid, currentWay.objectid);

                                    InterstArea nextArea = dal_intersect.Getlength(nextWay.objectid);
                                    Console.WriteLine("\r\n开始");
                                    Console.WriteLine(nextWay.objectid + "。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。");
                                    InterstArea nextBuffer = dal_intersect.GetBuffer(nextWay.objectid);
                                    Console.WriteLine("结束\r\n");

                                    //double trustValue1=0;
                                    //double trustValue2=0;
                                    //if (Length != DBNull.Value && Buffer != DBNull.Value)
                                    //{
                                        intersectLength = dal_intersect.GetIntersectLength(nextWay.objectid, currentWay.objectid);
                                        intersectBuffer = dal_intersect.GetIntersectBuffer(nextWay.objectid, currentWay.objectid);
                                        double trustValue1 = intersectLength.IntersectArea / (Math.Max(currentLength.IntersectArea, nextArea.IntersectArea));
                                        double trustValue2 = intersectBuffer.IntersectArea / (Math.Max(currentBuffer.IntersectArea, nextBuffer.IntersectArea));
                                    //}
                                    //else 
                                    //{

                                    //    //intersectLength = 0.0;
                                    //}
                                    
                                    
                                    nextWay.areaV += (trustValue1 + trustValue2);
                                    if (nextWay.areaG == null)
                                    { nextWay.areaG = 0; }
                                    nextWay.areaG = nextWay.areaG + 1;
                                    nextWay.areadiffsim = Convert.ToDecimal(nextWay.areaV / (nextWay.areaG * 2));
                                    userid.Add(nextWay.userid);
                                    dal_polylinesonversion.Update(nextWay);

                                    //if (second == sublistline.Count - 1)
                                    //{
                                    //    //this.textBox1.Text += "当前被评价的版本为：" + third + ",其长度相似度为：" + nextWay.areadiffsim + "\r\n";
                                    //}

                                }
                                catch (Exception ex)
                                {
                                    string erro = "计算长度相似度，当前线的objectid为：" + second + ",被评价版本的objectid为：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                    errolog.WriteEduAppLog(erro, ex.StackTrace);

                                    //this.textBox1.Text += "执行失败，当前处理到评价版本：" + second + ",被评价版本：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                }
                            }
                            //}
                        }
                    }
                }

            }
        }


        /// <summary>
        /// 计算线的形状相似度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Getshapediffsimline()
        {
            Dal_Shapedifsim dal_shapedifsim = new Dal_Shapedifsim();
            Dal_polylinesonversion dal_polylinesonversion = new Dal_polylinesonversion();
            List<polylinesonversion> versionList = new List<polylinesonversion>();
            //sublist = dal_polylinesonversion.GetList();
            polylinesonversion currentWay = new polylinesonversion();
            polylinesonversion nextWay = new polylinesonversion();
            List<long> idnum = new List<long>();
            idnum = dal_polylinesonversion.GetIdList();
            this.progressBar1.Maximum = idnum.Count - 1;
            this.progressBar1.Minimum = 0;
            //开始进行面积相似度比较，第一重循环用来遍历所有不同的面，根据ID
            for (int frist = 0; frist < idnum.Count; frist++)
            {
                this.progressBar1.Value = frist;
                //获取当次循环需要比较的所有面
                sublistline = dal_polylinesonversion.GetList(idnum[frist]);
                //this.textBox1.Text += "当前评价id为" + idnum[frist] + "的面：\r\n";

                //第二重循环用来遍历当前ID的所有版本，每次循环评价一个版本，从第0个版本到最后一个版本
                for (int second = 0; second < sublistline.Count; second++)
                {
                    //当前评价版本
                    currentWay = sublistline[second];
                    //InterstArea currentArea = dal_intersect.GetArea(currentWay.objectid);
                    List<long> userid = new List<long>();
                    //第三重循环用来比较当前版本与其他版本，并进行计算
                    for (int third = second - 1; third >= 0; third--)
                    {
                        nextWay = sublistline[third];
                        //不评价自己的贡献，不评价已经评价过的贡献
                        if (currentWay.userid == nextWay.userid)
                            break;
                        else
                        {
                            //只评价最接近评价者的一个版本，
                            if (userid.Exists(delegate(long p)
                            {
                                if (p == nextWay.userid)
                                    return true;
                                else
                                    return false;
                            }))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    string p = currentWay.points;
                                    string[] sp = currentWay.points.Split(',');
                                    int n = sp.Length;//sp表示split之后得到的点串
                                    double[,] points = new double[n, 2];
                                    double points2array = Dal_Shapedifsim.points2Array(p, points);

                                    double[,] turn = new double[n, 2];
                                    //string p = currentWay.points;//p表示得到的坐标字符串
                                    double[,] turnAngleDist = Dal_Shapedifsim.turnFunction(p, turn, false);
                                    string p1 = currentWay.points;
                                    string p2 = nextWay.points;
                                    double similarity = Dal_Shapedifsim.turnFunctionSimLine(p1, p2);
                                    double trustValue = similarity;
                                    nextWay.centroidX += trustValue;
                                    if (nextWay.centroidY == null)
                                    { nextWay.centroidY = 0; }
                                    nextWay.centroidY = nextWay.centroidY + 1;
                                    nextWay.shapediffsim = Convert.ToDecimal(nextWay.centroidX / nextWay.centroidY);
                                    userid.Add(nextWay.userid);
                                    dal_polylinesonversion.update(nextWay);

                                    if (second == sublistline.Count - 1)
                                    {
                                        //this.textBox1.Text += "当前被评价的版本为：" + third + ",其形状相似度为：" + nextWay.shapediffsim + "\r\n";
                                    }

                                }
                                catch (Exception ex)
                                {
                                    string erro = "计算线的形状相似度，当前线的objectid为" + second + ",被评价版本objectid为：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                    errolog.WriteEduAppLog(erro, ex.StackTrace);
                                    //this.textBox1.Text += "执行失败，当前处理到评价版本：" + second + ",被评价版本：" + third + "；\r\n失败原因：" + ex.ToString() + "\r\n";
                                }
                            }
                        }
                    }
                }
            }
        }
        #region（该部分暂停使用20180608-hz）
        ///// <summary>
        ///// 计算面目标的信誉度
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Gettrustvalue()
        //{
        //    Dal_IntersectArea dal_intersect = new Dal_IntersectArea();
        //    List<long> Idall = new List<long>();
        //    Idall = dal_polygonsonversion.GetIdList();
        //    this.progressBar1.Maximum = Idall.Count - 1;
        //    this.progressBar1.Minimum = 0;
        //    polygonsonversion currentWay = new polygonsonversion();
        //    polygonsonversion nextWay = new polygonsonversion();
        //    try
        //    {

        //        for (int i = 0; i < Idall.Count; i++)
        //        {
        //            this.progressBar1.Value = i;
        //            sublist = dal_polygonsonversion.GetList(Idall[i]);
        //            double trustvalue = 0;

        //            for (int j = 0; j < sublist.Count; j++)
        //            {
        //                currentWay = sublist[j];
        //                if (j == 0)
        //                {
        //                    trustvalue = currentWay.userReputation;

        //                }
        //                else
        //                {
        //                    try
        //                    {

        //                        nextWay = sublist[j - 1];
        //                        double R = currentWay.userReputation;
        //                        if (R == -1)
        //                        {
        //                            R = dal_polygonsonversion.Getavguserreputation();
        //                        }
        //                        double T = nextWay.trustValue;
        //                        string p = currentWay.points;
        //                        string[] sp = currentWay.points.Split(',');
        //                        int n = sp.Length;//sp表示split之后得到的点串
        //                        double[,] points = new double[n, 2];
        //                        double points2array = Dal_Shapedifsim.points2Array(p, points);

        //                        double[,] turn = new double[n, 2];
        //                        double[,] turnAngleDist = Dal_Shapedifsim.turnFunction(p, turn, false);
        //                        string p1 = currentWay.points;
        //                        string p2 = nextWay.points;
        //                        double shapediffsim = Dal_Shapedifsim.turnFunctionSim(p1, p2);
        //                        double areadiffsim = 0.5;
        //                        try
        //                        {
        //                            InterstArea currentArea = dal_intersect.GetArea(currentWay.objectid);
        //                            InterstArea intersectArea = dal_intersect.GetIntersectArea(nextWay.objectid, currentWay.objectid);
        //                            InterstArea nextArea = dal_intersect.GetArea(nextWay.objectid);
        //                            areadiffsim = intersectArea.IntersectArea / (Math.Max(currentArea.IntersectArea, nextArea.IntersectArea));
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                            string erro = "计算面的可信度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                            errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                        }
        //                        double versim = areadiffsim + shapediffsim;


        //                        trustvalue = versim * R + (1 - versim) * Math.Max(R, T);
        //                    }
        //                    catch (System.Exception ex)
        //                    {

        //                        string erro = "计算面的可信度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                        errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                    }

        //                }
        //                try
        //                {
        //                    dal_polygonsonversion.UpdateUserT(currentWay.objectid, trustvalue);
        //                }
        //                catch
        //                {
        //                    trustvalue = -1;
        //                    dal_polygonsonversion.UpdateUserT(currentWay.objectid, trustvalue);
        //                }
        //                int a = 0;
        //                try
        //                {
        //                    a = Convert.ToInt16(Math.Floor(trustvalue * 10));
                       
        //                int rank = 0;
        //                switch (a)
        //                {
        //                    case 0:
        //                        rank = 1;

        //                        break;
        //                    case 1:
        //                        rank = 2;
        //                        break;
        //                    case 2:
        //                        rank = 3;

        //                        break;
        //                    case 3:
        //                        rank = 4;

        //                        break;
        //                    case 4:
        //                        rank = 5;

        //                        break;
        //                    case 5:
        //                        rank = 6;

        //                        break;
        //                    case 6:
        //                        rank = 7;

        //                        break;
        //                    case 7:
        //                        rank = 8;

        //                        break;
        //                    case 8:
        //                        rank = 9;

        //                        break;
        //                    case 9:
        //                        rank = 10;

        //                        break;
        //                    case 10:
        //                        rank = 10;
        //                        break;
        //                }
        //                   dal_polygonsonversion.UpdatePRank(currentWay.objectid, rank);
        //                }
        //                catch (System.Exception ex)
        //                {
                            
        //                    string erro = "计算面的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                    errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                }

        //            }

        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        string erro = "计算面的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //        errolog.WriteEduAppLog(erro, ex.StackTrace);
        //    }
        //    // MessageBox.Show("计算完成！");
        //}
        ///// <summary>
        ///// 计算线的目标信誉值
        ///// </summary>
        //private void GettrustvalueLine()
        //{
        //    Dal_IntersectArea dal_intersect = new Dal_IntersectArea();
        //    List<long> Idall = new List<long>();
        //    Idall = dal_polylinesonversion.GetIdList();
        //    this.progressBar1.Maximum = Idall.Count - 1;
        //    this.progressBar1.Minimum = 0;
        //    polylinesonversion currentWay = new polylinesonversion();
        //    polylinesonversion nextWay = new polylinesonversion();
        //    try
        //    {

        //        for (int i = 0; i < Idall.Count; i++)
        //        {
        //            this.progressBar1.Value = i;
        //            sublistline = dal_polylinesonversion.GetList(Idall[i]);
        //            double trustvalue = 0;

        //            for (int j = 0; j < sublistline.Count; j++)
        //            {
        //                currentWay = sublistline[j];
        //                if (j == 0)
        //                {
        //                    trustvalue = currentWay.userReputation;

        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        nextWay = sublistline[j - 1];
        //                        double R = currentWay.userReputation;
        //                        if (R == -1)
        //                        {
        //                            R = dal_polylinesonversion.Getavguserreputation();
        //                        }
        //                        double T = nextWay.trustValue;
        //                        string p = currentWay.points;
        //                        string[] sp = currentWay.points.Split(',');
        //                        int n = sp.Length;//sp表示split之后得到的点串
        //                        double[,] points = new double[n, 2];
        //                        double points2array = Dal_Shapedifsim.points2Array(p, points);

        //                        double[,] turn = new double[n, 2];
        //                        //string p = currentWay.points;//p表示得到的坐标字符串
        //                        double[,] turnAngleDist = Dal_Shapedifsim.turnFunction(p, turn, false);
        //                        string p1 = currentWay.points;
        //                        string p2 = nextWay.points;
        //                        double shapediffsim = Dal_Shapedifsim.turnFunctionSimLine(p1, p2);
        //                        double areadiffsim = 0.5;

        //                        InterstArea currentLength = dal_intersect.Getlength(currentWay.objectid);
        //                        InterstArea intersectLength = dal_intersect.GetIntersectLength(nextWay.objectid, currentWay.objectid);
        //                        InterstArea nextLength = dal_intersect.Getlength(nextWay.objectid);
        //                        areadiffsim = intersectLength.IntersectArea / (Math.Max(currentLength.IntersectArea, nextLength.IntersectArea));

        //                        //nextWay.areaV += trustValue;
        //                        //if (nextWay.areaG == null)
        //                        //{ nextWay.areaG = 0; }
        //                        //nextWay.areaG = nextWay.areaG + 1;
        //                        //nextWay.areadiffsim = Convert.ToDecimal(nextWay.areaV / nextWay.areaG);
        //                        double versim = areadiffsim + shapediffsim;

        //                        trustvalue = versim * R + (1 - versim) * Math.Max(R, T);
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                        string erro = "计算线的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                        errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                        //return;
        //                    }
        //                    //}

        //                }
        //                try
        //                {
        //                    dal_polylinesonversion.UpdateUserT(currentWay.objectid, trustvalue);
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    string erro = "计算线的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                    errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                }

        //                int a = 0;
        //                try
        //                {
        //                    a = Convert.ToInt32(Math.Floor(trustvalue * 10));
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    string erro = "计算线的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //                    errolog.WriteEduAppLog(erro, ex.StackTrace);
        //                }

        //                int rank = 0;
        //                switch (a)
        //                {
        //                    case 0:
        //                        rank = 1;

        //                        break;
        //                    case 1:
        //                        rank = 2;
        //                        break;
        //                    case 2:
        //                        rank = 3;

        //                        break;
        //                    case 3:
        //                        rank = 4;

        //                        break;
        //                    case 4:
        //                        rank = 5;

        //                        break;
        //                    case 5:
        //                        rank = 6;

        //                        break;
        //                    case 6:
        //                        rank = 7;

        //                        break;
        //                    case 7:
        //                        rank = 8;

        //                        break;
        //                    case 8:
        //                        rank = 9;

        //                        break;
        //                    case 9:
        //                        rank = 10;

        //                        break;
        //                    case 10:
        //                        rank = 10;
        //                        break;
        //                }
        //                dal_polylinesonversion.UpdatePRank(currentWay.objectid, rank);

        //            }

        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        string erro = "计算线的信誉度，当前处理到评价版本objectid：" + currentWay.objectid + ",被评价版本objectid：" + nextWay.objectid + "；\r\n失败原因：" + ex.ToString() + "\r\n";
        //        errolog.WriteEduAppLog(erro, ex.StackTrace);
        //        //return;
        //    }
        //    // MessageBox.Show("计算完成！");
        //}
        #endregion
        #endregion 相关函数操作

        private void oscRepuBtn_Click(object sender, EventArgs e)
        {
            if (this.openFileDlg.ShowDialog() == DialogResult.OK)
            {
                List<string> wayIds = ReadIdFile(this.openFileDlg.FileName, "id");
                int cnt = wayIds.Count;
            }
        }
        /// <summary>
        /// 根据XML文本路径读取需要计算信誉度的增量线数据ID，返回ID字符串数组
        /// </summary>
        /// <param name="fileName">XML文件</param>
        /// <param name="idEleName">XML文件中ID节点的名称</param>
        /// <returns>增量线数据ID       </returns>
        public static List<string> ReadIdFile(string fileName, string idEleName)
        {
            List<string> needRepuWayIds = new List<string>();
            XmlTextReader xr = new XmlTextReader(fileName);
            while (xr.Read())
            {
                if (xr.Name == idEleName && xr.NodeType == XmlNodeType.Element)
                {
                    xr.Read();
                    needRepuWayIds.Add(xr.Value);
                }
            }
            return needRepuWayIds;
        }




        #endregion 线处理
        //GIS.UI.MapUI ui;
        //private void cmbRpulation_Click(object sender, EventArgs e)
        //{
        //    FrmRepulation frmRepution = new FrmRepulation(ui);
        //    frmRepution.Show();

        //}

        //private void buttonShowData_Click(object sender, EventArgs e)
        //{
        //    if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
        //    {
        //        MessageBox.Show("数据库连接失败！请重新连接数据库");
        //    }
        //    else
        //    {
        //        //表格不自动生成行
        //        this.dataGridView2.AutoGenerateColumns = false;
        //        try
        //        {
        //            if (radioButton1.Checked)
        //            {
        //                sublistline = dal_polylinesonversion.GetList();  //获取表中的所有面记录
        //                this.dataGridView2.DataSource = sublistline;  //将面记录显示在表格中
        //                this.label2.Text = "共" + sublistline.Count() + "个";

        //            }
        //            else
        //            {
        //                sublist = dal_polygonsonversion.GetList();  //获取表中的所有面记录
        //                this.dataGridView2.DataSource = sublist;  //将面记录显示在表格中
        //                this.label2.Text = "共" + sublist.Count() + "个";
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            MessageBox.Show("错误：" + ex.ToString());
        //        }
        //    }

        //}

        private void button2_Click_1(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            MessageBox.Show("点击确定，开始运行！");
            sw.Start();
            try
            {
                List<long> countlist = new List<long>();
                List<long> alllist = new List<long>();
                long objectid = dal_polylinesonversion.Getmaxobjectid();
                long maxid = long.Parse(dal_polylinesonversion.GetID(objectid));
                countlist = dal_polylinesonversion.GetIdList();
                countlist.Remove(maxid);
                alllist = dal_polyline.GetIdList();
                for (int i = 0; i < countlist.Count; i++)
                {

                    long a = countlist[i];

                    for (int j = 0; j < alllist.Count; j++)
                    {
                        // polyline b = new polyline();
                        long b = alllist[j];
                        if (b.Equals(a))
                        {
                            alllist.Remove(b);
                        }
                    }
                }
                //List<polyline> lista = new List<polyline>();
                List<string> liststring = new List<string>();
                for (int h = 0; h < alllist.Count; h++)
                {
                    //string k = alllist[h].ToString();
                    //lista = dal_polyline.GetListBYID(k);
                    //listlineAll.AddRange(lista);
                    string k = alllist[h].ToString();
                    liststring.Add(k);
                }
                string c = string.Join(",", liststring.ToArray());
                listlineAll = dal_polyline.GetListBYID(c);

                //获得线的子版本
                Getsubversionline();
                //清理用户相同的连续版本
                DeletesameIDline();
                //计算长度相似度
                Getlengthdiffsim();
                //计算线形状相似度
                Getshapediffsimline();
                //计算用户信誉度
                Getuserreputation(w1, w2, paraC, initReputation);
                sw.Stop();
                this.textBox1.Text += "\r\n历时:" + sw.Elapsed;
                MessageBox.Show("Job Done!历时:" + sw.Elapsed);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("错误：" + ex.ToString());

            }



        }

        private void buttonFillRup_Click(object sender, EventArgs e)
        {
            OracleConnection osmCon = new OracleConnection(OSMDataBaseLinkForm.conStringTemp);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            //int[] No1 = { 0, 0};
            //No1 = fillReputationField("osmarea", "polygonsonversion", osmCon);
            progressBar1.Value = 50;
            int[] No2 = { 0, 0 };
            No2 = fillReputationField("osmline", "polylinesonversion", osmCon);
            progressBar1.Value = 100;
            //MessageBox.Show(string.Format("填入面表的信誉值个数："+No1.ToString()+"\r\n 填入线表的信誉值个数："+No2.ToString()));
            MessageBox.Show("操作完成！");
        }
         /// <summary>
        /// 将outTableName的目标、用户信誉值插入inTableName表中
        /// </summary>
        /// <param name="inTableName"></param>
        /// <param name="outTableName"></param>
        /// <param name="osmCon"></param>
        /// <returns></returns>
        public int[] fillReputationField(string inTableName, string outTableName, OracleConnection osmCon)
        {
            int[] noFilledRep = { 0, 0 };

            string mySQL1 = "update " + inTableName + " set oreputation=" + outTableName + ".trustvalue from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version=" + outTableName + ".version and " + outTableName + ".trustvalue>=0 and "+ outTableName +".versionsub=0;";
            string mySQL2 = "update " + inTableName + " set ureputation=" + outTableName + ".userreputation from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version=" + outTableName + ".version and " + outTableName + ".userreputation>=0 and " + outTableName + ".versionsub=0;";
            try
            {
                
                int No1 = 0;
                int No2 = 0;
                No1+=changeTable(mySQL1, osmCon);
                mySQL1 = "update " + inTableName + " set oreputation=" + outTableName + ".trustvalue from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version=" + outTableName + ".version and " + outTableName + ".trustvalue>=0 and " + outTableName + ".versionsub!=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No1 += changeTable(mySQL1, osmCon);
                mySQL1 = "update " + inTableName + " set oreputation=" + outTableName + ".trustvalue from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version+1=" + outTableName + ".version and " + outTableName + ".trustvalue>=0 and " + outTableName + ".versionsub=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No1 += changeTable(mySQL1, osmCon);
                mySQL1 = "update " + inTableName + " set oreputation=" + outTableName + ".trustvalue from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version+2=" + outTableName + ".version and " + outTableName + ".trustvalue>=0 and " + outTableName + ".versionsub=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No1 += changeTable(mySQL1, osmCon);

                No2 += changeTable(mySQL2, osmCon);
                mySQL2 = "update " + inTableName + " set ureputation=" + outTableName + ".userreputation from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version=" + outTableName + ".version and " + outTableName + ".userreputation>=0 and " + outTableName + ".versionsub!=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No2 += changeTable(mySQL2, osmCon);
                mySQL2 = "update " + inTableName + " set ureputation=" + outTableName + ".userreputation from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version+1=" + outTableName + ".version and " + outTableName + ".userreputation>=0 and " + outTableName + ".versionsub=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No2 += changeTable(mySQL2, osmCon);
                mySQL2 = "update " + inTableName + " set ureputation=" + outTableName + ".userreputation from " + outTableName + " where cast(" + inTableName + ".osmid as integer)=" + outTableName + ".id and " + inTableName + ".version+2=" + outTableName + ".version and " + outTableName + ".userreputation>=0 and " + outTableName + ".versionsub=0 and cast(" + inTableName + ".uid as integer)=" + outTableName + ".userid;";
                No2 += changeTable(mySQL2, osmCon);
                
                noFilledRep[0] = No1;
                noFilledRep[1] = No2; 
                return noFilledRep;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                noFilledRep[0]=-1;
                noFilledRep[1]=-1;
                return noFilledRep;
            }

        }
        /// <summary>
        /// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回-1
        /// </summary>
        /// <param name="changeString"></param>
        /// <param name="conn"></param>
        public  int changeTable(string changeString, OracleConnection conn)
        {
            try
            {
                conn.Close();
                conn.Open();
                OracleCommand objCommand = new OracleCommand(changeString, conn);
                int affectedNo = objCommand.ExecuteNonQuery();
                conn.Close();
                return affectedNo;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                conn.Close();
                return -1;
            }

        }

        private void buttonShow_Click_1(object sender, EventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("数据库连接失败！请重新连接数据库");
                return;
            }
            //ReputationDataChoice tableChoice = new ReputationDataChoice();
            //tableChoice.ShowDialog();
            //if (tableChoice.DialogResult == DialogResult.OK)
            //{
                try
                {
                    //表格不自动生成行
                    //this.dataGridView1.AutoGenerateColumns = false;
                    string tableName = comboBox1.Text;
                    if (string.IsNullOrEmpty(tableName))
                    {
                        MessageBox.Show("未选中任何表！");
                        return;
                    }
                    else 
                    {
                        GetList(tableName);
                        Console.WriteLine(tableName+"表中的信誉度数据显示完成");
                    }
                    //switch (tableName)
                    //{
                        
                        //case "POLYLINESONVERSION":
                        //    sublistline = dal_polylinesonversion.GetList();
                        //    this.dataGridView1.DataSource = sublistline;
                        //    this.label2.Text = "共" + sublistline.Count() + "个";
                        //    break;
                        //case "POLYGONSONVERSION":
                        //    sublist = dal_polygonsonversion.GetList();
                        //    this.dataGridView1.DataSource = sublist;
                        //    this.label2.Text = "共" + sublist.Count() + "个";
                        //    break;
                        //case "RESIDENTIAL_AREA":
                        //    //List<polylinesonversion> lst = new List<polylinesonversion>();
                        //    //lst = GetList("RESIDENTIAL_AREA");
                        //    //this.dataGridView2.DataSource = listlineAll;
                        //    GetList("RESIDENTIAL_AREA");
                            
                        //    break;
                        //case "polygon":
                        //    listAll = dal_polygon.GetList();
                        //    this.dataGridView1.DataSource = listAll;
                        //    this.label2.Text = "共" + listAll.Count() + "个";
                        //    break;
                    //}
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("错误：" + ex.ToString());
                }
            //}
        }






        public void GetList( string tableName)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("目标编号");
            dataTable.Columns.Add("版本号");
            dataTable.Columns.Add("用户名");
            dataTable.Columns.Add("用户编号");
            dataTable.Columns.Add("用户信誉度");
            dataTable.Columns.Add("目标信誉度");

            //StringBuilder strSql = new StringBuilder("SELECT * FROM polylinesonversion where id=4825630");
            StringBuilder strSql = new StringBuilder("SELECT osmid,versionid,username,userid,userReputation,trustValue FROM " + tableName + " where trustvalue <>0 order by osmid,versionid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                //List<polylinesonversion> lst = new List<polylinesonversion>();
                while (dr.Read())
                {
                    DataRow row = dataTable.NewRow();
                    row["目标编号"] = dr[0];
                    row["版本号"] = dr[1];
                    row["用户名"] = dr[2];
                    row["用户编号"] = dr[3];
                    row["用户信誉度"] = dr[4];
                    row["目标信誉度"] = dr[5];
                    dataTable.Rows.Add(row);
                    //lst.Add(GetModel(dr));
                }
                //return lst;
               
                //Console.WriteLine(dataTable.Rows.Count);
                this.dataGridView1.DataSource = dataTable;
                this.label2.Text = "共" + dataTable.Rows.Count + "个";
                //return dataTable;
            }
        }

        //private List<polylinesonversion> GetList(OracleDataReader dr)
        //{
        //    List<polylinesonversion> lst = new List<polylinesonversion>();
        //    while (dr.Read())
        //    {
        //        lst.Add(GetModel(dr));
        //    }
        //    return lst;
        //}

        private polylinesonversion GetModel(OracleDataReader dr)
        {
            polylinesonversion model = new polylinesonversion();
            //model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            //model.versionsub = Convert.ToInt32(dr["versionsub"]);
            //model.versionfinal = Convert.ToInt32(dr["versionfinal"]);
            model.userid = Convert.ToInt64(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            try
            {
                if (dr["userReputation"].ToString() == "")
                {
                    model.userReputation = -1;
                }
                else
                {
                    model.userReputation = Convert.ToDouble(dr["userReputation"]);
                }

            }
            catch (System.Exception ex)
            {
                model.userReputation = -1;
            }

            //model.changeset = Convert.ToInt32(dr["changeset"]);
            //model.timestamp = Convert.ToDateTime(dr["starttime"]);
            //model.tags = Convert.ToString(dr["tags"]);
            //model.pointids = Convert.ToString(dr["pointsid"]);
            //model.points = Convert.ToString(dr["points"]);
            try
            {
                if (dr["trustValue"].ToString() == "")
                {
                    model.trustValue = -1;
                }
                else
                {
                    model.trustValue = Convert.ToDouble(dr["trustValue"]);
                }
            }
            catch (System.Exception ex)
            {
                model.areadiffsim = -1;
            }


            //try
            //{
            //    if (dr["areadiffsim"].ToString() == "")
            //    {
            //        model.areadiffsim = -1;
            //    }
            //    else
            //    {
            //        model.areadiffsim = Convert.ToDecimal(dr["areadiffsim"]);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    model.areadiffsim = -1;
            //}

            //try
            //{
            //    if (dr["shapediffsim"].ToString() == "")
            //    {
            //        model.shapediffsim = -1;
            //    }
            //    else
            //    {
            //        model.shapediffsim = Convert.ToDecimal(dr["shapediffsim"]);
            //    }


            //}
            //catch (System.Exception ex)
            //{
            //    model.shapediffsim = -1;
            //}

            return model;
        }














        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


        int count1 = 0;
        private void button2_Click_2(object sender, EventArgs e)
        {
            this.label5.Text = "正在计算基态和增量的可信度";
            OracleDBHelper conHelper = new OracleDBHelper();
            
            string[] sonVersionTable = { "polygonsonversion", "polylinesonversion" };
            foreach (string tableName in sonVersionTable)
            {
                string sql = string.Format("select count(*) from {0}", tableName);
                using (OracleDataReader dr = conHelper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        count1 += Convert.ToInt32(dr["count(*)"]);
                    }
                }
            }
            progressBar1.Minimum = 0;
            progressBar1.Maximum = count1*10;
            getTrustvalue("polygonsonversion", "RESIDENTIAL_AREA");
            getTrustvalue("polygonsonversion", "SOIL_AREA");
            getTrustvalue("polygonsonversion", "TRAFFIC_AREA");
            getTrustvalue("polygonsonversion", "VEGETATION_AREA");
            getTrustvalue("polygonsonversion", "WATER_AREA");
            Console.WriteLine("基态面赋值完成");
            getTrustvalue("polylinesonversion", "RESIDENTIAL_LINE");
            getTrustvalue("polylinesonversion", "SOIL_LINE");
            getTrustvalue("polylinesonversion", "TRAFFIC_LINE");
            getTrustvalue("polylinesonversion", "VEGETATION_LINE");
            getTrustvalue("polylinesonversion", "WATER_LINE");
            Console.WriteLine("基态线赋值完成");
            getTrustvalue("polygonsonversion", "RESIDENTIAL_NEWAREA");
            getTrustvalue("polygonsonversion", "SOIL_NEWAREA");
            getTrustvalue("polygonsonversion", "TRAFFIC_NEWAREA");
            getTrustvalue("polygonsonversion", "VEGETATION_NEWAREA");
            getTrustvalue("polygonsonversion", "WATER_NEWAREA");
            Console.WriteLine("增量面赋值完成");
            getTrustvalue("polylinesonversion", "RESIDENTIAL_NEWLINE");
            getTrustvalue("polylinesonversion", "SOIL_NEWLINE");
            getTrustvalue("polylinesonversion", "TRAFFIC_NEWLINE");
            getTrustvalue("polylinesonversion", "VEGETATION_NEWLINE");
            getTrustvalue("polylinesonversion", "WATER_NEWLINE");
            Console.WriteLine("增量线赋值完成");
            this.label5.Text = "计算完成";
        }

        public void getTrustvalue(string tableName,string eleTableName)
        {
            OracleDBHelper conHelper = new OracleDBHelper();

            string sql = string.Format("select osmid,versionid,trustvalue,userreputation from {0}", tableName);
            using (OracleDataReader dr = conHelper.queryReader(sql))
            {
                while (dr.Read())
                {
                    progressBar1.Value++;
                    sql = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", eleTableName, dr["osmid"], dr["versionid"]);
                    using (OracleDataReader dr1 = conHelper.queryReader(sql))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() != "0")
                            {//如果有相同osmid versionID 则直接插入
                                //OracleDBHelper helper = new OracleDBHelper();
                                sql = string.Format("update {0} set trustvalue= {1},userreputation={4} where osmid={2} and versionid={3}", eleTableName, dr["trustvalue"], dr["osmid"], dr["versionid"], dr["userreputation"]);
                                conHelper.sqlExecuteUnClose(sql);
                                Console.WriteLine("插入一行");
                            }
                        
                        }
                    }
                }
            }
        
        }





     
    }
}














