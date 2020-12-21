using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Test.Web.Models
{
    [WangSql.Abstract.Attributes.Table(TableName = "tb_order")]
    public class Order
    {
        [WangSql.Abstract.Attributes.Column(IsPrimaryKey = true)]
        public string Id { get; set; }
        [WangSql.Abstract.Attributes.Column()]
        public string UserId { get; set; }
        [WangSql.Abstract.Attributes.Column()]
        public string OrderName { get; set; }
    }
}
