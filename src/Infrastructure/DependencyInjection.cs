using Application.Abstractions;
using Application.DTOs;
using Infrastructure.Api.Services;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(typeof(InfrastructureMappingProfile));

        services.AddHostedService<DailyHostedService>();

        services.AddScoped<IItemService<MovieDTO>, MovieService>();
        services.AddSingleton<MovieHttpClient>();

        services.AddScoped<IItemService<GameDTO>, GameService>();
        services.AddSingleton<GameHttpClient>();

        return services;
    }
}
