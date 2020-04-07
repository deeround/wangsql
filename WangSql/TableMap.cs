﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WangSql
{
    public static class TableMap
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TableInfo> Maps = new ConcurrentDictionary<Type, TableInfo>();
        private static readonly int MapsLength = 50000;

        static TableMap()
        {
            InitMap();
        }
        private static void InitMap()
        {
            var types = new List<Type>();//动态获取所有程序集类型（排除框架）
            System.IO.Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(
                    x => !x.StartsWith("Microsoft") &&
                         !x.StartsWith("System") &&
                         !x.StartsWith("runtime") &&
                         !x.StartsWith("Newtonsoft") &&
                         !x.StartsWith("AutoMapper") &&
                         !x.StartsWith("FluentValidation") &&
                         !x.StartsWith("NLog") &&
                         !x.StartsWith("WangSql") &&
                         !x.StartsWith("Swashbuckle"))
                .ToList()
                .ForEach(item =>
                {
                    types.AddRange(
                        Assembly.LoadFile(item).GetTypes()
                        .Select(x => x.AssemblyQualifiedName)
                        .Select(x => Type.GetType(x))
                        .Where(op => op.GetCustomAttributes(typeof(TableAttribute), false).Any())
                    );
                });

            foreach (var type in types)
            {
                var map = GetMap(type);
                SetMap(type, map);
            }
        }

        public static TableMapTable<T> Entity<T>() where T : class
        {
            return new TableMapTable<T>();
        }

        public static TableInfo GetMap<T>() where T : class
        {
            var type = typeof(T);
            return GetMap(type);
        }

        public static TableInfo SetMap<T>(TableInfo value) where T : class
        {
            var type = typeof(T);
            return SetMap(type, value);
        }

        public static TableInfo ClearMap<T>()
        {
            var type = typeof(T);
            return ClearMap(type);
        }

        public static IList<TableInfo> GetMaps()
        {
            return Maps.Select(x => x.Value).ToList();
        }


        private static TableInfo GetMap(Type type)
        {
            TableInfo result;
            if (Maps.ContainsKey(type))
            {
                result = Maps[type];
            }
            else
            {
                result = GetTableInfo(type);
                if (Maps.Count > MapsLength)
                {
                    Maps.Clear();
                }
                Maps[type] = result;
            }
            return result;
        }

        private static TableInfo SetMap(Type type, TableInfo value)
        {
            if (Maps.ContainsKey(type))
            {
                Maps[type] = value;
            }
            else
            {
                if (Maps.Count > MapsLength)
                {
                    Maps.Clear();
                }
                Maps[type] = value;
            }

            return value;
        }

        private static TableInfo ClearMap(Type type)
        {
            TableInfo result = GetTableInfo(type);
            if (Maps.Count > MapsLength)
            {
                Maps.Clear();
            }
            Maps[type] = result;
            return result;
        }

        private static TableInfo GetTableInfo(Type type)
        {
            TableInfo result = new TableInfo();
            var table = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute), false);
            if (table == null)//没有特性
            {
                table = new TableAttribute
                {
                    Name = type.Name
                };
            }
            result.Name = string.IsNullOrEmpty(table.Name) ? type.Name : table.Name;
            result.ClassName = type.Name;
            result.ClassType = type;
            result.Comment = table.Comment;
            result.AutoCreate = table.AutoCreate;
            foreach (var item in type.GetProperties())
            {
                var ignoreColumn = (IgnoreColumnAttribute)item.GetCustomAttribute(typeof(IgnoreColumnAttribute), false);
                if (ignoreColumn != null)
                {
                    continue;
                }
                var column = (ColumnAttribute)item.GetCustomAttribute(typeof(ColumnAttribute), false);
                if (column == null)
                {
                    column = new ColumnAttribute
                    {
                        Name = item.Name
                    };
                }
                result.Columns.Add(new ColumnInfo()
                {
                    Name = string.IsNullOrEmpty(column.Name) ? item.Name : column.Name,
                    PropertyName = item.Name,
                    PropertyType = item.PropertyType,
                    Comment = column.Comment,
                    DataType = column.DataType,
                    Length = column.Length,
                    Precision = column.Precision,
                    IsNotNull = column.IsNotNull,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsUnique = column.IsUnique,
                    UniqueGroup = column.UniqueGroup
                });
            }

            //如果没有设置主键，则将第一个含有ID的字段当做主键
            if (!result.Columns.Any(op => op.IsPrimaryKey))
            {
                var property = result.Columns.FirstOrDefault(op => op.Name.ToUpper().Contains("ID"));
                if (property != null)
                {
                    property.IsPrimaryKey = true;
                }
            }

            return result;
        }

    }

    public class TableMapTable<T> where T : class
    {
        /// <summary>
        ///     清除列相关配置信息
        /// </summary>
        /// <returns></returns>
        public TableMapTable<T> Clear()
        {
            TableMap.ClearMap<T>();
            return this;
        }

        /// <summary>
        ///     设置表名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TableMapTable<T> ToTable(string name = null, string comment = null, bool autoCreate = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }
            var map = TableMap.GetMap<T>();
            map.Name = name;
            map.Comment = comment;
            map.AutoCreate = autoCreate;
            TableMap.SetMap<T>(map);
            return this;
        }

        /// <summary>
        ///     设置列名
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public TableMapColumn<T> HasColumn(Expression<Func<T, object>> expression, string name = null)
        {
            var key = GetPropertyName(expression);
            return new TableMapColumn<T>(key).Name(name);
        }

        /// <summary>
        ///     设置主键
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TableMapTable<T> IgnoreColumn(Expression<Func<T, object>> expression)
        {
            var map = TableMap.GetMap<T>();
            var key = GetPropertyName(expression);
            if (!string.IsNullOrEmpty(key))
            {
                var property = map.Columns.FirstOrDefault(op => op.PropertyName == key);
                if (property != null)
                {
                    map.Columns.Remove(property);
                }
            }

            TableMap.SetMap<T>(map);
            return this;
        }

        private string GetPropertyName(Expression<Func<T, object>> fields)
        {
            string columns = null;
            if (fields.Body is NewExpression exp1)
            {
                foreach (var item in exp1.Members)
                {
                    columns += item.Name + ",";
                }

                columns = columns?.TrimEnd(',');
            }
            else if (fields.Body is MemberExpression exp2)
            {
                columns = exp2.Member.Name;
            }
            else if (fields.Body is UnaryExpression exp3)    //表示具有一元运算符的表达式
            {
                var exp31 = exp3.Operand as MemberExpression;
                columns = exp31.Member.Name;
            }

            return columns?.Split(',').First();
        }
    }
    public class TableMapColumn<T> where T : class
    {
        private readonly string _propertyName;

        public TableMapColumn(string propertyName)
        {
            _propertyName = propertyName;
        }

        public TableMapColumn<T> Name(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = _propertyName;
            }
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.Name = name;
            return this;
        }
        public TableMapColumn<T> Comment(string comment)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.Comment = comment;
            return this;
        }
        public TableMapColumn<T> DataType(SimpleStandardType dataType)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.DataType = dataType;
            return this;
        }
        public TableMapColumn<T> Length(int? length)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.Length = length;
            return this;
        }
        public TableMapColumn<T> Precision(int? precision)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.Precision = precision;
            return this;
        }
        public TableMapColumn<T> IsNotNull()
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.IsNotNull = true;
            return this;
        }
        public TableMapColumn<T> DefaultValue(object defaultValue)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.DefaultValue = defaultValue;
            return this;
        }
        public TableMapColumn<T> IsPrimaryKey()
        {
            var columns = TableMap.GetMap<T>().Columns;
            columns.Where(x => x.IsPrimaryKey).ToList().ForEach(x => { x.IsPrimaryKey = false; });
            var map = columns.First(x => x.PropertyName == _propertyName);
            map.IsPrimaryKey = true;
            return this;
        }
        public TableMapColumn<T> IsUnique()
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.IsUnique = true;
            return this;
        }
        public TableMapColumn<T> UniqueGroup(string uniqueGroup)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.UniqueGroup = uniqueGroup;
            return this;
        }
        public TableMapColumn<T> IsIndex()
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.IsIndex = true;
            return this;
        }
        public TableMapColumn<T> IndexGroup(string indexGroup)
        {
            var map = TableMap.GetMap<T>().Columns.First(x => x.PropertyName == _propertyName);
            map.IndexGroup = indexGroup;
            return this;
        }
    }
}