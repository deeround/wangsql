namespace WangSql.Migrate.BuildProviders.CodeFirst
{
    public interface ICodeFirstProvider
    {
        void Run(SqlMapper sqlMapper);
    }
}