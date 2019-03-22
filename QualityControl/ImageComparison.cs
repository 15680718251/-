using System;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;


namespace QualityControl
{
    public partial class ImageComparison : Form
    {

        public ImageComparison()
        {
            InitializeComponent();
            ss();
        }

        private AxMapControl axMapControl;

        public AxMapControl axMapControl_
        {
            get { return axMapControl; }
            set { axMapControl = value; }
        }

       
        public ImageComparison(AxMapControl axmapcontrol)
        {
            this.axMapControl_ = axmapcontrol;
            InitializeComponent();
        }
      

        public void ss()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ObjectId");
            dataTable.Columns.Add("PolyType");
            dataTable.Columns.Add("Coordinate");
            dataTable.Columns.Add("Length");
            dataTable.Columns.Add("Area");
            dataTable.Columns.Add("Accuracy");
            dataGridView1.DataSource = dataTable;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            
            this.FormClosed += new FormClosedEventHandler(ImageComparison_FormClosed);
            
        }

        void ImageComparison_FormClosed(object sender, FormClosedEventArgs e)
        {
            try 
            {
                select();
            }

            catch
            {
                throw new NotImplementedException();
            }
            
            
        }
        public void select()
        {
            IIdentify pIdentify = axMapControl_.Map.get_Layer(0) as IIdentify;
            IGeometry pGeo = axMapControl_.TrackRectangle() as IGeometry;
            IArray pIDArray;
            IIdentifyObj pIdObj;
            pIDArray = pIdentify.Identify(pGeo);
            string str = "\n";
            string lyrName = "";
            for (int i = 0; i < pIDArray.Count; i++)
            {
                pIdObj = pIDArray.get_Element(i) as IIdentifyObj;
                pIdObj.Flash(axMapControl_.ActiveView.ScreenDisplay);
                str += pIdObj.Name + "\n";
                lyrName = pIdObj.Layer.Name;
            }
            MessageBox.Show("Layer: " + lyrName + "\n" + "Feature: " + str);
        }

        
     }

        


  }

