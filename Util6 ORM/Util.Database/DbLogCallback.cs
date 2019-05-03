/**********************************************************************************
* 程序说明：     数据库日志代理
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

namespace Util.Database
{


    public delegate void DbLogCallback(string info, string title , string logpath , string encoding );
    
}

