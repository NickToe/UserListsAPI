using Microsoft.EntityFrameworkCore;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Enums;

namespace UserListsAPI.Data.Repositories;

public class GameRepository : IItemRepository<Game>
{
    private readonly ILogger<GameRepository> _logger;
    private readonly AppDbContext _context;

    public GameRepository(ILogger<GameRepository> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(string id)
    {
        return await _context.Games.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok);
    }

    public async Task<Game?> GetByTitleAsync(string title)
    {
        return await _context.Games.AsNoTracking().FirstOrDefaultAsync(item => item.Title.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok);
    }

    public async Task<IEnumerable<Game>> GetAllByTitleAsync(string title, int maxItems)
    {
        return await _context.Games.AsNoTracking().Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower())).ToListAsync();
    }

    public void Add(Game game)
    {
        _context.ChangeTracker.Clear(); // Workaround needed to stop tracking Game when it is retrieved from db but not filled
        _context.Games.Update(game);
    }

    public async Task<bool> AnyAsync(string id) =>
      await _context.Games.AnyAsync(item => item.Id == id);

    public async Task<Game?> FindAsync(string id) => await _context.Games.FindAsync(id);

    public async Task UpdateAllAsync(IEnumerable<Game> games)
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
                if (await AnyAsync(game.Id) == false)
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

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}