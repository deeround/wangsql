using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Test.Models
{
    public class RoleInfo
    {
        public RoleInfo()
        {

        }

        public RoleInfo(string userId, string userName)
        {
            RoleId = userId;
            RoleName = userName;
        }

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int? OrderNum { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
