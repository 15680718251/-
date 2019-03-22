using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrustValueAndReputation
{
    public partial class ReputationDataChoice : Form
    {
        private string tableName;

        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public ReputationDataChoice()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            try
            {
                tableName = this.checkedListBox1.SelectedItem.ToString();
            }
            catch (System.Exception ex)
            {
                tableName = null;
            }
            
        }

        private void ReputationDataChoice_Load(object sender, EventArgs e)
        {
            
            //List<string> TableNames ={"","","",""};
            string[] lst = { "POLYGONSONVERSION", "POLYLINESONVERSION", "RESIDENTIAL_AREA", "SOIL_AREA", "TRAFFIC_AREA", "VEGETATION_AREA", "WATER_AREA", "RESIDENTIAL_LINE", "SOIL_LINE", "TRAFFIC_LINE", "VEGETATION_LINE", "WATER_LINE", "RESIDENTIAL_NEWAREA", "SOIL_NEWAREA", "TRAFFIC_NEWAREA", "VEGETATION_NEWAREA", "WATER_NEWAREA", "RESIDENTIAL_NEWLINE", "SOIL_NEWLINE", "TRAFFIC_NEWLINE", "VEGETATION_NEWLINE", "WATER_NEWLINE" };
            //List<osmDataTable> lst=new List<osmDataTable>();
            //Dal_osmDataTable dal_osmDataTable=new Dal_osmDataTable();
            //lst=dal_osmDataTable.GetList("poly");
            foreach (string  model in lst)
            {
                checkedListBox1.Items.Add(model);
            }
                
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count > 0)
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (i != e.Index)
                    {
                        this.checkedListBox1.SetItemCheckState(i, System.Windows.Forms.CheckState.Unchecked);
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}
