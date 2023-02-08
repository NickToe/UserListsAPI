using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Entities;

namespace UserListsAPI.DataLayer;

public class AppDbContext : DbContext
{
  public DbSet<Game> Games { get; set; } = null!;
  public DbSet<Movie> Movies { get; set; } = null!;

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}