using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YCT.Infrastructure;
using YCT.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (EF Core, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(YCT.Application.Common.ResponseBase<>).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(YCT.Application.Common.ResponseBase<>).Assembly);

// JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// CORS — orígenes permitidos desde appsettings o variable de entorno Cors__AllowedOrigins__0
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Rate limiting — límite fijo por IP
var permitLimit = builder.Configuration.GetValue<int?>("RateLimit:PermitLimit") ?? 100;
var windowSeconds = builder.Configuration.GetValue<int?>("RateLimit:WindowSeconds") ?? 60;
var queueLimit = builder.Configuration.GetValue<int?>("RateLimit:QueueLimit") ?? 0;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    });

    // Endpoints de auth con límite más estricto (anti brute-force)
    options.AddPolicy("auth", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Aplicar migraciones pendientes al iniciar (crea/actualiza el esquema).
// Reintenta porque la base de datos puede tardar en estar lista al levantar el stack.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    for (var intento = 1; intento <= 12; intento++)
    {
        try
        {
            db.Database.Migrate();
            startupLogger.LogInformation("Migraciones aplicadas correctamente.");
            break;
        }
        catch (Exception ex) when (intento < 12)
        {
            startupLogger.LogWarning("Base de datos no lista (intento {Intento}/12): {Mensaje}. Reintentando en 5s...", intento, ex.Message);
            Thread.Sleep(5000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");
app.UseRateLimiter();

// Servir archivos estáticos (uploads de productos)
var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
