using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WangSql.Abstract.Migrate
{
    public class DefaultMigrateProvider : IMigrateProvider
    {
        #region constructor
        protected ISqlExe _sqlMapper { get; set; }
        public virtual void Init()
        {
        }
        public virtual void Init(ISqlExe sqlMapper)
        {
            _sqlMapper = sqlMapper;
        }
        #endregion

        public virtual void CreateTable()
        {
            throw new System.NotImplementedException();
        }

        public virtual void BuildModel()
        {
            throw new System.NotImplementedException();
        }
    }
}
