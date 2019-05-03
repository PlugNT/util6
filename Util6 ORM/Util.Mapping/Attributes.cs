/**********************************************************************************
* 代码说明：     业务逻辑属性类
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Util.EntityMapping
{

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string _TableName = string.Empty;
        public string TableName
        {
            get { return _TableName; }
            set { _TableName = value; }
        }

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }


        public static TableAttribute GetTableName(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attributes.Length > 0)
            {
                foreach (object obj in attributes)
                {
                    if (obj is TableAttribute)
                    {
                        return obj as TableAttribute;
                    }
                }
            }
            return null;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {

        private bool _IsPrimaryKey = false;
        public bool IsPrimaryKey
        {
            get { return _IsPrimaryKey; }
            set { _IsPrimaryKey = value; }
        }


        private bool _IsIdentity = false;
        public bool IsIdentity
        {
            get { return _IsIdentity; }
            set { _IsIdentity = value; }
        }




        private PropertyInfo _ModelProperty = null;
        public PropertyInfo ModelProperty
        {
            get { return _ModelProperty; }
            set { _ModelProperty = value; }
        }



        public static FieldAttribute GetFieldAttribute(PropertyInfo pInfo)
        {
            object[] attributes = pInfo.GetCustomAttributes(typeof(FieldAttribute), true);
            if (attributes.Length > 0)
            {
                foreach (object obj in attributes)
                {
                    FieldAttribute tmpField = obj as FieldAttribute;
                    tmpField.ModelProperty = pInfo;
                    return tmpField;
                }
            }
            return null;
        }


    }




}

