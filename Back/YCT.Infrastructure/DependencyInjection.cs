using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YCT.Application.Common;
using YCT.Domain.Interfaces;
using YCT.Infrastructure.Persistence;
using YCT.Infrastructure.Repositories;
using YCT.Infrastructure.Services;

namespace YCT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddDataProtection();
        services.AddSingleton<IEmailActionTokenService, EmailActionTokenService>();

        services.AddHostedService<DailyReportService>();

        return services;
    }
}
