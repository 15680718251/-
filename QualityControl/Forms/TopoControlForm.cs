using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using GIS.UI.Forms;
using GIS.UI.AdditionalTool;
using QualityControl.Topo;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using GIS.UI.AdditionalTool;
using System.Threading;
/*
 * 拓扑计算的界面设计
 * zh编写
 * 2018年9月19日
 */

namespace QualityControl.Forms
{
    public partial class TopoControlForm : Form
    {
        #region 变量
        private AxMapControl axMapControl1;//这是字段
        private AxTOCControl axTOCControl;
        IWorkspace workspace;//创建工作空间对象
        private string layer1;
        private string layer2;
        #endregion

        public TopoControlForm()
        {
            InitializeComponent();
        }

        public TopoControlForm(AxMapControl axMapControl1,AxTOCControl axTOCControl1)
        {
            InitializeComponent();
            this.axMapControl1 = axMapControl1;
            this.axTOCControl=axTOCControl1;
        }

        
        private void bt_readOracle_Click(object sender, EventArgs e)
        {
            string server = OSMDataBaseLinkForm.Server_;
            string user = OSMDataBaseLinkForm.User_;
            string password = OSMDataBaseLinkForm.Password_;
            string database = OSMDataBaseLinkForm.DataBase_;


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


            this.cbb_layer1.Items.Clear();
            this.cbb_layer2.Items.Clear();
            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象
            
            //获取图层名  
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                this.cbb_layer1.Items.Add(datasetName.Name);
                this.cbb_layer2.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
            
            MessageBox.Show("读取成功");
        }

        

        private void bt_confirm_Click(object sender, EventArgs e)
        {
            string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 1000, info);
            creatInsectTable();
            layer1 = this.cbb_layer1.Text.ToString();//增量数据
            layer2 = this.cbb_layer2.Text.ToString();//基态数据

            //两个下拉框都为空
            if (layer1.Equals("") && layer2.Equals(""))
            {
                MessageBox.Show("请选择要检测的图层！");
            }
            else if (!layer1.Equals("") && !layer2.Equals(""))
            {

                layer1 = layer1.Substring(7);
                layer2 = layer2.Substring(7);
                //为表建立索引
                OracleDBHelper db = new OracleDBHelper();
                try
                {
                    db.createIndex(layer1);
                    db.createIndex(layer2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("");
                }
                //判断目标的维数
                OracleConnection conn = db.getOracleConnection();
                TopoLineCalculation topo = new TopoLineCalculation();
                ArrayList idList1 = topo.getOSCIDList(conn, layer1);//获取增量数据所有id
                ArrayList idList2 = topo.getOSMIDList(conn, layer2);//获取基态数据的id
                int weishu1 = topo.getOSCDimension(conn, layer1, (string)idList1[0]);
                int weishu2 = topo.getOSMDimension(conn, layer2, (string)idList2[0]);

                if (weishu1 == 1 && weishu2 == 1)//线/线
                {
                    if (layer1.Equals(layer2)) new System.Threading.Thread(new System.Threading.ThreadStart(startTopoSameLineAndLineControl)).Start();//同类线要素
                    else new System.Threading.Thread(new System.Threading.ThreadStart(startTopoLineAndLineControl)).Start();//不同类线要素
                }
                else if (weishu1 == 2 && weishu2 == 2)//面/面
                {
                    new System.Threading.Thread(new System.Threading.ThreadStart(startTopoAreaAndAreaControl)).Start();
                }
                else if ((weishu1 == 1 && weishu2 == 2) || (weishu1 == 2 && weishu2 == 1))//线/面
                {
                    new System.Threading.Thread(new System.Threading.ThreadStart(startTopoLineAndAreaControl)).Start();
                }
                //执行完后加载增量图层
                //addLayer(layer2);

            }
            else
            {
                //只选择了一个图层
                //String layer = null;
                //if (layer1.Equals("")) layer = layer2;
                //else layer = layer1;
                MessageBox.Show("只选择了一个图层！");
            }

            #region 测试
            //TopoLineByAreaTest topo = new TopoLineByAreaTest();
            //OracleDBHelper db = new OracleDBHelper();
            //OracleConnection conn = db.getOracleConnection();
            //topo.getTopoByLineInsectArea(conn, "OSMLINE", "OSMAREA", "10162", "2");
            #endregion
        }

        //计算完毕后自动添加显示图层，非树形
        public void addLayer()
        {
            string tablename1 = layer1;
            string tablename2 = layer2;
            string[] tablelist = { tablename1, tablename2 };
            AddMap map = new AddMap();
            map.showMap(tablelist, axMapControl1);
        }

