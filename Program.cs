using UserListsAPI.HttpLayer;
using UserListsAPI.DataLayer;
using UserListsAPI.ServiceLayer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserListsAPI.DataLayer.Repo;
using UserListsAPI.DataLayer.Entities;
using System.Reflection;

namespace UserListsAPI;

public class Program
{
  public async static Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddEnvironmentVariables().AddUserSecrets(Assembly.GetExecutingAssembly(), true);

    var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    string host = builder.Configuration.GetValue<string>("UserListsMVC:Host");
    int port = builder.Configuration.GetValue<int>("UserListsMVC:Port");
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins($"https://{host}:{port}").AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddHostedService<DailyHostedService>();

    builder.Services.AddScoped<IItemService<Movie>, MovieService>();
    builder.Services.AddSingleton<MovieHttpClient>();
    builder.Services.AddScoped<IItemRepo<Movie>, MovieRepo>();

    builder.Services.AddScoped<IItemService<Game>, GameService>();
    builder.Services.AddSingleton<GameHttpClient>();
    builder.Services.AddScoped<IItemRepo<Game>, GameRepo>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
  }
}