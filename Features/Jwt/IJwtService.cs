namespace SupermarketSystem.Api.Services.Jwt;

public interface IJwtService
{
    /// <summary>
    /// Builds a signed JWT for the given employee, embedding their permissions as claims.
    /// </summary>
    (string Token, DateTime ExpiresAt) GenerateToken(Employee employee, List<string> permissions);
}