        //计算完毕后自动添加显示图层，非树形
        public void addLayer(string layer)
        {
            AddMap map = new AddMap();
            map.showMap(layer, axMapControl1);
        }

        //计算完毕后自动添加显示图层，树形,有错
        public void addLayerByTree()
        {
            string tablename1 = layer1;
            string tablename2 = layer2;
            string[] tablelist = { tablename1, tablename2 };
            string path = @"..\..\..\testfile";//获取文件路径
            AddMap addmap = new AddMap(axMapControl1, axTOCControl);//将两个控件传过去
            addmap.showMap(tablelist);
        }

        //判断arraylist中是否含有某个值
        public bool isContain(ArrayList list1,ArrayList list2, string s1,string s2)
        {
            for (int i = 0; i < list1.Count; i++)
            {
                if (((string)list1[i]).Equals(s2) && ((string)list2[i]).Equals(s1)) return true;
            }
            return false;
        }

        //高亮显示拓扑冲突的数据集
        public void highLightShow(ArrayList errorList, string layer)
        {
            OperateMap m_OperateMap = new OperateMap();
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer);
            AddMap map = new AddMap();
            map.showByAttibute(pFeatLyr, "objectid", "=", errorList,255,0,0);
            axMapControl1.Refresh();
        }

        //高亮显示拓扑冲突的数据集
        public void highLightShow(ArrayList errorList, string layer,int r,int g,int b)
        {
            OperateMap m_OperateMap = new OperateMap();
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer);
            AddMap map = new AddMap();
            map.showByAttibute(pFeatLyr, "objectid", "=", errorList, r, g, b);
            axMapControl1.Refresh();
        }

        //线程，计算线线不同要素拓扑冲突
        public void startTopoLineAndLineControl()
        {
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            TopoLineCalculation topo = new TopoLineCalculation();
            ArrayList idList1 = topo.getOSCIDList(conn, layer1);//获取增量数据所有id
            ArrayList idList2 = topo.getOSMIDList(conn, layer1, layer2);//获取基态数据与增量相交的id
            DataTable dt = new DataTable();
            dt.Columns.Add("FID", typeof(String));
            dt.Columns.Add("图层1ID", typeof(String));
            dt.Columns.Add("图层2ID", typeof(String));
            dt.Columns.Add("图层1属性", typeof(String));
            dt.Columns.Add("图层2属性", typeof(String));
            dt.Columns.Add("type", typeof(String));
            ArrayList errorList1 = new ArrayList();//错误拓扑集图层1
            ArrayList errorList2 = new ArrayList();//错误拓扑集图层2
            int fid = 0;

            #region 类别个数统计
            int countA = 0;
            int countB = 0;
            int countC = 0;
            int countD = 0;
            int countE = 0;
            int countF = 0;
            int countG = 0;
            int countH = 0;
            int countI = 0;
            int countJ = 0;
            int countK = 0;
            #endregion

            
            for (int i = 0; i < idList1.Count; i++)
            {
                for (int j = 0; j < idList2.Count; j++)
                {
                    //除去需要缓冲区判断的类型，纠正type
                    //ArrayList type = topo.getTopoByLineInsectBufferLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    //bool flag = true;
                    //for (int n = 0; n < type.Count; n++)
                    //{
                    //    if (((string)type[n]).Equals("E")) flag = false;
                    //}
                    //if (flag) type = topo.getTopoByLineInsectLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);

                    ArrayList type = topo.getTopoByLineInsectLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    string featrueA = topo.getNationeleName(conn, layer1, (string)idList1[i]);
                    string featrueB = topo.getNationeleName(conn, layer2, (string)idList2[j]);
                    for (int k = 0; k < type.Count; k++)
                    {
                        if (!(topo.isTrueTopo(conn, featrueA, featrueB, (string)type[k]) && topo.isTrueTopo(conn, featrueB, featrueA, (string)type[k])))//根据规则库判断线线是否拓扑错误
                        {
                            fid++;
                            errorList1.Add(idList1[i]);
                            errorList2.Add(idList2[j]);
                            string typelist = "";
                            for (int x = 0; x < type.Count; x++)
                            {
                                typelist += type[x];
                                switch ((string)type[x])
                                {
                                    case "A":
                                        countA++;
                                        break;
                                    case "B":
                                        countB++;
                                        break;
                                    case "C":
                                        countC++;
                                        break;
                                    case "D":
                                        countD++;
                                        break;
                                    case "E":
                                        countE++;
                                        break;
                                    case "F":
                                        countF++;
                                        break;
                                    case "G":
                                        countG++;
                                        break;
                                    case "H":
                                        countH++;
                                        break;
                                    case "I":
                                        countI++;
                                        break;
                                    case "J":
                                        countJ++;
                                        break;
                                    case "K":
                                        countK++;
                                        break;
                                }
                            }
                            dt.Rows.Add(fid,(string)idList1[i], (string)idList2[j], featrueA, featrueB, typelist);
                            break;
                        }
                    }
                }
                    #region 实时更新进度条
                    if (i == 1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                        this.Invoke(new SetState(SetMyState), idList1.Count, info);
                    }
                    else if (i % 2 == 0)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1}", i+1, idList1.Count);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i+1, info);
                    }
                    else if (i == idList1.Count-1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1} ", i+1, idList1.Count );
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i+1, info);
                    }
                    #endregion
                
            }
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "拓扑检测完成!\r\n"
                + "A类个数：" + countA + "\r\n"
                + "B类个数：" + countB + "\r\n"
                + "C类个数：" + countC + "\r\n"
                + "D类个数：" + countD + "\r\n"
                + "E类个数：" + countE + "\r\n"
                + "F类个数：" + countF + "\r\n"
                + "G类个数：" + countG + "\r\n"
                + "H类个数：" + countH + "\r\n"
                + "I类个数：" + countI + "\r\n"
                + "J类个数：" + countJ + "\r\n"
                + "K类个数：" + countK + "\r\n");
            addLayer();
            Thread.Sleep(5 * 1000);
            highLightShow(errorList1, layer1);
            highLightShow(errorList2, layer2);
            //MessageBox.Show("A类个数：" + countA + "\r\n"
            //    + "B类个数：" + countB + "\r\n"
            //    + "C类个数：" + countC + "\r\n"
            //    + "D类个数：" + countD + "\r\n"
            //    + "E类个数：" + countE + "\r\n"
            //    + "F类个数：" + countF + "\r\n"
            //    + "G类个数：" + countG + "\r\n"
            //    + "H类个数：" + countH + "\r\n"
            //    + "I类个数：" + countI + "\r\n"
            //    + "J类个数：" + countJ + "\r\n"
            //    + "K类个数：" + countK + "\r\n");
            showErrorData form = new showErrorData(axMapControl1,layer1,layer2,dt);
            form.ShowDialog();
            addLayer("INSECTLINE");
            addLayer("INSECTPOINT");
        }


        //线程，计算同类要素线线拓扑冲突
        public void startTopoSameLineAndLineControl()
        {
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            TopoLineCalculation topo = new TopoLineCalculation();
            ArrayList idList1 = topo.getOSCIDList(conn, layer1);//获取增量数据所有id
            DataTable dt = new DataTable();
            dt.Columns.Add("FID", typeof(String));
            dt.Columns.Add("图层1ID", typeof(String));
            dt.Columns.Add("图层2ID", typeof(String));
            dt.Columns.Add("图层1属性", typeof(String));
            dt.Columns.Add("图层2属性", typeof(String));
            dt.Columns.Add("type", typeof(String));
            ArrayList errorList1 = new ArrayList();//错误拓扑集图层1
            ArrayList errorList2 = new ArrayList();//错误拓扑集图层2
            int fid = 0;

            #region 类别个数统计
            int countA = 0;
            int countB = 0;
            int countC = 0;
            int countD = 0;
            int countE = 0;
            int countF = 0;
            int countG = 0;
            int countH = 0;
            int countI = 0;
            int countJ = 0;
            int countK = 0;
            #endregion


            for (int i = 0; i < idList1.Count; i++)
            {
                ArrayList idList2 = topo.getOSMIDList(conn,layer1,(string)idList1[i],layer2);
                for (int j = 0; j < idList2.Count; j++)
                {
                    if(((string)idList1[i]).Equals((string)idList2[j])) continue;
                    if (isContain(errorList1, errorList2, (string)idList1[i],(string)idList2[j])) continue;
                    
                    //除去需要缓冲区判断的类型，纠正type
                    //ArrayList type = topo.getTopoByLineInsectBufferLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    //bool flag = true;
                    //for (int n = 0; n < type.Count; n++)
                    //{
                    //    if (((string)type[n]).Equals("C")) flag = false;
                    //    if (((string)type[n]).Equals("D")) flag = false;
                    //    if (((string)type[n]).Equals("E")) flag = false;
                    //}
                    //if (flag) type = topo.getTopoByLineInsectLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]); 

                    ArrayList type = topo.getTopoByLineInsectLine(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    string featrueA = topo.getNationeleName(conn, layer1, (string)idList1[i]);
                    string featrueB = topo.getNationeleName(conn, layer2, (string)idList2[j]);
                    for (int k = 0; k < type.Count; k++)
                    {
                        if (
                            (((string)type[k]).Equals("A") && !featrueA.Equals(featrueB))||
                            (!((string)type[k]).Equals("A")&&featrueA.Equals(featrueB))
                            )//判断线线是否拓扑错误
                        {
                            fid++;
                            errorList1.Add(idList1[i]);
                            errorList2.Add(idList2[j]);
                            string typelist = "";
                            for (int x = 0; x < type.Count; x++)
                            {
                                typelist += type[x];
                                switch ((string)type[x])
                                {
                                    case "A":
                                        countA++;
                                        break;
                                    case "B":
                                        countB++;
                                        break;
                                    case "C":
                                        countC++;
                                        break;
                                    case "D":
                                        countD++;
                                        break;
                                    case "E":
                                        countE++;
                                        break;
                                    case "F":
                                        countF++;
                                        break;
                                    case "G":
                                        countG++;
                                        break;
                                    case "H":
                                        countH++;
                                        break;
                                    case "I":
                                        countI++;
                                        break;
                                    case "J":
                                        countJ++;
                                        break;
                                    case "K":
                                        countK++;
                                        break;
                                }
                            }
                            dt.Rows.Add(fid,(string)idList1[i], (string)idList2[j], featrueA, featrueB, typelist);
                            break;
                        }
                    }
                }
                    #region 实时更新进度条
                    if (i == 1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                        this.Invoke(new SetState(SetMyState), idList1.Count, info);
                    }
                    else if (i % 2 == 0)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1}", i + 1, idList1.Count);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                    }
                    else if (i == idList1.Count - 1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1} ", i + 1, idList1.Count);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                    }
                    #endregion
            }
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "拓扑检测完成!" + "\r\n"
                + "A类个数：" + countA + "\r\n"
                + "B类个数：" + countB + "\r\n"
                + "C类个数：" + countC + "\r\n"
                + "D类个数：" + countD + "\r\n"
                + "E类个数：" + countE + "\r\n"
                + "F类个数：" + countF + "\r\n"
                + "G类个数：" + countG + "\r\n"
                + "H类个数：" + countH + "\r\n"
                + "I类个数：" + countI + "\r\n"
                + "J类个数：" + countJ + "\r\n"
                + "K类个数：" + countK + "\r\n");
            addLayer(layer1);
            
            //MessageBox.Show("A类个数：" + countA + "\r\n"
            //    + "B类个数：" + countB + "\r\n"
            //    + "C类个数：" + countC + "\r\n"
            //    + "D类个数：" + countD + "\r\n"
            //    + "E类个数：" + countE + "\r\n"
            //    + "F类个数：" + countF + "\r\n"
            //    + "G类个数：" + countG + "\r\n"
            //    + "H类个数：" + countH + "\r\n"
            //    + "I类个数：" + countI + "\r\n"
            //    + "J类个数：" + countJ + "\r\n"
            //    + "K类个数：" + countK + "\r\n");
            Thread.Sleep(5 * 1000);
            for (int i = 0; i < errorList2.Count; i++)
            {
                errorList1.Add(errorList2[i]);
            }
            highLightShow(errorList1, layer1);
            //highLightShow(errorList2, layer2);
            showErrorData form = new showErrorData(axMapControl1, layer1, layer2, dt);
            form.ShowDialog();
            addLayer("INSECTLINE");
            addLayer("INSECTPOINT");
        }

        //线程，计算面面拓扑冲突
        public void startTopoAreaAndAreaControl()
        {
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            TopoAreaCalculation topo = new TopoAreaCalculation();
            ArrayList idList1 = topo.getOSCIDList(conn, layer1);//获取增量数据所有id
            ArrayList idList2 = topo.getOSMIDList(conn, layer1, layer2);//获取基态数据与增量相交的id
            DataTable dt = new DataTable();
            dt.Columns.Add("图层1ID", typeof(String));
            dt.Columns.Add("图层2ID", typeof(String));
            dt.Columns.Add("图层1属性", typeof(String));
            dt.Columns.Add("图层2属性", typeof(String));
            dt.Columns.Add("type", typeof(String));
            ArrayList errorList1 = new ArrayList();//错误拓扑集图层1
            ArrayList errorList2 = new ArrayList();//错误拓扑集图层2
            //类别个数统计
            int countA = 0;
            int countB = 0;
            int countC = 0;
            int countD = 0;
            int countE = 0;
            int countF = 0;
            int countG = 0;
            int countH = 0;
            int countI = 0;


            for (int i = 0; i < idList1.Count; i++)
            {
                for (int j = 0; j < idList2.Count; j++)
                {
                    ArrayList type = topo.getTopoByAreaInsectArea(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    string featrueA = topo.getNationeleName(conn, layer1, (string)idList1[i]);
                    string featrueB = topo.getNationeleName(conn, layer2, (string)idList2[j]);
                    for (int k = 0; k < type.Count; k++)
                    {
                        //if (!(topo.isTrueTopo(conn, featrueA, featrueB, (string)type[k]) && topo.isTrueTopo(conn, featrueB, featrueA, (string)type[k])))//判断面面是否拓扑错误
                        //{
                        errorList1.Add(idList1[i]);
                        errorList2.Add(idList2[j]);
                        string typelist = "";
                        for (int x = 0; x < type.Count; x++)
                        {
                            typelist += type[x];
                            switch ((string)type[x])
                            {
                                case "A":
                                    countA++;
                                    break;
                                case "B":
                                    countB++;
                                    break;
                                case "C":
                                    countC++;
                                    break;
                                case "D":
                                    countD++;
                                    break;
                                case "E":
                                    countE++;
                                    break;
                                case "F":
                                    countF++;
                                    break;
                                case "G":
                                    countG++;
                                    break;
                                case "H":
                                    countH++;
                                    break;
                                case "I":
                                    countI++;
                                    break;
                            }
                        }
                        dt.Rows.Add((string)idList1[i], (string)idList2[j], featrueA, featrueB, typelist);
                        break;
                        //}
                    }
                }
                    #region 实时更新进度条
                    if (i == 1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                        this.Invoke(new SetState(SetMyState), idList1.Count, info);
                    }
                    else if (i % 2 == 0)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1}", i + 1, idList1.Count);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                    }
                    else if (i == idList1.Count - 1)
                    {
                        string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1} ", i + 1, idList1.Count);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                    }
                    #endregion
                
            }
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "拓扑检测完成!" + "\r\n"
                + "A类个数：" + countA + "\r\n"
                + "B类个数：" + countB + "\r\n"
                + "C类个数：" + countC + "\r\n"
                + "D类个数：" + countD + "\r\n"
                + "E类个数：" + countE + "\r\n"
                + "F类个数：" + countF + "\r\n"
                + "G类个数：" + countG + "\r\n"
                + "H类个数：" + countH + "\r\n"
                + "I类个数：" + countI + "\r\n");
            addLayer();
            Thread.Sleep(5 * 1000);
            highLightShow(errorList1, layer1);
            highLightShow(errorList2, layer2);
            //MessageBox.Show("J类个数：" + countJ + "\r\n"
            //    + "K类个数：" + countK + "\r\n");
            showErrorData form = new showErrorData(axMapControl1,layer1,layer2,dt);
            form.ShowDialog();
            addLayer("INSECTAREA");
        }


        //线程，计算线面拓扑冲突
        public void startTopoLineAndAreaControl()
        {
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            TopoLineByAreaCalculation2 topo = new TopoLineByAreaCalculation2();
            ArrayList idList1 = topo.getOSCIDList(conn, layer1, layer2);//获取增量数据所有id
            DataTable dt = new DataTable();
            dt.Columns.Add("FID", typeof(String));
            dt.Columns.Add("图层1ID", typeof(String));
            dt.Columns.Add("图层2ID", typeof(String));
            dt.Columns.Add("图层1属性", typeof(String));
            dt.Columns.Add("图层2属性", typeof(String));
            dt.Columns.Add("type", typeof(String));
            ArrayList errorList1 = new ArrayList();//错误拓扑集图层1
            ArrayList errorList2 = new ArrayList();//错误拓扑集图层2
            int fid = 0;
            //
            #region 线面交线类型
            //-----------------------------
            int countA = 0;
            int countB = 0;
            int countC = 0;
            int countD = 0;
            int countE = 0;
            int countF = 0;
            int countG = 0;
            int countH = 0;
            int countI = 0;
            int countJ = 0;
            int countK = 0;
            int countL = 0;
            int countM = 0;
            int countN = 0;
            int countO = 0;
            int countP = 0;
            int countQ = 0;
            int countR = 0;
            int countS = 0;
            int countT = 0;
            int countU = 0;
            //-----------------------------
            int countV = 0;
            int countW = 0;
            #endregion

           
            for (int i = 0; i < idList1.Count; i++)
            {
                ArrayList idList2 = topo.getOSMIDList(conn, layer1, (string)idList1[i], layer2);//获取基态数据与增量相交的id
                for (int j = 0; j < idList2.Count; j++)
                {
                    ArrayList type = topo.getTopoByLineInsectArea(conn, layer1, layer2, (string)idList1[i], (string)idList2[j]);
                    
                    string featrueA = topo.getNationeleName(conn, layer1, (string)idList1[i]);
                    string featrueB = topo.getNationeleName(conn, layer2, (string)idList2[j]);
                    if (type.Count > 0)
                    {
                        fid++;
                        errorList1.Add((string)idList1[i]);
                        errorList2.Add((string)idList2[j]);
                        string typelist = "";
                        for (int x = 0; x < type.Count; x++)
                        {
                            typelist += type[x];
                            switch ((string)type[x])
                            {
                                case "A":
                                    countA++;
                                    break;
                                case "B":
                                    countB++;
                                    break;
                                case "C":
                                    countC++;
                                    break;
                                case "D":
                                    countD++;
                                    break;
                                case "E":
                                    countE++;
                                    break;
                                case "F":
                                    countF++;
                                    break;
                                case "G":
                                    countG++;
                                    break;
                                case "H":
                                    countH++;
                                    break;
                                case "I":
                                    countI++;
                                    break;
                                case "J":
                                    countJ++;
                                    break;
                                case "K":
                                    countK++;
                                    break;
                                case "L":
                                    countL++;
                                    break;
                                case "M":
                                    countM++;
                                    break;
                                case "N":
                                    countN++;
                                    break;
                                case "O":
                                    countO++;
                                    break;
                                case "P":
                                    countP++;
                                    break;
                                case "Q":
                                    countQ++;
                                    break;
                                case "R":
                                    countR++;
                                    break;
                                case "S":
                                    countS++;
                                    break;
                                case "T":
                                    countT++;
                                    break;
                                case "U":
                                    countU++;
                                    break;
                                case "V":
                                    countV++;
                                    break;
                                case "W":
                                    countW++;
                                    break;
                            }
                        }
                        dt.Rows.Add(fid, (string)idList1[i], (string)idList2[j], featrueA, featrueB, typelist);
                    }
                }
                #region 实时更新进度条
                if (i == 1)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                    this.Invoke(new SetState(SetMyState), idList1.Count, info);
                }
                else if (i % 2 == 0)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1}", i + 1, idList1.Count);
                    this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                }
                else if (i == idList1.Count - 1)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1} ", i + 1, idList1.Count);
                    this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                }
                #endregion

            }
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "拓扑检测完成!\r\n"
                + "A类个数：" + countA + "\r\n"
                + "B类个数：" + countB + "\r\n"
                + "C类个数：" + countC + "\r\n"
                + "D类个数：" + countD + "\r\n"
                + "E类个数：" + countE + "\r\n"
                + "F类个数：" + countF + "\r\n"
                + "G类个数：" + countG + "\r\n"
                + "H类个数：" + countH + "\r\n"
                + "I类个数：" + countI + "\r\n"
                + "J类个数：" + countJ + "\r\n"
                + "K类个数：" + countK + "\r\n"
                + "L类个数：" + countL + "\r\n"
                + "M类个数：" + countM + "\r\n"
                + "N类个数：" + countN + "\r\n"
                + "O类个数：" + countO + "\r\n"
                + "P类个数：" + countP + "\r\n"
                + "Q类个数：" + countQ + "\r\n"
                + "R类个数：" + countR + "\r\n"
                + "S类个数：" + countS + "\r\n"
                + "T类个数：" + countT + "\r\n"
                + "U类个数：" + countU + "\r\n"
                + "V类个数：" + countV + "\r\n"
                + "W类个数：" + countW + "\r\n");
            addLayer();
            Thread.Sleep(5 * 1000);
            highLightShow(errorList1, layer1);
            highLightShow(errorList2, layer2, 250, 215, 185);
            //MessageBox.Show("A类个数：" + countA + "\r\n"
            //    + "B类个数：" + countB + "\r\n"
            //    + "C类个数：" + countC + "\r\n"
            //    + "D类个数：" + countD + "\r\n"
            //    + "E类个数：" + countE + "\r\n"
            //    + "F类个数：" + countF + "\r\n"
            //    + "G类个数：" + countG + "\r\n"
            //    + "H类个数：" + countH + "\r\n"
            //    + "I类个数：" + countI + "\r\n"
            //    + "J类个数：" + countJ + "\r\n"
            //    + "K类个数：" + countK + "\r\n"
            //    + "L类个数：" + countL + "\r\n"
            //    + "M类个数：" + countM + "\r\n"
            //    + "N类个数：" + countN + "\r\n"
            //    + "O类个数：" + countO + "\r\n"
            //    + "P类个数：" + countP + "\r\n"
            //    + "Q类个数：" + countQ + "\r\n"
            //    + "R类个数：" + countR + "\r\n"
            //    + "S类个数：" + countS + "\r\n"
            //    + "T类个数：" + countT + "\r\n"
            //    + "U类个数：" + countU + "\r\n");
            showErrorData form = new showErrorData(axMapControl1, layer1, layer2, dt);
            form.ShowDialog();
            addLayer("INSECTLINE");
            addLayer("INSECTPOINT");
        }


        //线程，计算线面拓扑类型
        public void startTopoLineAndAreaType()
        {
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            TopoLineByAreaTest topo = new TopoLineByAreaTest();
            //ArrayList idList1 = topo.getOSCIDList(conn, layer1,layer2);//获取增量数据所有id
            DataTable dt = new DataTable();
            dt.Columns.Add("FID", typeof(String));
            dt.Columns.Add("图层1ID", typeof(String));
            dt.Columns.Add("图层2ID", typeof(String));
            dt.Columns.Add("图层1属性", typeof(String));
            dt.Columns.Add("图层2属性", typeof(String));
            dt.Columns.Add("type", typeof(String));
            ArrayList errorList1 = new ArrayList();//错误拓扑集图层1
            ArrayList errorList2 = new ArrayList();//错误拓扑集图层2
            int fid = 0;
            //
            #region 线面交线类型
            //-----------------------------
            int countA = 0;
            int countB = 0;
            int countC = 0;
            int countD = 0;
            int countE = 0;
            int countF = 0;
            int countG = 0;
            int countH = 0;
            int countI = 0;
            int countJ = 0;
            int countK = 0;
            int countL = 0;
            int countM = 0;
            int countN = 0;
            int countO = 0;
            int countP = 0;
            int countQ = 0;
            int countR = 0;
            int countS = 0;
            int countT = 0;
            int countU = 0;
            //-----------------------------
            int countV = 0;
            int countW = 0;
            #endregion

            int[] mian = { 2, 3, 4, 57, 58, 134, 135, 138, 139, 146, 186, 189, 206, 231, 232, 233, 278, 292, 297, 299, 300, 304, 564, 565, 614, 802 };
            for (int i = 0; i < mian.Length; i++)
            {
                string id1 = "" + mian[i];//线id
                //ArrayList idList2 = topo.getOSMIDList(conn, layer1,(string)idList1[i], layer2);//获取基态数据与增量相交的id
                for (int j = 1; j < 10169; j++)
                {
                    ArrayList type = topo.getTopoByLineInsectArea(conn, layer1, layer2, ""+j, id1);
                    //string featrueA = topo.getNationeleName(conn, layer1, (string)idList1[i]);
                    //string featrueB = topo.getNationeleName(conn, layer2, (string)idList2[j]);
                    string featrueA = "";
                    string featrueB = "";
                    if (type.Count > 0)
                    {
                        fid++;
                        errorList1.Add(id1);
                        errorList2.Add(j);
                        string typelist = "";
                        for (int x = 0; x < type.Count; x++)
                        {
                            typelist += type[x];
                            switch ((string)type[x])
                            {
                                case "A":
                                    countA++;
                                    break;
                                case "B":
                                    countB++;
                                    break;
                                case "C":
                                    countC++;
                                    break;
                                case "D":
                                    countD++;
                                    break;
                                case "E":
                                    countE++;
                                    break;
                                case "F":
                                    countF++;
                                    break;
                                case "G":
                                    countG++;
                                    break;
                                case "H":
                                    countH++;
                                    break;
                                case "I":
                                    countI++;
                                    break;
                                case "J":
                                    countJ++;
                                    break;
                                case "K":
                                    countK++;
                                    break;
                                case "L":
                                    countL++;
                                    break;
                                case "M":
                                    countM++;
                                    break;
                                case "N":
                                    countN++;
                                    break;
                                case "O":
                                    countO++;
                                    break;
                                case "P":
                                    countP++;
                                    break;
                                case "Q":
                                    countQ++;
                                    break;
                                case "R":
                                    countR++;
                                    break;
                                case "S":
                                    countS++;
                                    break;
                                case "T":
                                    countT++;
                                    break;
                                case "U":
                                    countU++;
                                    break;
                                case "V":
                                    countV++;
                                    break;
                                case "W":
                                    countW++;
                                    break;
                            }
                        }
                        dt.Rows.Add(fid, ""+id1, j, featrueA, featrueB, typelist);
                    }
                }
                #region 实时更新进度条
                if (i == 1)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                    this.Invoke(new SetState(SetMyState), mian.Length, info);
                }
                else if (i % 2 == 0)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1}", i + 1, mian.Length);
                    this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                }
                else if (i == mian.Length - 1)
                {
                    string info = string.Format("正在拓扑检测...\r\n\r\n    进度：{0}/{1} ", i + 1, mian.Length);
                    this.Invoke(new UpdateState(UpdateMyStateInitInfo), i + 1, info);
                }
                #endregion

            }
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "拓扑检测完成!\r\n"
                +"A类个数：" + countA + "\r\n"
                + "B类个数：" + countB + "\r\n"
                + "C类个数：" + countC + "\r\n"
                + "D类个数：" + countD + "\r\n"
                + "E类个数：" + countE + "\r\n"
                + "F类个数：" + countF + "\r\n"
                + "G类个数：" + countG + "\r\n"
                + "H类个数：" + countH + "\r\n"
                + "I类个数：" + countI + "\r\n"
                + "J类个数：" + countJ + "\r\n"
                + "K类个数：" + countK + "\r\n"
                + "L类个数：" + countL + "\r\n"
                + "M类个数：" + countM + "\r\n"
                + "N类个数：" + countN + "\r\n"
                + "O类个数：" + countO + "\r\n"
                + "P类个数：" + countP + "\r\n"
                + "Q类个数：" + countQ + "\r\n"
                + "R类个数：" + countR + "\r\n"
                + "S类个数：" + countS + "\r\n"
                + "T类个数：" + countT + "\r\n"
                + "U类个数：" + countU + "\r\n"
                + "V类个数：" + countV + "\r\n"
                + "W类个数：" + countW + "\r\n");
            addLayer();
            Thread.Sleep(10 * 1000);//有错误，加载图层是独立的线程，显示图层中的错误时可能图层没有加载完
            highLightShow(errorList1, layer1);
            highLightShow(errorList2, layer2);
            //MessageBox.Show("A类个数：" + countA + "\r\n"
            //    + "B类个数：" + countB + "\r\n"
            //    + "C类个数：" + countC + "\r\n"
            //    + "D类个数：" + countD + "\r\n"
            //    + "E类个数：" + countE + "\r\n"
            //    + "F类个数：" + countF + "\r\n"
            //    + "G类个数：" + countG + "\r\n"
            //    + "H类个数：" + countH + "\r\n"
            //    + "I类个数：" + countI + "\r\n"
            //    + "J类个数：" + countJ + "\r\n"
            //    + "K类个数：" + countK + "\r\n"
            //    + "L类个数：" + countL + "\r\n"
            //    + "M类个数：" + countM + "\r\n"
            //    + "N类个数：" + countN + "\r\n"
            //    + "O类个数：" + countO + "\r\n"
            //    + "P类个数：" + countP + "\r\n"
            //    + "Q类个数：" + countQ + "\r\n"
            //    + "R类个数：" + countR + "\r\n"
            //    + "S类个数：" + countS + "\r\n"
            //    + "T类个数：" + countT + "\r\n"
            //    + "U类个数：" + countU + "\r\n");
            showErrorData form = new showErrorData(axMapControl1, layer1, layer2, dt);
            form.ShowDialog();
            addLayer("INSECTLINE");
            addLayer("INSECTPOINT");
        }


        #region 进度条和提示lable的设置和更新委托
        public delegate void SetState(int maxValue, string info);
        public void SetMyState(int maxValue, string info)
        {
            this.probar_topo.Minimum = 0;
            this.probar_topo.Maximum = maxValue;
            this.resultBox.Text = info;
        }
        public delegate void UpdateState(int value, string info);
        public void UpdateMyState(int value, string info)
        {
            this.probar_topo.Value = value;
            this.resultBox.Text = this.resultBox.Text + info;
        }
        public void UpdateMyStateInitInfo(int value, string info)
        {
            this.probar_topo.Value = value;
            this.resultBox.Text = info;
        }
        #endregion

        private void TopoControlForm_Load(object sender, EventArgs e)
        {
            string server = OSMDataBaseLinkForm.Server_;
            string user = OSMDataBaseLinkForm.User_;
            string password = OSMDataBaseLinkForm.Password_;
            string database = OSMDataBaseLinkForm.DataBase_;


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


            this.cbb_layer1.Items.Clear();
            this.cbb_layer2.Items.Clear();
            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象

            //获取图层名  
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                this.cbb_layer1.Items.Add(datasetName.Name);
                this.cbb_layer2.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
        }

        //创建相交部分的表
        public void creatInsectTable()
        {
            OracleDBHelper db = new OracleDBHelper();
            string createTableSql = "(uniqueid NUMBER(30),layer1 VARCHAR2(30),layer2 VARCHAR2(30),objectid1 NUMBER(30),objectid2 NUMBER(30),shape SDO_GEOMETRY,feature1 VARCHAR2(30),feature2 VARCHAR2(30),type VARCHAR2(30))";
            db.createTable1("insectpoint", createTableSql);
            db.CreateSequenceSelfIncrease("insectpoint","uniqueid");
            db.createTable1("insectline", createTableSql);
            db.CreateSequenceSelfIncrease("insectline", "uniqueid");
            db.createTable1("insectarea", createTableSql);
            db.CreateSequenceSelfIncrease("insectarea", "uniqueid");
        }





    }
}
