using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.UI.AdditionalTool;

namespace QualityControl
{
     public class RollBack
    {

        public static void rollback( string tableName,string time)
        {
            if (CalculateThreshold()>0)
            {
                string tableName_copy = tableName + "_copy";
                OracleDBHelper.ExecuteSql("create table " + tableName_copy + " as select * from " + tableName + " as of timestamp to_timestamp('" + time + "','yyyy-mm-dd hh24:mi:ss')");
                OracleDBHelper.ExecuteSql("drop table  " + tableName);
                OracleDBHelper.ExecuteSql("rename " + tableName_copy + " to " + tableName);
            }
            
        }
        public static double CalculateThreshold()
        {
            double accuracy = (AttributeAccuracy.ratio + PositionAccuracy.ratio + UtilizationRatio.ratioSpec + UtilizationRatio.ratioTrust)/4;
            return accuracy;
        }
    }
}
