/**********************************************************************************
* ����˵����     ���ݿ��ǩ����
* �������ڣ�     2009.9.20
* �޸����ڣ�     2013.07.15
* ����������     agui 
* ��ϵ��ʽ��     mailto:354990393@qq.com  
* ��Ȩ���У�     www.util6.com 
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

