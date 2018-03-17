using System.Data.Common;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling;

namespace Traducir.Core.Services
{
    public interface IDbService
    {
        DbConnection GetConnection();
        SqlConnection GetRawConnection();
    }

    public class DbService : IDbService
    {
        private static string ConnectionString { get; set; }

        public DbService(IConfiguration configuration)
        {
            SqlMapper.Settings.InListStringSplitCount = 11;
            ConnectionString = configuration.GetValue<string>("CONNECTION_STRING");
        }

        public SqlConnection GetRawConnection(){
            return new SqlConnection(ConnectionString);
        }

        public DbConnection GetConnection()
        {
            return new StackExchange.Profiling.Data.ProfiledDbConnection(GetRawConnection(), MiniProfiler.Current);
        }
    }
}