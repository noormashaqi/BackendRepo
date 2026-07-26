using System.Reflection;
using FluentValidation;
using MediatR;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Data;
using SupermarketSystem.Api.Services.Jwt;
using SupermarketSystem.Api.Middleware;
using DotNetEnv; // 👈 استدعاء مكتبة DotNetEnv
using System.Text.Json.Serialization;

// 0️⃣ تحميل ملف الـ .env أولاً
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

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

// 4️⃣ إضافة MediatR لقراءة كافة الـ Handlers في المشروع
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 4.1️⃣ تسجيل كل الـ FluentValidation Validators تلقائيًا
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// 4.2️⃣ ربط الـ Validators بالـ MediatR Pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 5️⃣ إضافة OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// 5.1️⃣ ميدل وير معالجة الأخطاء - لازم يكون أول شي بالـ pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

// app.UseHttpsRedirection();

// 6️⃣ ربط الـ Controllers
app.MapControllers();

app.Run();