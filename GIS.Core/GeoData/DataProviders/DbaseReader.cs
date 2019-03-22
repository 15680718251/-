using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace GIS.GeoData.DataProviders
{
    public class DbaseReader : IDisposable
    {
        private struct DBF_HEADER
        {
            /// <summary>
            /// 头文件版本
            /// </summary>
            public char vers;           
            // public DateTime lastUpdate;     //年月日
            /// <summary>
            /// 文件包含的记录数
            /// </summary>
            public Int32 no_recs; 
            /// <summary>
            /// 头文件的长度
            /// </summary>
            public Int16 head_len;
            /// <summary>
            /// 记录的长度
            /// </summary>
            public Int16 rec_len;
         
            //  public  Encoding _FileEncoding;//编码方式
        }
        private struct DBF_FIELD
        {
            /// <summary>
            /// 字段的名称
            /// </summary>
            public string ColumnName;
            /// <summary>
            /// 字段类型
            /// </summary>
            public Type DataType;
            
            public int Address;
            /// <summary>
            /// 数据项的精度
            /// </summary>
            public int Decimals; 
            /// <summary>
            /// 数据项的长度
            /// </summary>
            public int Length;   
        }
        private string m_FileName;
        private FileStream fs;
        private BinaryReader br; 
        private bool HeaderIsParsed;
        private bool m_IsOpen = false;
        private DBF_HEADER m_Header;
        private List<DBF_FIELD> DbaseColumns = new List<DBF_FIELD>();
        private GeoDataTable m_BaseTable;

        //bool bHasClasID = false;
        //bool bHasFeatID = false;
        //bool bHasBeginTime = false;

        public bool IsOpen
        {
            get { return m_IsOpen; }
            set { m_IsOpen = value; }
        }

        public DbaseReader(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("未能找到文件 \"{0}\" ", filename);
            m_FileName = filename;
            HeaderIsParsed = false;
        }
        public void Open()
        {
            try
            {
                fs = new FileStream(m_FileName, FileMode.Open, FileAccess.ReadWrite);
                br = new BinaryReader(fs);
                m_IsOpen = true;
                if (!HeaderIsParsed)
                    ParsedDbHeader(m_FileName);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void Close()
        {
            br.Close();
            fs.Close();
            IsOpen = false;
        }
        public void Dispose()
        {
            if (IsOpen)
                Close();
            br = null;
            fs = null;
        }
        private void ParsedDbHeader(string m_FileName)
        {
            try
            {
                m_Header.vers = br.ReadChar();
                br.ReadBytes(3);//跳过最近更新
                //m_Header.lastUpdate = new DateTime((int)br.ReadByte() + 1900, (int)br.ReadByte(), (int)br.ReadByte());
                m_Header.no_recs = br.ReadInt32(); //读取记录个数
                m_Header.head_len = br.ReadInt16();//读取文件头的长度
                m_Header.rec_len = br.ReadInt16(); //读取每条记录的长度
                // fs.Seek(29, SeekOrigin.Begin);     //跳过保留字段
                // m_Header._FileEncoding = GetDbaseLanguageDriver(br.ReadByte());//读取语言驱动
                fs.Seek(32, SeekOrigin.Begin);

                int NumberOfColumns = (m_Header.head_len - 31) / 32;
               
                //for (int i = 0; i < NumberOfColumns; ++i)
                //{
                //    string ColumnName = RebuildString(Encoding.UTF7.GetString(br.ReadBytes(11)).Trim()); //获取字段名
                //    if (ColumnName == "ClasID") //************************/判断 3个关键字段是否存在
                //        bHasClasID = true;
                //    else if (ColumnName == "FeatID")
                //        bHasFeatID = true;
                //    else if (ColumnName == "BeginTime")
                //        bHasBeginTime = true;
                //    fs.Seek(21, SeekOrigin.Current); //跳过保留字段
                //}

                //补齐字段
                DBF_FIELD field = new DBF_FIELD();
                field.ColumnName = "FID"; //获取字段名
                field.DataType = typeof(Int32);//获取字段类型       
                field.Length = 10;
                DbaseColumns.Add(field);
                fs.Seek(32, SeekOrigin.Begin);

                for (int i = 0; i < NumberOfColumns; ++i)
                {
                    DBF_FIELD field1 = new DBF_FIELD();
                    string ColumnName = RebuildString(Encoding.UTF7.GetString(br.ReadBytes(11)).Trim()); //获取字段名
                    field1.ColumnName = ColumnName;

                    field1.DataType = GetTypeByChar(br.ReadChar());//获取字段类型
                    field1.Address = br.ReadInt32();
                    field1.Length = (int)br.ReadByte();
                    if (field1.Length < 0) field1.Length += 256;
                    field1.Decimals = (int)br.ReadByte();
                    //如果DECIMAL= 0 ，DOUBLE型改为INT型
                    if (field1.Decimals == 0 && field1.DataType == typeof(double))
                    {
                        if (field1.Length <= 2)
                            field1.DataType = typeof(Int16);
                        else if (field1.Length <= 4)
                            field1.DataType = typeof(Int32);
                        else
                            field1.DataType = typeof(Int64);
                    }
                    DbaseColumns.Add(field1);
                    fs.Seek(14, SeekOrigin.Current); //跳过保留字段
                }

                /*

                if (!bHasFeatID)
                {
                    DBF_FIELD fieldFeatID = new DBF_FIELD();
                    fieldFeatID.ColumnName = "FeatID"; //获取字段名
                    fieldFeatID.DataType = typeof(string);//获取字段类型       
                    fieldFeatID.Length = 36;
                    DbaseColumns.Add(fieldFeatID);
                }
                if (!bHasClasID)
                {
                    DBF_FIELD fieldClasID = new DBF_FIELD();
                    fieldClasID.ColumnName = "ClasID"; //获取字段名
                    fieldClasID.DataType = typeof(Int64);//获取字段类型       
                    fieldClasID.Length = 10;
                    DbaseColumns.Add(fieldClasID);
                }
                if (!bHasBeginTime)
                {
                    DBF_FIELD fieldBeginTime = new DBF_FIELD();
                    fieldBeginTime.ColumnName = "BeginTime"; //获取字段名
                    fieldBeginTime.DataType = typeof(string);//获取字段类型       
                    fieldBeginTime.Length = 12;               
                    DbaseColumns.Add(fieldBeginTime);
                }

                */

                #region 20121008修改：添加UserID
                //DBF_FIELD fielduser = new DBF_FIELD();
                //fielduser.ColumnName = "UserID"; //获取字段名
                //fielduser.DataType = typeof(String);//获取字段类型       
                //fielduser.Length = 10;
                //DbaseColumns.Add(fielduser);
                #endregion

                #region 20121009修改：删除ChangeType列
                DBF_FIELD fieldChangeType = new DBF_FIELD();
                fieldChangeType.ColumnName = "ChangeType"; //获取字段名
                fieldChangeType.DataType = typeof(string);//获取字段类型       
                fieldChangeType.Length = 12;
                DbaseColumns.Add(fieldChangeType);  
                #endregion


                HeaderIsParsed = true;
                CreateBaseTable();
            }
            catch
            {
                throw new Exception(m_FileName + "文件头不正确");
            }
        }
        public int GetRecordNumber()
        {
            return m_Header.no_recs;
        }
        /// <summary>
        /// 创建新表，将列名赋值
        /// </summary>
        private void CreateBaseTable()
        {
            
            m_BaseTable = new GeoDataTable();
            foreach (DBF_FIELD field in DbaseColumns)
            {
                if (m_BaseTable.Columns.Contains(field.ColumnName))
                    m_BaseTable.Columns.Remove(field.ColumnName);

                m_BaseTable.Columns.Add(field.ColumnName, field.DataType);
            }
        }
        /// <summary>
        /// 创建与m_BaseTable字段结构一样的属性表
        /// </summary>
        /// <returns>返回与m_BaseTable字段结构一样的属性表</returns>
        internal GeoDataTable NewTable
        {
            get { return m_BaseTable.Clone(); }
        }
        private Type GetTypeByChar(char fieldtype)
        {
            switch (fieldtype)
            {
                case 'L':
                    return typeof(bool);    //布尔逻辑型

                case 'C':
                    return typeof(string);  //字符型

                case 'D':
                    return typeof(DateTime);//日期型

                case 'N':
                    return typeof(double);  //数值型

                case 'F':
                    return typeof(float);   //单精度浮点型

                case 'B':
                    return typeof(byte[]);  //二进制

                default:
                    throw (new NotSupportedException("Invalid or unknown DBase field type "));
            }
        }
        private Encoding GetDbaseLanguageDriver(byte dbasecode)
        {
            switch (dbasecode)
            {
                case 0x01:
                    return Encoding.GetEncoding(437); //DOS USA code page 437 
                case 0x02:
                    return Encoding.GetEncoding(850); // DOS Multilingual code page 850 
                case 0x03:
                    return Encoding.GetEncoding(1252); // Windows ANSI code page 1252 
                case 0x04:
                    return Encoding.GetEncoding(10000); // Standard Macintosh 
                case 0x08:
                    return Encoding.GetEncoding(865); // Danish OEM
                case 0x09:
                    return Encoding.GetEncoding(437); // Dutch OEM
                case 0x0A:
                    return Encoding.GetEncoding(850); // Dutch OEM Secondary codepage
                case 0x0B:
                    return Encoding.GetEncoding(437); // Finnish OEM
                case 0x0D:
                    return Encoding.GetEncoding(437); // French OEM
                case 0x0E:
                    return Encoding.GetEncoding(850); // French OEM Secondary codepage
                case 0x0F:
                    return Encoding.GetEncoding(437); // German OEM
                case 0x10:
                    return Encoding.GetEncoding(850); // German OEM Secondary codepage
                case 0x11:
                    return Encoding.GetEncoding(437); // Italian OEM
                case 0x12:
                    return Encoding.GetEncoding(850); // Italian OEM Secondary codepage
                case 0x13:
                    return Encoding.GetEncoding(932); // Japanese Shift-JIS
                case 0x14:
                    return Encoding.GetEncoding(850); // Spanish OEM secondary codepage
                case 0x15:
                    return Encoding.GetEncoding(437); // Swedish OEM
                case 0x16:
                    return Encoding.GetEncoding(850); // Swedish OEM secondary codepage
                case 0x17:
                    return Encoding.GetEncoding(865); // Norwegian OEM
                case 0x18:
                    return Encoding.GetEncoding(437); // Spanish OEM
                case 0x19:
                    return Encoding.GetEncoding(437); // English OEM (Britain)
                case 0x1A:
                    return Encoding.GetEncoding(850); // English OEM (Britain) secondary codepage
                case 0x1B:
                    return Encoding.GetEncoding(437); // English OEM (U.S.)
                case 0x1C:
                    return Encoding.GetEncoding(863); // French OEM (Canada)
                case 0x1D:
                    return Encoding.GetEncoding(850); // French OEM secondary codepage
                case 0x1F:
                    return Encoding.GetEncoding(852); // Czech OEM
                case 0x22:
                    return Encoding.GetEncoding(852); // Hungarian OEM
                case 0x23:
                    return Encoding.GetEncoding(852); // Polish OEM
                case 0x24:
                    return Encoding.GetEncoding(860); // Portuguese OEM
                case 0x25:
                    return Encoding.GetEncoding(850); // Portuguese OEM secondary codepage
                case 0x26:
                    return Encoding.GetEncoding(866); // Russian OEM
                case 0x37:
                    return Encoding.GetEncoding(850); // English OEM (U.S.) secondary codepage
                case 0x40:
                    return Encoding.GetEncoding(852); // Romanian OEM
                case 0x4D:
                    return Encoding.GetEncoding(936); // Chinese GBK (PRC)
                case 0x4E:
                    return Encoding.GetEncoding(949); // Korean (ANSI/OEM)
                case 0x4F:
                    return Encoding.GetEncoding(950); // Chinese Big5 (Taiwan)
                case 0x50:
                    return Encoding.GetEncoding(874); // Thai (ANSI/OEM)
                case 0x57:
                    return Encoding.GetEncoding(1252); // ANSI
                case 0x58:
                    return Encoding.GetEncoding(1252); // Western European ANSI
                case 0x59:
                    return Encoding.GetEncoding(1252); // Spanish ANSI
                case 0x64:
                    return Encoding.GetEncoding(852); // Eastern European MS朌OS
                case 0x65:
                    return Encoding.GetEncoding(866); // Russian MS朌OS
                case 0x66:
                    return Encoding.GetEncoding(865); // Nordic MS朌OS
                case 0x67:
                    return Encoding.GetEncoding(861); // Icelandic MS朌OS
                case 0x68:
                    return Encoding.GetEncoding(895); // Kamenicky (Czech) MS-DOS 
                case 0x69:
                    return Encoding.GetEncoding(620); // Mazovia (Polish) MS-DOS 
                case 0x6A:
                    return Encoding.GetEncoding(737); // Greek MS朌OS (437G)
                case 0x6B:
                    return Encoding.GetEncoding(857); // Turkish MS朌OS
                case 0x6C:
                    return Encoding.GetEncoding(863); // French朇anadian MS朌OS
                case 0x78:
                    return Encoding.GetEncoding(950); // Taiwan Big 5
                case 0x79:
                    return Encoding.GetEncoding(949); // Hangul (Wansung)
                case 0x7A:
                    return Encoding.GetEncoding(936); // PRC GBK
                case 0x7B:
                    return Encoding.GetEncoding(932); // Japanese Shift-JIS
                case 0x7C:
                    return Encoding.GetEncoding(874); // Thai Windows/MS朌OS
                case 0x7D:
                    return Encoding.GetEncoding(1255); // Hebrew Windows 
                case 0x7E:
                    return Encoding.GetEncoding(1256); // Arabic Windows 
                case 0x86:
                    return Encoding.GetEncoding(737); // Greek OEM
                case 0x87:
                    return Encoding.GetEncoding(852); // Slovenian OEM
                case 0x88:
                    return Encoding.GetEncoding(857); // Turkish OEM
                case 0x96:
                    return Encoding.GetEncoding(10007); // Russian Macintosh 
                case 0x97:
                    return Encoding.GetEncoding(10029); // Eastern European Macintosh 
                case 0x98:
                    return Encoding.GetEncoding(10006); // Greek Macintosh 
                case 0xC8:
                    return Encoding.GetEncoding(1250); // Eastern European Windows
                case 0xC9:
                    return Encoding.GetEncoding(1251); // Russian Windows
                case 0xCA:
                    return Encoding.GetEncoding(1254); // Turkish Windows
                case 0xCB:
                    return Encoding.GetEncoding(1253); // Greek Windows
                case 0xCC:
                    return Encoding.GetEncoding(1257); // Baltic Windows
                default:
                    return Encoding.UTF7;
            }
        }
        /// <summary>
        /// 将字符串截断至结束符'\0'
        /// </summary>
        /// <param name="str">传入字符串str,输出截断至结束符'\0'</param>
        private string RebuildString(string str)
        {
            int nPos = str.IndexOf('\0');
            if (nPos > 0)
                return str.Remove(nPos);
            else
                return str;
        }

        private object ReadDbfValue(DBF_FIELD dbf)
        {
            string temp = RebuildString(Encoding.Default.GetString(br.ReadBytes(dbf.Length))).Trim();

            switch (dbf.DataType.ToString())
            {
                case "System.String":
                    return temp;
                case "System.Single":
                    float flt;
                    if (float.TryParse(temp, out flt))
                        return flt;
                    return DBNull.Value;
                case "System.Double":
                    double dbl;
                    if (double.TryParse(temp, out dbl))
                        return dbl;
                    return DBNull.Value;
                case "System.Int16":
                    Int16 i16;
                    if (Int16.TryParse(temp, out i16))
                        return i16;
                    return DBNull.Value;
                case "System.Int32":
                    Int32 i32;
                    if (Int32.TryParse(temp, out i32))
                        return i32;
                    return DBNull.Value;
                case "System.Int64":
                    Int64 i64;
                    if (Int64.TryParse(temp, out i64))
                        return i64;
                    return DBNull.Value;
                case "System.Boolean":
                    return temp == "T" || temp == "t" || temp == "y" || temp == "Y";

                case "System.DateTime":
                    DateTime date;
                    if (DateTime.TryParseExact(temp, "yyyyMMdd", new CultureInfo("en-US", false).NumberFormat, DateTimeStyles.None, out date))
                        return date;
                    return DBNull.Value;
                default:
                    throw (new NotSupportedException("Cannot parse DBase field '" + dbf.ColumnName + "' of type '" +
                                                     dbf.DataType.ToString() + "'"));
            }
        }
        //不读属性数据
        internal GeoDataRow GetDataRow(GeoDataTable table)
        {
            if (!IsOpen)
                throw (new ApplicationException("DBF文件未打开"));
            GeoDataRow dr = table.NewRow();//创建dr与table结构相同             
            return dr;
        }
        /// <summary>
        /// 根据ID，将对应的记录添加到表TABLE中
        /// </summary>
        /// <param name="oid">记录的ID</param>
        /// <param name="table">需要添加的表TABLE</param>
        /// <returns>返回ID对应的相应的记录行，并添加到表TABLE中</returns>
        internal GeoDataRow GetDataRow(uint oid, GeoDataTable table)
        {
            if (!IsOpen)
                throw (new ApplicationException("DBF文件未打开"));
            if (oid < 0 || oid > m_Header.no_recs)
                throw new ArgumentOutOfRangeException("读取内容超出范围");

            
            GeoDataRow dr = table.NewRow();
            fs.Seek(m_Header.head_len + oid * m_Header.rec_len, 0);
            br.ReadChar();//略过标记是否已经删除 if (br.ReadChar() == '*')  //记录标记已经删除
            dr[0] = (int)oid;

            for (int i = 1; i < DbaseColumns.Count; i++)
            {
                DBF_FIELD dbf = DbaseColumns[i];
                try
                {
                    dr[dbf.ColumnName] = ReadDbfValue(dbf);
                }
                catch
                {
                    throw new Exception("读取第" + oid + "个空间目标的属性时发生错误");
                }
            }
            return dr;
        }
        internal GeoDataRow FillDataRow(uint oid, GeoDataTable table)
        {
            if (!IsOpen)
                throw (new ApplicationException("DBF文件未打开"));
            if (oid < 0 || oid > m_Header.no_recs)
                throw new ArgumentOutOfRangeException("读取内容超出范围");


            GeoDataRow dr = table[(int)oid];
            fs.Seek(m_Header.head_len + oid * m_Header.rec_len, 0);
            br.ReadChar();//略过标记是否已经删除 if (br.ReadChar() == '*')  //记录标记已经删除
            dr[0] = (int)oid;

            for (int i = 1; i < DbaseColumns.Count; i++)
            {
                DBF_FIELD dbf = DbaseColumns[i];
                try
                {
                    dr[dbf.ColumnName] = ReadDbfValue(dbf);
                }
                catch
                {
                    throw new Exception("读取第" + oid + "个空间目标的属性时发生错误");
                }
            }

            /*
            if (!bHasBeginTime)
            {
                DateTime time = DateTime.Today;
                string time1= string.Format("{0}{1:d2}{2:d2}", time.Year.ToString(),int.Parse( time.Month.ToString()), int.Parse(time.Day.ToString()));
                dr["BeginTime"] = time1;
            }
            if (!bHasClasID)
            {
                dr["ClasID"] = 0;
            }
            if (!bHasFeatID)
            {
               // dr["FeatID"] = (long)oid;
                dr["FeatID"] = System.Guid.NewGuid().ToString();
            }
             */ 

            #region 20121009修改：删除ChangeType列
            dr["ChangeType"] = EditState.Original;
            #endregion

            #region 20121009修改：添加UserIDea列
            //dr["UserID"] = "空";
            #endregion 
            return dr;
        }
    }
}
