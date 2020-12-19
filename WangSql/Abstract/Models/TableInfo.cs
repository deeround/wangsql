using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract.Models
{
    public class TableInfo
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 类类型
        /// </summary>
        public Type ClassType { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 驱动名
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 是否自动创建表
        /// </summary>
        public bool AutoCreate { get; set; }
        /// <summary>
        /// 字段
        /// </summary>
        public List<ColumnInfo> Columns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TableInfo()
        {
            Columns = new List<ColumnInfo>();
        }
    }
}
