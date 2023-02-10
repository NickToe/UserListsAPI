using AutoMapper;
using UserListsAPI.Data.Entities;
using UserListsAPI.Data.Enums;
using UserListsAPI.Data.Repositories;
using UserListsAPI.DTOs;
using UserListsAPI.ExternalApi;
using UserListsAPI.JsonModels;

namespace UserListsAPI.Services;

public class MovieService : IItemService<MovieDTO>
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

    public async Task<MovieDTO?> GetByIdAsync(string id)
    {
        Movie? movie = await _movieRepo.GetByIdAsync(id) ?? await GetByIdFromApiAsync(id);
        return _mapper.Map<MovieDTO>(movie);
    }

    public async Task<IEnumerable<MovieDTO>> GetAllByIdsAsync(IEnumerable<string> ids)
    {
        ICollection<Movie> movies = new List<Movie>();
        Movie? movie = default;
        foreach (string id in ids)
        {
            movie = await GetByIdFromAsync(id) ?? await GetByIdFromApiAsync(id);
            if (movie != default)
            {
                movies.Add(movie);
            }
        }
        _logger.LogInformation("Total movies obtained by id: {count}", movies.Count);
        return _mapper.Map<IEnumerable<MovieDTO>>(movies);
    }

    private async Task<Movie?> GetByIdFromAsync(string id)
    {
        Movie? movie = await _movieRepo.GetByIdAsync(id);
        if (movie != default)
        {
            _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
            return movie;
        }
        return default;
    }

    public async Task<MovieDTO?> GetByExactTitleAsync(string title)
    {
        Movie? movie = await _movieRepo.GetByTitleAsync(title);
        if (movie != default)
        {
            _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
            return _mapper.Map<MovieDTO>(movie);
        }
        movie = await GetByIdFromShortJson((await _httpClient.GetAllByTitleAsync(title)).FirstOrDefault());
        return _mapper.Map<MovieDTO>(movie);
    }

    public async Task<IEnumerable<MovieDTO>> GetAllByTitleAsync(string title, int maxItems)
    {
        IEnumerable<Movie> movies = await _movieRepo.GetAllByTitleAsync(title, maxItems);
        ICollection<Movie> resultMovies = new List<Movie>(movies);
        if (resultMovies.Count < maxItems)
        {
            _logger.LogInformation("Movies found in database with title {title}: {count}", title, resultMovies.Count);
            IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetAllByTitleAsync(title);
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
                    movie = await GetByIdFromShortJson(jsonShort);
                    if (movie != default)
                    {
                        resultMovies.Add(movie);
                    }
                }
            }
        }
        _logger.LogInformation("Total movies obtained: {count}", resultMovies.Count);
        return _mapper.Map<IEnumerable<MovieDTO>>(resultMovies);
    }

    /// <summary>
    /// Get Movie from Imdb API. In case the result is irrelevant - set ItemStatus to Irrelevant, save to db to prevent unneeded API requests and return default
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<Movie?> GetByIdFromApiAsync(string id)
    {
        MovieJson? movieJson = await _httpClient.GetByIdAsync(id);

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

    private async Task<Movie?> GetByIdFromShortJson(MovieJsonShort? MovieJsonShort)
    {
        if (MovieJsonShort == default) return default;
        if (string.IsNullOrEmpty(MovieJsonShort.Poster) || string.IsNullOrEmpty(MovieJsonShort.Description)) return default;
        return await GetByIdFromApiAsync(MovieJsonShort.Id);
    }

    public Task UpdateAllAsync()
    {
        throw new NotImplementedException();
    }
}