using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Util.Database;
using Util.EntityMapping;
using System.Linq.Expressions;
namespace Util.UnitTest
{
    [TestClass]
    public class TestDatabase_Unit
    {


        #region lmd生成sql条件测试

        
        [TestMethod]
        public void TestSqlLmdResolve()
        {
            //ORM数据映射
            DbConfig.UseDefaultConfig(new TModelDbConfig(GetDbPath()));


            var where = GetSqlWhere<cms_category>(m => m.enabled && m.name == "test");
            Console.WriteLine("LmdSql1:" + where);
            where = GetSqlWhere<cms_category>(m => !m.enabled && m.name.Contains("test") && m.enabled);
            Console.WriteLine("LmdSql2:" + where);

            //条件优先级
            where = GetSqlWhere<cms_category>(m => (!m.enabled && m.name.Contains("test") && m.enabled) || m.name.StartsWith("test"));
            Console.WriteLine("LmdSql3:" + where);
            where = GetSqlWhere<cms_category>(m => (m.enabled && m.name.Contains("test") && m.enabled) || (m.name.StartsWith("test") && !m.isused && m.isused));

            //其他判断
            Console.WriteLine("LmdSql4:" + where);
            where = GetSqlWhere<cms_category>(m => !m.enabled && m.name.Contains("test") && !m.enabled);
            Console.WriteLine("LmdSql5:" + where);
            where = GetSqlWhere<cms_category>(m => !m.enabled && m.name.Contains("test") && m.enabled == true);
            Console.WriteLine("LmdSql6:" + where);
            where = GetSqlWhere<cms_category>(m => m.name.Contains("test") && m.enabled || m.name.StartsWith("test"));
            Console.WriteLine("LmdSql7:" + where);
            where = GetSqlWhere<cms_category>(m => m.enabled);
            Console.WriteLine("LmdSql8:" + where);
            where = GetSqlWhere<cms_category>(m => !m.enabled);
            Console.WriteLine("LmdSql9:" + where);
            where = GetSqlWhere<cms_category>(m => m.name.StartsWith("test"));
            Console.WriteLine("LmdSql10:" + where);
            where = GetSqlWhere<cms_category>(m => !m.name.StartsWith("test"));
            Console.WriteLine("LmdSql11:" + where);
            where = GetSqlWhere<cms_category>(m => m.name.StartsWith("test") || m.name.Contains("test"));
            Console.WriteLine("LmdSql12:" + where);

            //条件判断是否前包含，判断常量相等，多层判断
            var extend = new cms_category_extend();
            extend.mytest2 = new cms_category_extend();
            extend.mytest2.mytest1 = new cms_category { name = "hehhe" };
            where = GetSqlWhere<cms_category>(m => m.name.StartsWith("test") || m.name == cms_category.TestConst ||
                m.name == extend.mytest2.mytest1.name);
            Console.WriteLine("LmdSql13:" + where);

            //判断列表包含
            var list = new List<string> { "a", "b", "c" };
            where = GetSqlWhere<cms_category>(m => list.Contains(m.name));
            Console.WriteLine("LmdSql14:" + where);

            object testName = "test";
            where = GetSqlWhere<cms_category>(m => m.enabled && m.name == (string)testName);
            Console.WriteLine("LmdSql15:" + where);
            object testParent_id = 1;
            //枚举判断
            where = GetSqlWhere<cms_category>(m => (m.id == (int)testParent_id) || (m.enabled && m.parent_id == Status.Success));
            Console.WriteLine("LmdSql16:" + where);

            //静态字段判断
            where = GetSqlWhere<cms_category>(m => m.name == cms_category.TestStatic);
            Console.WriteLine("LmdSql17:" + where);
        }

        private string GetSqlWhere<T>(Expression<Func<T, bool>> expression)
        {
            SqlLmdResolver exp = new SqlLmdResolver();
            exp.ResolveExpression(expression);
            return exp.SqlWhere + "\r\n" + string.Join(",", exp.Parameters.Select(m => m.ParameterName + ":" + m.Value.ToString()));
        }


        #endregion
        
        #region access orm测试

