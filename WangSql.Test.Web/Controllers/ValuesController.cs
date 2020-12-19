using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WangSql.Abstract.Linq;
using WangSql.Test.Web.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WangSql.Test.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var session = new SqlMapper();
            var sqlBuilder = new SqlBuilder();

            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
                NickName = "dapper",
            };
            var order = new Order()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                OrderName = "asasa"
            };

            session.Migrate().CreateTable();

            var r01 = session.From<User>().Insert(user).SaveChanges();
            var r02 = session.From<Order>().Insert(order).SaveChanges();
            var r1 = session.From<User>().ToList();

            user = new User() { Id = user.Id, NickName = "李四" };//更新所有字段（where id=2），支持批量，显然除NickName之外将被更新成null
            var r2 = session.From<User>().Update(user).SaveChanges();

            //更新部分字段
            var r3 = session.From<User>()
               .Update(user)
               .Set(a => a.NickName, "李四")//condition为true时更新该字段
               .Set(a => a.Balance, a => a.Balance + 100)//余额在原来基础增加100
               .Where(a => a.Id.In(user.Id, "2", "3"))//将id为1，2，3的记录进行更新
               .SaveChanges();

            var r4 = session.From<User>().ToList();

            //
            string userId = "a";
            var r5 = session.From<Order, User>()
                .Join((a, b) => a.UserId == b.Id, JoinType.Inner)
                .Select((a, b) => new
                {
                    a.UserId,
                    b.NickName,
                    a.OrderName
                })
                .Where((a, b) => a.UserId == user.Id || a.UserId == userId)
                .ToList<Dictionary<string, object>>();

            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
