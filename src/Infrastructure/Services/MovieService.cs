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
            .FirstOrDefaultAsync(item => item.Id == id && item.ItemStatus == ItemStatus.Ok)
            ??
            await GetFromApiByIdAsync(id);

        return _mapper.Map<MovieDTO>(movie);
    }

    public async Task<IEnumerable<MovieDTO>> GetAllByIdsAsync(IEnumerable<string> ids)
    {
        ICollection<Movie> retrievedMovies = await _context.Movies
            .AsNoTracking()
            .Where(item => ids.Contains(item.Id) && item.ItemStatus == ItemStatus.Ok)
            .ToListAsync();

        IEnumerable<Movie> movies = ids
            .Select(id => new Movie() { Id = id })
            .Except(retrievedMovies, new MovieIdComparer());

        foreach (var loopMovie in movies)
        {
            Movie? movie = await GetFromApiByIdAsync(loopMovie.Id);
            if(movie is not null)
            {
                retrievedMovies.Add(movie);
            }
        }

        return _mapper.Map<IEnumerable<MovieDTO>>(retrievedMovies);
    }

    public async Task<MovieDTO?> GetByExactTitleAsync(string title)
    {
        Movie? movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.FullTitle.ToLower() == title.ToLower() && item.ItemStatus == ItemStatus.Ok)
            ??
            await GetFromApiByTitleAsync(title);

        return _mapper.Map<MovieDTO>(movie);
    }

    private async Task<Movie?> GetFromApiByTitleAsync(string title)
    {
        IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetByTitleAsync(title);

        foreach (var movieJsonShort in movieJsonShorts)
        {
            if (movieJsonShort.IsValid())
            {
                Movie? movie = await GetFromApiByIdAsync(movieJsonShort.Id);
                if (movie is not null)
                {
                    return movie;
                }
            }
        }

        return default;
    }

    public async Task<IEnumerable<MovieDTO>> GetAllByTitleAsync(string title, int maxItems)
    {
        ICollection<Movie> movies = await _context.Movies
            .AsNoTracking()
            .Where(item => item.Title.ToLower().Contains(title.ToLower()) && item.ItemStatus == ItemStatus.Ok)
            .Take(maxItems)
            .ToListAsync();

        if (movies.Count < maxItems)
        {
            IEnumerable<MovieJsonShort> movieJsonShorts = await _httpClient.GetByTitleAsync(title);

            foreach (var movieJsonShort in movieJsonShorts)
            {
                if(movieJsonShort.IsValid() && !movies.Any(item => item.Id == movieJsonShort.Id))
                {
                    Movie? movie = await GetFromApiByIdAsync(movieJsonShort.Id);
                    if (movie is not null)
                    {
                        movies.Add(movie);
                        if (movies.Count >= maxItems) break;
                    }
                }
            }
        }

        return _mapper.Map<IEnumerable<MovieDTO>>(movies);
    }

    private async Task<Movie?> GetFromApiByIdAsync(string id)
    {
        MovieJson movieJson = await _httpClient.GetByIdAsync(id);

        if (!movieJson.IsValidMovie())
        {
            _logger.LogWarning("Movie {id} API error: {errorMessage}", id, movieJson.ErrorMessage);
            return default;
        }

        Movie movie = _mapper.Map<Movie>(movieJson);

        await AddMovieAsync(movie);

        return movie;
    }

    private async Task AddMovieAsync(Movie movie)
    {
        // Unfortunately, we have to check if Id is already in DB because results from external api are not reliable and may return irrelevant movies
        if (!await _context.Movies.AnyAsync(item => item.Id == movie.Id))
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
        }
    }

    public Task AddAllAsync()
    {
        throw new NotImplementedException();
    }
}