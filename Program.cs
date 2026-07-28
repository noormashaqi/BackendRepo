using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SupermarketSystem.Api.Data;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Middleware;
using SupermarketSystem.Api.Services.Jwt;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IJwtService, JwtService>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
