using UserListsAPI.HttpLayer;
using UserListsAPI.DataLayer;
using UserListsAPI.ServiceLayer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserListsAPI.DataLayer.Repo;
using UserListsAPI.Utility;
using UserListsAPI.DataLayer.Entities;

namespace UserListsAPI;

public class Program
{
  public async static Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IItemService<Movie>, MovieService>();
    builder.Services.AddSingleton<MovieHttpClient>();
    builder.Services.AddScoped<IItemRepo<Movie>, MovieRepo>();

    builder.Services.AddScoped<IItemService<Game>, GameService>();
    builder.Services.AddSingleton<GameHttpClient>();
    builder.Services.AddScoped<IItemRepo<Game>, GameRepo>();

    builder.Services.AddScoped<TimerSetupService>();

    var app = builder.Build();

    SetupTimers(app);

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
  }

  public static void SetupTimers(WebApplication app)
  {
    TimerSetupService timerSetupService = app.Services.CreateScope().ServiceProvider.GetRequiredService<TimerSetupService>();
    TimerSetup timerSetup = new(app.Logger, timerSetupService);
    timerSetup.StartAll();
  }
}