using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;

using MySql.Data.MySqlClient;
using MySql.Data.Types;
using GATUtils.Fix;

namespace GATUtils.Utilities
{
    public class DataTableUtils
    {
        public static DataTable GetTableFromResultSet(ref MySqlDataReader reader)
        {
            var dt = new DataTable();

            if (reader == null) 
                return null;


            dt.Columns.AddRange(_GetHeaderColumnsFromReader(reader));
            
            if (reader.HasRows)
            while (reader.Read())
            {
                DataRow dr = dt.NewRow();

                for (int i = 0; i < dt.Columns.Count; i++ )
                {
                    try
                    { 
                        dr[i] = reader.GetValue(i);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("MySqlDateTime"))
                        {
                            MySqlDateTime tempDate = reader.GetMySqlDateTime(i);

                            if (tempDate.Year == 0)
                            {
                                dr[i] = DBNull.Value;
                            }
                            else { dr[i] = tempDate.Value; }
                            continue;
                        }

                        throw;
                    }
                }
                //dr["SOURCE"] = source;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static DataTable GetTableFromResultSet(ref SqlDataReader reader)
        {
            var dt = new DataTable();

            if (reader == null)
                return null;

            dt.Columns.AddRange(_GetHeaderColumnsFromReader(reader));

            if (reader.HasRows)
                while (reader.Read())
                {
                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (reader.GetFieldType(i) == typeof(string))
                            dr[i] = _CleanValue(reader.GetValue(i).GetType() == typeof(DBNull) ? "" : (string)reader.GetValue(i));
                        else
                            dr[i] = reader.GetValue(i);
                    }
                    dt.Rows.Add(dr);
                }

            return dt;
        }

        public static DataRow GetDataRowFromFixMessage(string source, List<KeyValuePair<FixTag, string>> fieldMap, QuickFix.Message fixMsg)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(_GetHeaderListFromMap(fieldMap));
            DataRow dr = dt.NewRow();
            LoadDataRow(ref dr, fieldMap, fixMsg);
            dr["SOURCE"] = source;
            return dr;
        }

        public static DataTable GetEmptyDataTableFromHeaderFieldList(IEnumerable<string> columnHeaders)
        {
            DataTable dt = new DataTable();

            foreach (string column in columnHeaders)
            {
                dt.Columns.Add(column);
            }
            dt.Columns.Add("SOURCE");
            return dt;
        }

        public static string GetInsertQueryStringFromDataRow(DataRow dr, string db, string table)
        {
            StringBuilder fields = new StringBuilder();
            StringBuilder values = new StringBuilder();
            
            int itemCount = dr.ItemArray.Count();
            for (int i = 0; i < itemCount; i++)
            {
                string currentField = dr.Table.Columns[i].ToString();
                if (currentField.Equals("RECORDINSERTSTAMP"))
                        continue;
                fields.Append(string.Format("{0},", _FormatQueryField(currentField)));
                string value;
                string typeName = dr[i].GetType().Name;
                if (typeName.Contains("DateTime"))
                {
                    if (dr[i] == DBNull.Value)
                    {
                        value = "";
                    }
                    else
                    {
                        value = currentField.Equals("DATE") || currentField.Equals("EXPIRATIONDATE") ||
                            currentField.Equals("MATURITYDATE") || currentField.Equals("SETTLEDATE") ?
                            ((MySqlDateTime)dr[i]).GetDateTime().ToString("yyyy-MM-dd") :
                            ((MySqlDateTime)dr[i]).GetDateTime().ToString("yyyy-MM-dd H:mm:ss.ffff");
                    }
                }
                else { value = dr[i].ToString(); }
                values.Append(string.Format("{0},", _FormatQueryValue(value)));
            }

            return string.Format("INSERT IGNORE Into {2}.{3} ({0}) VALUES ({1});", fields.ToString().TrimEnd(','), values.ToString().TrimEnd(','), db, table);
        }

        public static void LoadDataRow(ref DataRow destinationRow, List<KeyValuePair<int, string>> fieldMap, string [] fields)
        {
            foreach (KeyValuePair<int, string> columnIndexToDbField in fieldMap)
            {
                destinationRow[columnIndexToDbField.Value] = fields[columnIndexToDbField.Key];
            }
        }

