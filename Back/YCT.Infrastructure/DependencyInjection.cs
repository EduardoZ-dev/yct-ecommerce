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
        services.AddHttpClient<IWhatsAppNotifier, WhatsAppCloudNotifier>();
        services.AddDataProtection();
        services.AddSingleton<IEmailActionTokenService, EmailActionTokenService>();

        services.AddHostedService<DailyReportService>();

        // Reloj inyectable (los casos de uso no llaman DateTime.UtcNow: se pueden probar con hora fija).
        services.AddSingleton(TimeProvider.System);

        // Avisos (WhatsApp…) que salen después de responder, sin hacer esperar a quien opera.
        services.AddSingleton<ColaAvisos>();
        services.AddSingleton<IColaAvisos>(sp => sp.GetRequiredService<ColaAvisos>());
        services.AddHostedService<AvisosEnSegundoPlanoService>();

        return services;
    }
}
