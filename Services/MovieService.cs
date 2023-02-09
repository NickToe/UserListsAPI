using UserListsAPI.JsonModels;
using AutoMapper;
using UserListsAPI.Data.Enums;
using UserListsAPI.Data.Repositories;
using UserListsAPI.Data.Entities;
using UserListsAPI.ExternalApi;

namespace UserListsAPI.Services;

public class MovieService : IItemService<Movie>
{
  private readonly ILogger<MovieService> _logger;
  private readonly MovieHttpClient _httpClient;
  private readonly IItemRepository<Movie> _movieRepo;
  private readonly IMapper _mapper;

  public MovieService(ILogger<MovieService> logger, MovieHttpClient httpClient, IItemRepository<Movie> movieRepo, IMapper mapper)
  {
    _logger = logger;
    _httpClient = httpClient;
    _movieRepo = movieRepo;
    _mapper = mapper;
  }

  public async Task<Movie?> GetByIdAsync(string id)
  {
    Movie? movie = await _movieRepo.GetByIdAsync(id);
    if (movie != default)
    {
      _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
      return movie;
    }
    return await GetFullMovieByIdAsync(id);
  }

  public async Task<IEnumerable<Movie>> GetAllByIdsAsync(IEnumerable<string> ids)
  {
    ICollection<Movie> movies = new List<Movie>();
    Movie? movie = default;
    foreach (string id in ids)
    {
      movie = await GetByIdAsync(id);
      if (movie != default)
      {
        movies.Add(movie);
      }
    }
    _logger.LogInformation("Total movies obtained by id: {count}", movies.Count);
    return movies;
  }

  public async Task<Movie?> GetByExactTitleAsync(string title)
  {
    Movie? movie = await _movieRepo.GetByTitleAsync(title);
    if (movie != default)
    {
      _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
      return movie;
    }
    return await GetFullMovie((await _httpClient.GetAllByTitle(title)).FirstOrDefault());
  }

  public async Task<IEnumerable<Movie>> GetAllByTitleAsync(string title, int maxItems)
  {
    IEnumerable<Movie> movies = await _movieRepo.GetAllByTitleAsync(title, maxItems);
    ICollection<Movie> resultMovies = new List<Movie>(movies);
    if (resultMovies.Count < maxItems)
    {
      _logger.LogInformation("Movies found in database with title {title}: {count}", title, resultMovies.Count);
      IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetAllByTitle(title);
      Movie? movie = default;
      foreach (var jsonShort in movieJsonShorts)
      {
        if (resultMovies.Count >= maxItems)
        {
          break;
        }
        // API may return irrelavant results, so check if movie was already added
        if (!await _movieRepo.AnyAsync(jsonShort.Id))
        {
          movie = await GetFullMovie(jsonShort);
          if (movie != default)
          {
            resultMovies.Add(movie);
          }
        }
      }
    }
    _logger.LogInformation("Total movies obtained: {count}", resultMovies.Count);
    return resultMovies;
  }

  private async Task<Movie?> GetFullMovieByIdAsync(string id)
  {
    MovieJson? movieJson = await _httpClient.GetById(id);

    if (movieJson == default) return default;

    ItemStatus itemStatus = ItemStatus.Ok;
    // id can only be received from external API, so if the error of type 'not valid' was received - the item is irrelevant
    // API returns a lot of irrelevant results, so additional checks are required as well as adding these ids to DB to prevent new API requests
    if (!string.IsNullOrEmpty(movieJson.ErrorMessage) || movieJson.Type != "Movie" || string.IsNullOrEmpty(movieJson.Title))
    {
      if (movieJson.ErrorMessage.Contains("not valid"))
      {
        _logger.LogWarning("Error from external API for id({id}): {error}", id, movieJson.ErrorMessage);
      }
      else
      {
        _logger.LogWarning("Movie {id} was not found in API or error received or type is not 'Movie'", id);
      }
      movieJson.Id = id;
      movieJson.Title = String.Empty;
      itemStatus = ItemStatus.Irrelevant;
    }

    Movie movie = _mapper.Map<Movie>(movieJson);
    movie.ItemStatus = itemStatus;
    if (!await _movieRepo.AnyAsync(movieJson.Id))
    {
      _movieRepo.Add(movie);
      await _movieRepo.SaveChangesAsync();
    }

    _logger.LogInformation("Movie {id}-{title} ({status}) was retrieved from API", movie.Id, movie.Title, movie.ItemStatus);
    return itemStatus == ItemStatus.Ok ? movie : default;
  }

  private async Task<Movie?> GetFullMovie(MovieJsonShort? MovieJsonShort)
  {
    if (MovieJsonShort == default) return default;
    if (string.IsNullOrEmpty(MovieJsonShort.Poster) || string.IsNullOrEmpty(MovieJsonShort.Description)) return default;
    return await GetFullMovieByIdAsync(MovieJsonShort.Id);
  }

  public Task UpdateAllAsync()
  {
    throw new NotImplementedException();
  }
}
