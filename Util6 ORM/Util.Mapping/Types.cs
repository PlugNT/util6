/**********************************************************************************
* ����˵����     SQL��ѯ�������������
* �������ڣ�     2014.05.15
* �޸����ڣ�     2014.05.15
* ����������     agui 
* ��ϵ��ʽ��     mailto:354990393@qq.com  
* ��Ȩ���У�     www.util6.com 
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

