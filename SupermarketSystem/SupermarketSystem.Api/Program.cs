using System.Reflection;
using SupermarketSystem.Api.Interface;
using SupermarketSystem.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ تسجيل الـ Connection Factory
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// 2️⃣ إضافة خدمات الـ Controllers
builder.Services.AddControllers();

// 3️⃣ إضافة MediatR لقراءة كافة الـ Handlers في المشروع
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 4️⃣ إضافة OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

// 5️⃣ ربط الـ Controllers
app.MapControllers();

app.Run();