        private string GetDbPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            if (path.EndsWith("debug", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.LastIndexOf('\\'));
                path = path.Substring(0, path.LastIndexOf('\\'));
                path = path.Substring(0, path.LastIndexOf('\\'));
            }
            path = path.TrimEnd('\\') + @"\DataBase";
            return path;
        }
        [TestMethod]
        public void TestDbConfig()
        {
            //初始化配置
            DbConfig.UseDefaultConfig(new TModelDbConfig(GetDbPath()));

            //T4模版获取数据库信息
            List<TableInfo> list = DbFactory.GetShemaTables();
            Console.WriteLine(list.Count.ToString());
        }


        [TestMethod]
        public void TestAccessOrm()
        {

            //ORM数据映射
            DbConfig.UseDefaultConfig(new TModelDbConfig(GetDbPath()));
            Console.WriteLine("Start loadding...");
            Console.WriteLine(new cms_category().Query(m => m.name == "城市").ToCount());
            var cat = new cms_category().Query(m => m.name == "城市").SortAsc(m => m.name).ToModel();
            Console.WriteLine(cat.name);

            //设置只更新部分
            //cat.SetPartHandled();
            //cat.description = "test";
            //cat.Update(m=>m.id == 1);

            Console.WriteLine(cat.ToValue(m => m.name));
            Console.WriteLine(new cms_category().Query(m => m.name == "城市").ToList()[0].name);
            Console.WriteLine(new cms_category().Query(m => m.name == "城市" && m.id > 0 && m.name == "" || (m.id == 0 || m.name == "")).ToCount());
            //指定条件规则查询
            Console.WriteLine(new cms_category().Query(m => (m.name == "城市" && (m.id > 0 || m.name == "")) || (m.id == 0 || m.name == "")).ToCount());

            var cityList = new List<string> { "城市", "b", "c" };
            var layer = new LayerModel { List = cityList };
            Console.WriteLine(new cms_category().Query(m => m.name == "城市" || cityList.Contains(m.name) || m.parent_id == Status.Success).ToCount());
            Console.WriteLine(new cms_category().Query(m => m.name == "城市" || layer.List.Contains(m.name)).ToCount());
            

            //获取全部
            var datsList = new cms_category().Query().ToList();
            Console.WriteLine(datsList.Count);
            //获取N条
            datsList = new cms_category().Query().ToList(6);
            Console.WriteLine(datsList.Count);
            //获取部分
            var partList = new cms_category().Query().ToPartList(6, "id", "name").Select(m => new cms_category
            {
                id = int.Parse(m[0]),
                name = m[1]
            }).ToList();
            Console.WriteLine(partList.Count);
            //分页查询
            var mapper = new cms_category().Query();
            var dataCount = mapper.ToCount();
            datsList = mapper.ToList(20, 1, dataCount);
            Console.WriteLine(datsList.Count);
            //条件拼接查询
            mapper.And(m => m.name == "test")
                .And(m => m.id > 0)
                .Or(m => m.parent_id > 0);
            mapper.Or(m => m.parent_id > 0);



            var channels = new cms_channel().Query().ToList();
            Console.WriteLine(channels.Count);
            var grade = new ucl_grade { id = 5 };
            grade.grade_name = "新手1";
            var dal = new UclGradeDataAccess(grade);
            //保持数据库连接
            using (var db = new DbBuilder(new TModelDbConfig(GetDbPath())).KeepConnect())
            {
                //使用数据库db操作并跟踪实体修改状态
                dal.UseDatabase(db).SetPartHandled();
                grade.grade = 8;
                grade.grade_name = "新手";
                dal.Update();
            }
            //db销毁后重连数据库
            Console.WriteLine(dal.ToValue(m => m.grade_name));


            //使用事务(在事务中处理)
            using (var db = new DbBuilder(new TModelDbConfig(GetDbPath())).KeepConnect())
            {
                try
                {
                    db.BeginTransaction();
                    //TODO:something
                    //使用数据库db操作并跟踪实体修改状态
                    dal.UseDatabase(db).SetPartHandled();
                    grade.grade = 8;
                    grade.grade_name = "新手";
                    dal.Update();
                    db.CommitTransaction();
                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();
                }
            }
            
            //使用事务(批处理事务)
            var parList = new List<DbParamInfo>();
            //添加到批处理事务中，如果执行失败则回滚事务
            parList.Add(dal.GetUpdateDbParamInfo().UseVerifyExecResult());
            //TODO:添加其他操作到parList
            var execCount = new DbBuilder(new TModelDbConfig(GetDbPath())).ExecuteSqlTran(parList);
            Console.WriteLine(execCount);
        }


