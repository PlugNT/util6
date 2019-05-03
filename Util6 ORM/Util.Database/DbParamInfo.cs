/**********************************************************************************
* ����˵����     Sql���Ͳ�����
* �������ڣ�     2009.9.20
* �޸����ڣ�     2013.07.15
* ����������     agui 
* ��ϵ��ʽ��     mailto:354990393@qq.com  
* ��Ȩ���У�     www.util6.com 
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

