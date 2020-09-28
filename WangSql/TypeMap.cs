using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WangSql
{
    public class TypeMap
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<Type, StandardType> Type2StandardTypeMap = new ConcurrentDictionary<Type, StandardType>();
        //private static readonly int Type2StandardTypeMapLength = 1000;
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<Type, SimpleStandardType> Type2SimpleStandardTypeMap = new ConcurrentDictionary<Type, SimpleStandardType>();
        //private static readonly int Type2SimpleStandardTypeMapLength = 1000;

        static TypeMap()
        {
            #region Type2StandardTypeMap
            Type2StandardTypeMap[typeof(string)] = StandardType.Simple;

            Type2StandardTypeMap[typeof(char)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(byte)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(sbyte)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(short)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(ushort)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(int)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(uint)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(long)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(ulong)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(float)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(double)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(decimal)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(bool)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(char)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(DateTime)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(DateTimeOffset)] = StandardType.Simple;

            Type2StandardTypeMap[typeof(char?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(byte?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(sbyte?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(short?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(ushort?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(int?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(uint?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(long?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(ulong?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(float?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(double?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(decimal?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(bool?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(char?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(DateTime?)] = StandardType.Simple;
            Type2StandardTypeMap[typeof(DateTimeOffset?)] = StandardType.Simple;

            Type2StandardTypeMap[typeof(Dictionary<string, object>)] = StandardType.Dictionary;
            Type2StandardTypeMap[typeof(IDictionary<string, object>)] = StandardType.Dictionary;

            Type2StandardTypeMap[typeof(byte[])] = StandardType.Simple;
            #endregion

            #region Type2SimpleStandardTypeMap
            Type2SimpleStandardTypeMap[typeof(string)] = SimpleStandardType.Varchar;

            Type2SimpleStandardTypeMap[typeof(char)] = SimpleStandardType.Char;
            Type2SimpleStandardTypeMap[typeof(byte)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(sbyte)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(short)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(ushort)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(int)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(uint)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(long)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(ulong)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(float)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(double)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(decimal)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(bool)] = SimpleStandardType.Boolean;
            Type2SimpleStandardTypeMap[typeof(DateTime)] = SimpleStandardType.DateTime;
            Type2SimpleStandardTypeMap[typeof(DateTimeOffset)] = SimpleStandardType.DateTimeOffset;

            Type2SimpleStandardTypeMap[typeof(char?)] = SimpleStandardType.Char;
            Type2SimpleStandardTypeMap[typeof(byte?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(sbyte?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(short?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(ushort?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(int?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(uint?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(long?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(ulong?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(float?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(double?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(decimal?)] = SimpleStandardType.Numeric;
            Type2SimpleStandardTypeMap[typeof(bool?)] = SimpleStandardType.Boolean;
            Type2SimpleStandardTypeMap[typeof(DateTime?)] = SimpleStandardType.DateTime;
            Type2SimpleStandardTypeMap[typeof(DateTimeOffset?)] = SimpleStandardType.DateTimeOffset;

            Type2SimpleStandardTypeMap[typeof(byte[])] = SimpleStandardType.Binary;
            #endregion
        }

        public static StandardType GetStandardType(object obj)
        {
            Type type = obj is Type type1 ? type1 : obj.GetType();
            if (Type2StandardTypeMap.ContainsKey(type))
                return Type2StandardTypeMap[type];
            if (typeof(IDictionary).IsAssignableFrom(type))
                return StandardType.Dictionary;
            if (type.IsEnum)
                return StandardType.Simple;
            if (type.IsClass)
                return StandardType.Class;

            throw new SqlException("未知标准类型：" + type.ToString());
        }

        public static SimpleStandardType GetSimpleStandardType(object obj)
        {
            Type type = obj is Type type1 ? type1 : obj.GetType();
            if (Type2SimpleStandardTypeMap.ContainsKey(type))
                return Type2SimpleStandardTypeMap[type];
            if (type.IsEnum)
                return SimpleStandardType.Enum;

            throw new SqlException("未知标准简单类型：" + type.ToString());
        }




        /// <summary>
        /// 数据库字段值=》根据c#类型转成c#值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertToType(object obj, Type type)
        {
            if (obj == null || obj is DBNull)
            {
                return null;
            }

            var result = obj;
            var objType = obj.GetType();
            type = GetTrueType(type);
            if (type == objType || type.IsAssignableFrom(objType))
            {
                //objType是type子类
                return result;
            }

            try
            {
                var stype = GetSimpleStandardType(type);
                switch (stype)
                {
                    case SimpleStandardType.None:
                        {
                            throw new SqlException("未知标准简单类型：" + type.ToString());
                        }

                    case SimpleStandardType.Varchar:
                    case SimpleStandardType.Text:
                    case SimpleStandardType.Char:
                        {
                            //转换成字符
                            var sobjtype = GetSimpleStandardType(objType);
                            if (sobjtype == SimpleStandardType.DateTime)
                            {
                                var obj1 = (DateTime)obj;
                                result = obj1.ToString(DateTimeFormat);
                            }
                            else if (sobjtype == SimpleStandardType.DateTimeOffset)
                            {
                                var obj1 = (DateTimeOffset)obj;
                                result = obj1.ToString(DateTimeOffsetFormat);
                            }
                            else
                            {
                                result = Convert.ChangeType(obj, type);
                            }
                        }
                        break;
                    case SimpleStandardType.Numeric:
                    case SimpleStandardType.DateTime:
                    case SimpleStandardType.DateTimeOffset:
                        {
                            result = Convert.ChangeType(obj, type);
                        }
                        break;

                    case SimpleStandardType.Enum:
                        {
                            if (int.TryParse(obj.ToString(), out int temp))
                            {
                                result = Enum.ToObject(type, temp);
                            }
                            else
                            {
                                result = Enum.Parse(type, obj.ToString());
                            }
                        }
                        break;
                    case SimpleStandardType.Boolean:
                        {
                            if (obj == null) result = null;
                            if (obj.ToString().ToLower() == "true" || obj.ToString().ToLower() == "1") result = true;
                            else result = false;
                        }
                        break;
                    case SimpleStandardType.Binary:
                        {
                            result = (byte[])obj;
                        }
                        break;
                }
            }
            catch
            {
                throw new SqlException($"无法将值{obj ?? "null"}转换为{type}");
            }

            return result;
        }
        private static Type GetTrueType(Type type)
        {
            if (
                type == typeof(string) ||
                type.IsEnum ||

                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal) ||
                type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(byte[])
                )
            {
                return type;
            }

            Type type1 = type;

            if (type1 == typeof(byte?)) type1 = typeof(byte);
            else if (type1 == typeof(sbyte?)) type1 = typeof(sbyte);
            else if (type1 == typeof(short?)) type1 = typeof(short);
            else if (type1 == typeof(ushort?)) type1 = typeof(ushort);
            else if (type1 == typeof(int?)) type1 = typeof(int);
            else if (type1 == typeof(uint?)) type1 = typeof(uint);
            else if (type1 == typeof(long?)) type1 = typeof(long);
            else if (type1 == typeof(ulong?)) type1 = typeof(ulong);
            else if (type1 == typeof(float?)) type1 = typeof(float);
            else if (type1 == typeof(double?)) type1 = typeof(double);
            else if (type1 == typeof(decimal?)) type1 = typeof(decimal);
            else if (type1 == typeof(bool?)) type1 = typeof(bool);
            else if (type1 == typeof(char?)) type1 = typeof(char);
            else if (type1 == typeof(DateTime?)) type1 = typeof(DateTime);
            else if (type1 == typeof(DateTimeOffset?)) type1 = typeof(DateTimeOffset);

            return type1;
        }
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string DateTimeOffsetFormat = "yyyy-MM-dd HH:mm:ss";



        /// <summary>
        /// 将C#值=》处理成数据库字段值
        /// 枚举=》转字符
        /// 布尔=》转字符(true 1;false 0;未知 null)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ResolveParamValue(object value)
        {
            if (value == null) return DBNull.Value;

            var obj = value;
            var type = TypeMap.GetSimpleStandardType(value);
            switch (type)
            {
                case SimpleStandardType.None:
                    {
                        throw new SqlException("未知标准简单类型：" + value.GetType().ToString());
                    }
                case SimpleStandardType.Numeric:
                    break;
                case SimpleStandardType.Varchar:
                    break;
                case SimpleStandardType.Text:
                    break;
                case SimpleStandardType.Char:
                    break;
                case SimpleStandardType.Enum:
                    {
                        obj = value.ToString();
                    }
                    break;
                case SimpleStandardType.DateTime:
                    break;
                case SimpleStandardType.DateTimeOffset:
                    break;
                case SimpleStandardType.Boolean:
                    {
                        if (obj == null) obj = null;
                        else if ((bool)obj) obj = "1";
                        else obj = "0";
                    }
                    break;
                case SimpleStandardType.Binary:
                    break;
            }
            return obj;
        }





    }
}