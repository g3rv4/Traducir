using System;
using System.Data.SqlClient;
using SimpleMigrations;
using SimpleMigrations.Console;
using SimpleMigrations.DatabaseProvider;

namespace Traducir.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            var migrationsAssembly = typeof(Program).Assembly;
            using(var db = new SqlConnection(connectionString))
            {
                var databaseProvider = new MssqlDatabaseProvider(db);
                var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);

                var consoleRunner = new ConsoleRunner(migrator);
                consoleRunner.Run(args);
            }
        }
    }
}