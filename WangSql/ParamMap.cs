using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WangSql
{
    public class ParamHandler
    {
        struct ParamKey
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

        private readonly List<ParamKey> _paramKeys;
        private readonly DbProvider _dbProvider;
        private readonly string _sql;

        public ParamHandler(DbProvider dbProvider, string sql, CommandType commandType)
        {
            _paramKeys = new List<ParamKey>();
            _dbProvider = dbProvider;
            _sql = sql;
            CommandType = commandType;
            var regex = new Regex(@"[#\$]([\s\S]*?)[#\$]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var ms = regex.Matches(sql);
            foreach (Match item in ms)
            {
                if (item.Groups.Count > 0 && !string.IsNullOrEmpty(item.Groups[1].Value))
                {
                    var v1 = item.Value;
                    var v2 = item.Groups[1].Value;
                    var v3 = v1.StartsWith("#") ? "#" : "$";

                    ParamKey k = new ParamKey(v1, v2, v3);
                    _paramKeys.Add(k);
                }
            }
        }

        public CommandType CommandType { get; private set; }

        public string PrepareSql { get; private set; }

        public List<IDbDataParameter> PrepareParameters { get; private set; }

        public void Prepare(IDbCommand cmd, object param)
        {
            PrepareSql = _sql;
            PrepareParameters = new List<IDbDataParameter>();
            if (param != null)
            {
                if (CommandType == CommandType.Text)
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
                else if (CommandType == CommandType.StoredProcedure)
                {
                    throw new SqlException("暂不支持存储过程");
                }
            }

            cmd.CommandText = PrepareSql;
            cmd.CommandType = CommandType;
            if (PrepareParameters != null && PrepareParameters.Count > 0)
                PrepareParameters.ToList().ForEach(op => { cmd.Parameters.Add(op); });
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
            if (param == null || param.Count == 0) return;
            int pi = 1;
            foreach (var item in _paramKeys)
            {
                if (!DictionaryContainsKey(param, item.Name)) throw new SqlException($"参数{item.Name}未绑定值");

                if (item.Type == "#")
                {
                    Regex regex = new Regex(item.FullName, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    var parameterValue = TypeMap.ResolveParamValue(DictionaryGetValue(param, item.Name));
                    if (parameterValue is Array arr)
                    {
                        StringBuilder sb = new StringBuilder();
                        int ai = 1;
                        foreach (var a in arr)
                        {
                            //参数名
                            var p1 = _dbProvider.FormatNameForParameter($"param{pi}_{(ai++)}");
                            //参数对象
                            var p = cmd.CreateParameter();
                            p.ParameterName = p1;
                            p.Value = TypeMap.ResolveParamValue(a);
                            PrepareParameters.Add(p);
                            //生成sql部分
                            sb.Append($"{p1},");
                        }
                        var parameterName = sb.ToString().TrimEnd(',');
                        PrepareSql = regex.Replace(PrepareSql, $" ({parameterName})", 1);
                        pi++;
                    }
                    else
                    {
                        var parameterName = _dbProvider.FormatNameForParameter($"param{pi++}");
                        PrepareSql = regex.Replace(PrepareSql, parameterName, 1);
                        var p = cmd.CreateParameter();
                        p.ParameterName = parameterName;
                        p.Value = TypeMap.ResolveParamValue(DictionaryGetValue(param, item.Name));
                        PrepareParameters.Add(p);
                    }
                }
                else
                {
                    var obj = TypeMap.ResolveParamValue(DictionaryGetValue(param, item.Name));
                    if (obj is Array arr)//数组参数处理
                    {
                        var objType = TypeMap.GetCollectionStandardType(obj);
                        if (objType == SimpleStandardType.Numeric)
                        {
                            PrepareSql = PrepareSql.Replace(item.FullName, $" ({(string.Join(",", arr))})");
                        }
                        else
                        {
                            PrepareSql = PrepareSql.Replace(item.FullName, $" ('{(string.Join("','", arr))}')");
                        }
                    }
                    else
                    {
                        PrepareSql = PrepareSql.Replace(item.FullName, obj == null ? "NULL" : obj?.ToString());
                    }
                }
            }
        }

        private void SerializerSimple(IDbCommand cmd, object param)
        {
            if (param == null) return;
            int pi = 1;
            foreach (var item in _paramKeys)
            {
                if (item.Type == "#")
                {
                    Regex regex = new Regex(item.FullName, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    var parameterValue = TypeMap.ResolveParamValue(param);
                    if (parameterValue is Array arr)
                    {
                        StringBuilder sb = new StringBuilder();
                        int ai = 1;
                        foreach (var a in arr)
                        {
                            //参数名
                            var p1 = _dbProvider.FormatNameForParameter($"param{pi}_{(ai++)}");
                            //参数对象
                            var p = cmd.CreateParameter();
                            p.ParameterName = p1;
                            p.Value = TypeMap.ResolveParamValue(a);
                            PrepareParameters.Add(p);
                            //生成sql部分
                            sb.Append($"{p1},");
                        }
                        var parameterName = sb.ToString().TrimEnd(',');
                        PrepareSql = regex.Replace(PrepareSql, $" ({parameterName})", 1);
                        pi++;
                    }
                    else
                    {
                        var parameterName = _dbProvider.FormatNameForParameter($"param{pi++}");
                        PrepareSql = regex.Replace(PrepareSql, parameterName, 1);
                        var p = cmd.CreateParameter();
                        p.ParameterName = parameterName;
                        p.Value = TypeMap.ResolveParamValue(param);
                        PrepareParameters.Add(p);
                    }
                }
                else
                {
                    var obj = TypeMap.ResolveParamValue(param);
                    if (obj is Array arr)//数组参数处理
                    {
                        var objType = TypeMap.GetCollectionStandardType(obj);
                        if (objType == SimpleStandardType.Numeric)
                        {
                            PrepareSql = PrepareSql.Replace(item.FullName, $" ({(string.Join(",", arr))})");
                        }
                        else
                        {
                            PrepareSql = PrepareSql.Replace(item.FullName, $" ('{(string.Join("','", arr))}')");
                        }
                    }
                    else
                    {
                        PrepareSql = PrepareSql.Replace(item.FullName, obj == null ? "NULL" : obj?.ToString());
                    }
                }
            }
        }

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
    }

    public class ParamMap
    {
        /// <summary>
        /// 使用静态变量注意内存溢出
        /// </summary>
        private static readonly ConcurrentDictionary<string, ParamHandler> ParamHandlerCache = new ConcurrentDictionary<string, ParamHandler>();
        private static readonly int ParamHandlerCacheSize = 100000;

        public ParamHandler GetCacheMap(DbProvider dbProvider, string sql, CommandType commandType)
        {
            string code = string.Format("{0}_{1}_{2}", dbProvider.Name, commandType.ToString(), sql);

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

                var handler = new ParamHandler(dbProvider, sql, commandType);
                ParamHandlerCache[code] = handler;
                return handler;
            }
        }
    }
}