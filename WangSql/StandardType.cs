namespace WangSql
{
    public enum StandardType
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        /// <summary>
        /// 简单类型
        /// </summary>
        Simple,
        /// <summary>
        /// 字典类型
        /// </summary>
        Dictionary,
        /// <summary>
        /// 实体
        /// </summary>
        Class
    }
    public enum SimpleStandardType
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        /// <summary>
        /// 数值类型（用户指定的精度，精确）
        /// </summary>
        Numeric,
        /// <summary>
        /// 字符类型（变长，有长度限制）
        /// </summary>
        Varchar,
        /// <summary>
        /// 大文本类型（变长，无长度限制）
        /// </summary>
        Text,
        /// <summary>
        /// 字符类型（变长，长度限制为1）
        /// </summary>
        Char,
        /// <summary>
        /// 字符类型（变长，有长度限制）
        /// </summary>
        Enum,
        /// <summary>
        ///  日期/时间类型（日期和时间，无时区）
        /// </summary>
        DateTime,
        /// <summary>
        /// 日期/时间类型（日期和时间，有时区）
        /// </summary>
        DateTimeOffset,
        /// <summary>
        /// 布尔类型（boolean 有"true"(真)或"false"(假)两个状态， 第三种"unknown"(未知)状态，用 NULL 表示。）
        /// 使用（变长，长度为1的字符类型）存储，"1"为true，"0"为false，null为未知状态
        /// </summary>
        Boolean,
        /// <summary>
        /// 二进制流（对应c#字节数组）
        /// </summary>
        Binary
    }
}
