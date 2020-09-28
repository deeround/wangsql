using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.Test
{
    public class SqliteTest1
    {
        public static void Test()
        {
            //DbProviderManager.Set(
            //        "SQLite",
            //        "Default",
            //        "Data Source=mario.db;",
            //        "System.Data.SQLite.SQLiteConnection,System.Data.SQLite",
            //        true,
            //        true,
            //        "@",
            //        false,
            //        false
            //        );
            DbProviderManager.Set();
            var _sqlMapper = new SqlMapper();

            //Fluent API
            var roleMap = TableMap.Entity<Models.RoleInfo>().ToTable("tb_role", "角色", true);
            roleMap.HasColumn(x => x.RoleId).Name("RoleId").Length(50).Comment("ID").IsPrimaryKey().IsNotNull();
            roleMap.HasColumn(x => x.RoleName).Name("RoleName").Length(50).Comment("角色名称").IsUnique().IsNotNull();
            roleMap.HasColumn(x => x.OrderNum).Name("OrderNum").Comment("排序号");
            roleMap.HasColumn(x => x.CreateDateTime).Name("CreateDateTime").Comment("创建时间");


            //删除表
            try
            {
                string sql = $"DROP TABLE {_sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(TableMap.GetMap<Models.RoleInfo>().Name)}";
                _sqlMapper.Execute(sql, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("没有表，不用删除：" + ex.Message);
            }

            //创建表
            _sqlMapper.Migrate.Run();

            //删除数据
            _sqlMapper.Entity<Models.RoleInfo>().Delete();

            //插入数据
            Models.RoleInfo roleInfo = new Models.RoleInfo(Guid.NewGuid().ToString(), "deeround");
            roleInfo.CreateDateTime = DateTime.Now;
            _sqlMapper.Entity<Models.RoleInfo>().Insert(roleInfo);

            //查询
            var roles = _sqlMapper.Entity<Models.RoleInfo>().ToList();
        }
    }
}
