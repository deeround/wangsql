using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql.BuildProviders.Paged
{
    public class SqlitePageProvider : DefaultPageProvider, IPageProvider
    {
        public override int PageCount(string sql, object param)
        {
            sql = $"SELECT COUNT(*) FROM ({sql}) llll";
            return sqlExe.Scalar<int>(sql, param);
        }
        public override async Task<int> PageCountAsync(string sql, object param)
        {
            sql = $"SELECT COUNT(*) FROM ({sql}) llll";
            return await sqlExe.ScalarAsync<int>(sql, param);
        }
        public override IEnumerable<T> PageQuery<T>(string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return sqlExe.Query<T>(sql, param);
        }
        public override async Task<IEnumerable<T>> PageQueryAsync<T>(string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize}";
            }
            else
            {
                sql = $@"SELECT llll.* FROM ({sql}) llll LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            }
            return await sqlExe.QueryAsync<T>(sql, param);
        }
    }
}
