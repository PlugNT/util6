/**********************************************************************************
* 程序说明：     数据库提供类
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Util.Database
{
    public class DbProvider
    {
        
        public static DbDataAdapter CreateDataAdapter(DbConfig dbConfig)
        {
            DbDataAdapter dadaadpter = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dadaadpter = new System.Data.SqlClient.SqlDataAdapter();
                    break;
                case "System.Data.OleDb":
                    dadaadpter = new System.Data.OleDb.OleDbDataAdapter();
                    break;
                default:
                    break;
            }
            return dadaadpter;
        }
        
        public static DbConnection CreateConnection(DbConfig dbConfig)
        {
            DbConnection connection = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    connection = new System.Data.SqlClient.SqlConnection(dbConfig.ConnectionString);
                    break;
                case "System.Data.OleDb":
                    connection= new System.Data.OleDb.OleDbConnection(dbConfig.ConnectionString);
                    break;
                default:
                    break;
            }
            return connection;
        }
        

        public static DbParameter MakeParam(DbConfig dbConfig, string paraName, object objValue)
        {
            DbParameter dbParam = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, objValue);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, objValue);
                    break;
                default:
                    break;
            }
            return dbParam;
        }

        public static DbParameter MakeParam(DbConfig dbConfig, string paraName, DbType dType)
        {
            DbParameter dbParam = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, (SqlDbType)dType);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, (System.Data.OleDb.OleDbType)dType);
                    break;
                default:
                    break;
            }
            return dbParam;
        }

        public static DbParameter MakeParam(DbConfig dbConfig, string paraName, DbType dType, int dSize)
        {
            DbParameter dbParam = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, (SqlDbType)dType, dSize);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, (System.Data.OleDb.OleDbType)dType, dSize);
                    break;
                default:
                    break;
            }
            return dbParam;
        }

        public static DbParameter MakeIntParam(DbConfig dbConfig, string paraName)
        {
            DbParameter dbParam = null;
            switch (dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, SqlDbType.Int);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, System.Data.OleDb.OleDbType.Integer);
                    break;
                default:
                    break;
            }
            return dbParam;
        }




        private DbConfig _dbConfig = null;
        private List<DbParameter> dbParameters = null;
        public DbProvider(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
            dbParameters = new List<DbParameter>();
        }
        
        public DbProvider AppendParam(string paraName, object objValue)
        {
            DbParameter dbParam = null;
            switch (_dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, objValue);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, objValue);
                    break;
                default:
                    break;
            }
            dbParameters.Add(dbParam);
            return this;
        }

        public DbProvider AppendParam(string paraName, DbType dType)
        {
            DbParameter dbParam = null;
            switch (_dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, (SqlDbType)dType);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, (System.Data.OleDb.OleDbType)dType);
                    break;
                default:
                    break;
            }
            dbParameters.Add(dbParam);
            return this;
        }

        public DbProvider AppendParam(string paraName, DbType dType, int dSize)
        {
            DbParameter dbParam = null;
            switch (_dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, (SqlDbType)dType, dSize);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, (System.Data.OleDb.OleDbType)dType, dSize);
                    break;
                default:
                    break;
            }
            dbParameters.Add(dbParam);
            return this;
        }

        public DbProvider AppendParam(string paraName)
        {
            DbParameter dbParam = null;
            switch (_dbConfig.DbType)
            {
                case "System.Data.SqlClient":
                    dbParam = new System.Data.SqlClient.SqlParameter(paraName, SqlDbType.Int);
                    break;
                case "System.Data.OleDb":
                    dbParam = new System.Data.OleDb.OleDbParameter(paraName, System.Data.OleDb.OleDbType.Integer);
                    break;
                default:
                    break;
            }
            dbParameters.Add(dbParam);
            return this;
        }
        
        public DbParameter[] GetParameters()
        {
            return dbParameters.ToArray();
        }


    }
}

