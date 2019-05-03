/**********************************************************************************
* 代码说明：     SQL查询或更新条件类型
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.EntityMapping
{

    public enum SqlWhereType
    {
        And,
        Or
    }

    public enum SqlCompareType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEquals,
        LessThanOrEquals,
        Contains,
        StartsWith,
        EndsWith,
        In,
        NotIn
    }


}

