/**********************************************************************************
* ����˵����     ����������
* �������ڣ�     2014.05.15
* �޸����ڣ�     2014.05.15
* ����������     agui 
* ��ϵ��ʽ��     mailto:354990393@qq.com  
* ��Ȩ���У�     www.util6.com 
* ********************************************************************************/
using System;
namespace Util.Database
{
    public class AppSettings
    {
        public static string Default
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["Default"] ?? string.Empty; }
        }
        public static string DbLogMode
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DbLogMode"]; }
        }
        public static string DbLogKeyword
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["DbLogKeyword"]; }
        }

    }
}

