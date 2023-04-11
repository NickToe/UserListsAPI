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

public class MovieService : IItemService<MovieDTO>
{
    private readonly ILogger<MovieService> _logger;
    private readonly MovieHttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MovieService(ILogger<MovieService> logger, MovieHttpClient httpClient, AppDbContext context, IMapper mapper)
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = context;
        _mapper = mapper;
    }

    public async Task<MovieDTO?> GetByIdAsync(string id)
    {
        Movie? movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok) ?? await GetByIdFromApiAsync(id);

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
        Movie? movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok);

        if (movie != default)
        {
            _logger.LogInformation("Movie {id}-{title} was retrieved from DB", movie.Id, movie.Title);
            return movie;
        }

        return default;
    }

    public async Task<MovieDTO?> GetByExactTitleAsync(string title)
    {
        Movie? movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.FullTitle.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok);

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
        ICollection<Movie> movies = await _context.Movies
            .AsNoTracking()
            .Where(item => item.ItemStatus == ItemStatus.Ok && item.Title.ToLower().Contains(title.ToLower()))
            .OrderBy(item => item.FullTitle)
            .Take(maxItems)
            .ToListAsync();

        if (movies.Count < maxItems)
        {
            _logger.LogInformation("Movies found in database with title {title}: {count}", title, movies.Count);
            IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetAllByTitleAsync(title);
            Movie? movie = default;
            foreach (var jsonShort in movieJsonShorts)
            {
                if (movies.Count >= maxItems)
                {
                    break;
                }
                // API may return irrelavant results, so check if movie was already added
                if (!await _context.Movies.AnyAsync(item => item.Id == jsonShort.Id))
                {
                    movie = await GetByIdFromShortJson(jsonShort);
                    if (movie != default)
                    {
                        movies.Add(movie);
                    }
                }
            }
        }

        _logger.LogInformation("Total movies obtained: {count}", movies.Count);

        return _mapper.Map<IEnumerable<MovieDTO>>(movies);
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
            movieJson.Title = string.Empty;
            itemStatus = ItemStatus.Irrelevant;
        }

        Movie movie = _mapper.Map<Movie>(movieJson);
        movie.ItemStatus = itemStatus;

        if (!await _context.Movies.AnyAsync(item => item.Id == movieJson.Id))
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
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