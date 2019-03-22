using System;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Text;
namespace GIS.UI.Forms
{
    public partial class OSMDataBaseLinkForm : Form
    {


        public string conString = null;
        public static string conStringTemp = null;
        public static bool OSMLinkSuccess = false;
        public static string Server_ = null;
        public static string User_ = null;
        public static string DataBase_ = null;
        public static string Password_ = null;
        public static string dataBasePara = null;

        public OSMDataBaseLinkForm()
        {
            InitializeComponent();
            string path = @"..\..\..\fileImport\rememberPassword.txt";

                string[] contents = File.ReadAllLines(path, Encoding.Default);//读取外部文件所有行的信息
                Server.Text = contents[0];
                Password.Text = contents[1];
                DataBase.Text = contents[2];


        }        


        public static IWorkspace workSpace = null;

       
        
        //连接数据库
        private void button2_Click(object sender, EventArgs e)
        {                      
            User_ = User.Text;
            Password_ = Password.Text;
            Server_ = Server.Text;
            DataBase_ = DataBase.Text;
            conStringTemp = String.Format("User ID={0};Password={1};Data Source=(DESCRIPTION = (ADDRESS_LIST= (ADDRESS = (PROTOCOL = TCP)(HOST = {2})(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME ={3})))", User_, Password_, Server_, DataBase_);
            dataBasePara = User_ + "/" + Password_ + "@" + Server_ + "/" + DataBase_;
            using (OracleConnection con = new OracleConnection(conStringTemp))
            {
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    OSMLinkSuccess = true;
                    conString = conStringTemp;
                    //单例模式，初始化oracle连接 zh 0705
                    OracleDBHelper db = new OracleDBHelper();
                    db.setOracleConnection(conStringTemp);
                    db.conString = dataBasePara;
                }

            } //by 丁洋修改


            IPropertySet propertySet = new PropertySetClass();
            propertySet.SetProperty("server", Server.Text);
           // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
            propertySet.SetProperty("INSTANCE", "sde:oracle11g:"+Server.Text+"/"+DataBase.Text);// by 丁洋修改
            propertySet.SetProperty("database", DataBase.Text);
            propertySet.SetProperty("user", User.Text);
            propertySet.SetProperty("password", Password.Text);
            propertySet.SetProperty("version", "SDE.DEFAULT");

            IWorkspaceFactory workspaceFactory = new SdeWorkspaceFactory();
             workSpace = workspaceFactory.Open(propertySet, 0);
            //con.Close();
            string path = @"..\..\..\fileImport\rememberPassword.txt";
            if (keepPasswordCkBox.Checked == true)
            {
                Server_ = Server.Text;
                Password_ = Password.Text;
                DataBase_ = DataBase.Text;
                System.IO.File.WriteAllText(path, string.Empty);//清空文件里面的内容
                //讲用户名和密码写入外部文件
                StreamWriter sw = new StreamWriter(path, true);
                sw.WriteLine(Server_);
                sw.WriteLine(Password_);
                sw.WriteLine(DataBase_);
                sw.Close();
            }
            else//如果没有记住密码，就清空内容
            {
                //System.IO.File.WriteAllText(path, string.Empty);//清空文件里面的内容
            }
            MessageBox.Show("连接数据库成功");
            this.Close();
        }        
        
        private void button1_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void keepPasswordCkBox_CheckedChanged(object sender, EventArgs e)
        {
            //string path = @"..\..\..\fileImport\rememberPassword.txt";
            //if (keepPasswordCkBox.Checked == true)
            //{

            //    Server_ = Server.Text;
            //    Password_ = Password.Text;
            //    DataBase_ = DataBase.Text;
            //    System.IO.File.WriteAllText(path, string.Empty);//清空文件里面的内容
            //    //讲用户名和密码写入外部文件
            //    StreamWriter sw = new StreamWriter(path, true);
            //    sw.WriteLine(Server_);
            //    sw.WriteLine(Password_);
            //    sw.WriteLine(DataBase_);
            //    sw.Close();
            //}
            //else//如果没有记住密码，就清空内容
            //{
            //    System.IO.File.WriteAllText(path, string.Empty);//清空文件里面的内容
            //}
        }      
        
    }
}

 