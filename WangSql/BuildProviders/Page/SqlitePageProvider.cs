using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Page
{
    public class SqlitePageProvider : DefaultPageProvider, IPageProvider
    {
        public override int BuildPageCountSql<T>(string sql, object param)
        {
            sql = $"SELECT COUNT(*) FROM ({sql}) llll";
            return sqlMapper.Scalar<int>(sql, param);
        }
        public override IEnumerable<T> BuildPageSql<T>(string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return sqlMapper.Query<T>(sql, param);
        }
    }
}
