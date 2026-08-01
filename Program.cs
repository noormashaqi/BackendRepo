using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using DotNetEnv;

using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Data;
using SupermarketSystem.Api.Services.Jwt;
using SupermarketSystem.Api.Services.Permissions;
using SupermarketSystem.Api.Middleware;


Env.Load();

var builder = WebApplication.CreateBuilder(args);


// Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Supermarket API",
            Version = "v1"
        });


    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {JWT Token}"
        });


    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});



// JWT

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)

.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "SupermarketSystem",
            ValidAudience = "SupermarketSystem",

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        Environment.GetEnvironmentVariable("JWT_SECRET")!
                    ))
        };
});


builder.Services.AddAuthorization();



// Database

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();



// Services

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();



// Controllers

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.NumberHandling =
        JsonNumberHandling.Strict;
});



// MediatR

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        Assembly.GetExecutingAssembly()
    ));



// FluentValidation

builder.Services.AddValidatorsFromAssembly(
    Assembly.GetExecutingAssembly()
);



// Validation Pipeline

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>)
);



var app = builder.Build();



// Middleware

app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}



app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();


app.Run();