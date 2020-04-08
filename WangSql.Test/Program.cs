using System;
using System.Diagnostics;

namespace WangSql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SqliteTest.Test();
            //SqliteTest1.Test();
            //PgsqlTest.Test();
            //PgsqlTest1.Test();
            //OracleTest.Test();

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
