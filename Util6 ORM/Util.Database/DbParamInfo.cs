/**********************************************************************************
* 程序说明：     Sql语句和参数类
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
    public class DbParamInfo
    {
        private bool _HasInsertIdentityKey;
        private bool _IsVerifyExecResult;
        private string _SqlString;
        private DbParameter[] _ParameterArray;

        public bool HasInsertIdentityKey
        {
            get
            {
                return _HasInsertIdentityKey;
            }
        }
        public bool IsVerifyExecResult
        {
            get
            {
                return _IsVerifyExecResult;
            }
        }
        public string SqlString
        {
            get
            {
                return _SqlString;
            }
        }
        public DbParameter[] ParameterArray
        {
            get
            {
                return _ParameterArray;
            }
        }
        
        public DbParamInfo(string sql, DbParameter[] parameters = null)
        {
            _SqlString = sql;
            _ParameterArray = parameters;
        }


        public DbParamInfo UseInsertIdentityKey(bool hasIdentity=true)
        {
            _HasInsertIdentityKey = hasIdentity;
            return this;
        }
        public DbParamInfo UseVerifyExecResult(bool isVerifyExecResult=true)
        {
            _IsVerifyExecResult = isVerifyExecResult;
            return this;
        }
    }
}

