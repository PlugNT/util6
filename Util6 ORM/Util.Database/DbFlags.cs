/**********************************************************************************
* 程序说明：     数据库标签常量
* 创建日期：     2009.9.20
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;

namespace Util.Database
{

    public enum DbFlagExecutive
    {
        Normal = 0,
        CloseConnection = 1
    }



    public class DbTypeLabel
    {



        public const string SqlClient = "System.Data.SqlClient";

        public const string OleDb = "System.Data.OleDb";

        public const string OracleClient = "System.Data.OracleClient";

    }


}

