namespace WangSql.BuildProviders.Migrate
{
    public interface IMigrateProvider
    {
        void Run(ISqlMapper sqlMapper);
    }
}