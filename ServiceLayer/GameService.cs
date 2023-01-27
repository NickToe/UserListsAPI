using UserListsAPI.HttpLayer;
using UserListsAPI.DataLayer.Entities;
using UserListsAPI.JsonModels;
using UserListsAPI.DataLayer.Repo;
using UserListsAPI.DataLayer.Enums;

namespace UserListsAPI.ServiceLayer;

public class GameService : IItemService<Game>
{
  private readonly ILogger<GameService> _logger;
  private readonly GameHttpClient _httpClient;
  private readonly IItemRepo<Game> _repo;

  public GameService(ILogger<GameService> logger, GameHttpClient httpClient, IItemRepo<Game> repo)
  {
    _logger = logger;
    _httpClient = httpClient;
    _repo = repo;
  }

  public async Task<Game?> GetByExactTitle(string title)
  {
    Game? game = await _repo.GetByTitle(title);
    if (game is null)
    {
      _logger.LogInformation("Game with title {title} was not found!", title);
      return default(Game);
    }
    return await GetFullGame(game);
  }

  public async Task<IEnumerable<Game>> GetAllByTitle(string title, int maxNumber)
  {
    IEnumerable<Game> games = (await _repo.GetAllByTitle(title)).ToList();
    if (!games.Any())
    {
      _logger.LogInformation("Games title({title}) were not found in DB", title);
      return Enumerable.Empty<Game>();
    }
    return await GetFullGames(games, maxNumber);
  }

  public async Task<Game?> GetById(string id)
  {
    Game? game = await _repo.GetById(id);
    if (game == null)
    {
      _logger.LogWarning("Game id({id}) was not found in DB", id);
      return default(Game);
    }
    return await GetFullGame(game);
  }

  public async Task<IEnumerable<Game>> GetAllByIds(IEnumerable<string> ids)
  {
    ICollection<Game> games = new List<Game>();
    Game? game = default(Game);
    foreach (string id in ids)
    {
      game = await GetById(id);
      if (game != default(Game))
      {
        games.Add(game);
      }
    }
    _logger.LogInformation("Games obtained by id: {count}", games.Count);
    return games;
  }

  private async Task<IEnumerable<Game>> GetFullGames(IEnumerable<Game> games, int maxNumber)
  {
    ICollection<Game> retrievedGames = new List<Game>();
    Game? retrievedGame = default(Game);
    foreach (Game game in games)
    {
      if (retrievedGames.Count < maxNumber)
      {
        retrievedGame = await GetFullGame(game);
        if (retrievedGame == default) break;
        retrievedGames.Add(retrievedGame);
      }
    }
    return retrievedGames;
  }

  private async Task<Game?> GetFullGame(Game game)
  {
    if (!game.IsFilled() && game.ItemStatus == ItemStatus.Ok)
    {
      GameJson? gameJson;
      GameReviewsJson? gameReviewsJson;
      gameJson = await _httpClient.GetItem(game.Id);
      if (gameJson == default)
      {
        await _repo.UpdateSuccess(game.Id, ItemStatus.Unavailable);
        return default(Game);
      }
      gameReviewsJson = await _httpClient.GetReviews(game.Id);
      game = Game.ToEntity(gameJson, gameReviewsJson);
      await _repo.Add(game);
      _logger.LogInformation("Game {id}-{title} was retrieved from API", game.Id, game.Title);
    }
    else
    {
      _logger.LogInformation("Game {id}-{title} was retrieved from DB", game.Id, game.Title);
    }
    return game;
  }

  public async Task UpdateAll()
  {
    IEnumerable<GameJsonShort> gameJsonShorts = await _httpClient.GetList();
    IEnumerable<Game> games = gameJsonShorts.Select(item => new Game(item.Id, item.Title));
    await _repo.UpdateAll(games);
  }
}