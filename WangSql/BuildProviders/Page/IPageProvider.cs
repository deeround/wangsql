using System.Collections.Generic;

namespace WangSql.BuildProviders.Page
{
    public interface IPageProvider
    {
        IPageProvider Instance(ISqlExe sqlMapper);
        int BuildPageCountSql<T>(string sql, object param);
        IEnumerable<T> BuildPageSql<T>(string sql, object param, int pageIndex, int pageSize);
    }
}