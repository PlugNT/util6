/**********************************************************************************
* 程序说明：     数据库工厂类
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.Database
{

    public class TableInfo
    {
        public string Name;
        public List<ColumnInfo> ColList;
    }
    public class ColumnInfo
    {
        public string Name;
        public string TypeName;
        public string Description;
        public bool IsPrimaryKey;
        public bool IsIdentity;
    }



    public class DbFactory
    {

        public static List<TableInfo> GetShemaTables(DbConfig config =null)
        {
            if (config == null)
            {
                config = DbConfig.Default;
            }
            List<TableInfo> tables = new List<TableInfo>();
            DataTable shemaTables = null;
            int tabNameIndex = 0, tabRowCount = 0;
            if (config.DbType == "System.Data.OleDb")
            {
                System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection();
                conn.ConnectionString = config.ConnectionString;
                conn.Open();
                shemaTables = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                tabNameIndex = shemaTables.Columns.IndexOf("TABLE_NAME");
                tabRowCount = shemaTables.Rows.Count;
                for (int i = 0; i < tabRowCount; i++)
                {
                    DataRow tabDataRow = shemaTables.Rows[i];
                    string strTable = tabDataRow.ItemArray.GetValue(tabNameIndex).ToString();
                    DataTable dtColumns = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Columns, new object[] { null, null, strTable, null });
                    DataTable dtPrimarys = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Primary_Keys, new string[] { null, null, strTable });

                    int tColumnsCount = dtColumns.Rows.Count;
                    int tColumnNameIndex = dtColumns.Columns.IndexOf("COLUMN_NAME");
                    int tColumnTypeNameIndex = dtColumns.Columns.IndexOf("DATA_TYPE");

                    TableInfo tInfo = new TableInfo();
                    tInfo.Name = strTable;
                    tInfo.ColList = new List<ColumnInfo>();
                    for (int j = 0; j < tColumnsCount; j++)
                    {
                        DataRow drColumn = dtColumns.Rows[j];
                        ColumnInfo cInfo = new ColumnInfo();
                        cInfo.Name = drColumn.ItemArray.GetValue(tColumnNameIndex).ToString();
                        cInfo.TypeName = GetCSharpTypeByAccess(drColumn.ItemArray.GetValue(tColumnTypeNameIndex).ToString());
                        cInfo.Description = string.Empty;
                        foreach (DataRow row in dtPrimarys.Rows)
                        {
                            if (row["COLUMN_NAME"].ToString() == cInfo.Name)
                            {
                                cInfo.IsPrimaryKey = true;
                                cInfo.IsIdentity = true;
                                break;
                            }
                        }
                        tInfo.ColList.Add(cInfo);
                    }
                    tables.Add(tInfo);
                }
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            else
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection();
                conn.ConnectionString = config.ConnectionString;
                conn.Open();
                shemaTables = conn.GetSchema("Tables");
                DataTable dtColumns = conn.GetSchema("Columns");
                int colIndex = dtColumns.Columns.IndexOf("COLUMN_NAME");
                int typIndex = dtColumns.Columns.IndexOf("DATA_TYPE");
                tabNameIndex = shemaTables.Columns.IndexOf("TABLE_NAME");
                tabRowCount = shemaTables.Rows.Count;                

                string sqlColumns = "SELECT OBJECT_NAME(c.object_id) [TableName],c.name [ColumnName],ISNULL(ex.value,'') [DescriptionName] FROM sys.columns c LEFT OUTER JOIN sys.extended_properties ex ON ex.major_id = c.object_id AND ex.minor_id = c.column_id AND ex.name = 'MS_Description' WHERE OBJECTPROPERTY(c.object_id, 'IsMsShipped')=0";
                DataTable dtColumnDescriptions = new DataTable();
                var adapter = new System.Data.SqlClient.SqlDataAdapter(sqlColumns, conn);
                adapter.Fill(dtColumnDescriptions);

                sqlColumns = "select o.name as [TableName],c.name as [ColumnName] from sysindexes i  join sysindexkeys k on i.id = k.id and i.indid = k.indid  join sysobjects o on i.id = o.id  join syscolumns c on i.id=c.id and k.colid = c.colid  where o.xtype = 'U' and exists(select 1 from sysobjects where xtype = 'PK' and name = i.name)";
                DataTable dtColumnPrimaryKeys = new DataTable();
                adapter = new System.Data.SqlClient.SqlDataAdapter(sqlColumns, conn);
                adapter.Fill(dtColumnPrimaryKeys);
                
                sqlColumns = "select b.name as [TableName],a.name as [ColumnName],a.is_identity as [IsIdentity] from sys.columns a inner join sys.objects b on a.object_id=b.object_id ";
                DataTable dtColumnIdentitys = new DataTable();
                adapter = new System.Data.SqlClient.SqlDataAdapter(sqlColumns, conn);
                adapter.Fill(dtColumnIdentitys);

                for (int i = 0; i < tabRowCount; i++)
                {
                    DataRow itemDataRow = shemaTables.Rows[i];
                    string strTable = itemDataRow.ItemArray.GetValue(tabNameIndex).ToString();
                    DataRow[] drColumns = dtColumns.Select("TABLE_NAME='" + strTable + "'");
                    int drColumnsLength = drColumns.Length;
                    DataRow[] drDescriptions = dtColumnDescriptions.Select("TableName='" + strTable + "'");
                    DataRow[] drPrimaryKeys = dtColumnPrimaryKeys.Select("TableName='" + strTable + "'");
                    DataRow[] drIdentitys = dtColumnIdentitys.Select("TableName='" + strTable + "'");

                    TableInfo tInfo = new TableInfo();
                    tInfo.Name = strTable;
                    tInfo.ColList = new List<ColumnInfo>();
                    for (int j = 0; j < drColumnsLength; j++)
                    {
                        DataRow tmpDataRow = drColumns[j];

                        ColumnInfo cInfo = new ColumnInfo();
                        cInfo.Name = tmpDataRow.ItemArray.GetValue(colIndex).ToString();
                        cInfo.TypeName = GetCSharpTypeBySqlServer(tmpDataRow.ItemArray.GetValue(typIndex).ToString());
                        var tmpDescription = drDescriptions.FirstOrDefault(m => m["ColumnName"].ToString() == cInfo.Name);
                        cInfo.Description = (tmpDescription != null) ? tmpDescription["DescriptionName"].ToString() : string.Empty;
                        var tmpPrimaryKey = drPrimaryKeys.FirstOrDefault(m => m["ColumnName"].ToString() == cInfo.Name);
                        cInfo.IsPrimaryKey = (tmpPrimaryKey != null);
                        var tmpIsIdentity = drIdentitys.FirstOrDefault(m => m["ColumnName"].ToString() == cInfo.Name);
                        cInfo.IsIdentity = (tmpIsIdentity != null ? (tmpIsIdentity["IsIdentity"].ToString().ToLower() == "true" || tmpIsIdentity["IsIdentity"].ToString() == "1") : false);
                        tInfo.ColList.Add(cInfo);
                    }
                    tables.Add(tInfo);
                }
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            return tables;
        }

        private static string GetCSharpTypeBySqlServer(string typeName)
        {
            string rTypeName = typeName.ToLower();
            switch (rTypeName)
            {
                case "bigint": rTypeName = "long"; break;
                case "binary": rTypeName = "object"; break;
                case "bit": rTypeName = "bool"; break;
                case "char": rTypeName = "string"; break;
                case "date": rTypeName = "DateTime"; break;
                case "datetime": rTypeName = "DateTime"; break;
                case "decimal": rTypeName = "decimal"; break;
                case "float": rTypeName = "double"; break;
                case "image": rTypeName = "byte[]"; break;
                case "int": rTypeName = "int"; break;
                case "money": rTypeName = "decimal"; break;
                case "nchar": rTypeName = "string"; break;
                case "ntext": rTypeName = "string"; break;
                case "numeric": rTypeName = "decimal"; break;
                case "nvarchar": rTypeName = "string"; break;
                case "real": rTypeName = "float"; break;
                case "smalldatetime": rTypeName = "DateTime"; break;
                case "smallint": rTypeName = "short"; break;
                case "smallmoney": rTypeName = "decimal"; break;
                case "text": rTypeName = "string"; break;
                case "timestamp": rTypeName = "byte[]"; break;
                case "tinyint": rTypeName = "byte"; break;
                case "uniqueidentifier": rTypeName = "Guid"; break;
                case "varbinary": rTypeName = "byte[]"; break;
                case "varchar": rTypeName = "string"; break;
                case "xml": rTypeName = "string"; break;
                case "sql_variant": rTypeName = "object"; break;
            }
            return rTypeName;
        }
        private static string GetCSharpTypeByAccess(string typeName)
        {
            string rTypeName = string.Empty;
            switch (int.Parse(typeName))
            {
                case 2: rTypeName = "int"; break;
                case 3: rTypeName = "int"; break;
                case 4: rTypeName = "decimal"; break;
                case 5: rTypeName = "double"; break;
                case 6: rTypeName = "decimal"; break;
                case 7: rTypeName = "DateTime"; break;
                case 11: rTypeName = "bool"; break;
                case 17: rTypeName = "byte"; break;
                case 72: rTypeName = "string"; break;
                case 130: rTypeName = "string"; break;
                case 131: rTypeName = "decimal"; break;
                case 128: rTypeName = "string"; break;
                default: rTypeName = "string"; break;
            }
            return rTypeName;
        }


        public static void InsertSqlBulkCopy(DataTable table, int bulkCopyTimeout = 100, int? batchSize = null,DbConfig config = null)
        {
            if (config == null)
            {
                config = DbConfig.Default;
            }
            using (var connection = new System.Data.SqlClient.SqlConnection(config.ConnectionString))
            {
                connection.Open();
                var sqlbulkcopy = new System.Data.SqlClient.SqlBulkCopy(connection);
                sqlbulkcopy.BulkCopyTimeout = bulkCopyTimeout;
                sqlbulkcopy.BatchSize = (batchSize.HasValue) ? batchSize.Value : table.Rows.Count;
                sqlbulkcopy.DestinationTableName = table.TableName;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    sqlbulkcopy.ColumnMappings.Add(i, i);
                }
                sqlbulkcopy.WriteToServer(table);
                connection.Close();
                connection.Dispose();
            }
        }




        
    }

}

