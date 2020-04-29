using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Formula
{
    public class FormulaParamter
    {
        /// <summary>
        /// 是否常数
        /// </summary>
        public bool IsConstant { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isConstant"></param>
        /// <param name="value"></param>
        public FormulaParamter(bool isConstant, object value)
        {
            IsConstant = isConstant;
            Value = value;
        }
    }
    public interface IFormulaProvider
    {
        #region 聚合函数
        /// <summary>
        /// 计数函数（如：COUNT(ID)）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Count(object value);
        string Count_method(FormulaParamter[] paramters);

        /// <summary>
        /// 求和函数（如：SUM(ID)）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Sum(object value);
        string Sum_method(FormulaParamter[] paramters);

        /// <summary>
        /// 平均值函数（如：AVG(ID)）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Avg(object value);
        string Avg_method(FormulaParamter[] paramters);

        /// <summary>
        /// 最大值函数（如：MAX(ID)）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Max(object value);
        string Max_method(FormulaParamter[] paramters);

        /// <summary>
        /// 最小值函数（如：MIN(ID)）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Min(object value);
        string Min_method(FormulaParamter[] paramters);
        #endregion

        #region 比较函数
        /// <summary>
        /// 比较函数（可以比较非数值类型的字段）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Compare(object value);
        string Compare_method(FormulaParamter[] paramters);

        /// <summary>
        /// 判断NULL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsNull(object value);
        string IsNull_method(FormulaParamter[] paramters);

        /// <summary>
        /// 判断非NULL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsNotNull(object value);
        string IsNotNull_method(FormulaParamter[] paramters);
        #endregion

        #region 数值函数
        /// <summary>
        /// 绝对值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Abs(object value);
        string Abs_method(FormulaParamter[] paramters);

        /// <summary>
        /// 向上取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Ceil(object value);
        string Ceil_method(FormulaParamter[] paramters);

        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Floor(object value);
        string Floor_method(FormulaParamter[] paramters);

        /// <summary>
        /// 取余数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        DbOperator Mod(object value, decimal x);
        string Mod_method(FormulaParamter[] paramters);

        /// <summary>
        /// 取小数（四舍五入）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Round(object value, int s);
        string Round_method(FormulaParamter[] paramters);

        /// <summary>
        /// 取小数（舍）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Trunc(object value, int s);
        string Trunc_method(FormulaParamter[] paramters);
        #endregion

        #region 字符函数
        /// <summary>
        /// 只能连接两个字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Concat(object value1, object value2);
        string Concat_method(FormulaParamter[] paramters);

        /// <summary>
        /// 大写转小写
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Lower(object value);
        string Lower_method(FormulaParamter[] paramters);

        /// <summary>
        /// 小写转大写
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Upper(object value);
        string Upper_method(FormulaParamter[] paramters);

        /// <summary>
        /// 字符的长度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Length(object value);
        string Length_method(FormulaParamter[] paramters);

        /// <summary>
        /// 字符串替换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Replace(object value, string o, string n);
        string Replace_method(FormulaParamter[] paramters);
        #endregion

        #region 转换函数
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator ToChar(object value, string f);
        string ToChar_method(FormulaParamter[] paramters);

        /// <summary>
        /// 转换为数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator ToNumber(object value, string f);
        string ToNumber_method(FormulaParamter[] paramters);

        /// <summary>
        /// 转换为日期
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator ToDate(object value, string f);
        string ToDate_method(FormulaParamter[] paramters);
        #endregion

        #region 其他函数
        /// <summary>
        /// 字符转成时间戳
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DbOperator Nvl(object value1, object value2);
        string Nvl_method(FormulaParamter[] paramters);
        #endregion

        #region 自定义扩展函数
        ///// <summary>
        ///// 自定义扩展函数
        ///// 无入参的函数
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //DbOperator Fun(string name);
        //string Fun_method(string name);

        ///// <summary>
        ///// 自定义扩展函数
        ///// 一个参数的函数
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //DbOperator Fun(string name, object value);
        //string Fun_method(string name, object value);

        ///// <summary>
        ///// 自定义扩展函数
        ///// 两个参数的函数
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //DbOperator Fun(string name, object value1, object value2);
        //string Fun_method(string name, object value1, object value2);

        ///// <summary>
        ///// 自定义扩展函数
        ///// 三个参数的函数
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //DbOperator Fun(string name, object value1, object value2, object value3);
        //string Fun_method(string name, object value1, object value2, object value3);
        #endregion
    }
}
