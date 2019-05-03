/**********************************************************************************
* 代码说明：     Sql语句操作类
* 创建日期：     2009.6.13
* 修改日期：     2013.07.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Text;

using Util.Database;
namespace Util.EntityMapping
{


    internal static class SqlHelper
    {

        
        public static string GetSqlString(string tableName, int topCount,  string colNames, string andWhereOrderBy, DbConfig config)
        {
            if (string.IsNullOrWhiteSpace(colNames))
            {
                colNames = "*";
            }
            if (string.IsNullOrWhiteSpace(andWhereOrderBy))
            {
                andWhereOrderBy = string.Empty;
            }
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select " + GetTopStartCondition(topCount, config) + " " + colNames + " from " + tableName +
                "  where 1=1 " + andWhereOrderBy + GetTopEndCondition(topCount, config));

            return strSql.ToString();
        }


        public static string GetSqlString(string tableName, int pageSize, int pageIndex, int recordCount, string colNames, string andWhere, string orderBy,DbConfig config)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select " + colNames + " from " + tableName);
            if (string.IsNullOrWhiteSpace(andWhere) == false)
            {
                strSql.Append(" where 1=1 " + andWhere);
            }
            return GetPagingString(pageSize, pageIndex, recordCount, strSql.ToString(), orderBy, config);
        }


        public static string GetPagingString(int pageSize, int pageIndex, int recordCount, string strSql, string orderFields, DbConfig config)
        {
            if (config.IsMssql)
            {
                return GetPagingStringByMsSql(pageSize, pageIndex, recordCount, strSql, orderFields, config);
            }
            return GetPagingStringByFull(pageSize, pageIndex, recordCount, strSql, orderFields, config);
        }


        public static string GetPagingStringByFull(int pageSize, int pageIndex, int recordCount, string strSql, string orderFields,DbConfig config)
        {
            if (string.IsNullOrWhiteSpace(strSql))
            {
                throw new Exception("SQL语句不能为空！");
            }
            if (string.IsNullOrWhiteSpace(orderFields))
            {
                throw new Exception("排序字段不能为空！");
            }

            string[] arrStrOrders = orderFields.Split(',');
            StringBuilder sbOriginalOrder = new StringBuilder();
            StringBuilder sbReverseOrder = new StringBuilder();
            for (int i = 0; i < arrStrOrders.Length; i++)
            {
                arrStrOrders[i] = arrStrOrders[i].Trim();
                if (i != 0)
                {
                    sbOriginalOrder.Append(", ");
                    sbReverseOrder.Append(", ");
                }
                sbOriginalOrder.Append(arrStrOrders[i]);
                int index = arrStrOrders[i].IndexOf(" "); //判断是否有升降标识
                if (index > 0)
                {
                    bool flag = arrStrOrders[i].IndexOf(" desc", StringComparison.OrdinalIgnoreCase) != -1;
                    sbReverseOrder.AppendFormat("{0} {1}", arrStrOrders[i].Remove(index), flag ? "asc" : "desc");
                }
                else
                {
                    sbReverseOrder.AppendFormat("{0} desc", arrStrOrders[i]);
                }
            }
            pageSize = pageSize == 0 ? recordCount : pageSize;
            int pageCount = (recordCount + pageSize - 1) / pageSize;
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            else if (pageIndex > pageCount)
            {
                pageIndex = pageCount;
            }
            StringBuilder sbSql = new StringBuilder();
            if (pageIndex == 1)
            {
                sbSql.AppendFormat(" select {0} * ", GetTopStartCondition(pageSize, config));
                sbSql.AppendFormat(" from ({0}) as t ", strSql);
                sbSql.AppendFormat(" order by {0} {1}", sbOriginalOrder.ToString(), GetTopEndCondition(pageSize, config));
            }
            else if (pageIndex == pageCount)
            {
                int tmpTopCount = recordCount - pageSize * (pageIndex - 1);
                sbSql.Append(" select * from ");
                sbSql.Append(" ( ");
                sbSql.AppendFormat(" select {0} * ", GetTopStartCondition(tmpTopCount, config));
                sbSql.AppendFormat(" from ({0}) as t ", strSql);
                sbSql.AppendFormat(" order by {0} {1}", sbReverseOrder.ToString(), GetTopEndCondition(tmpTopCount, config));
                sbSql.Append(" ) as t ");
                sbSql.AppendFormat(" order by {0} {1}", sbOriginalOrder.ToString(), GetTopEndCondition(pageSize, config));
            }
            else if (pageIndex < (pageCount / 2 + pageCount % 2))
            {
                int tmpTopCount = pageSize * pageIndex;
                sbSql.Append(" select * from ");
                sbSql.Append(" ( ");
                sbSql.AppendFormat(" select {0} * from ", GetTopStartCondition(pageSize, config));
                sbSql.Append(" ( ");
                sbSql.AppendFormat(" select {0} * ", GetTopStartCondition(tmpTopCount, config));
                sbSql.AppendFormat(" from ({0}) as t ", strSql);
                sbSql.AppendFormat(" order by {0} {1}", sbOriginalOrder.ToString(), GetTopEndCondition(tmpTopCount, config));
                sbSql.Append(" ) as t ");
                sbSql.AppendFormat(" order by {0} {1}", sbReverseOrder.ToString(), GetTopEndCondition(pageSize, config));
                sbSql.Append(" ) as t ");
                sbSql.AppendFormat(" order by {0} {1}", sbOriginalOrder.ToString(), GetTopEndCondition(pageSize, config));
            }
            else
            {
                int tmpTopCount = recordCount - (pageIndex - 1) * pageSize;
                sbSql.AppendFormat(" select {0} * from ", GetTopStartCondition(pageSize, config));
                sbSql.Append(" ( ");
                sbSql.AppendFormat(" select {0} * ", GetTopStartCondition(tmpTopCount, config));
                sbSql.AppendFormat(" from ({0}) as t ", strSql);
                sbSql.AppendFormat(" order by {0} {1}", sbReverseOrder.ToString(), GetTopEndCondition(tmpTopCount, config));
                sbSql.Append(" ) as t ");
                sbSql.AppendFormat(" order by {0} {1}", sbOriginalOrder.ToString(), GetTopEndCondition(pageSize, config));
            }
            return sbSql.ToString();
        }


        public static string GetPagingStringByMsSql(int pageSize, int pageIndex, int recordCount, string strSql, string orderFields, DbConfig config)
        {
            if (string.IsNullOrWhiteSpace(strSql))
            {
                throw new Exception("SQL语句不能为空！");
            }
            if (string.IsNullOrWhiteSpace(orderFields))
            {
                throw new Exception("排序字段不能为空！");
            }

            pageSize = pageSize == 0 ? recordCount : pageSize;
            int pageCount = (recordCount + pageSize - 1) / pageSize;
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            var sbSql = new StringBuilder();
            var begin = ((pageIndex - 1) * pageSize) + 1;
            var end = pageIndex * pageSize;
            sbSql.Append("select * from (select row_number() over(order by " + orderFields + ") as row_number,* from (" + strSql + ") as it) as t ");
            sbSql.Append("where row_number between " + begin + " and " + end);
            return sbSql.ToString();
        }




        public static string Join(string strSql, string asTableName, string colNames, string onJoinAndWhere)
        {
            return "select  " + colNames + " from(" + strSql + ")" + asTableName + " " + onJoinAndWhere;
        }


        public static string Join(int pageSize, string strSql, string asTableName, string colNames, string onJoinAndWhere, DbConfig config)
        {
            return "select  " + GetTopStartCondition(pageSize, config) + " " + colNames + " from(" + strSql + ")" +
                asTableName + " " + onJoinAndWhere + GetTopEndCondition(pageSize, config);
        }


        public static string GetJoinOrderBy(string orderBy, string asTableName)
        {
            int asIndex = asTableName.IndexOf(" ");
            if (asIndex > 0)
            {
                asTableName = asTableName.Substring(asIndex);
            }
            StringBuilder sbOrderBy = new StringBuilder();
            string[] arrOrder = orderBy.Split(',');
            if (arrOrder.Length > 0)
            {
                bool first = true;
                foreach (string by in arrOrder)
                {
                    if (!first)
                    {
                        sbOrderBy.Append(",");
                    }
                    else
                    {
                        first = false;
                    }
                    sbOrderBy.AppendFormat(asTableName + ".{0}", by.Trim());
                }
            }
            else
            {
                sbOrderBy.AppendFormat(asTableName + ".{0}", orderBy);
            }
            return " order by " + sbOrderBy.ToString();
        }

        public static string GetTopSqlstr(int topCount, string strSql, DbConfig config)
        {
            return string.Format(" select {0} * from ({1}) {2} as t ",
                GetTopStartCondition(topCount, config), strSql, GetTopEndCondition(topCount, config));
        }

        public static string GetCountString(string strSql)
        {
            return string.Format(" select count(1) as tcount from ({0}) as t ", strSql);
        }

        public static string GetTopStartCondition(int topCount, DbConfig config)
        {
            if (topCount <= 0)
            {
                return string.Empty;
            }
            return (config.IsMysql) ? string.Empty : string.Concat(" top ", topCount, " ");
        }


        public static string GetTopEndCondition(int topCount, DbConfig config)
        {
            if (topCount <= 0)
            {
                return string.Empty;
            }
            return (config.IsMysql) ? string.Concat(" limit ", topCount, " ") : string.Empty;
        }



        public static bool IsSafeSqlName(string sqlName)
        {
            if (sqlName == null)
            {
                return false;
            }
            return System.Text.RegularExpressions.Regex.IsMatch(sqlName, @"^[_|a-zA-Z\d]+$");
        }




        public static string GetLockProcessIDAndTableNameForMsSql()
        {
            return @"SELECT request_session_id spid,OBJECT_NAME(resource_associated_entity_id)tableName FROM  sys.dm_tran_locks WHERE resource_type='OBJECT'";
        }

        public static string GetKillLockProcessForMsSql(int processID)
        {
            return "KILL " + processID;
        }



    }
}


