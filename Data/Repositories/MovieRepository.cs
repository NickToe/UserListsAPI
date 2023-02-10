using Microsoft.EntityFrameworkCore;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Enums;

namespace UserListsAPI.Data.Repositories;

public class MovieRepository : IItemRepository<Movie>
{
    private readonly ILogger<MovieRepository> _logger;
    private readonly AppDbContext _context;

    public MovieRepository(ILogger<MovieRepository> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(string id)
    {
        return await _context.Movies.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok);
    }

    public async Task<Movie?> GetByTitleAsync(string title)
    {
        return await _context.Movies.AsNoTracking().FirstOrDefaultAsync(item => item.FullTitle.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok);
    }

    public async Task<IEnumerable<Movie>> GetAllByTitleAsync(string title, int maxItems)
    {
        return await _context.Movies.AsNoTracking().Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower())).OrderBy(item => item.FullTitle).Take(maxItems).ToListAsync();
    }

    public void Add(Movie movie)
    {
        _context.Movies.Add(movie);
    }

    public async Task<bool> AnyAsync(string id) =>
      await _context.Movies.AnyAsync(item => item.Id == id);

    public async Task<Movie?> FindAsync(string id) => await _context.Movies.FindAsync(id);

    public Task UpdateAllAsync(IEnumerable<Movie> items)
    {
        throw new NotImplementedException();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
