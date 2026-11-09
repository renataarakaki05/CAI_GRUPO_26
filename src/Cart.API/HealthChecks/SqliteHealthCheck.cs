using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks
{
    // chequea q la base responda haciendo un SELECT 1
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        public SqliteHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");
                if (connectionString == null)
                {
                    connectionString = "Data Source=cart.db";
                }

                using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync(cancellationToken);
                await conn.ExecuteScalarAsync<int>("SELECT 1");

                return HealthCheckResult.Healthy("SELECT 1 ejecutado OK");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite", ex);
            }
        }
    }
}