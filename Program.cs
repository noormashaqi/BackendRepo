using System.Reflection;
using System.Text.Json.Serialization;
using DotNetEnv;
using FluentValidation;
using MediatR;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Middleware;
using SupermarketSystem.Api.Services.Jwt;
using SupermarketSystem.Api.Data; 

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ تسجيل الـ Connection Factory
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// 2️⃣ تسجيل خدمة الـ JWT
builder.Services.AddScoped<IJwtService, JwtService>();

// 3️⃣ إضافة خدمات الـ Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    });

// 4️⃣ إضافة MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 4.1️⃣ تسجيل FluentValidation تلقائياً
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// 4.2️⃣ ربط الـ Validation Pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 5️⃣ إضافة Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5.1️⃣ Middleware الأخطاء
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6️⃣ ربط الـ Controllers
app.MapControllers();

app.Run();