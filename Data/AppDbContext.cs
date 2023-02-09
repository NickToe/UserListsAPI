using Microsoft.EntityFrameworkCore;
using UserListsAPI.Data.Entities;

namespace UserListsAPI.Data;

public class AppDbContext : DbContext
{
  public DbSet<Game> Games { get; set; } = null!;
  public DbSet<Movie> Movies { get; set; } = null!;

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}