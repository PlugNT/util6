/**********************************************************************************
* 代码说明：     对象映射助手
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using Util.Database;
namespace Util.EntityMapping
{

    public static class MapingHelper
    {
        
        public static List<T> GetList<T>(PageInfo<T> model)
        {
            if (string.IsNullOrWhiteSpace(model.StrSelect))
            {
                model.StrSelect = "*";
            }
            if (model.Config == null)
            {
                throw new Exception("Config不能为空！");
            }
            if (string.IsNullOrWhiteSpace(model.StrFrom))
            {
                throw new Exception("StrFrom不能为空！");
            }
            if (string.IsNullOrWhiteSpace(model.StrOrder))
            {
                throw new Exception("StrOrder不能为空！");
            }

            var getModel = model.GetModel;
            var list = new List<T>();
            using (var db = new DbBuilder(model.Config))
            {
                string sql = "select count(1) as tcount from (" + model.StrFrom + ") where 1=1 " + model.StrWhere;
                var recordCount = (int)new DbBuilder(model.Config).GetSingle(sql, model.Parameters);
                sql = SqlHelper.GetSqlString(model.StrFrom, model.PageSize, model.PageIndex, recordCount, "*", model.StrWhere, model.StrOrder, model.Config);
                var reader = db.GetDataReader(sql, model.Parameters);
                if (getModel != null)
                {
                    while (reader.Read())
                    {
                        list.Add(getModel(reader));
                    }
                }
                else
                {
                    var builder = EntityBuilder<T>.CreateBuilder(reader);
                    while (reader.Read())
                    {
                        list.Add(builder.Build(reader));
                    }
                }
            }
            return list;
        }


        public static List<T> GetList<T>(QueryInfo<T> model)
        {
            var getModel = model.GetModel;
            var list = new List<T>();
            using (var db = new DbBuilder(model.Config))
            {
                var reader = db.GetDataReader(model.SqlString, model.Parameters);
                if (getModel != null)
                {
                    while (reader.Read())
                    {
                        list.Add(getModel(reader));
                    }
                }
                else
                {
                    var builder = EntityBuilder<T>.CreateBuilder(reader);
                    while (reader.Read())
                    {
                        list.Add(builder.Build(reader));
                    }
                }
            }
            return list;
        }


        public static DataTable GetTable(PageInfo model)
        {
            if (string.IsNullOrWhiteSpace(model.StrSelect))
            {
                model.StrSelect = "*";
            }
            if (model.Config == null)
            {
                throw new Exception("Config不能为空！");
            }
            if (string.IsNullOrWhiteSpace(model.StrFrom))
            {
                throw new Exception("StrFrom不能为空！");
            }
            if (string.IsNullOrWhiteSpace(model.StrOrder))
            {
                throw new Exception("StrOrder不能为空！");
            }

            using (var db = new DbBuilder(model.Config))
            {
                string sql = "select count(1) as tcount from (" + model.StrFrom + ") where 1=1 " + model.StrWhere;
                var recordCount = (int)new DbBuilder(model.Config).GetSingle(sql, model.Parameters);
                sql = SqlHelper.GetSqlString(model.StrFrom, model.PageSize, model.PageIndex, recordCount, "*", model.StrWhere, model.StrOrder, model.Config);
                var table = db.GetDataTable(sql, model.Parameters);
                return table;
            }
        }


        public static T DeepClone<T>(T obj, bool isXmlSerializer = true) where T : class
        {
            if (obj == null)
            {
                return default(T);
            }
            using (Stream stream = new MemoryStream())
            {
                if (isXmlSerializer)
                {
                    var formatter = new XmlSerializer(typeof(T));
                    formatter.Serialize(stream, obj);
                    stream.Seek(0L, SeekOrigin.Begin);
                    return formatter.Deserialize(stream) as T;
                }
                else
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, obj);
                    stream.Seek(0L, SeekOrigin.Begin);
                    return formatter.Deserialize(stream) as T;
                }
            }
        }

    }
}

