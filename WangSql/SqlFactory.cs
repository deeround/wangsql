using System.Data;
using System.Data.Common;

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
            var cmd = conn.CreateCommand();
            if (timeout != null) cmd.CommandTimeout = (int)timeout;
            ParamMap.GetCacheMap(DbProvider, sql, commandType).Prepare(cmd, param);
            return cmd;
        }

        public DbConnection CreateConnection(bool isReadDb)
        {
            var conn = DbProvider.CreateConnection(isReadDb);
            return conn;
        }
    }
}
