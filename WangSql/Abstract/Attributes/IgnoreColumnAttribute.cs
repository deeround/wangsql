using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Attributes
{
    /// <summary>
    /// 字段特性(忽略列)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreColumnAttribute : Attribute
    {
        public IgnoreColumnAttribute()
        {
        }
    }
}
