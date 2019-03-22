using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GISEditor.EditorTool.BasicClass;
using GIS.Converters.WellKnownText;
using GIS.Geometries;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace QualityControl
{
    class TopologyControl
    {
        public TopologyControl()
    {
        OracleDBHelper help = new OracleDBHelper();
            OracleConnection con = help.getOracleConnection();
            if (con.State == ConnectionState.Closed)
                con.Open();
            string sql = "select osmid, sdo_util.to_wktgeometry(shape) shape from ";
            using (OracleCommand conmand = new OracleCommand(sql, con))
            {
                using (OracleDataReader rd = conmand.ExecuteReader())
                {
                    while (rd.Read())//逐条读取
                    {
                        //DataRow dataTableRow = pointDataTable.NewRow();
                        //dataTableRow["osmid"] = rd["osmid"].ToString();
                        //dataTableRow["shape"] = rd["shape"].ToString();
                        //pointDataTable.Rows.Add(dataTableRow);
                    }
                    rd.Close();
                }
               
     }
}

    }
}
