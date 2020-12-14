using System.Collections.Generic;
using System.Threading.Tasks;

namespace WangSql
{
    public interface IPageProvider
    {
        IPageProvider Instance(ISqlExe sqlExe);
        IPageProvider Instance(ISqlMapper sqlMapper);
        IEnumerable<T> QueryPage<T>(string sql, object param, int pageIndex, int pageSize);
        Task<IEnumerable<T>> QueryPageAsync<T>(string sql, object param, int pageIndex, int pageSize);
    }
}