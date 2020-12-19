using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Attributes
{
    /// <summary>
    /// 驱动特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProviderAttribute : Attribute
    {
        public ProviderAttribute()
        {
        }
        /// <summary>
        /// 驱动名
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 是否自动创建表
        /// </summary>
        public bool AutoCreate { get; set; }
    }
}
