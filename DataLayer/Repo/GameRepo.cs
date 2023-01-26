using UserListsAPI.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Enums;

namespace UserListsAPI.DataLayer.Repo;

public class GameRepo : IItemRepo<Game>
{
  private readonly ILogger<GameRepo> _logger;
  private readonly AppDbContext _context;

  public GameRepo(ILogger<GameRepo> logger, AppDbContext context)
  {
    _logger = logger;
    _context = context;
  }

  public async Task<Game?> GetById(string id)
  {
    return await _context.Games.FirstOrDefaultAsync(item => item.Id == id);
  }

  public async Task<Game?> GetByTitle(string title)
  {
    return await _context.Games.FirstOrDefaultAsync(item => item.Title == title);
  }

  public async Task<IEnumerable<Game>> GetAll(string title)
  {
    return await _context.Games.Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower())).ToListAsync();
  }

  public async Task<bool> Add(Game game)
  {
    _context.ChangeTracker.Clear(); // Workaround needed to stop tracking Game when it is retrieved from db but not filled
    _context.Games.Update(game);
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task<bool> Any(string id) =>
    await _context.Games.AnyAsync(item => item.Id == id);

  public async Task<bool> UpdateSuccess(string id, ItemStatus itemStatus)
  {
    Game? game = await _context.Games.FindAsync(id);
    if (game == default)
    {
      _logger.LogInformation("Couldn't find a game with id({id}) to change status to {status}", id, itemStatus);
      return false;
    }
    game.ItemStatus = itemStatus;
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task UpdateAll(IEnumerable<Game> games)
  {
    int totalGames = await _context.Games.CountAsync();
    if (totalGames == 0)
    {
      await _context.AddRangeAsync(games);
    }
    else
    {
      foreach (Game game in games)
      {
        if (await Any(game.Id) == false)
        {
          await _context.AddAsync(game);
        }
      }
    }
    int updatedRecords = await _context.SaveChangesAsync();
    _logger.LogInformation("Current time: {time}", DateTime.Now);
    _logger.LogInformation("{updatedRecords} records were updated", updatedRecords);
    _logger.LogInformation("Games in database: {count}", _context.Games.Count());
  }
}