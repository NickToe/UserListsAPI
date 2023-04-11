using Application.Abstractions;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Api.JsonModels;
using Infrastructure.Api.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class GameService : IItemService<GameDTO>
{
    private readonly ILogger<GameService> _logger;
    private readonly GameHttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GameService(ILogger<GameService> logger, GameHttpClient httpClient, AppDbContext context, IMapper mapper)
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = context;
        _mapper = mapper;
    }

    public async Task<GameDTO?> GetByIdAsync(string id)
    {
        Game? game = await GetFullGameByIdAsync(id);
        return _mapper.Map<GameDTO>(game);
    }

    public async Task<IEnumerable<GameDTO>> GetAllByIdsAsync(IEnumerable<string> ids)
    {
        ICollection<Game> games = new List<Game>();
        Game? game = default;
        foreach (string id in ids)
        {
            game = await GetFullGameByIdAsync(id);
            if (game != default)
            {
                games.Add(game);
            }
        }
        _logger.LogInformation("Games obtained by id: {count}", games.Count);
        return _mapper.Map<IEnumerable<GameDTO>>(games);
    }

    private async Task<Game?> GetFullGameByIdAsync(string id)
    {
        Game? game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok);

        if (game == null)
        {
            _logger.LogInformation("Game id({id}) was not found in DB", id);
            return default;
        }

        game = await GetByIdFromApiAsync(game);

        return game;
    }

    public async Task<GameDTO?> GetByExactTitleAsync(string title)
    {
        Game? game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Title.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok);

        if (game is null)
        {
            _logger.LogInformation("Game with title {title} was not found!", title);
            return default;
        }

        game = await GetByIdFromApiAsync(game);
        return _mapper.Map<GameDTO>(game);
    }

    public async Task<IEnumerable<GameDTO>> GetAllByTitleAsync(string title, int maxItems)
    {
        IEnumerable<Game> games = await _context.Games
            .AsNoTracking()
            .Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower()))
            .ToListAsync();

        if (!games.Any())
        {
            _logger.LogInformation("Games title({title}) were not found in DB", title);
            return Enumerable.Empty<GameDTO>();
        }

        games = await GetAllByIdFromApiAsync(games, maxItems);
        return _mapper.Map<IEnumerable<GameDTO>>(games);
    }

    private async Task<IEnumerable<Game>> GetAllByIdFromApiAsync(IEnumerable<Game> games, int maxItems)
    {
        ICollection<Game> retrievedGames = new List<Game>();
        Game? retrievedGame = default;
        foreach (Game game in games)
        {
            if (retrievedGames.Count < maxItems)
            {
                retrievedGame = await GetByIdFromApiAsync(game);
                if (retrievedGame == default) continue;
                retrievedGames.Add(retrievedGame);
            }
        }
        return retrievedGames;
    }

    /// <summary>
    /// Get Game from Steam API. In case game details are not available - set ItemStatus to Unavailable, save to db to prevent unneded API requests and return default.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private async Task<Game?> GetByIdFromApiAsync(Game game)
    {
        if (!game.IsFilled())
        {
            GameJson? gameJson;
            GameReviewsJson? gameReviewsJson;
            gameJson = await _httpClient.GetByIdAsync(game.Id);
            if (gameJson == default)
            {
                Game? tmpGame = await _context.Games.FindAsync(game.Id);
                if (tmpGame != default)
                {
                    tmpGame.ItemStatus = ItemStatus.Unavailable;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Game {id}-{title} ({status}) was retrieved from API", tmpGame.Id, tmpGame.Title, tmpGame.ItemStatus);
                }
                return default;
            }
            game = _mapper.Map<Game>(gameJson);
            gameReviewsJson = await _httpClient.GetReviewsByIdAsync(game.Id) ?? new GameReviewsJson();
            game = _mapper.Map(gameReviewsJson, game);
            _context.ChangeTracker.Clear(); // Workaround needed to stop tracking Game when it is retrieved from db but not filled
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Game {id}-{title} ({status}) was retrieved from API", game.Id, game.Title, game.ItemStatus);
        }
        else
        {
            _logger.LogInformation("Game {id}-{title} was retrieved from DB", game.Id, game.Title);
        }
        return game;
    }

    /// <summary>
    /// Get list of games from Steam API and save it to own db to be able to retrieve them by title
    /// </summary>
    /// <returns></returns>
    public async Task UpdateAllAsync()
    {
        IEnumerable<GameJsonShort> gameJsonShorts = await _httpClient.GetListAsync();
        IEnumerable<Game> games = gameJsonShorts.Select(item => new Game(item.Id, item.Title));

        int totalGames = await _context.Games.CountAsync();
        if (totalGames == 0)
        {
            await _context.AddRangeAsync(games);
        }
        else
        {
            foreach (Game game in games)
            {
                if (await _context.Games.AnyAsync(item => item.Id == game.Id) == false)
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