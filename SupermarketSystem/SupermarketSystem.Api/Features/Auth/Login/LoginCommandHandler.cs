using Dapper;
using MediatR;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

namespace SupermarketSystem.Api.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    // رسالة واحدة موحدة لكل حالات الفشل - ما منفرق بين "يوزرنيم غلط" و"باسورد غلط"
    // عشان حدا ما يقدر يخمن الأسماء الموجودة بالنظام
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
        using var connection = _dbConnectionFactory.CreateConnection();

        const string employeeSql = @"
            SELECT Id, FullName, Username, PasswordHash, IsActive, CreatedAt
            FROM Employees
            WHERE Username = @Username
            LIMIT 1;";

        var employee = await connection.QuerySingleOrDefaultAsync<Employee>(
            employeeSql, new { request.Username });

        if (employee is null)
            return LoginResult.Fail(InvalidCredentialsMessage);

        // موظف مفصول (Deactivated) ما بقدر يسجل دخول أبدًا حتى لو كلمة المرور صح
        if (!employee.IsActive)
            return LoginResult.Fail(InvalidCredentialsMessage);

        bool passwordValid;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash مخزن بشكل غير صالح - نتعامل معه كباسورد خاطئ بدل ما نرمي استثناء للمستخدم
            passwordValid = false;
        }

        if (!passwordValid)
            return LoginResult.Fail(InvalidCredentialsMessage);

        const string permissionsSql = @"
            SELECT PermissionKey
            FROM EmployeePermission
            WHERE EmployeeId = @EmployeeId;";

        var permissions = (await connection.QueryAsync<string>(
            permissionsSql, new { EmployeeId = employee.Id })).ToList();

        // تسجيل بداية الشفت - هاد اللي بيغذي تقرير "كل موظف ايمتا دخل وايمتا خرج"
        const string insertAttendanceSql = @"
            INSERT INTO AttendanceLog (EmployeeId, LoginTime)
            VALUES (@EmployeeId, @LoginTime);";

        await connection.ExecuteAsync(insertAttendanceSql, new
        {
            EmployeeId = employee.Id,
            LoginTime = DateTime.UtcNow
        });

        var (token, expiresAt) = _jwtService.GenerateToken(employee, permissions);

        var response = new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Permissions = permissions
        };

        return LoginResult.Ok(response);
    }
}
