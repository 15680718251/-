using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.TreeIndex.Forms;

namespace GIS.TreeIndex.Tool
{
    public class MapRemoveHolesTool:MapTool
    {
        string []strs;
        double area;
        public MapRemoveHolesTool(MapUI ui) : base(ui)
        {
            RemoveHoles();
        }

        private void RemoveHoles()
        {
            FrmRemoveHoles frh = new FrmRemoveHoles();
            if (frh.ShowDialog() == DialogResult.OK)
            {
                strs=frh.strPathNames;
                area=frh.darea;

                foreach (string str in strs)
                {
                    //ThreadPool.QueueUserWorkItem(dowork, new object[] { str, tmp });

                    try
                    {
                        GIS.HandleShpsAndImage.HandleRemoveHoles hrh = new GIS.HandleShpsAndImage.HandleRemoveHoles(str, area);
                        hrh.Work();
                        hrh.Dispose();

                    }
                    catch (Exception ex)
                    {
                        //m_MapUI.OutPutTextInfo(ex.ToString());
                    }
                    finally
                    {
                        //autoResetEvent.Set();
                    }
                }

                MessageBox.Show("执行完成","提示");
                frh.Close();
            }
        }
  
    }
}
