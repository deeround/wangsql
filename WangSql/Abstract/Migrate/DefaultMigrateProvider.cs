using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql.Abstract.Migrate
{
    public class DefaultMigrateProvider : IMigrateProvider
    {
        public virtual void Init(ISqlMapper sqlMapper)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Init(ISqlExe sqlMapper)
        {
            throw new System.NotImplementedException();
        }
    }
}
