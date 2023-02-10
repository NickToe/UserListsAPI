using AutoMapper;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Enums;
using UserListsAPI.Data.Repositories;
using UserListsAPI.DTOs;
using UserListsAPI.ExternalApi;
using UserListsAPI.JsonModels;

namespace UserListsAPI.Services;

public class GameService : IItemService<GameDTO>
{
    private readonly ILogger<GameService> _logger;
    private readonly GameHttpClient _httpClient;
    private readonly IItemRepository<Game> _repo;
    private readonly IMapper _mapper;

    public GameService(ILogger<GameService> logger, GameHttpClient httpClient, IItemRepository<Game> repo, IMapper mapper)
    {
        _logger = logger;
        _httpClient = httpClient;
        _repo = repo;
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
        Game? game = await _repo.GetByIdAsync(id);
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
        Game? game = await _repo.GetByTitleAsync(title);
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
        IEnumerable<Game> games = await _repo.GetAllByTitleAsync(title, maxItems);
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
                Game? tmpGame = await _repo.FindAsync(game.Id);
                if (tmpGame != default)
                {
                    tmpGame.ItemStatus = ItemStatus.Unavailable;
                    await _repo.SaveChangesAsync();
                    _logger.LogInformation("Game {id}-{title} ({status}) was retrieved from API", tmpGame.Id, tmpGame.Title, tmpGame.ItemStatus);
                }
                return default;
            }
            game = _mapper.Map<Game>(gameJson);
            gameReviewsJson = await _httpClient.GetReviewsByIdAsync(game.Id) ?? new GameReviewsJson();
            game = _mapper.Map(gameReviewsJson, game);
            _repo.Add(game);
            await _repo.SaveChangesAsync();
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
        await _repo.UpdateAllAsync(games);
    }
}