using UserListsAPI.DataLayer.Entities;
using UserListsAPI.HttpLayer;
using UserListsAPI.JsonModels;
using UserListsAPI.DataLayer.Repo;
using UserListsAPI.DataLayer.Enums;

namespace UserListsAPI.ServiceLayer;

public class MovieService : IItemService<Movie>
{
  private readonly ILogger<MovieService> _logger;
  private readonly MovieHttpClient _httpClient;
  private readonly IItemRepo<Movie> _movieRepo;

  public MovieService(ILogger<MovieService> logger, MovieHttpClient httpClient, IItemRepo<Movie> movieRepo)
  {
    _logger = logger;
    _httpClient = httpClient;
    _movieRepo = movieRepo;
  }

  public async Task<Movie?> GetById(string id)
  {
    Movie? movie = await _movieRepo.GetById(id);
    if(movie != default)
    {
      _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
      return movie;
    }
    return await GetFullMovieById(id);
  }

  public async Task<IEnumerable<Movie>> GetAllByIds(IEnumerable<string> ids)
  {
    ICollection<Movie> movies = new List<Movie>();
    Movie? movie = default(Movie);
    foreach (string id in ids)
    {
      movie = await GetById(id);
      if (movie != default)
      {
        movies.Add(movie);
      }
    }
    _logger.LogInformation("Total movies obtained by id: {count}", movies.Count);
    return movies;
  }

  public async Task<Movie?> GetByExactTitle(string title)
  {
    Movie? movie = await _movieRepo.GetByTitle(title);
    if (movie != default)
    {
      _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
      return movie;
    }
    return await GetFullMovie((await _httpClient.GetAllByTitle(title)).FirstOrDefault());
  }

  public async Task<IEnumerable<Movie>> GetAllByTitle(string title, int maxNumber)
  {
    ICollection<Movie> movies = (await _movieRepo.GetAllByTitle(title)).ToList();
    if (movies.Count < maxNumber)
    {
      _logger.LogInformation("Movies found in database with title {title}: {count}", title, movies.Count);
      IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetAllByTitle(title);
      Movie? movie = default(Movie);
      foreach (var jsonShort in movieJsonShorts)
      {
        if (movies.Count >= maxNumber)
        {
          break;
        }
        // API may return irrelavant results, so check if movie was already added
        if(!await _movieRepo.Any(jsonShort.Id))
        {
          movie = await GetFullMovie(jsonShort);
          if (movie != default)
          {
            movies.Add(movie);
          }
        }
      }
    }
    _logger.LogInformation("Total movies obtained: {count}", movies.Count);
    return movies;
  }

  private async Task<Movie?> GetFullMovieById(string id)
  {
    MovieJson? movieJson = await _httpClient.GetById(id);
    if (movieJson == default || movieJson.Id == default || movieJson.Title == default)
    {
      _logger.LogInformation("Movie {id} was not found in API", id);
      return default(Movie);
    }
    // API returns a lot of irrelevant results, so additional checks are required as well as adding these ids to DB to prevent new API requests
    if(movieJson.Type == default || movieJson.Type != "Movie" || movieJson.RuntimeMins == default || movieJson.ReleaseDate == default)
    {
      _logger.LogInformation("Type of item with id({id}) is not 'Movie'", id);
      await _movieRepo.Add(Movie.ToEntity(movieJson));
      await _movieRepo.UpdateSuccess(id, ItemStatus.Irrelevant);
      return default(Movie);
    }
    Movie movie = Movie.ToEntity(movieJson);
    bool isAdded = await _movieRepo.Add(movie);
    _logger.LogInformation("Movie {id}-{title} was retrieved from API", movie.Id, movie.Title);
    return movie;
  }

  private async Task<Movie?> GetFullMovie(MovieJsonShort? MovieJsonShort)
  {
    if (MovieJsonShort == default) return default(Movie);
    if (string.IsNullOrEmpty(MovieJsonShort.Poster) || string.IsNullOrEmpty(MovieJsonShort.Description)) return default(Movie);
    return await GetFullMovieById(MovieJsonShort.Id);
  }

  public Task UpdateAll()
  {
    throw new NotImplementedException();
  }
}
