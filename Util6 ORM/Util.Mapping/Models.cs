/**********************************************************************************
* 代码说明：     实体参数
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Util.Database;
namespace Util.EntityMapping
{

    public class QueryInfo<T> 
    {
        public DbConfig Config { get; set; }

        public string SqlString { get; set; }
        public DbParameter[] Parameters { get; set; }

        public Func<IDataReader, T> GetModel { get; set; }
    }

    public class PageInfo
    {
        public DbConfig Config { get; set; }

        public DbParameter[] Parameters { get; set; }

        public string StrSelect { get; set; }
        public string StrFrom { get; set; }

        public string StrWhere { get; set; }
        public string StrOrder { get; set; }

        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public class PageInfo<T> : PageInfo
    {
        public Func<IDataReader, T> GetModel { get; set; }
    }


    
    public class SqlWhereItem
    {


        private bool _IsIdentity = false;
        public bool IsIdentity
        {
            get { return _IsIdentity; }
            set { _IsIdentity = value; }
        }


        private string _Name = string.Empty;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private object _Val = null;
        public object Val
        {
            get { return _Val; }
            set { _Val = value; }
        }


        private SqlWhereType _WhereType = SqlWhereType.And;
        public SqlWhereType WhereType
        {
            get { return _WhereType; }
            set { _WhereType = value; }
        }


        private SqlCompareType _CompareType = SqlCompareType.Equals;
        public SqlCompareType CompareType
        {
            get { return _CompareType; }
            set { _CompareType = value; }
        }


    }


}

