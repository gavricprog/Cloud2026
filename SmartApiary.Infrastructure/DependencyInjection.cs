using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartApiary.Application.Interfaces;
using SmartApiary.Infrastructure.HostedServices;
using SmartApiary.Infrastructure.Persistence;
using SmartApiary.Infrastructure.Services;

namespace SmartApiary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<ISprayingCompletionService, SprayingCompletionService>();
        services.AddHostedService<SprayingLogHostedService>();

        return services;
    }
}
