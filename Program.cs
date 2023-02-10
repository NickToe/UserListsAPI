using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using UserListsAPI.Data;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Repositories;
using UserListsAPI.DTOs;
using UserListsAPI.ExternalApi;
using UserListsAPI.Middleware;
using UserListsAPI.Services;

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

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "ApiKey must appear in header",
                Type = SecuritySchemeType.ApiKey,
                Name = "XApiKey",
                In = ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });
            var key = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            };
            var requirement = new OpenApiSecurityRequirement { { key, new List<string>() } };
            options.AddSecurityRequirement(requirement);
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
          new UrlSegmentApiVersionReader(),
          new HeaderApiVersionReader("XApiVersion"));
        });

        builder.Services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        string host = builder.Configuration.GetValue<string>("UserListsMVC:Host");
        int port = builder.Configuration.GetValue<int>("UserListsMVC:Port");
        builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins($"https://{host}:{port}").AllowAnyMethod().AllowAnyHeader()));

        builder.Services.AddAutoMapper(typeof(AppMappingProfile));

        builder.Services.AddHostedService<DailyHostedService>();

        builder.Services.AddScoped<IItemService<MovieDTO>, MovieService>();
        builder.Services.AddSingleton<MovieHttpClient>();
        builder.Services.AddScoped<IItemRepository<Movie>, MovieRepository>();

        builder.Services.AddScoped<IItemService<GameDTO>, GameService>();
        builder.Services.AddSingleton<GameHttpClient>();
        builder.Services.AddScoped<IItemRepository<Game>, GameRepository>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                  description.GroupName.ToUpperInvariant());
                }
            });
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseMiddleware<ApiKeyMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}