using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class SqliteMigrateProvider : DefaultMigrateProvider, IMigrateProvider
    {
        private SqlMapper _sqlMapper;

        public override void Run(SqlMapper sqlMapper)
        {
            _sqlMapper = sqlMapper;
            var tables = TableMap.GetMaps().Where(x => x.AutoCreate).ToList();
            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i];
                var sqls = CreateSql(table);
                using (var trans = _sqlMapper.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in sqls)
                        {
                            trans.Execute(item, null);
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SqlException(ex.Message);
                    }
                }
            }
        }

        private IList<string> CreateSql(TableInfo table)
        {
            if (table == null || table.Columns == null || !table.Columns.Any()) return new List<string>();

            IList<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            //表结构
            sb.AppendLine($"create table if not exists {_sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)}(");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var item = table.Columns[i];
                ResolveColumnInfo(item);
                string colSql = $"{_sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} {ResolveDataType(item)} {(item.IsNotNull ? "not null" : "")}";
                if (item.IsPrimaryKey)
                {
                    colSql += " primary key";
                }

                if (item.IsUnique)
                {
                    colSql += " unique";
                }

                if (i < table.Columns.Count - 1)
                {
                    colSql += ",";
                }

                sb.AppendLine(colSql);
            }
            sb.AppendLine(")");
            result.Add(sb.ToString());
            return result;
        }

        private void ResolveColumnInfo(ColumnInfo column)
        {
            if (column.DataType != SimpleStandardType.None) return;

            column.DataType = TypeMap.GetSimpleStandardType(column.PropertyType);
        }

        private string ResolveDataType(ColumnInfo column)
        {
            switch (column.DataType)
            {
                case SimpleStandardType.None:
                    {
                        throw new SqlException("不支持数据类型:" + column.PropertyType.ToString());
                    }
                case SimpleStandardType.Numeric:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        if (column.Precision != null)
                        {
                            return $"numeric({column.Length},{column.Precision})";
                        }
                        else
                        {
                            return $"numeric({column.Length})";
                        }
                    }
                case SimpleStandardType.Varchar:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        return $"varchar({column.Length})";
                    }
                case SimpleStandardType.Text:
                    {
                        return $"text";
                    }
                case SimpleStandardType.Char:
                    {
                        column.Length = 1;
                        return $"varchar(1)";
                    }
                case SimpleStandardType.Enum:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        return $"varchar({column.Length})";
                    }
                case SimpleStandardType.DateTime:
                    {
                        return $"datetime";
                    }
                case SimpleStandardType.DateTimeOffset:
                    {
                        return $"datetime";
                    }
                case SimpleStandardType.Boolean:
                    {
                        column.Length = 1;
                        return $"varchar(1)";
                    }
                default:
                    {
                        throw new SqlException("不支持数据类型:" + column.PropertyType.ToString());
                    }
            }
        }
    }
}
