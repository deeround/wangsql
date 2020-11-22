using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace WangSql
{
    public class SqlFactory
    {
        private readonly string _name;

        public DbProvider DbProvider { get { return DbProviderManager.Get(_name); } }

        public ParamMap ParamMap { get; }

        public ResultMap ResultMap { get; }

        public SqlFactory()
        {
            ParamMap = new ParamMap();
            ResultMap = new ResultMap();
        }

        public SqlFactory(string name)
        {
            _name = name;
            ParamMap = new ParamMap();
            ResultMap = new ResultMap();
        }

        public DbCommand CreateCommand(DbConnection conn, string sql, object param, CommandType commandType, int? timeout = null)
        {
            //调试日志
            if (DbProvider.Debug)
            {
                Debug.WriteLine("处理前SQL:" + sql);
            }
            var cmd = conn.CreateCommand();
            if (timeout != null) cmd.CommandTimeout = (int)timeout;
            ParamMap.GetCacheMap(DbProvider, sql).Prepare(cmd, param, commandType);
            //调试日志
            if (DbProvider.Debug)
            {
                Debug.WriteLine("处理后SQL:" + cmd.CommandText);
                StringBuilder sb = new StringBuilder();
                var ps = cmd.Parameters;
                if (ps != null && ps.Count > 0)
                {
                    foreach (IDbDataParameter item in ps)
                    {
                        sb.Append($"\"{item.ParameterName}\":\"{item.Value?.ToString()}\",");
                    }
                }
                Debug.WriteLine("处理后参数:" + "{" + sb.ToString().TrimEnd(',') + "}");
            }
            return cmd;
        }

        public DbConnection CreateConnection(bool isReadDb)
        {
            var conn = DbProvider.CreateConnection(isReadDb);
            return conn;
        }
    }
}
