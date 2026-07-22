using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SupermarketSystem.Api.Data;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Services.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Database - Dapper عبر Factory بسيط (بدون EF Core)
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// MediatR - نمط CQRS (Commands/Queries + Handlers)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// JWT Service (توليد التوكن)
builder.Services.AddScoped<IJwtService, JwtService>();

// Authentication - JWT Bearer
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is missing from configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // أداة مؤقتة لتوليد BCrypt Hash لباسورد تجربة (Development فقط، ما بتشتغل بالإنتاج)
    // استخدمها مرة وحدة لعمل موظف تجربة، وبعدين احذفها.
    app.MapGet("/api/dev/hash-password", (string password) =>
        Results.Ok(new { password, hash = BCrypt.Net.BCrypt.HashPassword(password) }));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
