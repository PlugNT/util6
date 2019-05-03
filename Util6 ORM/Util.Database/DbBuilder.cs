/**********************************************************************************
* 代码说明：     数据库构造类
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Util.Database
{


    public class DbBuilder : IDisposable
    {
        private DbConfig _dbConfig;
        private DbConnection _connection;
        private DbDataAdapter _dadaadpter;
        private DbTransaction _transaction;
        private DbCommand _command;
        private bool _commited = false;
        private bool _isDispose = false;



        public DbBuilder(DbConfig dbconfig = null)
        {
            if (dbconfig == null)
            {
                dbconfig = DbConfig.Default;
            }
            _dbConfig = dbconfig;
            if (_dbConfig == null)
            {
                throw new NullReferenceException("默认配置连接字符串为空或未设置！");
            }
        }





        public bool IsDispose
        {
            get { return _isDispose; }
        }

        public void Dispose()
        {
            CloseConnection();
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }
            if (_dadaadpter != null)
            {
                _dadaadpter.Dispose();
                _dadaadpter = null;
            }
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            _isDispose = true;
        }


        public string ConnectionString
        {
            set
            {
                if (_connection == null)
                {
                    _connection = DbProvider.CreateConnection(_dbConfig);
                }
                _connection.ConnectionString = value;
            }
        }


        public string GetDbType()
        {
            return _dbConfig.DbType;
        }


        private bool _isKeepConnect = false;
        public bool IsKeepConnect
        {
            get { return _isKeepConnect; }
        }

        public DbBuilder KeepConnect(bool isKeepConnect =true)
        {
            _isKeepConnect = isKeepConnect;
            return this;
        }


        private void OpenConnection()
        {
            if (_connection == null)
            {
                _connection = DbProvider.CreateConnection(_dbConfig);
            }
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }


        private void OpenConnAndCreateCommand()
        {
            OpenConnection();
            if (_command == null)
            {
                _command = _connection.CreateCommand();
                _command.Connection = _connection;
            }
        }


        private void OpenConnAndCreateDadapter()
        {
            OpenConnection();
            if (_command == null)
            {
                _command = _connection.CreateCommand();
                _command.Connection = _connection;
            }
            if (_dadaadpter == null)
            {
                _dadaadpter = DbProvider.CreateDataAdapter(_dbConfig);
            }
        }


        private void ClearCommandParameters()
        {
            if (_command.Parameters.Count > 0)
            {
                _command.Parameters.Clear();
            }
            _command.CommandText = string.Empty;
            _command.CommandType = CommandType.Text;
        }





        public void BeginTransaction(bool isKeepConnect = true)
        {
            BeginTransaction(IsolationLevel.ReadCommitted, isKeepConnect);
        }


        public void BeginTransaction(IsolationLevel level, bool isKeepConnect = true)
        {
            _isKeepConnect = isKeepConnect;
            OpenConnAndCreateCommand();
            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction(level);
                _command.Transaction = _transaction;
            }
            _commited = false;
        }


        public void CommitTransaction(DbFlagExecutive dbFlags = DbFlagExecutive.Normal)
        {
            try
            {
                if (_connection != null && _connection.State != ConnectionState.Closed)
                {
                    if (_transaction != null && !_commited)
                    {
                        _transaction.Commit();
                        _commited = true;
                    }
                }
                if (dbFlags == DbFlagExecutive.CloseConnection)
                {
                    CloseConnection();
                }
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }
        public void RollbackTransaction(DbFlagExecutive dbFlags = DbFlagExecutive.Normal)
        {
            try
            {
                if (_connection != null && _connection.State != ConnectionState.Closed)
                {
                    if (_transaction != null && !_commited)
                    {
                        _transaction.Rollback();
                        _commited = true;
                    }
                }
                if (dbFlags == DbFlagExecutive.CloseConnection)
                {
                    CloseConnection();
                }
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }
        private void DisposeTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }



        public void CloseConnection()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
            _isKeepConnect = false;
        }


        private void DbLogCallback(string sql, IDataParameter[] parameters = null)
        {

            if (_dbConfig.LogCallback != null)
            {
                var isWriteLog = true;
                if (_dbConfig.LogKeywords != null && _dbConfig.LogKeywords.Count > 0)
                {
                    isWriteLog = _dbConfig.IsAndKeyword;
                    foreach (var item in _dbConfig.LogKeywords)
                    {
                        if (_dbConfig.IsAndKeyword)
                        {
                            if (!sql.Contains(item))
                            {
                                isWriteLog = false;
                                break;
                            }
                        }
                        else
                        {
                            if (sql.Contains(item))
                            {
                                isWriteLog = true;
                                break;
                            }
                        }
                    }
                }
                if (isWriteLog)
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        sql += "\r\n" + string.Join(",", parameters.Select(m => m.ParameterName + ":" + m.Value.ToString()));
                    }
                    _dbConfig.LogCallback(sql, DbConfig.DbLogPrefix + "DbBuilder", null, null);
                }
            }
            if (_dbConfig.VerifySafeSql != null)
            {
                var error = _dbConfig.VerifySafeSql(sql);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception(error);
                }
            }
        }



        public int GetMaxField(string fieldName, string tableName)
        {
            string strsql = "select max(" + fieldName + ") from " + tableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }


        public bool Exists(string strSql)
        {
            var obj = GetSingle(strSql);
            if (obj == null)
            {
                return false;
            }
            var count = 0;
            var strValue = obj.ToString();
            if (int.TryParse(strValue, out count))
            {
                return count > 0;
            }
            else
            {
                return strValue != string.Empty;
            }
        }


        public bool Exists(string strSql, params DbParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            return (obj == null) ? false : obj.ToString() != string.Empty;
        }

        public object GetSingle(string strSql)
        {
            try
            {
                DbLogCallback(strSql);
                OpenConnAndCreateCommand();
                _command.CommandText = strSql;
                object obj = _command.ExecuteScalar();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return null;
                }
                else
                {
                    return obj;
                }
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }

        public object GetSingle(string strSql, params DbParameter[] cmdParms)
        {
            try
            {
                DbLogCallback(strSql, cmdParms);
                OpenConnAndCreateCommand();
                PrepareCommandParameters(strSql, cmdParms);
                object obj = _command.ExecuteScalar();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return null;
                }
                else
                {
                    return obj;
                }
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }





        public DbDataReader GetDataReader(string strSql)
        {
            try
            {
                DbLogCallback(strSql);
                OpenConnAndCreateCommand();
                _command.CommandText = strSql;
                DbDataReader myReader = _command.ExecuteReader(CommandBehavior.CloseConnection);
                ClearCommandParameters();
                return myReader;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }


        public DataTable GetDataTable(string strSql)
        {
            return GetDataSet(strSql).Tables[0];
        }

        public DataSet GetDataSet(string strSql)
        {
            try
            {
                DbLogCallback(strSql);
                DataSet dataset = new DataSet();
                OpenConnAndCreateDadapter();
                _command.CommandText = strSql;
                _dadaadpter.SelectCommand = _command;
                _dadaadpter.Fill(dataset);
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return dataset;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }



        public DbDataReader GetDataReader(string strSql, params DbParameter[] cmdParms)
        {
            try
            {
                DbLogCallback(strSql, cmdParms);
                OpenConnAndCreateCommand();
                PrepareCommandParameters(strSql, cmdParms);
                DbDataReader myReader = _command.ExecuteReader(CommandBehavior.CloseConnection);
                ClearCommandParameters();
                return myReader;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }

        public DataTable GetDataTable(string strSql, params DbParameter[] cmdParms)
        {
            return GetDataSet(strSql, cmdParms).Tables[0];
        }


        public DataSet GetDataSet(string strSql, params DbParameter[] cmdParms)
        {
            try
            {
                DbLogCallback(strSql, cmdParms);
                DataSet dataset = new DataSet();
                OpenConnAndCreateDadapter();
                PrepareCommandParameters(strSql, cmdParms);
                _dadaadpter.SelectCommand = _command;
                _dadaadpter.Fill(dataset, "ds");
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return dataset;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }





        public object GetSingleByProcedure(string storedProcName)
        {
            return GetSingleByProcedure(storedProcName, null);
        }

        public object GetSingleByProcedure(string storedProcName, IDataParameter[] parameters)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateCommand();
                BuildQueryCommand(storedProcName, parameters);
                object obj = _command.ExecuteScalar();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return null;
                }
                else
                {
                    return obj;
                }
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }

        public DbDataReader GetDataReaderByProcedure(string storedProcName)
        {
            return GetDataReaderByProcedure(storedProcName, null);
        }

        public DbDataReader GetDataReaderByProcedure(string storedProcName, IDataParameter[] parameters)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateCommand();
                BuildQueryCommand(storedProcName, parameters);
                DbDataReader returnReader = _command.ExecuteReader(CommandBehavior.CloseConnection);
                ClearCommandParameters();
                return returnReader;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }


        public DataTable GetDataTableByProcedure(string storedProcName)
        {
            return GetDataTableByProcedure(storedProcName, null);
        }

        public DataTable GetDataTableByProcedure(string storedProcName, IDataParameter[] parameters)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateDadapter();
                DataSet dataSet = new DataSet();
                BuildQueryCommand(storedProcName, parameters);
                _dadaadpter.SelectCommand = _command;
                _dadaadpter.Fill(dataSet);
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return dataSet.Tables[0];
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }


        public DataSet GetDataSetByProcedure(string storedProcName)
        {
            return GetDataSetByProcedure(storedProcName, null, null, null, 0);
        }


        public DataSet GetDataSetByProcedure(string storedProcName, IDataParameter[] parameters)
        {
            return GetDataSetByProcedure(storedProcName, parameters, null, null, 0);
        }


        public DataSet GetDataSetByProcedure(string storedProcName, IDataParameter[] parameters, string setName)
        {
            return GetDataSetByProcedure(storedProcName, parameters, null, setName, 0);
        }



        public DataSet GetDataSetByProcedure(string storedProcName, IDataParameter[] parameters, DataSet dataSet, string setName, int Times)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateDadapter();
                if (dataSet == null)
                {
                    dataSet = new DataSet();
                }
                BuildQueryCommand(storedProcName, parameters);
                _dadaadpter.SelectCommand = _command;
                if (Times > 0)
                {
                    _dadaadpter.SelectCommand.CommandTimeout = Times;
                }
                if (setName == null || setName == string.Empty)
                {
                    _dadaadpter.Fill(dataSet);
                }
                else
                {
                    _dadaadpter.Fill(dataSet, setName);
                }
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return dataSet;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }







        public int ExecuteSql(string strSql)
        {
            try
            {
                DbLogCallback(strSql);
                OpenConnAndCreateCommand();
                _command.CommandText = strSql;
                int rows = _command.ExecuteNonQuery();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return rows;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }

        public int ExecuteSqlByTime(string strSql, int Times)
        {
            try
            {
                DbLogCallback(strSql);
                OpenConnAndCreateCommand();
                _command.CommandText = strSql;
                _command.CommandTimeout = Times;
                int rows = _command.ExecuteNonQuery();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return rows;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }


        public int ExecuteSqlTran(List<string> sqlList)
        {
            try
            {
                BeginTransaction(_isKeepConnect);
                int count = 0;
                for (int n = 0; n < sqlList.Count; n++)
                {
                    string strsql = sqlList[n];
                    DbLogCallback(strsql);
                    if (strsql.Trim().Length > 1)
                    {
                        _command.CommandText = strsql;
                        count += _command.ExecuteNonQuery();
                    }
                }
                CommitTransaction();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    DisposeTransaction();
                    CloseConnection();
                }
                return count;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    RollbackTransaction();
                    DisposeTransaction();
                    CloseConnection();
                }
                throw;
            }
        }






        public int ExecuteSqlTran(List<DbParamInfo> paramList)
        {
            try
            {
                BeginTransaction(_isKeepConnect);
                int count = 0;
                foreach (DbParamInfo para in paramList)
                {
                    string cmdText = para.SqlString;
                    DbParameter[] cmdParms = para.ParameterArray;
                    DbLogCallback(cmdText, cmdParms);
                    PrepareCommandParameters(cmdText, cmdParms);
                    var execResult = _command.ExecuteNonQuery();
                    if (para.IsVerifyExecResult && execResult <= 0)
                    {
                        RollbackTransaction();
                        ClearCommandParameters();
                        if (!_isKeepConnect)
                        {
                            DisposeTransaction();
                            CloseConnection();
                        }
                        return 0;
                    }
                    count += execResult;
                    _command.Parameters.Clear();
                }
                CommitTransaction();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    DisposeTransaction();
                    CloseConnection();
                }
                return count;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    RollbackTransaction();
                    DisposeTransaction();
                    CloseConnection();
                }
                throw;
            }
        }


        public int ExecuteSql(string strSql, params DbParameter[] cmdParms)
        {
            try
            {
                DbLogCallback(strSql, cmdParms);
                OpenConnAndCreateCommand();
                PrepareCommandParameters(strSql, cmdParms);
                int rows = _command.ExecuteNonQuery();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return rows;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }


        public int ExecuteSql(DbParamInfo dbParamInfo)
        {
            if (dbParamInfo == null)
            {
                return -1;
            }
            return ExecuteSql(dbParamInfo.SqlString, dbParamInfo.ParameterArray);
        }




        public int ExecuteProcedure(string storedProcName)
        {
            return ExecuteProcedure(storedProcName, null);
        }

        public int ExecuteProcedure(string storedProcName, IDataParameter[] parameters)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateCommand();
                BuildIntCommand(storedProcName, parameters, false);
                int rowsAffected = _command.ExecuteNonQuery();
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return rowsAffected;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }
        public int ExecuteProcedure(string storedProcName, IDataParameter[] parameters, out int returnValue)
        {
            try
            {
                DbLogCallback(storedProcName, parameters);
                OpenConnAndCreateCommand();
                returnValue = 0;
                BuildIntCommand(storedProcName, parameters, true);
                int rowsAffected = _command.ExecuteNonQuery();
                if (_command.Parameters.Contains("ReturnValue"))
                {
                    returnValue = (int)_command.Parameters["ReturnValue"].Value;
                }
                ClearCommandParameters();
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                return rowsAffected;
            }
            catch
            {
                if (!_isKeepConnect)
                {
                    CloseConnection();
                }
                throw;
            }
        }



        private void PrepareCommandParameters(string cmdText, DbParameter[] cmdParms)
        {
            _command.CommandText = cmdText;
            _command.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (DbParameter parameter in cmdParms)
                {
                    if (parameter != null)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        _command.Parameters.Add(parameter);
                    }
                }
            }
        }



        private void BuildQueryCommand(string storedProcName, IDataParameter[] parameters)
        {
            _command.CommandText = storedProcName;
            _command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                foreach (DbParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        _command.Parameters.Add(parameter);
                    }
                }
            }
        }


        private void BuildIntCommand(string storedProcName, IDataParameter[] parameters, bool hasReturnValue)
        {
            BuildQueryCommand(storedProcName, parameters);
            DbParameter dbpar = DbProvider.MakeIntParam(_dbConfig, "ReturnValue");
            if (hasReturnValue)
            {
                dbpar.Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(dbpar);
            }
        }


    }
}

