using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Models
{
    public class ColumnInfo
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 树形类型
        /// </summary>
        public Type PropertyType { get; set; }
        /// <summary>
        /// 字段名
        /// </summary>
        public string ColumnName { get; set; }
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
        public int? Length { get; set; }
        /// <summary>
        /// 精准度
        /// </summary>
        public int? Precision { get; set; }
        /// <summary>
        /// 是否不可为空
        /// </summary>
        public bool IsNotNull { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 是否为唯一键
        /// </summary>
        public bool IsUnique { get; set; }
        /// <summary>
        /// 唯一键分组
        /// </summary>
        public string UniqueGroup { get; set; }
        /// <summary>
        /// 是否为索引
        /// </summary>
        public bool IsIndex { get; set; }
        /// <summary>
        /// 索引分组
        /// </summary>
        public string IndexGroup { get; set; }
    }
}
