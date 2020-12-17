using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Attributes
{
    /// <summary>
    /// 表特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute()
        {
        }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 是否自动创建表
        /// </summary>
        public bool AutoCreate { get; set; }
    }
}
