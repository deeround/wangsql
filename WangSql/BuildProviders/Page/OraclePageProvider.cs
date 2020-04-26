using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Page
{
    public class OraclePageProvider : DefaultPageProvider, IPageProvider
    {
        public override int BuildPageCountSql<T>(ISqlExe sqlMapper, string sql, object param)
        {
            sql = $"SELECT COUNT(*) FROM ({sql}) llll";
            return sqlMapper.Scalar<int>(sql, param);
        }
        public override IEnumerable<T> BuildPageSql<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize)
        {
            if (pageIndex == 1)
            {
                sql = $@"SELECT llll.*, ROWNUM FROM ({sql}) llll WHERE ROWNUM <= {pageSize}";
            }
            else
            {
                sql = $@"SELECT lllll.* FROM (SELECT llll.*,ROWNUM FROM ({sql}) llll) lllll WHERE ROWNUM BETWEEN {(pageIndex - 1) * pageSize} AND {pageIndex * pageSize + 1}";
            }
            return sqlMapper.Query<T>(sql, param);
        }
    }
}
