using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GIS;

namespace GIS.TreeIndex.Forms
{
    public partial class PolygonSymbolizeFrm : Form
    {
        private MapUI m_mapui;           //便于绘画
        //public Symbolize SymbolValues;   //记录某字段特定值相关的要素属性
        private int aphaValue;         //记录符号化时的透明度值
        GIS.Layer.GeoVectorLayer lyr = null;
        Dictionary<string, Color> dic = new Dictionary<string, Color>();

        public PolygonSymbolizeFrm(MapUI ui)
        {
            InitializeComponent();
            m_mapui = ui;
        }

        //产生一种随机颜色
        public Color GetRandomColor(string str)
        {
            Color color;
            switch (str)
            {
                case "草地":
                    color = Color.FromArgb(255, 255, 128, 255);
                    break;
                case "森林":
                case "forest":
                    color = Color.FromArgb(255, 0, 255, 0);
                    break;
                case "灌木":
                    color = Color.FromArgb(255, 0, 255, 255);
                    break;
                case "耕地":
                case "agriculture":
                    color = Color.FromArgb(255, 255, 255, 0);
                    break;
                case "水体":
                case "river":
                    color = Color.FromArgb(255, 0, 0, 255);
                    break;
                case "人造覆盖":
                case "urban":
                    color = Color.FromArgb(255, 255, 0, 0);
                    break;
                case "裸地":
                    color = Color.FromArgb(255, 118, 143, 139);
                    break;
                case "湿地":
                case "sands":
                    color = Color.FromArgb(255, 255, 128, 255);
                    break;
                default:
                    Random RandomNum_First = new Random(Guid.NewGuid().GetHashCode());
                    Random RandomNum_Sencond = new Random(Guid.NewGuid().GetHashCode());

                    //  为了在白色背景上显示，尽量生成深色
                    int int_Red = RandomNum_First.Next(256);
                    int int_Green = RandomNum_Sencond.Next(256);
                    int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
                    int_Blue = (int_Blue > 255) ? 255 : int_Blue;
                    color = Color.FromArgb(255, int_Red, int_Green, int_Blue);//得到具有ARGB成分的颜色
                    break;
            }
            return color;
         
        }

        //在其中初始化combobox控件
        private void PolygonSymbolizeFrm_Load(object sender, EventArgs e)
        {
            //当前活动矢量图层
            lyr = m_mapui.GetActiveVectorLayer() as GIS.Layer.GeoVectorLayer;
            this.Text = "符号化：" + lyr.LayerName;

            int FieldCount=lyr.DataTable.Columns.Count;//矢量图层数据字段的个数
            for (int i = 0; i < FieldCount;++i )
            {
                string name = lyr.DataTable.Columns[i].Caption;
                if (name == "FID" || name == "FeatID" || name == "ClasID" || name == "BeginTime" || name == "ChangeType")
                    continue;

                this.m_cbxFiledNames.Items.Add(name);
            }

            // 直接加载已有的符号
            if (lyr.FileSymbol != null&&lyr.SymbolField!=null)
            {
                this.m_cbxFiledNames.Text = lyr.SymbolField;
                dic = lyr.FileSymbol;
                FillDataGridView(dic);

                this.aphaValueTrackBar.Value = lyr.apha;
                aphaValue = this.aphaValueTrackBar.Value;
            }
            //this.m_cbxFiledNames.SelectedIndex = 0;//默认选择第一项
        }
        
        private void m_cbxFiledNames_SelectedValueChanged(object sender, EventArgs e)
        {

            if (this.m_cbxFiledNames.Text == lyr.SymbolField)
            {
                FillDataGridView(lyr.FileSymbol);
            }
            else
            {
                FillDataGridView();
            }
        }

        //填充datagridview的行记录
        private void FillDataGridView()
        {
            //首先清除其中的所有记录
            if (this.m_dgvSymbolize.Rows.Count != 0)
            {
                this.m_dgvSymbolize.Rows.Clear();
            }

            dic.Clear();

            DataTable dt = lyr.DataTable;

            DataView dv = dt.DefaultView;
            DataTable ndt = new DataTable("Name");
            ndt = dv.ToTable(true, m_cbxFiledNames.Text.Trim());

            string FiledName = this.m_cbxFiledNames.SelectedItem.ToString();

            int RowCount = ndt.Rows.Count;
            for (int i = 0; i < RowCount; ++i)
            {
                string str = ndt.Rows[i][m_cbxFiledNames.Text.Trim()].ToString();

                Color clr = GetRandomColor(str);
                int index = m_dgvSymbolize.Rows.Add();

                m_dgvSymbolize.Rows[index].Cells["符号"].Value = " ";
                m_dgvSymbolize.Rows[index].Cells["属性"].Value = str;
                m_dgvSymbolize[0, index].Style.BackColor = clr;

                dic.Add(str, clr);
            }

            lyr.SymbolField = FiledName;
            lyr.FileSymbol = dic;
        }

