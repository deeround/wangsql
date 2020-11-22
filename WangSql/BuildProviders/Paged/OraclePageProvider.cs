using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangSql.BuildProviders.Paged
{
    public class OraclePageProvider : DefaultPageProvider, IPageProvider
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
                sql = $@"SELECT llll.*, ROWNUM FROM ({sql}) llll WHERE ROWNUM <= {pageSize}";
            }
            else
            {
                sql = $@"SELECT lllll.* FROM (SELECT llll.*,ROWNUM RN FROM ({sql}) llll) lllll WHERE RN > {(pageIndex - 1) * pageSize } AND RN <= {pageIndex * pageSize }";
            }
            return sqlExe.Query<T>(sql, param);
        }
        public override async Task<IEnumerable<T>> PageQueryAsync<T>(string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.*, ROWNUM FROM ({sql}) llll WHERE ROWNUM <= {pageSize}";
            }
            else
            {
                sql = $@"SELECT lllll.* FROM (SELECT llll.*,ROWNUM RN FROM ({sql}) llll) lllll WHERE RN > {(pageIndex - 1) * pageSize } AND RN <= {pageIndex * pageSize }";
            }
            return await sqlExe.QueryAsync<T>(sql, param);
        }
    }
}
