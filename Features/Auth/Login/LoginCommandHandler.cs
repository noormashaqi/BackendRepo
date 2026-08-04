using System.Security.Cryptography;
using System.Text;
using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private const string InvalidCredentialsMessage = "بيانات الدخول غير صحيحة";

    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IDbConnectionFactory dbConnectionFactory, IJwtService jwtService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _jwtService = jwtService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var employee = await AuthDataAccess.GetEmployeeByUsernameAsync(
            _dbConnectionFactory,
            request.Username,
            cancellationToken);

        if (employee is null || !employee.IsActive)
            return LoginResult.Fail(InvalidCredentialsMessage);

        bool passwordValid;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            passwordValid = false;
        }

        if (!passwordValid)
            return LoginResult.Fail(InvalidCredentialsMessage);

        var permissions = await AuthDataAccess.GetPermissionsAsync(
            _dbConnectionFactory,
            employee,
            cancellationToken);

        // 1️⃣ إنشاء الاتصال بـ Database والـ Transaction
        using var connection = _dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // 2️⃣ تسجيل بداية الشفت (AttendanceLogs)
        const string insertAttendanceSql = @"
            INSERT INTO AttendanceLogs (EmployeeId, LoginTime)
            VALUES (@EmployeeId, @LoginTime);";

        await connection.ExecuteAsync(
            insertAttendanceSql,
            new
            {
                EmployeeId = employee.Id,
                LoginTime = DateTime.UtcNow
            },
            transaction);

        // 3️⃣ توليد الـ Refresh Token وتجهيز الـ Hash الخاص به
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7); // صالحة لمدة 7 أيام مثلاً

        const string insertRefreshTokenSql = """
            INSERT INTO RefreshTokens
            (
                EmployeeId,
                TokenHash,
                ExpiresAt,
                CreatedAt,
                RevokedAt,
                ReplacedByTokenHash
            )
            VALUES
            (
                @EmployeeId,
                @TokenHash,
                @ExpiresAt,
                UTC_TIMESTAMP(),
                NULL,
                NULL
            );
            """;

        await connection.ExecuteAsync(
            insertRefreshTokenSql,
            new
            {
                EmployeeId = employee.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = refreshTokenExpiresAt
            },
            transaction);

        // 4️⃣ تأكيد العملية للـ Database
        transaction.Commit();

        // 5️⃣ إنتاج الـ Access Token وإرجاع النتيجة
        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(employee, permissions);

        return LoginResult.Ok(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Username = employee.Username,
            Role = employee.Role,
            Permissions = permissions
        });
    }
}