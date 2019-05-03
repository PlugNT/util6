/**********************************************************************************
* 程序说明：     数据库读写模块
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Database
{

    public class DbReadWrite
    {

        private int dbLogMode = 0;
        private DbConfig _DbReadConfig = null;
        private DbConfig _DbReadConfigLog = null;


        public DbConfig ReadConfig
        {
            get
            {
                if (dbLogMode == 1)
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        var dblog = System.Web.HttpContext.Current.Request["dblog"];
                        if (!string.IsNullOrWhiteSpace(dblog) && dblog == "1")
                        {
                            return _DbReadConfigLog;
                        }
                    }
                }
                if (dbLogMode == 2)
                {
                    return _DbReadConfigLog;
                }
                return _DbReadConfig;
            }
        }



        private DbConfig _DbWriteConfig = null;
        private DbConfig _DbWriteConfigLog = null;
        public DbConfig WriteConfig
        {
            get
            {
                if (dbLogMode == 1)
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        var dblog = System.Web.HttpContext.Current.Request["dblog"];
                        if (!string.IsNullOrWhiteSpace(dblog) && dblog == "1")
                        {
                            return _DbWriteConfigLog;
                        }
                    }
                }
                if (dbLogMode == 2)
                {
                    return _DbWriteConfigLog;
                }
                return _DbWriteConfig;
            }
        }
        
       

        public DbReadWrite(string keyRead, string keyWrite = null, DbLogCallback readLog= null, DbLogCallback writeLog=null)
        {
            if (string.IsNullOrWhiteSpace(keyWrite) || keyWrite.Trim().ToLower() == "null")
            {
                keyWrite = keyRead;
            }
            var dbread = System.Configuration.ConfigurationManager.ConnectionStrings[keyRead];
            var dbwrite = System.Configuration.ConfigurationManager.ConnectionStrings[keyWrite];

            _DbReadConfig = new DbConfig(dbread.ProviderName, dbread.ConnectionString);
            _DbWriteConfig = new DbConfig(dbwrite.ProviderName, dbwrite.ConnectionString);

            var tDbLogMode = AppSettings.DbLogMode;
            if (tDbLogMode != null)
            {
                tDbLogMode = tDbLogMode.Trim().ToLower();
                if (tDbLogMode == "web" || tDbLogMode == "1")
                {
                    dbLogMode = 1;
                }
                if (tDbLogMode == "all" || tDbLogMode == "2")
                {
                    dbLogMode = 2;
                }

                var logKeyword = AppSettings.DbLogKeyword;
                if (logKeyword != null)
                {
                    _DbReadConfigLog = new DbConfig(dbread.ProviderName, dbread.ConnectionString, readLog, logKeyword);
                    _DbWriteConfigLog = new DbConfig(dbwrite.ProviderName, dbwrite.ConnectionString, writeLog, logKeyword);
                }
            }
        }


    }
}

