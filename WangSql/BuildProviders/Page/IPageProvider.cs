using System.Collections.Generic;

namespace WangSql.BuildProviders.Page
{
    public interface IPageProvider
    {
        int BuildPageCountSql<T>(ISqlExe sqlMapper, string sql, object param);
        IEnumerable<T> BuildPageSql<T>(ISqlExe sqlMapper, string sql, object param, int pageIndex, int pageSize);
    }
}