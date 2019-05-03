/**********************************************************************************
* 程序说明：     数据库的信息类
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
namespace Util.Database
{
    public class DbConfig
    {
        

        protected string _ConnectionString;
        public string ConnectionString
        {
            get { return _ConnectionString; }
        }



        protected string _DbType;
        public string DbType
        {
            get { return _DbType; }
        }




        public bool IsMysql
        {
            get { return (DbType == "MySql.Data.MySqlClient"); }
        }

        public bool IsAccess
        {
            get { return (DbType == "System.Data.OleDb"); }
        }

        public bool IsMssql
        {
            get { return (DbType == "System.Data.SqlClient"); }
        }



        protected DbLogCallback _LogCallback = null;
        public DbLogCallback LogCallback
        {
            get { return _LogCallback; }
        }

        public Func<string,string> VerifySafeSql { get; set; }




        private bool _IsAndKeyword = false;
        internal bool IsAndKeyword
        {
            get { return _IsAndKeyword; }
        }

        protected List<string> _LogKeywords = null;
        public List<string> LogKeywords
        {
            get { return _LogKeywords; }
        }


        internal const string DbLogPrefix = "Sql_";


        public DbConfig(string tDbType, string tConnectionString, DbLogCallback tLogCallback = null, string tLogKeyword = null)
        {
            _DbType = tDbType;
            _ConnectionString = tConnectionString;

            _LogCallback = tLogCallback;            
            if (tLogKeyword != null)
            {
                _LogKeywords = new List<string>();
                if (!string.IsNullOrWhiteSpace(tLogKeyword))
                {
                    var arrAnd = tLogKeyword.Split('&');
                    if (arrAnd.Length >= 2)
                    {
                        _IsAndKeyword = true;
                        _LogKeywords.AddRange(arrAnd);
                    }
                    else
                    {
                        var arrOr = tLogKeyword.Split('|');
                        _LogKeywords.AddRange(arrOr);
                    }
                }
            }
        }


        private static readonly object lockHelper = new object();
        private static DbConfig _config = null;
        public static void UseDefaultConfig(DbConfig config)
        {
            _config = config;
        }


        public static DbConfig Default
        {
            get
            {
                if (_config == null)
                {
                    lock (lockHelper)
                    {
                        if (_config == null)
                        {
                            var dbConnection = System.Configuration.ConfigurationManager.ConnectionStrings["Default"];
                            _config = new DbConfig(dbConnection.ProviderName, dbConnection.ConnectionString);
                        }
                    }
                }
                return _config;
            }
        }

        


    }
}

