using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class PgsqlMigrateProvider : DefaultMigrateProvider, IMigrateProvider
    {
        public override void AddColumn(string tableName, ColumnInfo columnInfo)
        {
            ResolveColumnInfo(columnInfo);
            string defaultValue = columnInfo.DefaultValue == null ? "" : (columnInfo.DefaultValue is string) ? $"'{columnInfo.DefaultValue}'" : $"{columnInfo.DefaultValue}";
            sqlMapper.Execute($"ALTER TABLE {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} ADD COLUMN {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(columnInfo.Name)} {ResolveDataType(columnInfo)} {(columnInfo.IsNotNull ? "not null" : "")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"default {defaultValue}")}", null);
        }

        public override void AddIndex(string tableName, string columnName, string indexName = null, string indexType = null)
        {
            var name = string.IsNullOrEmpty(indexName) ? $"ix_{tableName}_{columnName.Replace(",", "_")}" : indexName;
            var type = string.IsNullOrEmpty(indexType) ? "" : " using " + indexType;
            if (!ExsitIndex(name))
                sqlMapper.Execute($"create index {name} on {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)}{type}({ string.Join(",", columnName.Split(',').Select(x => sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(x)))})", null);
        }

        public override void RenameColumn(string tableName, string oldName, string newName)
        {
            sqlMapper.Execute($"alter table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} rename {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(oldName)} to {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(newName)}", null);
        }

        public override void AlterColumn(string tableName, ColumnInfo columnInfo)
        {
            ResolveColumnInfo(columnInfo);
            sqlMapper.Execute($"ALTER TABLE {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} ALTER COLUMN {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(columnInfo.Name)} type {ResolveDataType(columnInfo)}", null);
        }

        public override void AlterColumnNull(string tableName, string columnName, bool nullable = false)
        {
            if (nullable)
            {
                sqlMapper.Execute($"ALTER TABLE {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} ALTER COLUMN {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(columnName)} DROP NOT NULL", null);
            }
            else
            {
                sqlMapper.Execute($"ALTER TABLE {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} ALTER COLUMN {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(columnName)} SET NOT NULL", null);
            }
        }

        public override void CreateTable(TableInfo tableInfo)
        {
            var sqls = CreateSql(tableInfo);
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

        public override void DropColumn(string tableName, string columnName)
        {
            sqlMapper.Execute($"alter table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} drop column {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(columnName)}", null);
        }

        public override void DropIndex(string tableName, string columnName)
        {
            var name = $"ix_{tableName}_{columnName.Replace(",", "_")}";
            if (ExsitIndex(name))
                sqlMapper.Execute($"DROP INDEX {name}", null);
        }

        public override void DropIndex(string indexName)
        {
            if (ExsitIndex(indexName))
                sqlMapper.Execute($"DROP INDEX {indexName}", null);
        }

        public override void DropTable(string tableName)
        {
            sqlMapper.Execute($"DROP TABLE {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} CASCADE", null);
        }

        public override void AddTrigger(string tableName, string procedureName, TriggerPosition triggerPosition, TriggerEvent triggerEvent, string triggerName = null)
        {
            if (string.IsNullOrEmpty(triggerName)) triggerName = $"{tableName}_{procedureName}";
            if (!ExsitTrigger(triggerName))
                sqlMapper.Execute($"CREATE TRIGGER {triggerName} {triggerPosition.ToString()} {triggerEvent.ToString()} ON {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)} FOR EACH ROW EXECUTE PROCEDURE {procedureName}", null);
        }

        public override void DropTrigger(string tableName, string procedureName, string triggerName = null)
        {
            if (string.IsNullOrEmpty(triggerName)) triggerName = $"{tableName}_{procedureName}";
            if (ExsitTrigger(triggerName))
                sqlMapper.Execute($"DROP TRIGGER {triggerName} ON {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(tableName)}", null);
        }



        public override void Run()
        {
            var tables = TableMap.GetMaps().Where(x => x.AutoCreate).ToList();
            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i];
                var sqls = CreateSql(table);
                using (var trans = this.sqlMapper.BeginTransaction())
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

            //判断表是否存在
            if (ExsitTable(table.Name)) return new List<string>();

            IList<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            //表结构
            sb.AppendLine($"create table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)}(");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var item = table.Columns[i];
                ResolveColumnInfo(item);
                string defaultValue = item.DefaultValue == null ? "" : (item.DefaultValue is string) ? $"'{item.DefaultValue}'" : $"{item.DefaultValue}";
                if (i == table.Columns.Count - 1)
                {
                    sb.AppendLine($"{sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} {ResolveDataType(item)} {(item.IsNotNull ? "not null" : "")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"default {defaultValue}")}");
                }
                else
                {
                    sb.AppendLine($"{sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} {ResolveDataType(item)} {(item.IsNotNull ? "not null" : "")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"default {defaultValue}")},");
                }
            }
            sb.AppendLine(")");
            result.Add(sb.ToString());
            //主键
            if (table.Columns.Any(x => x.IsPrimaryKey))
            {
                //ALTER TABLE table_name
                //ADD CONSTRAINT MyPrimaryKey PRIMARY KEY(column1, column2...);
                result.Add($"alter table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint pk_{table.Name} primary key({string.Join(",", table.Columns.Where(x => x.IsPrimaryKey).Select(x => sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(x.Name)))})");
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
                    result.Add($"alter table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint uk_{table.Name}_{ukIndex} unique({string.Join(",", table.Columns.Where(x => x.IsUnique && x.UniqueGroup == item).Select(x => sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(x.Name)))})");
                    ukIndex++;
                }
                var ukColumns = table.Columns.Where(x => x.IsUnique && string.IsNullOrEmpty(x.UniqueGroup));
                foreach (var item in ukColumns)
                {
                    result.Add($"alter table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} add constraint uk_{table.Name}_{ukIndex} unique({sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)})");
                    ukIndex++;
                }
            }
            //注释
            if (!string.IsNullOrEmpty(table.Comment))
            {
                result.Add($"comment on table {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)} is '{table.Comment}'");
            }
            foreach (var item in table.Columns)
            {
                if (!string.IsNullOrEmpty(item.Comment))
                {
                    result.Add($"comment on column {sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(table.Name)}.{sqlMapper.SqlFactory.DbProvider.FormatQuotationForSql(item.Name)} is '{item.Comment}'");
                }
            }
            return result;
        }

        private bool ExsitTable(string tableName)
        {
            string sql = $"select count(*) from pg_class where relname = '{(sqlMapper.SqlFactory.DbProvider.UseQuotationInSql ? tableName : tableName.ToLower())}'";
            var count = sqlMapper.Scalar<int>(sql, null);
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
                        return $"character varying({column.Length})";
                    }
                case SimpleStandardType.Text:
                    {
                        return $"text";
                    }
                case SimpleStandardType.Char:
                    {
                        column.Length = 1;
                        return $"character varying(1)";
                    }
                case SimpleStandardType.Enum:
                    {
                        if (column.Length <= 0) column.Length = 50;
                        return $"character varying({column.Length})";
                    }
                case SimpleStandardType.DateTime:
                    {
                        return $"timestamp without time zone";
                    }
                case SimpleStandardType.DateTimeOffset:
                    {
                        return $"timestamp with time zone";
                    }
                case SimpleStandardType.Boolean:
                    {
                        column.Length = 1;
                        return $"character varying(1)";
                    }
                case SimpleStandardType.Binary:
                    {
                        return $"bytea";
                    }
                default:
                    {
                        throw new SqlException("不支持数据类型:" + column.PropertyType.ToString());
                    }
            }
        }

        private bool ExsitTrigger(string triggerName)
        {
            return sqlMapper.Scalar<int>("select count(1) from user_triggers where triggername =#triggername#", triggerName) > 0;
        }

        private bool ExsitIndex(string indexName)
        {
            return sqlMapper.Scalar<int>("select count(1) from user_indexes where index_name =#index_name#", indexName) > 0;
        }


    }
}
