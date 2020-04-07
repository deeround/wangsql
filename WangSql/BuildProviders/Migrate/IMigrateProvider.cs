namespace WangSql.Migrate.BuildProviders.CodeFirst
{
    public interface IMigrateProvider
    {
        void Run(SqlMapper sqlMapper);
    }
}