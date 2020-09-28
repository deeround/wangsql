namespace WangSql.BuildProviders.Migrate
{
    public enum TriggerPosition
    {
        BEFORE,
        AFTER
    }
    public enum TriggerEvent
    {
        INSERT,
        UPDATE,
        DELETE
    }

    public interface IMigrateProvider
    {
        IMigrateProvider Instance(ISqlMapper sqlMapper);

        /// <summary>
        /// 初始化表结构
        /// </summary>
        /// <param name="sqlMapper"></param>
        void Run();

        /// <summary>
        /// 建表
        /// </summary>
        /// <param name="tableInfo"></param>
        void CreateTable(TableInfo tableInfo);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        void DropTable(string tableName);

        /// <summary>
        /// 新增字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnInfo"></param>
        void AddColumn(string tableName, ColumnInfo columnInfo);

        /// <summary>
        /// 重命名字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        void RenameColumn(string tableName, string oldName, string newName);

        /// <summary>
        /// 删除字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        void DropColumn(string tableName, string columnName);

        /// <summary>
        /// 修改字段类型
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnInfo"></param>
        void AlterColumn(string tableName, ColumnInfo columnInfo);

        /// <summary>
        /// 修改字段是否可为空
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="nullable"></param>
        void AlterColumnNull(string tableName, string columnName, bool nullable = false);

        /// <summary>
        /// 新增索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        void AddIndex(string tableName, string columnName, string indexName = null, string indexType = null);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        void DropIndex(string tableName, string columnName);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="indexName"></param>
        void DropIndex(string indexName);

        /// <summary>
        /// 新增触发器
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="procedureName"></param>
        /// <param name="triggerName"></param>
        void AddTrigger(string tableName, string procedureName, TriggerPosition triggerPosition, TriggerEvent triggerEvent, string triggerName = null);

        /// <summary>
        /// 删除触发器
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="procedureName"></param>
        /// <param name="triggerName"></param>
        void DropTrigger(string tableName, string procedureName, string triggerName = null);

    }
}