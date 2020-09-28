using System;
using System.Collections.Generic;
using System.Text;

namespace WangSql.BuildProviders.Migrate
{
    public class DefaultMigrateProvider : IMigrateProvider
    {
        protected ISqlMapper sqlMapper;

        public virtual IMigrateProvider Instance(ISqlMapper sqlMapper)
        {
            this.sqlMapper = sqlMapper;
            return this;
        }

        public virtual void Run()
        {
            throw new SqlException("未实现");
        }

        public virtual void AddColumn(string tableName, ColumnInfo columnInfo)
        {
            throw new SqlException("未实现");
        }

        public virtual void AddIndex(string tableName, string columnName, string indexName = null, string indexType = null)
        {
            throw new SqlException("未实现");
        }

        public virtual void RenameColumn(string tableName, string oldName, string newName)
        {
            throw new SqlException("未实现");
        }

        public virtual void AlterColumn(string tableName, ColumnInfo columnInfo)
        {
            throw new SqlException("未实现");
        }

        public virtual void AlterColumnNull(string tableName, string columnName, bool nullable = false)
        {
            throw new SqlException("未实现");
        }

        public virtual void CreateTable(TableInfo tableInfo)
        {
            throw new SqlException("未实现");
        }

        public virtual void DropColumn(string tableName, string columnName)
        {
            throw new SqlException("未实现");
        }

        public virtual void DropIndex(string tableName, string columnName)
        {
            throw new SqlException("未实现");
        }

        public virtual void DropIndex(string indexName)
        {
            throw new SqlException("未实现");
        }

        public virtual void DropTable(string tableName)
        {
            throw new SqlException("未实现");
        }

        public virtual void AddTrigger(string tableName, string procedureName, TriggerPosition triggerPosition, TriggerEvent triggerEvent, string triggerName = null)
        {
            throw new SqlException("未实现");
        }

        public virtual void DropTrigger(string tableName, string procedureName, string triggerName = null)
        {
            throw new SqlException("未实现");
        }
    }
}
