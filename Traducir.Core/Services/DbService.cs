using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Traducir.Core.Services
{
    public interface IDbService
    {
        SqlConnection GetConnection();
    }

    public class DbService : IDbService
    {
        private static string ConnectionString { get; set; }

        public DbService(IConfiguration configuration)
        {
            ConnectionString = configuration.GetValue<string>("CONNECTION_STRING");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}