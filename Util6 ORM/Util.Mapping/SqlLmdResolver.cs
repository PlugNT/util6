/**********************************************************************************
* 代码说明：     Sql语句表达式解析类
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

using Util.Database;
namespace Util.EntityMapping
{


    public class SqlLmdResolver
    {

        internal int ParaIndex = 1;


        public string _SqlWhere = null;
        public string SqlWhere
        {
            get { return _SqlWhere; }
        }


        private List<DbParameter> _Parameters = null;
        public List<DbParameter> Parameters
        {
            get { return _Parameters; }
        }


        private DbConfig _DbConfig = null;


        public SqlLmdResolver(DbConfig config = null)
        {
            _DbConfig = config ?? DbConfig.Default;
            _SqlWhere = string.Empty;
            _Parameters = new List<DbParameter>();
        }



        public void ResolveExpression(Expression expression = null, SqlWhereType whereType = SqlWhereType.And)
        {
            if (expression == null)
            {
                _SqlWhere = string.Empty;
                return;
            }
            var sqlFormat = (whereType == SqlWhereType.And) ? " AND {0} " : " OR {0} ";
            SqlLmdResolver.MemberType type = SqlLmdResolver.MemberType.None;
            this._SqlWhere = string.Format(sqlFormat, GetResolveAll(expression, ref type).SqlConditions);
        }

        
        private enum MemberType
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private struct ParamInfo
        {
            public string SqlConditions;
            public object ObjectValue;
        }



        private string AddParametersReturnLeft(ref ParamInfo left, ParamInfo right)
        {
            string oldLeftKey = left.SqlConditions;
            left.SqlConditions = "P"+ ParaIndex + oldLeftKey;
            ParaIndex++;
            if (right.ObjectValue == null)
            {
                this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + left.SqlConditions, DBNull.Value));
            }
            else
            {
                this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + left.SqlConditions, right.ObjectValue));
            }
            return oldLeftKey;
        }
        private string AddParametersReturnRight(ParamInfo left, ref ParamInfo right)
        {
            string oldRightKey = right.SqlConditions;
            right.SqlConditions = "P" + ParaIndex + oldRightKey;
            ParaIndex++;
            if (left.ObjectValue == null)
            {
                this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + right.SqlConditions, DBNull.Value));
            }
            else
            {
                this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + right.SqlConditions, left.ObjectValue));
            }
            return oldRightKey;
        }



        private string GetOperator(ExpressionType expressiontype)
        {
            switch (expressiontype)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " =";
                case ExpressionType.GreaterThan:
                    return " >";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    throw new Exception(string.Format("不支持{0}此种运算符查找！", expressiontype.ToString()));
            }
        }


        private ParamInfo GetResolveAll(Expression exp, ref MemberType type, bool isTure = true)
        {
            if (exp is LambdaExpression)
            {
                return GetResolveLambda(exp);
            }
            else if (exp is BinaryExpression)
            {
                return GetResolveBinary(exp);
            }
            else if (exp is MethodCallExpression)
            {
                return GetResolveMethodCall(exp, ref type, isTure);
            }
            else if (exp is ConstantExpression)
            {
                return GetResolveConstant(exp, ref type);
            }
            else if (exp is MemberExpression)
            {
                return GetResolveMember(exp, ref type);
            }
            else if (exp is UnaryExpression)
            {
                return GetResolveUnary(exp, ref type);
            }
            return new ParamInfo();
        }
        
        private ParamInfo GetResolveLambda(Expression exp)
        {
            LambdaExpression lambda = exp as LambdaExpression;
            var expression = lambda.Body;
            MemberType EleType = MemberType.None;

            if (expression is UnaryExpression)
            {
                var me = expression as UnaryExpression;
                if (me.Operand is MemberExpression)
                {
                    var ime = me.Operand as MemberExpression;
                    return new ParamInfo { SqlConditions = ime.Member.Name.ToString() + "=0" };
                }
            }
            if (expression is MemberExpression)
            {
                var me = expression as MemberExpression;
                return new ParamInfo { SqlConditions = me.Member.Name.ToString() + "=1" };
            }
            return GetResolveAll(expression, ref EleType);
        }
        private ParamInfo GetResolveBinary(Expression exp)
        {
            var expression = exp as BinaryExpression;
            MemberType leftType = MemberType.None;
            MemberType rightType = MemberType.None;

            var left = GetResolveAll(expression.Left, ref leftType);
            var right = GetResolveAll(expression.Right, ref rightType);
            var oper = GetOperator(expression.NodeType);
            var isKeyOperValue = leftType == MemberType.Left && rightType == MemberType.Right;
            var isValueOperKey = rightType == MemberType.Left && leftType == MemberType.Right;

            if (leftType == MemberType.Left && rightType == MemberType.None)
            {
                if (expression.Left is UnaryExpression)
                {
                    var me = expression.Left as UnaryExpression;
                    if (me.Operand is MemberExpression)
                    {
                        left.SqlConditions = left.SqlConditions + "=0";
                    }
                }
                else if (expression.Left is MemberExpression)
                {
                    left.SqlConditions = left.SqlConditions + "=1";
                }
            }
            if (leftType == MemberType.None && rightType == MemberType.Left)
            {
                if (expression.Right is UnaryExpression)
                {
                    var me = expression.Right as UnaryExpression;
                    if (me.Operand is MemberExpression)
                    {
                        right.SqlConditions = right.SqlConditions + "=0";
                    }
                }
                else if (expression.Right is MemberExpression)
                {
                    right.SqlConditions = right.SqlConditions + "=1";
                }
            }

            if (isKeyOperValue & (right.ObjectValue == null) && oper.Trim() == "=")
            {
                var oldLeft = AddParametersReturnLeft(ref left, right);
                return new ParamInfo { SqlConditions = string.Format(" ({0} is null) ", oldLeft) };
            }
            else if (isKeyOperValue & (right.ObjectValue == null) && oper.Trim() == "<>")
            {
                var oldLeft = AddParametersReturnLeft(ref left, right);
                return new ParamInfo { SqlConditions = string.Format(" ({0} is not null) ", oldLeft) };
            }
            else if (isValueOperKey & (left.ObjectValue == null) && oper.Trim() == "=")
            {
                return new ParamInfo { SqlConditions = string.Format(" ({0} is null) ", right.SqlConditions) };
            }
            else if (isValueOperKey & (left.ObjectValue == null) && oper.Trim() == "<>")
            {
                return new ParamInfo { SqlConditions = string.Format(" ({0} is not null) ", right.SqlConditions) };
            }

            else if (isKeyOperValue)
            {
                var oldLeft = AddParametersReturnLeft(ref left, right);
                return new ParamInfo { SqlConditions = string.Format(" ({0} {1} @{2}) ", oldLeft, oper, left.SqlConditions) };
            }
            else if (isValueOperKey)
            {
                var oldRight = AddParametersReturnRight(left, ref right);
                return new ParamInfo { SqlConditions = string.Format(" (@{0} {1} {2}) ", right.SqlConditions, oper, oldRight) };
            }
            else if (leftType == MemberType.Right && rightType == MemberType.Right)
            {
                return new ParamInfo { SqlConditions = string.Format(" ('{0}' {1} '{2}') ", left.SqlConditions, oper, right.SqlConditions) };
            }
            else
            {
                return new ParamInfo { SqlConditions = string.Format(" ({0} {1} {2}) ", left.SqlConditions, oper, right.SqlConditions) };
            }
        }
        private ParamInfo GetResolveMethodCall(Expression exp, ref MemberType type, bool isTure)
        {
            MethodCallExpression mce = (MethodCallExpression)exp;
            string methodName = mce.Method.Name;
            if (methodName == "Contains")
            {
                MemberType leftType = MemberType.None;
                MemberType rightType = MemberType.None;
                if (mce.Method.DeclaringType != typeof(string) && mce.Method.DeclaringType.GetInterface("IEnumerable") != null)
                {
                    var left = GetResolveAll(mce.Arguments[0], ref rightType);
                    var right = GetResolveAll(mce.Object, ref leftType);
                    string oldLeftKey = left.SqlConditions;

                    string leftKey = "P" + ParaIndex + left.SqlConditions;
                    ParaIndex++;
                    var sqlParameterNames = "";
                    var memberType = MemberType.Right;
                    var list = GetResolveMember(mce.Object as MemberExpression, ref memberType).ObjectValue as IEnumerable;
                    var count = 1;
                    foreach (var item in list)
                    {
                        var parameterName = leftKey + count;
                        sqlParameterNames += ",@" + parameterName;
                        if (item == null)
                        {
                            this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + parameterName, DBNull.Value));
                        }
                        else
                        {
                            this._Parameters.Add(DbProvider.MakeParam(_DbConfig, "@" + parameterName, item));
                        }
                        count++;
                    }
                    sqlParameterNames = sqlParameterNames.TrimStart(',');
                    return new ParamInfo { SqlConditions = string.Format("({0} {1} IN ({2}))", oldLeftKey, isTure == false ? "  NOT " : "", sqlParameterNames) };
                }
                else
                {
                    var left = GetResolveAll(mce.Object, ref leftType);
                    var right = GetResolveAll(mce.Arguments[0], ref rightType);
                    var oldLeft = AddParametersReturnLeft(ref left, right);
                    return new ParamInfo { SqlConditions = string.Format("({0} {1} LIKE '%'+@{2}+'%')", oldLeft, isTure == false ? "  NOT " : "", left.SqlConditions) };
                }
            }
            else if (methodName == "StartsWith")
            {
                MemberType leftType = MemberType.None;
                MemberType rightType = MemberType.None;
                var left = GetResolveAll(mce.Object, ref leftType);
                var right = GetResolveAll(mce.Arguments[0], ref rightType);
                var oldLeft = AddParametersReturnLeft(ref left, right);
                return new ParamInfo { SqlConditions = string.Format("({0} {1} LIKE @{2}+'%')", oldLeft, isTure == false ? "  NOT " : "", left.SqlConditions) };
            }
            else if (methodName == "EndWith")
            {
                MemberType leftType = MemberType.None;
                MemberType rightType = MemberType.None;
                var left = GetResolveAll(mce.Object, ref leftType);
                var right = GetResolveAll(mce.Arguments[0], ref rightType);
                var oldLeft = AddParametersReturnLeft(ref left, right);
                return new ParamInfo { SqlConditions = string.Format("({0} {1} LIKE '%'+@{2})", oldLeft, isTure == false ? "  NOT " : "", left.SqlConditions) };
            }
            else if (methodName == "ToString")
            {
                type = MemberType.Right;
                return GetResolveAll(mce.Object, ref type);
            }
            else if (methodName.StartsWith("To"))
            {
                type = MemberType.Right;
                return GetResolveAll(mce.Arguments[0], ref type);
            }
            return new ParamInfo();
        }

        private ParamInfo GetResolveConstant(Expression exp, ref MemberType type)
        {
            type = MemberType.Right;
            ConstantExpression ce = ((ConstantExpression)exp);
            if (ce.Value == null)
            {
                return new ParamInfo();
            }
            else
            {
                return new ParamInfo { ObjectValue = ce.Value };
            }
        }
        private ParamInfo GetResolveUnary(Expression exp, ref MemberType type)
        {
            UnaryExpression ue = ((UnaryExpression)exp);
            var mex = ue.Operand;
            return GetResolveAll(mex, ref type, false);
        }

        private ParamInfo GetResolveMemberMethod(MemberExpression exp)
        {
            var proInfo = exp.Member as System.Reflection.PropertyInfo;
            if (proInfo != null)
            {
                object dynInv = proInfo.GetValue(null, null);
                return new ParamInfo { ObjectValue = dynInv };
            }
            else
            {
                var fieInfo = exp.Member as System.Reflection.FieldInfo;
                if (fieInfo != null)
                {
                    object dynInv = fieInfo.GetValue(null);
                    return new ParamInfo { ObjectValue = dynInv };
                }
            }
            return new ParamInfo();
        }
        private ParamInfo GetResolveMemberConstant(MemberExpression exp, object obj)
        {
            var proInfo = exp.Member as System.Reflection.PropertyInfo;
            if (proInfo != null)
            {
                var dynInv = proInfo.GetValue(obj, null);
                return new ParamInfo { ObjectValue = dynInv };
            }
            else
            {
                var fieInfo = exp.Member as System.Reflection.FieldInfo;
                if (fieInfo != null)
                {
                    var dynInv = fieInfo.GetValue(obj);
                    return new ParamInfo { ObjectValue = dynInv };
                }
            }
            return new ParamInfo();
        }
        private ParamInfo GetResolveMember(Expression exp, ref MemberType type)
        {
            MemberExpression me = ((MemberExpression)exp);
            if (me.Expression == null)
            {
                type = MemberType.Right;
                return GetResolveMemberMethod(me);
            }          

            if (me.Expression.NodeType != ExpressionType.Parameter)
            {
                type = MemberType.Right;
                object dynInv = null;
                try
                {
                    var conExp = me.Expression as ConstantExpression;
                    if (conExp != null)
                    {
                        return GetResolveMemberConstant(me, conExp.Value);
                    }
                    else
                    {
                        var memberInfos = new Stack<MemberInfo>();
                        while (exp is MemberExpression)
                        {
                            var memberExpr = exp as MemberExpression;
                            memberInfos.Push(memberExpr.Member);
                            exp = memberExpr.Expression;
                        }
                        
                        var constExpr = exp as ConstantExpression;
                        if (constExpr == null)
                        {
                            var member = exp as MemberExpression;
                            if (member == null)
                            {
                                throw new Exception("不支持的子表达式" + me.Member.Name);
                            }
                            return GetResolveMemberMethod(member);
                        }
                        var objReference = constExpr.Value;

                        while (memberInfos.Count > 0)  
                        {
                            var mi = memberInfos.Pop();
                            if (mi.MemberType == MemberTypes.Property)
                            {
                                objReference = objReference.GetType().GetProperty(mi.Name).GetValue(objReference, null);
                            }
                            else if (mi.MemberType == MemberTypes.Field)
                            {
                                objReference = objReference.GetType().GetField(mi.Name).GetValue(objReference);
                            }
                        }
                        dynInv = objReference;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("表达式解析出错(" + me.NodeType.ToString() + "):" + ex.Message);
                }

                if (dynInv == null)
                {
                    return new ParamInfo();
                }
                else
                {
                    return new ParamInfo { ObjectValue = dynInv };
                }
            }
            else
            {
                string name = me.Member.Name;
                type = MemberType.Left;
                return new ParamInfo { SqlConditions = name };
            }
        }

    }

}

