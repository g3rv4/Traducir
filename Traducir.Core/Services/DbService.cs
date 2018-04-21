using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace Traducir.Core.Services
{
    public interface IDbService
    {
        ProfiledDbConnection GetConnection();
    }

    public class DbService : IDbService
    {
        public DbService(IConfiguration configuration)
        {
            SqlMapper.Settings.InListStringSplitCount = 11;
            ConnectionString = configuration.GetValue<string>("CONNECTION_STRING");
        }

        private static string ConnectionString { get; set; }

        public ProfiledDbConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            return new ProfiledDbConnection(connection, MiniProfiler.Current);
        }
    }
}