using System.Collections.Generic;
using System.Threading.Tasks;

namespace WangSql.BuildProviders.Paged
{
    public interface IPageProvider
    {
        IPageProvider Instance(ISqlExe sqlExe);
        IPageProvider Instance(ISqlMapper sqlMapper);
        int PageCount(string sql, object param);
        Task<int> PageCountAsync(string sql, object param);
        IEnumerable<T> PageQuery<T>(string sql, object param, int pageIndex, int pageSize);
        Task<IEnumerable<T>> PageQueryAsync<T>(string sql, object param, int pageIndex, int pageSize);
    }
}