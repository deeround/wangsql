namespace WangSql
{
    public interface IMigrateProvider
    {
        void Run(SqlMapper sqlMapper);
    }
}