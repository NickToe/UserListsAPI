using UserListsAPI.JsonModels;
using AutoMapper;
using UserListsAPI.Data.Enums;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Repositories;
using UserListsAPI.ExternalApi;

namespace UserListsAPI.Services;

public class GameService : IItemService<Game>
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

  public async Task<Game?> GetByIdAsync(string id)
  {
    Game? game = await _repo.GetByIdAsync(id);
    if (game == null)
    {
      _logger.LogWarning("Game id({id}) was not found in DB", id);
      return default;
    }
    return await GetFullGameAsync(game);
  }

  public async Task<IEnumerable<Game>> GetAllByIdsAsync(IEnumerable<string> ids)
  {
    ICollection<Game> games = new List<Game>();
    Game? game = default;
    foreach (string id in ids)
    {
      game = await GetByIdAsync(id);
      if (game != default)
      {
        games.Add(game);
      }
    }
    _logger.LogInformation("Games obtained by id: {count}", games.Count);
    return games;
  }

  public async Task<Game?> GetByExactTitleAsync(string title)
  {
    Game? game = await _repo.GetByTitleAsync(title);
    if (game is null)
    {
      _logger.LogInformation("Game with title {title} was not found!", title);
      return default;
    }
    return await GetFullGameAsync(game);
  }

  public async Task<IEnumerable<Game>> GetAllByTitleAsync(string title, int maxItems)
  {
    IEnumerable<Game> games = await _repo.GetAllByTitleAsync(title, maxItems);
    if (!games.Any())
    {
      _logger.LogInformation("Games title({title}) were not found in DB", title);
      return Enumerable.Empty<Game>();
    }
    return await GetFullGamesAsync(games, maxItems);
  }

  private async Task<IEnumerable<Game>> GetFullGamesAsync(IEnumerable<Game> games, int maxItems)
  {
    ICollection<Game> retrievedGames = new List<Game>();
    Game? retrievedGame = default;
    foreach (Game game in games)
    {
      if (retrievedGames.Count < maxItems)
      {
        retrievedGame = await GetFullGameAsync(game);
        if (retrievedGame == default) break;
        retrievedGames.Add(retrievedGame);
      }
    }
    return retrievedGames;
  }

  private async Task<Game?> GetFullGameAsync(Game game)
  {
    if (!game.IsFilled() && game.ItemStatus == ItemStatus.Ok)
    {
      GameJson? gameJson;
      GameReviewsJson? gameReviewsJson;
      gameJson = await _httpClient.GetItem(game.Id);
      if (gameJson == default)
      {
        Game? tmpGame = await _repo.FindAsync(game.Id);
        if (tmpGame != default)
        {
          tmpGame.ItemStatus = ItemStatus.Unavailable;
          await _repo.SaveChangesAsync();
        }
        return default;
      }
      game = _mapper.Map<Game>(gameJson);
      gameReviewsJson = await _httpClient.GetReviews(game.Id) ?? new GameReviewsJson();
      game = _mapper.Map(gameReviewsJson, game);
      _repo.Add(game);
      await _repo.SaveChangesAsync();
      _logger.LogInformation("Game {id}-{title} was retrieved from API", game.Id, game.Title);
    }
    else
    {
      _logger.LogInformation("Game {id}-{title} was retrieved from DB", game.Id, game.Title);
    }
    return game;
  }

  public async Task UpdateAllAsync()
  {
    IEnumerable<GameJsonShort> gameJsonShorts = await _httpClient.GetList();
    IEnumerable<Game> games = gameJsonShorts.Select(item => new Game(item.Id, item.Title));
    await _repo.UpdateAllAsync(games);
  }
}