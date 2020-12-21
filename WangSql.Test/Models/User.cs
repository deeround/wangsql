using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Test.Models
{
    [WangSql.Abstract.Attributes.Table(TableName = "tb_user")]
    public class User
    {
        [WangSql.Abstract.Attributes.Column(IsPrimaryKey = true)]
        public string Id { get; set; }
        [WangSql.Abstract.Attributes.Column()]
        public string NickName { get; set; }
        [WangSql.Abstract.Attributes.Column()]
        public int Balance { get; set; }
        [WangSql.Abstract.Attributes.Column()]
        public DateTime? CreateTime { get; set; }
    }
}
