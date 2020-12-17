using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WangSql.Sqlite;
using WangSql.Test.Models;
using WangSql.Abstract.Utils;
using WangSql.Abstract.Linq;

namespace WangSql.Test
{
    public class SqliteTest
    {
        public static void Test()
        {
            SqliteMapperManager.Init("Data Source=wangsql.db;");
            EntityUtil.SetMaps(new Type[] { typeof(Models.User) });

            var session = new SqlMapper();
            var sqlBuilder = new SqlBuilder();

            var entity = new User()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                NickName = "dapper",
            };

            //session.Migrate().Init(session);

            //var r0 = session.From<User>().Insert(entity).SaveChanges();
            //var r1 = session.From<User>().ToList();

            //entity = new User() { Id = entity.Id, NickName = "李四" };//更新所有字段（where id=2），支持批量，显然除NickName之外将被更新成null
            //var r2 = session.From<User>().Update(entity).SaveChanges();

            ////更新部分字段
            //var r3 = session.From<User>()
            //   .Update(entity)
            //   .Set(a => a.NickName, "李四")//condition为true时更新该字段
            //   .Set(a => a.Balance, a => a.Balance + 100)//余额在原来基础增加100
            //   .Where(a => a.Id.In(entity.Id, "2", "3"))//将id为1，2，3的记录进行更新
            //   .SaveChanges();

            //
            var list = session.From<Order, User>()
                .Join((a, b) => a.UserId == b.Id, JoinType.Inner)
                .Select((a, b) => new
                {
                    a.UserId,
                    b.NickName,
                })
                .Where((a, b) => a.UserId == "a")
                .ToSql();


        }
    }
}
