namespace WangSql.BuildProviders.Migrate
{
    public interface IMigrateProvider
    {
        IMigrateProvider Instance(ISqlExe sqlExe);
        IMigrateProvider Instance(ISqlMapper sqlMapper);

        /// <summary>
        /// 初始化表结构
        /// </summary>
        /// <param name="sqlMapper"></param>
        void Run();
    }
}