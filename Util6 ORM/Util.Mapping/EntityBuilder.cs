/**********************************************************************************
* 代码说明：     数据库实体动态构建助手
* 创建日期：     2014.05.15
* 修改日期：     2014.05.15
* 程序制作：     agui 
* 联系方式：     mailto:354990393@qq.com  
* 版权所有：     www.util6.com 
* ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Util.EntityMapping
{

    public class EntityBuilder<T>
    {
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private delegate T Load(IDataRecord dataRecord);
        private Load handler;

        private EntityBuilder() { }

        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public static EntityBuilder<T> CreateBuilder(IDataRecord dataRecord)
        {
            EntityBuilder<T> dynamicBuilder = new EntityBuilder<T>();

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { typeof(IDataRecord) }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(dataRecord.GetName(i));
                Label endIfLabel = generator.DefineLabel();

                if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);

                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, getValueMethod);
                    generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());

                    generator.MarkLabel(endIfLabel);
                }
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
        public static List<T> GetList(IDataReader reader)
        {
            List<T> list = new List<T>();
            EntityBuilder<T> builder = EntityBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                T model = default(T);
                model = builder.Build(reader);
                list.Add(model);
            }
            return list;
        }

    }

    internal class EntityBuilder
    {

        public static string GetPropertyName<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }
            if (memberExpression == null)
            {
                throw new ArgumentException("Lambda属性在该对象上没有");
            }
            return memberExpression.Member.Name;
        }

    }

}

