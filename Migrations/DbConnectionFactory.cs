using System.Data;
using MySqlConnector;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "CONNECTION_STRING was not found in the .env file.");
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}