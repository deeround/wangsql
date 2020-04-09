using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Test.Models
{
    [WangSql.Table(Name = "tb_user", Comment = "用户信息", AutoCreate = true)]
    public class UserInfo
    {
        public UserInfo()
        {

        }

        public UserInfo(string userId, string userName, int? age, bool? sex, string text)
        {
            UserId = userId;
            UserName = userName;
            Age = age;
            Sex = sex;
            JieShao = text;
        }

        [WangSql.Column(Length = 36, IsPrimaryKey = true, IsNotNull = true, Comment = "用户ID")]
        public string UserId { get; set; }
        [WangSql.Column(IsUnique = true, IsNotNull = true, Comment = "用户名")]
        public string UserName { get; set; }
        [WangSql.Column(Length = 3, Comment = "年龄")]
        public int? Age { get; set; }
        [WangSql.Column(Comment = "性别")]
        public bool? Sex { get; set; }
        [WangSql.Column()]
        public DateTime CreateDateTime { get; set; }
        [WangSql.Column(DataType = SimpleStandardType.Text)]
        public string JieShao { get; set; }
    }

    public class UserInfo1
    {
        public UserInfo1()
        {

        }

       
        public string UserName { get; set; }
        public bool? Sex { get; set; }
    }

}