        [TestMethod]
        public void TestMappingField()
        {
            var cat = new cms_category();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var eachCount = 100000;
            for (var i = 0; i < eachCount; i++)
            {
                var field = new cms_category().ExpField(f => f.name);
            }
            watch.Stop();
            Console.WriteLine("Linq反射取" + eachCount + "次字段毫秒数:" + watch.ElapsedMilliseconds);
        }



        //===============================================================================================
        //access 测试配置类
        //===============================================================================================
        public class TModelDbConfig : DbConfig
        {
            public static void DBWriteLogInfo(string info, string title, string logpath, string encoding)
            {
                Console.WriteLine("dblog:" + info);
            }
            public TModelDbConfig(string solutionDir) : base("System.Data.OleDb",
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + solutionDir + @"\PlugNT_CMS.mdb;User ID=;Password=;",
                DBWriteLogInfo)
            { }

        }


        [Table("cms_channel")]
        public partial class cms_channel : BaseMapper<cms_channel>
        {
            public int id { get; set; }
            public string no { get; set; }
            public string title { get; set; }
        }
        public class LayerModel
        {
            public List<string> List { get; set; }
        }
        public partial class cms_category : BaseMapper<cms_category>
        {

            public static string TestStatic = "TestStatic";
            public const string TestConst = "TestConst";

            public int id { get; set; }
            public string name { get; set; }
            //public int parent_id { get; set; }
            public Status parent_id { get; set; }

            [Obsolete("test")]
            public bool enabled { get; set; }
            [Obsolete("test")]
            public bool isused { get; set; }

            
            public override string TableName
            {
                get { return "cms_category"; }
            }
            protected override cms_category ConvertEntity(IDataReader reader)
            {
                return new cms_category
                {
                    id = int.Parse(reader["id"].ToString()),
                    name = reader["name"].ToString(),
                    parent_id = (Status)int.Parse(reader["parent_id"].ToString()),
                };
            }
            protected override List<DbFieldInfo> ConvertFields(cms_category model)
            {
                return new List<DbFieldInfo>
                {
                    new DbFieldInfo { Name = "id", Value = model.id , IsIdentity =true },
                    new DbFieldInfo { Name = "name", Value = model.name  },
                    new DbFieldInfo { Name = "parent_id", Value = model.parent_id  },
                };
            }
        }

        public class cms_category_extend : cms_category
        {
            public cms_category mytest1 { get; set; }
            public cms_category_extend mytest2 { get; set; }
            public string myname { get; set; }
        }
        public class ucl_grade
        {
            public int id { get; set; }
            public int grade { get; set; }
            public string grade_name { get; set; }
        }

        public class UclGradeDataAccess : BaseMapper<ucl_grade>
        {
            public UclGradeDataAccess(ucl_grade model = null)
            {
                ContextEntity = model;
            }
            public override string TableName
            {
                get { return "ucl_grade"; }
            }
            protected override ucl_grade ConvertEntity(IDataReader reader)
            {
                return new ucl_grade
                {
                    id = int.Parse(reader["id"].ToString()),
                    grade = int.Parse(reader["grade"].ToString()),
                    grade_name = reader["grade_name"].ToString(),
                };
            }
            protected override List<DbFieldInfo> ConvertFields(ucl_grade model)
            {
                return new List<DbFieldInfo>
                {
                    new DbFieldInfo { Name = "id", Value = model.id , IsPrimaryKey =true , IsIdentity =true },
                    new DbFieldInfo { Name = "grade", Value = model.grade  },
                    new DbFieldInfo { Name = "grade_name", Value = model.grade_name  },
                };
            }
        }
        public enum Status
        {
            Success
        }
        


        #endregion
        
        
    }
}

