/**********************************************************************************
* ����˵����     �ֶ���Ϣ��
* �������ڣ�     2014.05.15
* �޸����ڣ�     2014.05.15
* ����������     agui 
* ��ϵ��ʽ��     mailto:354990393@qq.com  
* ��Ȩ���У�     www.util6.com 
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