        public static void LoadDataRow(ref DataRow destinationRow, List<KeyValuePair<FixTag, string>> fieldMap, QuickFix.Message fixMessage)
        {
            foreach (KeyValuePair<FixTag, string> fixTagToDbField in fieldMap)
            {
                FixTag currentTag = fixTagToDbField.Key;
                string valueColumn = fixTagToDbField.Value;
                string value;

                if (fixMessage.isSetField((int)currentTag))
                {
                    value = fixMessage.getField((int)currentTag);
                    if (string.IsNullOrEmpty(destinationRow[valueColumn] as string))
                    {
                        if ((int)currentTag == (int)FixTag.MaturityMonthYear)
                        {
                            destinationRow[valueColumn] =
                                FixFieldValueConverter.FormatMaturityDate(value, FixFieldValueConverter.TryGetFixValue(FixTag.MaturityDay, fixMessage));
                        }
                        else
                        {
                            destinationRow[valueColumn] =
                                FixFieldValueConverter.Instance[currentTag, value, valueColumn.Contains("DATE")];
                        }
                    }
                }
                else if (fixMessage.getHeader().isSetField((int)currentTag))
                {
                    value = fixMessage.getHeader().getField((int)currentTag);
                    if (string.IsNullOrEmpty(destinationRow[valueColumn] as string))
                        destinationRow[valueColumn] = FixFieldValueConverter.Instance[currentTag, value, valueColumn.Contains("DATE")];
                }
            }

        }

        public static string[] GetUniqueColumnValues(DataTable srcTable, string colName)
        {
            if (!srcTable.Columns.Contains(colName)) return null;

            List<string> uniqueValues = new List<string>();
            foreach (DataRow row in srcTable.Rows)
            {
                string value = row[colName].ToString();
                if(!uniqueValues.Contains(value))
                    uniqueValues.Add(value);
            }
            return uniqueValues.ToArray();
        }

        private static string _FormatQueryField(string field)
        {
            return string.Format("{1}{0}{1}", field, ct_fieldWrapper);
        }

        private static string _FormatQueryValue(string value)
        {
            return string.Format("{1}{0}{1}", _CleanValue(value), ct_valueWrapper);
        }
        
        private static DataColumn[] _GetHeaderColumnsFromReader(MySqlDataReader reader)
        {
            int fieldCount = reader.FieldCount;

            DataColumn[] headers = new DataColumn[fieldCount+1];
            for (int i = 0; i < fieldCount; i++)
            {
                Type fieldType = reader.GetFieldType(i);
                headers[i] = new DataColumn(reader.GetName(i).ToUpper(), fieldType);
            }

            return headers;
        }

        private static DataColumn[] _GetHeaderColumnsFromReader(SqlDataReader reader)
        {
            int fieldCount = reader.FieldCount;

            DataColumn[] headers = new DataColumn[fieldCount + 1];
            for (int i = 0; i < fieldCount; i++)
            {
                Type fieldType = reader.GetFieldType(i);
                headers[i] = new DataColumn(reader.GetName(i).ToUpper(), fieldType);
            }

            return headers;
        }

        private static DataColumn[] _GetHeaderListFromMap(List<KeyValuePair<FixTag, string>> columnToDbFieldMap)
        {
            Dictionary<string, DataColumn> headers = new Dictionary<string, DataColumn>();

            bool foundSource = false;
            for (int i = 0; i < columnToDbFieldMap.Count; i++)//KeyValuePair<FixTag, string> columnIdxToDbField in columnToDbFieldMap)
            {
                string colName = columnToDbFieldMap[i].Value.ToUpper();
                DataColumn column = new DataColumn(colName);
                if (!headers.Keys.Contains(colName))
                {
                    headers.Add(colName, column);
                    if (colName.Equals("SOURCE"))
                        foundSource = true;
                }
            }

            if(!foundSource)
                headers.Add("SOURCE", new DataColumn("SOURCE"));
            return headers.Values.ToArray();
        } 

        private static string _CleanValue(string value)
        {
            value = value.Replace("\\", "");
            value = value.Replace(",", "");
            value = value.Replace("'", "");
            return value;
        }
        
        const string ct_fieldWrapper = "`";
        const string ct_valueWrapper = "'";
    }
}
