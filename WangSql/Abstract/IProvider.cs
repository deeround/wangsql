using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.Abstract
{
    public interface IProvider
    {
        void Init();
        void Init(ISqlExe sqlMapper);
    }
}
