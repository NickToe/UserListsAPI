using Application.Abstractions;
using Application.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DailyHostedService : BackgroundService
{
    private readonly ILogger<DailyHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    public DailyHostedService(ILogger<DailyHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var gameService = scope.ServiceProvider.GetRequiredService<IItemService<GameDTO>>();
                    await gameService.AddAllAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while handling daily task: {exception}", ex.Message);
            }
            TimeSpan timeSpan = new TimeOnly(0, 0, 0) - TimeOnly.FromDateTime(DateTime.Now);
            _logger.LogInformation("Next daily background task will execute in {timeSpan}", timeSpan);
            await Task.Delay(timeSpan);
        }
    }
}