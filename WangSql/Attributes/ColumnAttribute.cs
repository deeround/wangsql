using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql
{
    /// <summary>
    /// 字段特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        {
        }
        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public SimpleStandardType DataType { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 精准度
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// 是否可为空
        /// </summary>
        public bool IsNotNull { get; set; }
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 是否为唯一键
        /// </summary>
        public bool IsUniqueKey { get; set; }
        /// <summary>
        /// 唯一键分组
        /// </summary>
        public string UniqueKeyGroup { get; set; }
    }
}
