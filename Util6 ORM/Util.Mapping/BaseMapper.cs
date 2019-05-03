/**********************************************************************************
* 代码说明：     数据映射操作基类
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Common;
using System.Reflection;

using Util.Database;
namespace Util.EntityMapping
{


    public abstract class BaseMapper<T> where T : class, new()
    {




        protected T ContextEntity { get; set; }

        protected virtual DbConfig DbReadConfig
        {
            get { return DbConfig.Default; }
        }

        protected virtual DbConfig DbWriteConfig
        {
            get { return DbConfig.Default; }
        }



        private string _tableName = null;
        [Newtonsoft.Json.JsonIgnore]
        public virtual string TableName
        {
            get
            {
                if (_tableName == null)
                {
                    var type = typeof(T);
                    var className = type.ToString();
                    var tabAttr = TableAttribute.GetTableName(type);
                    if (tabAttr == null)
                    {
                        throw new Exception("实体类" + className + "必须先设置TableAttribute属性！");
                    }
                    _tableName = tabAttr.TableName;
                }
                return _tableName;
            }
        }


        [Newtonsoft.Json.JsonIgnore]
        public string TableFrom { get; set; }




        private List<FieldAttribute> _fields = null;
        protected virtual List<DbFieldInfo> ConvertFields(T entity)
        {
            if (_fields == null)
            {
                _fields = new List<FieldAttribute>();
                var propertys = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var pInfo in propertys)
                {
                    var fAttr = FieldAttribute.GetFieldAttribute(pInfo);
                    if (fAttr != null)
                    {
                        _fields.Add(fAttr);
                    }
                }
            }
            var paList = new List<DbFieldInfo>();
            foreach (var field in _fields)
            {
                object value = field.ModelProperty.GetValue(entity, null);
                Type pType = field.ModelProperty.PropertyType;
                if (pType.IsValueType)
                {
                    if (value == null)
                    {
                        continue;
                    }
                }

                if (DbWriteConfig.IsAccess && value is DateTime)
                {
                    value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                }
                paList.Add(new DbFieldInfo
                {
                    IsPrimaryKey = field.IsPrimaryKey,
                    IsIdentity = field.IsIdentity,
                    Name = "@" + field.ModelProperty.Name,
                    Value = value
                });
            }
            return paList;
        }


        private string _allFields = null;
        private string _allFullFields = null;
        [Newtonsoft.Json.JsonIgnore]
        public virtual string AllFields
        {
            get
            {
                if (_allFields == null)
                {
                    if (_fields == null)
                    {
                        _fields = new List<FieldAttribute>();
                        var propertys = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var pInfo in propertys)
                        {
                            var fAttr = FieldAttribute.GetFieldAttribute(pInfo);
                            if (fAttr != null)
                            {
                                _fields.Add(fAttr);
                            }
                        }
                    }

                    var sbFieldBuffer = new StringBuilder();
                    foreach (var item in _fields)
                    {
                        sbFieldBuffer.Append(item.ModelProperty.Name + ",");
                    }
                    _allFields = sbFieldBuffer.ToString().TrimEnd(',');
                }
                return _allFields;
            }
        }

        public string GetJoinAllFields(string tabPrefixName)
        {
            return $"{tabPrefixName}." + AllFields.Replace(",", $",{tabPrefixName}.");
        }


        private DbBuilder _shareDbBuilder = null;
        private DbBuilder shareDbBuilder
        {
            get
            {
                if (_shareDbBuilder != null && _shareDbBuilder.IsDispose)
                {
                    _shareDbBuilder = null;
                }
                return _shareDbBuilder;
            }
        }
        public BaseMapper<T> UseDatabase(DbBuilder dbBuilder)
        {
            _shareDbBuilder = dbBuilder;
            return this;
        }

        public bool ContainsField(string field)
        {
            if (_allFullFields == null)
            {
                _allFullFields = "," + AllFields + ",";
            }
            return _allFullFields.Contains("," + field + ",");
        }

        protected virtual T BuildChildren(object obj = null)
        {
            return null;
        }


        private EntityBuilder<T> _builder = null;
        protected virtual T ConvertEntity(IDataReader reader)
        {
            if (_builder == null)
            {
                _builder = EntityBuilder<T>.CreateBuilder(reader);
            }
            return _builder.Build(reader);
        }

        protected List<T> GetList(IDataReader reader, bool isCloseConnection = true)
        {
            var list = new List<T>();
            while (reader.Read())
            {
                list.Add(ConvertEntity(reader));
            }
            if (isCloseConnection)
            {
                reader.Close();
            }
            return list;
        }



        private bool _isOrderBy = false;
        private StringBuilder _WhereBuilder = null;
        [Newtonsoft.Json.JsonIgnore]
        public StringBuilder WhereBuilder
        {
            get { return _WhereBuilder; }
        }

        private StringBuilder _SortBuilder = null;
        [Newtonsoft.Json.JsonIgnore]
        public StringBuilder SortBuilder
        {
            get { return _SortBuilder; }
        }

        private List<DbParameter> _Parameters = null;
        [Newtonsoft.Json.JsonIgnore]
        public List<DbParameter> Parameters
        {
            get { return _Parameters; }
        }

        private bool _IsPartHandled = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool IsPartHandled
        {
            get { return _IsPartHandled; }
        }

        private void ClearArguments()
        {
            _paraIndex = 1;
            _isOrderBy = false;
            if (_WhereBuilder == null)
            {
                _WhereBuilder = new StringBuilder();
            }
            else
            {
                _WhereBuilder.Clear();
            }
            if (_SortBuilder == null)
            {
                _SortBuilder = new StringBuilder();
            }
            else
            {
                _SortBuilder.Clear();
            }
            if (_Parameters == null)
            {
                _Parameters = new List<DbParameter>();
            }
            else
            {
                _Parameters.Clear();
            }
        }



        public BaseMapper<T> SortDesc<TResult>(Expression<Func<T, TResult>> field)
        {
            if (_SortBuilder == null)
            {
                _SortBuilder = new StringBuilder();
            }
            var fieldName = EntityBuilder.GetPropertyName<T, TResult>(field);
            if (_isOrderBy == false)
            {
                _SortBuilder.Append(string.Concat(" [", fieldName, "] desc"));
                _isOrderBy = true;
            }
            else
            {
                _SortBuilder.Append(string.Concat(",[", fieldName, "] desc"));
            }
            return this;
        }

        public BaseMapper<T> SortAsc<TResult>(Expression<Func<T, TResult>> field)
        {
            if (_SortBuilder == null)
            {
                _SortBuilder = new StringBuilder();
            }
            var fieldName = EntityBuilder.GetPropertyName<T, TResult>(field);
            if (_isOrderBy == false)
            {
                _SortBuilder.Append(string.Concat(" [", fieldName, "] asc"));
                _isOrderBy = true;
            }
            else
            {
                _SortBuilder.Append(string.Concat(",[", fieldName, "] asc"));
            }
            return this;
        }

        public BaseMapper<T> Query(Expression<Func<T, bool>> expression = null)
        {
            ClearArguments();
            var exp = new SqlLmdResolver(DbReadConfig);
            exp.ParaIndex = _paraIndex;
            exp.ResolveExpression(expression);
            _paraIndex = exp.ParaIndex;
            _WhereBuilder.Append(exp.SqlWhere);
            _Parameters.AddRange(exp.Parameters);
            return this;
        }

        public BaseMapper<T> SortDesc(string fieldName)
        {
            if (_SortBuilder == null)
            {
                _SortBuilder = new StringBuilder();
            }
            if (!ContainsField(fieldName))
            {
                throw new Exception("不存在字段" + fieldName);
            }
            if (_isOrderBy == false)
            {
                _SortBuilder.Append(string.Concat(" [", fieldName, "] desc"));
                _isOrderBy = true;
            }
            else
            {
                _SortBuilder.Append(string.Concat(",[", fieldName, "] desc"));
            }
            return this;
        }

        public BaseMapper<T> SortAsc(string fieldName)
        {
            if (_SortBuilder == null)
            {
                _SortBuilder = new StringBuilder();
            }
            if (!ContainsField(fieldName))
            {
                throw new Exception("不存在字段" + fieldName);
            }
            if (_isOrderBy == false)
            {
                _SortBuilder.Append(string.Concat(" [", fieldName, "] asc"));
                _isOrderBy = true;
            }
            else
            {
                _SortBuilder.Append(string.Concat(",[", fieldName, "] asc"));
            }
            return this;
        }
        

        private string GetWhereTypeString(SqlWhereType type)
        {
            return (type == SqlWhereType.And) ? " AND " : " OR ";
        }
        private string GetCompareTypeString(SqlCompareType type)
        {
            switch (type)
            {
                case SqlCompareType.Equals:
                    return " =@{0} ";
                case SqlCompareType.NotEquals:
                    return " <>@{0} ";
                case SqlCompareType.GreaterThan:
                    return " >@{0} ";
                case SqlCompareType.LessThan:
                    return " <@{0} ";
                case SqlCompareType.GreaterThanOrEquals:
                    return " >=@{0} ";
                case SqlCompareType.LessThanOrEquals:
                    return " <=@{0} ";
                case SqlCompareType.Contains:
                    return " LIKE '%'+@{0}+'%' ";
                case SqlCompareType.StartsWith:
                    return " LIKE @{0}+'%' ";
                case SqlCompareType.EndsWith:
                    return " LIKE '%'+@{0} ";
                case SqlCompareType.In:
                    return " IN ({0}) ";
                case SqlCompareType.NotIn:
                    return " NOT IN ({0}) ";
                default:
                    return "";
            }
        }


        private int _paraIndex = 1;
        private BaseMapper<T> BuildCondition(Expression<Func<T, bool>> expression, DbConfig config)
        {
            ClearArguments();
            if (expression == null)
            {
                return this;
            }
            var exp = new SqlLmdResolver(config);

            exp.ParaIndex = _paraIndex;
            exp.ResolveExpression(expression);
            _paraIndex = exp.ParaIndex;

            _WhereBuilder.Append(exp.SqlWhere);
            _Parameters.AddRange(exp.Parameters);
            return this;
        }


        public string ExpField<TResult>(Expression<Func<T, TResult>> field)
        {
            return EntityBuilder.GetPropertyName<T, TResult>(field);
        }

        public BaseMapper<T> And(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                return this;
            }
            var exp = new SqlLmdResolver(DbReadConfig);

            exp.ParaIndex = _paraIndex;
            exp.ResolveExpression(expression);
            _paraIndex = exp.ParaIndex;

            _WhereBuilder.Append(exp.SqlWhere);
            _Parameters.AddRange(exp.Parameters);
            return this;
        }

        public BaseMapper<T> Or(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                return this;
            }
            var exp = new SqlLmdResolver(DbReadConfig);

            exp.ParaIndex = _paraIndex;
            exp.ResolveExpression(expression, SqlWhereType.Or);
            _paraIndex = exp.ParaIndex;

            _WhereBuilder.Append(exp.SqlWhere);
            _Parameters.AddRange(exp.Parameters);
            return this;
        }



        private List<DbFieldInfo> initFieldList = null;
        private List<string> partFields = null;
        public void SetPartHandled(List<string> fields = null)
        {
            _IsPartHandled = true;
            partFields = fields;
            T curmodel = ContextEntity;
            if (curmodel == null)
            {
                curmodel = this as T;
                if (curmodel == null)
                {
                    throw new Exception("当前实体参数ContextEntity为空！");
                }
            }
            initFieldList = ConvertFields(curmodel);
        }



        public int Insert()
        {
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbWriteConfig);
            var paramInfo = GetInsertDbParamInfo();
            if (paramInfo.HasInsertIdentityKey)
            {
                var identity = tDbBuilder.GetSingle(paramInfo.SqlString, paramInfo.ParameterArray);
                return (int)identity;
            }
            return tDbBuilder.ExecuteSql(paramInfo.SqlString, paramInfo.ParameterArray);
        }

        public DbParamInfo GetInsertDbParamInfo()
        {
            var sbFields = new StringBuilder();
            var sbParams = new StringBuilder();
            var paramList = new List<DbParameter>();
            T curmodel = ContextEntity;
            if (curmodel == null)
            {
                curmodel = this as T;
                if (curmodel == null)
                {
                    throw new Exception("当前实体参数ContextEntity为空！");
                }
            }
            var fieldList = ConvertFields(curmodel);
            var hasIdentity = false;
            foreach (var param in fieldList)
            {
                if (!param.IsIdentity)
                {
                    Predicate<DbFieldInfo> predcate = (m) =>
                    {
                        if (m.Name == param.Name)
                        {
                            if (m.Value == null && param.Value == null)
                            {
                                return true;
                            }
                            if (m.Value == null || param.Value == null)
                            {
                                return false;
                            }
                            return m.Value.ToString() == param.Value.ToString();
                        }
                        return false;
                    };
                    if (_IsPartHandled)
                    {
                        if (partFields != null && partFields.Count > 0)
                        {
                            if (!partFields.Exists(m => m == param.Name))
                            {
                                continue;
                            }
                        }
                        else if (initFieldList.Exists(predcate))
                        {
                            continue;
                        }
                    }
                    sbFields.Append(",[");
                    sbFields.Append(param.Name);
                    sbFields.Append("]");
                    sbParams.Append(",@");
                    sbParams.Append(param.Name);
                    paramList.Add(DbProvider.MakeParam(DbWriteConfig, "@" + param.Name, param.Value));
                }
                else
                {
                    hasIdentity = true;
                }
            }
            initFieldList = fieldList;

            string sqlString = "insert into " + TableName + "(" + sbFields.ToString().TrimStart(',') + ") values(" + sbParams.ToString().TrimStart(',') + ")";
            if (hasIdentity)
            {
                if (DbWriteConfig.IsAccess)
                {
                    sqlString += ";select @@identity";
                }
                else if (DbWriteConfig.IsMysql)
                {
                }
                else
                {
                    sqlString += ";select cast(scope_identity() as int)";
                }
            }
            return new DbParamInfo(sqlString, paramList.ToArray()).UseInsertIdentityKey(hasIdentity);
        }



        public bool Update(Expression<Func<T, bool>> expression = null)
        {
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbWriteConfig);
            var paramInfo = GetUpdateDbParamInfo(expression);
            if (paramInfo == null)
            {
                return true;
            }
            return tDbBuilder.ExecuteSql(paramInfo.SqlString, paramInfo.ParameterArray) > 0;
        }
        public DbParamInfo GetUpdateDbParamInfo(Expression<Func<T, bool>> expression = null)
        {
            BuildCondition(expression, DbWriteConfig);
            return GetUpdateDbParamInfo();
        }
        private DbParamInfo GetUpdateDbParamInfo()
        {
            var sbSetlist = new StringBuilder();
            var paList = new List<DbParameter>();

            T curmodel = ContextEntity;
            if (curmodel == null)
            {
                curmodel = this as T;
                if (curmodel == null)
                {
                    throw new Exception("当前实体参数ContextEntity为空！");
                }
            }
            var fieldList = ConvertFields(curmodel);
            var keyValList = new List<DbFieldInfo>();
            foreach (var param in fieldList)
            {
                if (_WhereBuilder == null || _WhereBuilder.Length == 0)
                {
                    if (param.IsPrimaryKey)
                    {
                        keyValList.Add(param);
                        continue;
                    }
                }
                else
                {
                    if (param.IsPrimaryKey)
                    {
                        continue;
                    }
                }
                Predicate<DbFieldInfo> predcate = (m) =>
                {
                    if (m.Name == param.Name)
                    {
                        if (m.Value == null && param.Value == null)
                        {
                            return true;
                        }
                        if (m.Value == null || param.Value == null)
                        {
                            return false;
                        }
                        return m.Value.ToString() == param.Value.ToString();
                    }
                    return false;
                };
                if (_IsPartHandled)
                {
                    if (partFields != null && partFields.Count > 0)
                    {
                        if (!partFields.Exists(m => m == param.Name))
                        {
                            continue;
                        }
                    }
                    else if (initFieldList.Exists(predcate))
                    {
                        continue;
                    }
                }
                sbSetlist.Append(",[");
                sbSetlist.Append(param.Name);
                sbSetlist.Append("]");
                sbSetlist.Append("=@");
                sbSetlist.Append(param.Name);
                paList.Add(DbProvider.MakeParam(DbWriteConfig, "@" + param.Name, param.Value));
            }
            initFieldList = fieldList;

            if (paList.Count == 0)
            {
                return null;
            }
            var sbAndWhere = new StringBuilder();
            sbAndWhere.Append(" where 1=1 ");
            foreach (var param in keyValList)
            {
                sbAndWhere.Append(string.Concat(" and [", param.Name, "]=@", param.Name));
                paList.Add(DbProvider.MakeParam(DbWriteConfig, "@" + param.Name, param.Value));
            }
            if (_WhereBuilder != null)
            {
                sbAndWhere.Append(_WhereBuilder.ToString());
            }
            if (_Parameters != null)
            {
                paList.AddRange(_Parameters.ToArray());
            }
            string sqlString = "update " + TableName + " set " + sbSetlist.ToString().TrimStart(',') + sbAndWhere.ToString();
            return new DbParamInfo(sqlString, paList.ToArray());
        }


        public bool Delete(Expression<Func<T, bool>> expression = null)
        {
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbWriteConfig);
            var paramInfo = GetDeleteDbParamInfo(expression);
            return tDbBuilder.ExecuteSql(paramInfo.SqlString, paramInfo.ParameterArray) > 0;
        }
        public DbParamInfo GetDeleteDbParamInfo(Expression<Func<T, bool>> expression = null)
        {
            T curmodel = ContextEntity;
            if (curmodel == null)
            {
                curmodel = this as T;
            }
            List<DbFieldInfo> keList = null;
            var hasIdentity = false;
            var paList = new List<DbParameter>();
            if (expression != null)
            {
                ClearArguments();
                var exp = new SqlLmdResolver(DbWriteConfig);

                exp.ParaIndex = _paraIndex;
                exp.ResolveExpression(expression);
                _paraIndex = exp.ParaIndex;

                _WhereBuilder.Append(exp.SqlWhere);
                _Parameters.AddRange(exp.Parameters);
            }
            if (_WhereBuilder == null || _WhereBuilder.Length == 0)
            {
                if (curmodel == null)
                {
                    throw new Exception("当前实体参数ContextEntity为空！");
                }
                var fieldList = ConvertFields(curmodel);
                keList = fieldList.FindAll(m => m.IsPrimaryKey);
                hasIdentity = fieldList.Exists(m => m.IsIdentity);
            }

            var sbAndWhere = new StringBuilder();
            sbAndWhere.Append(" where 1=1 ");

            if (keList != null)
            {
                foreach (var param in keList)
                {
                    sbAndWhere.Append(string.Concat(" and [", param.Name, "]=@", param.Name));
                    paList.Add(DbProvider.MakeParam(DbWriteConfig, "@" + param.Name, param.Value));
                }
            }
            if (_WhereBuilder != null)
            {
                sbAndWhere.Append(_WhereBuilder.ToString());
            }
            if (_Parameters != null)
            {
                paList.AddRange(_Parameters.ToArray());
            }
            var sql = "delete from " + TableName + " " + sbAndWhere.ToString();
            return new DbParamInfo(sql, paList.ToArray());
        }

        
        private IDataReader GetReader(int topCount, string colNames = "*")
        {
            string andWhereOrderBy = null;
            if (_WhereBuilder != null)
            {
                andWhereOrderBy = _WhereBuilder.ToString();
                if (_SortBuilder != null)
                {
                    var sortNames = _SortBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(sortNames))
                    {
                        andWhereOrderBy += " order by " + sortNames;
                    }
                }
            }
            DbParameter[] tParameters = null;
            if (_Parameters != null)
            {
                tParameters = _Parameters.ToArray();
            }

            var strFrom = !string.IsNullOrWhiteSpace(TableFrom) ? "(" + TableFrom + ") as tmp " : TableName;
            var sql = SqlHelper.GetSqlString(strFrom, topCount, colNames, andWhereOrderBy, DbReadConfig);
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbReadConfig);
            return tDbBuilder.GetDataReader(sql, tParameters);
        }

        public T ToModel()
        {
            T entity = default(T);
            var dr = GetReader(1);
            if (dr != null)
            {
                if (dr.Read())
                {
                    entity = ConvertEntity(dr);
                }
                dr.Close();
            }
            return entity;
        }

        public string ToValue<TResult>(Expression<Func<T, TResult>> field)
        {
            var andWhereOrderBy = string.Empty;
            DbParameter[] tParameters = null;
            if (_WhereBuilder != null)
            {
                andWhereOrderBy = _WhereBuilder.ToString();
                if (_SortBuilder != null)
                {
                    var sortNames = _SortBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(sortNames))
                    {
                        andWhereOrderBy += " order by " + sortNames;
                    }
                }
            }
            if (_Parameters != null)
            {
                tParameters = _Parameters.ToArray();
            }
            string fieldName = ExpField(field);
            var strFrom = !string.IsNullOrWhiteSpace(TableFrom) ? "(" + TableFrom + ") as tmp " : TableName;
            string sql = string.Format("select " + SqlHelper.GetTopStartCondition(1, DbReadConfig) + " [{0}] from " + strFrom +
                " where 1=1 {1}", fieldName, andWhereOrderBy + SqlHelper.GetTopEndCondition(1, DbReadConfig));
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbReadConfig);
            object obj = tDbBuilder.GetSingle(sql, tParameters);
            return obj == null ? string.Empty : obj.ToString();
        }

        public string ToMax<TResult>(Expression<Func<T, TResult>> field)
        {
            return ToFunction(field, "max");
        }
        public string ToMin<TResult>(Expression<Func<T, TResult>> field)
        {
            return ToFunction(field, "min");
        }
        public string ToAvg<TResult>(Expression<Func<T, TResult>> field)
        {
            return ToFunction(field, "avg");
        }
        public string ToSum<TResult>(Expression<Func<T, TResult>> field)
        {
            return ToFunction(field, "sum");
        }
        private string ToFunction<TResult>(Expression<Func<T, TResult>> field, string func)
        {
            var andWhereOrderBy = string.Empty;
            DbParameter[] tParameters = null;
            if (_WhereBuilder != null)
            {
                andWhereOrderBy = _WhereBuilder.ToString();
                if (_SortBuilder != null)
                {
                    var sortNames = _SortBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(sortNames))
                    {
                        andWhereOrderBy += " order by " + sortNames;
                    }
                }
            }
            if (_Parameters != null)
            {
                tParameters = _Parameters.ToArray();
            }
            string fieldName = ExpField(field);
            var strFrom = !string.IsNullOrWhiteSpace(TableFrom) ? "(" + TableFrom + ") as tmp " : TableName;
            string sql = "select " + func + "(" + fieldName + ") as tobjject from " + strFrom + " where 1=1 " + andWhereOrderBy;
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbReadConfig);
            object obj = tDbBuilder.GetSingle(sql, tParameters);
            return obj == null ? string.Empty : obj.ToString();
        }



        public List<string> ToValueList<TResult>(Expression<Func<T, TResult>> field, int topCount = -1)
        {
            var list = new List<string>();
            string fieldName = ExpField(field);
            IDataReader dr = GetReader(topCount, string.Format("[{0}]", fieldName));
            if (dr != null)
            {
                while (dr.Read())
                {
                    list.Add(dr[fieldName].ToString());
                }
                dr.Close();
            }
            return list;
        }

        public List<string[]> ToPartList(params string[] fields)
        {
            return ToPartList(-1, fields);
        }

        public List<string[]> ToPartList(int topCount, params string[] fields)
        {
            var list = new List<string[]>();
            if (fields == null || fields.Length == 0)
            {
                return list;
            }
            IDataReader dr = GetReader(topCount, "[" + string.Join("],[", fields) + "]");
            if (dr != null)
            {
                while (dr.Read())
                {
                    var arr = new string[fields.Length];
                    for (int i = 0; i < fields.Length; i++)
                    {
                        arr[i] = dr[fields[i]].ToString();
                    }
                    list.Add(arr);
                }
                dr.Close();
            }
            return list;
        }


        public List<T> ToList(int topCount = -1)
        {
            var dr = GetReader(topCount);
            var list = new List<T>();
            if (dr != null)
            {
                while (dr.Read())
                {
                    list.Add(ConvertEntity(dr));
                }
                dr.Close();
            }
            return list;
        }

        public List<T> ToList(int pageSize, int pageIndex, int recordCount)
        {
            var orderBy = string.Empty;
            var andWhereOrderBy = string.Empty;
            DbParameter[] tParameters = null;
            if (_WhereBuilder != null)
            {
                andWhereOrderBy = _WhereBuilder.ToString();
            }
            if (_SortBuilder != null)
            {
                orderBy = _SortBuilder.ToString();
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "id";
            }
            if (_Parameters != null)
            {
                tParameters = _Parameters.ToArray();
            }

            var strFrom = !string.IsNullOrWhiteSpace(TableFrom) ? "(" + TableFrom + ") as tmp " : TableName;
            var sql = SqlHelper.GetSqlString(strFrom, pageSize, pageIndex, recordCount, "*", andWhereOrderBy, orderBy, DbReadConfig);
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbReadConfig);
            IDataReader dr = tDbBuilder.GetDataReader(sql, tParameters);
            var list = new List<T>();
            if (dr != null)
            {
                while (dr.Read())
                {
                    list.Add(ConvertEntity(dr));
                }
                dr.Close();
            }
            return list;
        }
        
        public int ToCount()
        {
            var andWhereOrderBy = string.Empty;
            DbParameter[] tParameters = null;
            if (_WhereBuilder != null)
            {
                andWhereOrderBy = _WhereBuilder.ToString();
            }
            if (_Parameters != null)
            {
                tParameters = _Parameters.ToArray();
            }

            var strFrom = !string.IsNullOrWhiteSpace(TableFrom) ? "(" + TableFrom + ") as tmp " : TableName;
            string sql = "select count(1) as tcount from " + strFrom + " where 1=1 " + andWhereOrderBy;
            var tDbBuilder = shareDbBuilder ?? new DbBuilder(DbReadConfig);
            return (int)tDbBuilder.GetSingle(sql, tParameters);
        }


    }
}


