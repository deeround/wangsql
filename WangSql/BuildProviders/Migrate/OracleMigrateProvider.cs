using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class OracleMigrateProvider : DefaultMigrateProvider, IMigrateProvider
    {
        public override void Run()
        {
            var tables = TableMap.GetMaps().Where(x => x.AutoCreate).ToList();

            if (sqlMapper != null)
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    var sqls = CreateSql(table);
                    using (var trans = sqlMapper.BeginTransaction())
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
            else
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    var sqls = CreateSql(table);
                    foreach (var item in sqls)
                    {
                        sqlExe.Execute(item, null);
                    }
                }
            }
        }

        private IList<string> CreateSql(TableInfo table)
        {
            if (table == null || table.Columns == null || !table.Columns.Any()) return new List<string>();

            //判断表是否存在
            if (ExsitTable(table.Name)) return new List<string>();

            IList<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            //表结构
            sb.AppendLine($"create table {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)}(");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var item = table.Columns[i];
                ResolveColumnInfo(item);
                string defaultValue = item.DefaultValue == null ? "" : (item.DefaultValue is string) ? $"'{item.DefaultValue}'" : $"{item.DefaultValue}";
                if (i == table.Columns.Count - 1)
                {
                    sb.AppendLine($"{sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} {ResolveDataType(item)} {(item.IsNotNull ? "not null" : "")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"default {defaultValue}")}");
                }
                else
                {
                    sb.AppendLine($"{sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} {ResolveDataType(item)} {(item.IsNotNull ? "not null" : "")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"default {defaultValue}")},");
                }
            }
            sb.AppendLine(")");
            result.Add(sb.ToString());
            //主键
            if (table.Columns.Any(x => x.IsPrimaryKey))
            {
                //ALTER TABLE table_name
                //ADD CONSTRAINT MyPrimaryKey PRIMARY KEY(column1, column2...);
                result.Add($"alter table {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint pk_{table.Name} primary key({string.Join(",", table.Columns.Where(x => x.IsPrimaryKey).Select(x => sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(x.Name)))})");
            }
            //唯一键
            int ukIndex = 1;
            if (table.Columns.Any(x => x.IsUnique))
            {
                //ALTER TABLE table_name
                //ADD CONSTRAINT MyUniqueConstraint UNIQUE(column1, column2...);
                var ukg = table.Columns.Where(x => x.IsUnique && !string.IsNullOrEmpty(x.UniqueGroup)).Select(x => x.UniqueGroup).Distinct();
                foreach (var item in ukg)
                {
                    result.Add($"alter table {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint uk_{table.Name}_{ukIndex} unique({string.Join(",", table.Columns.Where(x => x.IsUnique && x.UniqueGroup == item).Select(x => sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(x.Name)))})");
                    ukIndex++;
                }
                var ukColumns = table.Columns.Where(x => x.IsUnique && string.IsNullOrEmpty(x.UniqueGroup));
                foreach (var item in ukColumns)
                {
                    result.Add($"alter table {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint uk_{table.Name}_{ukIndex} unique({sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)})");
                    ukIndex++;
                }
            }
            //注释
            if (!string.IsNullOrEmpty(table.Comment))
            {
                result.Add($"comment on table {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} is '{table.Comment}'");
            }
            foreach (var item in table.Columns)
            {
                if (!string.IsNullOrEmpty(item.Comment))
                {
                    result.Add($"comment on column {sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)}.{sqlExe.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} is '{item.Comment}'");
                }
            }
            return result;
        }

        private bool ExsitTable(string tableName)
        {
            string sql = $"select count(*) from user_tables where table_name = '{(sqlExe.SqlFactory.DbProvider.UseQuotationInSql ? tableName : tableName.ToUpper())}'";
            var count = sqlExe.Scalar<int>(sql, null);
            return count > 0;
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
                            return $"number({column.Length},{column.Precision})";
                        }
                        else
                        {
                            return $"number({column.Length})";
                        }
                    }
                case SimpleStandardType.Varchar:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        return $"nvarchar2({column.Length})";
                    }
                case SimpleStandardType.Text:
                    {
                        return $"clob";
                    }
                case SimpleStandardType.Char:
                    {
                        column.Length = 1;
                        return $"nvarchar2(1)";
                    }
                case SimpleStandardType.Enum:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        return $"nvarchar2({column.Length})";
                    }
                case SimpleStandardType.DateTime:
                    {
                        return $"timestamp";
                    }
                case SimpleStandardType.DateTimeOffset:
                    {
                        return $"timestamp with time zone";
                    }
                case SimpleStandardType.Boolean:
                    {
                        column.Length = 1;
                        return $"nvarchar2(1)";
                    }
                case SimpleStandardType.Binary:
                    {
                        return $"blob";
                    }
                default:
                    {
                        throw new SqlException("不支持数据类型:" + column.PropertyType.ToString());
                    }
            }
        }
    }
}
