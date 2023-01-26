using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Entities;

namespace UserListsAPI.DataLayer;

public class AppDbContext : DbContext
{
  private readonly IConfiguration _configuration;
  public DbSet<Game> Games { get; set; } = null!;
  public DbSet<Movie> Movies { get; set; } = null!;

  public AppDbContext(IConfiguration configuration)
  { 
    _configuration = configuration;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();
    optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
  }
}