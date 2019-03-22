using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustValueAndReputation
{
    public class osmDataTable
    {
        public osmDataTable()
        { }

        public osmDataTable(string tableName)
        {
            _tableName = tableName;
        }

        #region Model
        private string _tableName;
        /// <summary>
        /// TableName
        /// 数据库中表的名称
        /// </summary>
        public string TableName
        {
            set { _tableName = value; }
            get { return _tableName; }
        }
        #endregion Model
    }
}
