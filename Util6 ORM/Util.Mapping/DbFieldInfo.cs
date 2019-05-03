/**********************************************************************************
* 代码说明：     字段信息类
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
namespace Util.EntityMapping
{

    public class DbFieldInfo
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}

