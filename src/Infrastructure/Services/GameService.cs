using Application.Abstractions;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Comparers;
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
        Game? game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok);

        if (game is not null && !game.IsFilled())
        {
            game = await GetFromApiByIdAsync(id);
        }

        return _mapper.Map<GameDTO>(game);
    }

    public async Task<IEnumerable<GameDTO>> GetAllByIdsAsync(IEnumerable<string> ids)
    {
        IEnumerable<Game> retrievedGames = await _context.Games
            .AsNoTracking()
            .Where(item => ids.Contains(item.Id) && item.ItemStatus == ItemStatus.Ok)
            .ToListAsync();

        ICollection<Game> games = new List<Game>();

        foreach (var game in retrievedGames)
        {
            if (!game.IsFilled())
            {
                Game? gameFromApi = await GetFromApiByIdAsync(game.Id);
                if (gameFromApi is not null)
                {
                    games.Add(gameFromApi);
                }
            }
            else
            {
                games.Add(game);
            }
        }

        return _mapper.Map<IEnumerable<GameDTO>>(games);
    }

    public async Task<GameDTO?> GetByExactTitleAsync(string title)
    {
        Game? game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Title.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok);

        if (game is not null && !game.IsFilled())
        {
            game = await GetFromApiByIdAsync(game.Id);
        }

        return _mapper.Map<GameDTO>(game);
    }

    public async Task<IEnumerable<GameDTO>> GetAllByTitleAsync(string title, int maxItems)
    {
        IEnumerable<Game> retrievedGames = await _context.Games
            .AsNoTracking()
            .Where(item => item.Title.ToLower().Contains(title.ToLower()) && item.ItemStatus == ItemStatus.Ok)
            .ToListAsync();

        if (!retrievedGames.Any())
        {
            _logger.LogInformation("Games title({title}) were not found in DB", title);
            return Enumerable.Empty<GameDTO>();
        }

        ICollection<Game> games = new List<Game>();

        foreach (var game in retrievedGames)
        {
            if (games.Count >= maxItems) break;

            if (!game.IsFilled())
            {
                Game? gameFromApi = await GetFromApiByIdAsync(game.Id);
                if (gameFromApi is not null)
                {
                    games.Add(gameFromApi);
                }
            }
            else
            {
                games.Add(game);
            }
        }

        return _mapper.Map<IEnumerable<GameDTO>>(games);
    }

    private async Task<Game?> GetFromApiByIdAsync(string id)
    {
        GameJson? gameJson = await _httpClient.GetByIdAsync(id);
        if (gameJson is null)
        {
            await UpdateGameStatus(id, ItemStatus.Unavailable);
            return default;
        }

        Game game = _mapper.Map<Game>(gameJson);

        GameReviewsJson? gameReviewsJson = await _httpClient.GetReviewsByIdAsync(id) ?? new GameReviewsJson();
        game = _mapper.Map(gameReviewsJson, game);

        _logger.LogInformation("Game {id}-{title} ({status}) was retrieved from API", game.Id, game.Title, game.ItemStatus);

        await UpdateGame(game);

        return game;
    }

    private async Task UpdateGame(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateGameStatus(string id, ItemStatus itemStatus)
    {
        Game? game = await _context.Games.FindAsync(id);

        if (game is null) return;

        game.ItemStatus = itemStatus;
        await _context.SaveChangesAsync();
    }

    public async Task AddAllAsync()
    {
        IEnumerable<GameJsonShort> gameJsonShorts = await _httpClient.GetListAsync();
        IEnumerable<Game> games = gameJsonShorts.Select(item => new Game(item.Id, item.Title));

        if (await _context.Games.CountAsync() == 0)
        {
            await _context.AddRangeAsync(games);
        }
        else
        {
            IEnumerable<Game> presentGames = await _context.Games.Select(item => new Game() { Id = item.Id }).ToListAsync();
            IEnumerable<Game> newGames = games.Except(presentGames, new GameIdComparer());
            await _context.AddRangeAsync(newGames);
        }

        int updatedRecords = await _context.SaveChangesAsync();

        _logger.LogInformation("Current time: {time}", DateTime.Now);
        _logger.LogInformation("{updatedRecords} records were updated", updatedRecords);
        _logger.LogInformation("Games in database: {count}", _context.Games.Count());
    }
}