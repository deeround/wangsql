using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WangSql
{
    public class ParamHandler
    {
        private readonly IList<ParamKey> _paramKeys = new List<ParamKey>();
        private readonly DbProvider _dbProvider;
        private readonly string _sql;

        public ParamHandler(DbProvider dbProvider, string sql)
        {
            _dbProvider = dbProvider;
            _sql = sql;
            var regex = new Regex(@"[#\$]([\s\S]*?)[#\$]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var ms = regex.Matches(sql);
            foreach (Match item in ms)
            {
                if (item.Groups.Count > 0 && !string.IsNullOrEmpty(item.Groups[1].Value))
                {
                    var v1 = item.Value;
                    var v2 = item.Groups[1].Value;
                    var v3 = v1.StartsWith("#") ? "#" : "$";

                    //if (!_paramKeys.Any(op => op.FullName.Equals(v1)))
                    //{
                    //    ParamKey k = new ParamKey(v1, v2, v3);
                    //    _paramKeys.Add(k);
                    //}
                    ParamKey k = new ParamKey(v1, v2, v3);
                    _paramKeys.Add(k);
                }
            }
        }

        public CommandType CommandType { get; private set; }

        public IList<IDbDataParameter> PrepareParameters { get; private set; }

        public string PrepareSql { get; private set; }

        public void Prepare(IDbCommand cmd, object param, CommandType commandType)
        {
            PrepareSql = _sql;
            CommandType = commandType;

            if (param != null)
            {
                if (commandType == CommandType.Text)
                {
                    var type = TypeMap.GetStandardType(param);
                    switch (type)
                    {
                        case StandardType.Dictionary:
                            SerializerDictionary(cmd, (IDictionary)param);
                            break;

                        case StandardType.Simple:
                            SerializerSimple(cmd, param);
                            break;

                        case StandardType.Class:
                            SerializerClass(cmd, param);
                            break;
                    }
                }
                else if (commandType == CommandType.StoredProcedure)
                {
                    throw new SqlException("暂不支持存储过程");
                }
            }

            cmd.CommandText = PrepareSql;
            cmd.CommandType = CommandType;
            if (PrepareParameters != null && PrepareParameters.Count > 0)
                PrepareParameters.ToList().ForEach(op => { cmd.Parameters.Add(op); });
        }



        //private void CheckParamValue(object value)
        //{
        //    return;

        //    //if (value == null || value is DBNull) return;
        //    //if (!(value is string)) return;

        //    //var vs = value.ToString().ToLower();
        //    //if (string.IsNullOrEmpty(vs)) return;
        //    //string[] ds =
        //    //{
        //    //    "select", "insert", "delete", "update", "drop", "truncate", "declare", "exec", "script", "master",
        //    //    "mid", "net user", "and", "or", "join"
        //    //};
        //    //if (ds.Any(item => vs.IndexOf(item, StringComparison.Ordinal) != -1))
        //    //{
        //    //    throw new SqlException("执行SQL中包含危险字符：" + vs);
        //    //}
        //}

        private bool DictionaryContainsKey(IDictionary param, string key)
        {
            foreach (var item in param.Keys)
            {
                if (item.ToString().ToUpper().Equals(key.ToUpper())) return true;
            }

            return false;
        }

        private object DictionaryGetValue(IDictionary param, string key)
        {
            foreach (var item in param.Keys)
            {
                if (item.ToString().ToUpper().Equals(key.ToUpper()))
                {
                    return param[item];
                }
            }

            return null;
        }

        private void SerializerClass(IDbCommand cmd, object param)
        {
            if (param == null) return;
            var param1 = new Dictionary<string, object>();
            param.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                .ToList().ForEach(op => param1.Add(op.Name, op.GetValue(param, null)));
            SerializerDictionary(cmd, param1);
        }

        private void SerializerDictionary(IDbCommand cmd, IDictionary param)
        {
            PrepareParameters = new List<IDbDataParameter>();
            if (param == null || param.Count == 0) return;
            int pi = 1;
            foreach (var item in _paramKeys)
            {
                if (!DictionaryContainsKey(param, item.Name)) throw new SqlException($"参数{item.Name}未绑定值");

                if (item.Type == "#")
                {
                    Regex regex = new Regex(item.FullName, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var parameterName = _dbProvider.FormatNameForParameter(item.Name) + "_" + (pi++);
                    PrepareSql = regex.Replace(PrepareSql, parameterName, 1);

                    var p = cmd.CreateParameter();
                    p.ParameterName = parameterName;
                    p.Value = TypeMap.ResolveParamValue(DictionaryGetValue(param, item.Name));
                    PrepareParameters.Add(p);

                }
                else
                {
                    var obj = TypeMap.ResolveParamValue(DictionaryGetValue(param, item.Name));
                    //CheckParamValue(obj);
                    PrepareSql = PrepareSql.Replace(item.FullName, obj?.ToString());
                }
            }
        }

        private void SerializerSimple(IDbCommand cmd, object param)
        {
            PrepareParameters = new List<IDbDataParameter>();
            if (param == null) return;
            int pi = 1;
            foreach (var item in _paramKeys)
            {
                if (item.Type == "#")
                {
                    Regex regex = new Regex(item.FullName, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var parameterName = _dbProvider.FormatNameForParameter(item.Name) + "_" + (pi++);
                    PrepareSql = regex.Replace(PrepareSql, parameterName, 1);

                    var p = cmd.CreateParameter();
                    p.ParameterName = parameterName;
                    p.Value = TypeMap.ResolveParamValue(param);
                    PrepareParameters.Add(p);
                }
                else
                {
                    var obj = TypeMap.ResolveParamValue(param);
                    //CheckParamValue(obj);
                    PrepareSql = PrepareSql.Replace(item.FullName, obj == null ? string.Empty : obj.ToString());
                }
            }
        }

        class ParamKey
        {
            public ParamKey(string v1, string v2, string v3)
            {
                FullName = v1;
                Name = v2;
                Type = v3;
            }

            public string FullName { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }

    public class ParamMap
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, ParamHandler> ParamHandlerCache = new ConcurrentDictionary<string, ParamHandler>();
        private static readonly int ParamHandlerCacheSize = 100000;

        public ParamHandler GetCacheMap(DbProvider dbProvider, string sql)
        {
            string code = Utils.GetHashCode(dbProvider.ToString() + sql);

            //存在
            if (ParamHandlerCache.ContainsKey(code))
            {
                return ParamHandlerCache[code];
            }
            //不存在
            {
                //缓存对象大小判断
                if (ParamHandlerCache.Count > ParamHandlerCacheSize)
                {
                    //暂时先全部移除
                    ParamHandlerCache.Clear();
                }

                var handler = new ParamHandler(dbProvider, sql);
                ParamHandlerCache[code] = handler;
                return handler;
            }
        }
    }
}