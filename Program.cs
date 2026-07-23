using System.Reflection;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Data;
using SupermarketSystem.Api.Services.Jwt;
using DotNetEnv; // 👈 استدعاء مكتبة DotNetEnv

// 0️⃣ تحميل ملف الـ .env أولاً
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ تسجيل الـ Connection Factory
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// 2️⃣ تسجيل خدمة الـ JWT
builder.Services.AddScoped<IJwtService, JwtService>();

// 3️⃣ إضافة خدمات الـ Controllers
builder.Services.AddControllers();

// 4️⃣ إضافة MediatR لقراءة كافة الـ Handlers في المشروع
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 5️⃣ إضافة OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

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