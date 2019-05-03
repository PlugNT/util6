/**********************************************************************************
* 代码说明：     配置设置类
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
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

