using System.Data.Common;
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
        private static string ConnectionString { get; set; }

        public DbService(IConfiguration configuration)
        {
            SqlMapper.Settings.InListStringSplitCount = 11;
            ConnectionString = configuration.GetValue<string>("CONNECTION_STRING");
        }

        public ProfiledDbConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);
        }
    }
}