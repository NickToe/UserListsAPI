using Microsoft.EntityFrameworkCore;
using UserListsAPI.Application;
using UserListsAPI.Application.Abstractions;
using UserListsAPI.Application.DTOs;
using UserListsAPI.Infrastructure.Api.Services;
using UserListsAPI.Infrastructure.Persistence;
using UserListsAPI.Infrastructure.Services;

namespace UserListsAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("CleanArchitectureDb"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }

        services.AddAutoMapper(typeof(AppMappingProfile));

        services.AddHostedService<DailyHostedService>();

        services.AddScoped<IItemService<MovieDTO>, MovieService>();
        services.AddSingleton<MovieHttpClient>();

        services.AddScoped<IItemService<GameDTO>, GameService>();
        services.AddSingleton<GameHttpClient>();

        return services;
    }
}
