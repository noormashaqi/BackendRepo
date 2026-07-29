namespace SupermarketSystem.Api.Services.Jwt;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(Employee employee, List<string> permissions);

    string GenerateRefreshToken();

    string ComputeRefreshTokenHash(string refreshToken);

    DateTime GetRefreshTokenExpiry();
}
