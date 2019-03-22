using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using GIS.Map;
using GIS.Layer;
using GIS.Geometries;


namespace GIS.Increment
{
  
        public  partial class IncManager
        {
            public void Visualable()
            {
                int sum = IncManagerfeatId.Count;

                for (int i = 0; i < sum; i++)
                {
                    GeoVectorLayer lyr = m_Map.GetLayerByName(IncManagerlyrName[i]) as GeoVectorLayer;

                    string filter = string.Format("FeatID = '{0}'", IncManagerfeatId[i]);
                    DataRow[] rowsFind = lyr.DataTable.Select(filter);

                    if (rowsFind.Length != 1)
                    {
                        continue;
                    }
                    GeoData.GeoDataRow row = rowsFind[0] as GeoData.GeoDataRow;
                    m_Map.AddSltObj(row);
                }

            }
        }
    
}
