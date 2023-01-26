using UserListsAPI.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Enums;

namespace UserListsAPI.DataLayer.Repo;

public class MovieRepo : IItemRepo<Movie>
{
  private readonly ILogger<MovieRepo> _logger;
  private readonly AppDbContext _context;

  public MovieRepo(ILogger<MovieRepo> logger, AppDbContext context)
  {
    _logger = logger;
    _context = context;
  }

  public async Task<Movie?> GetById(string id)
  {
    return await _context.Movies.FirstOrDefaultAsync(item => item.Id == id);
  }

  public async Task<Movie?> GetByTitle(string title)
  {
    return await _context.Movies.FirstOrDefaultAsync(item => item.FullTitle.ToLower() == title.ToLower());
  }

  public async Task<IEnumerable<Movie>> GetAll(string title)
  {
    return await _context.Movies.Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower())).ToListAsync();
  }

  public async Task<bool> Add(Movie imdbMovie)
  {
    if (await Any(imdbMovie.Id)) return false;
    await _context.Movies.AddAsync(imdbMovie);
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task<bool> Any(string id) =>
    await _context.Movies.AnyAsync(item => item.Id == id);

  public async Task<bool> UpdateSuccess(string id, ItemStatus itemStatus)
  {
    Movie? movie = await _context.Movies.FindAsync(id);
    if (movie == default)
    {
      _logger.LogInformation("Couldn't find a movie with id({id}) to change status to {status}", id, itemStatus);
      return false;
    }
    movie.ItemStatus = itemStatus;
    return await _context.SaveChangesAsync() > 0;
  }

  public Task UpdateAll(IEnumerable<Movie> items)
  {
    throw new NotImplementedException();
  }
}
