using System.Data;

namespace SupermarketSystem.Api.Interface;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}