using Microsoft.EntityFrameworkCore;
using UserListsAPI.Domain.Entities;

namespace UserListsAPI.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Movie> Movies { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}