        private void FillDataGridView(Dictionary<string, Color> dic)
        {
            //首先清除其中的所有记录
            if (this.m_dgvSymbolize.Rows.Count != 0)
            {
                this.m_dgvSymbolize.Rows.Clear();
            }

            int count = dic.Count;
            string FieldName = lyr.SymbolField;

            int index = 0;
            foreach (KeyValuePair<string, Color> values in dic)
            {
                index = m_dgvSymbolize.Rows.Add();
                m_dgvSymbolize.Rows[index].Cells[0].Value = "";
                Color clr = Color.FromArgb(255, values.Value);
                m_dgvSymbolize[0, index].Style.BackColor = clr;
                m_dgvSymbolize.Rows[index].Cells[1].Value = values.Key;
            }
        }

        //确定Button按钮
        private void m_btnSymbolize_Click(object sender, EventArgs e)
        {
            this.Close();
            lyr.bShowSymbol = true;
            lyr.SymbolField = m_cbxFiledNames.Text.Trim();
            lyr.FileSymbol = dic;
            lyr.apha = this.aphaValueTrackBar.Value;

            m_mapui.VecSymbolize = true;
            m_mapui.dic = this.dic;
            m_mapui.symField = m_cbxFiledNames.Text.Trim();

            m_mapui.Refresh();
            SaveSymbolFile(lyr.PathName);
        }

        //双击DataGridView重新设置单元格的背景色，即改变多边形符号化的颜色
        private void m_dgvSymbolize_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            colorDialog1.Color = m_dgvSymbolize.CurrentRow.Cells[0].Style.BackColor;
            if (DialogResult.OK == colorDialog1.ShowDialog())
            {
                m_dgvSymbolize.CurrentRow.Cells[0].Style.BackColor=colorDialog1.Color;
                string FieldValue = m_dgvSymbolize.CurrentRow.Cells[1].Value.ToString();
                int index =e.RowIndex;
                m_dgvSymbolize[0,index].Style.BackColor=Color.FromArgb(255,colorDialog1.Color);
                dic[FieldValue] = Color.FromArgb(this.aphaValueTrackBar.Value, colorDialog1.Color);
            }
        }
        
        //去掉选中的背景色
        private void m_dgvSymbolize_SelectionChanged(object sender, EventArgs e)
        {
            this.m_dgvSymbolize.ClearSelection();
            //this.m_dgvSymbolize.Invalidate();
        }
        
        //获取滑块的apha值 用来生成Color
        private void aphaValueTrackBar_Scroll(object sender, EventArgs e)
        {
            aphaValue = this.aphaValueTrackBar.Value;
            label3.Text = aphaValue.ToString();

            Dictionary<string, Color> temp = new Dictionary<string, Color>();

            foreach (KeyValuePair<string, Color> values in dic)
            {
                Color clr = Color.FromArgb(aphaValue, values.Value);
                temp.Add(values.Key, clr);
            }

            dic = temp;
            lyr.apha = aphaValue;
        }

        private void SaveSymbolFile(string str)
        {
            string strPath=str.Substring(0,str.Length-3)+"sym";

            if(lyr.FileSymbol!=null)
            {
                if (File.Exists(strPath))//若已经存在，首先删除原始符号文件
                {
                    File.Delete(strPath);
                }

                FileStream fs = new FileStream(strPath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(lyr.SymbolField);

                Dictionary<string, Color> dictionary = lyr.FileSymbol;
                foreach (KeyValuePair<string, Color> values in dictionary)
                {
                    Color clr=values.Value;
                    sw.WriteLine(values.Key+":"+clr.A.ToString()+":"+ clr.R.ToString()+":"+clr.G.ToString()+":"+clr.B.ToString());
                }
                sw.Close();
                fs.Close();

            }
        }


    }
}
