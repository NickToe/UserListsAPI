using Microsoft.AspNetCore.Mvc;
using UserListsAPI.DTOs;
using AutoMapper;
using UserListsAPI.Services;
using UserListsAPI.Data.Entities;

namespace UserListsAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class MovieController : ControllerBase
{
  private readonly ILogger<MovieController> _logger;
  private readonly IItemService<Movie> _service;
  private readonly IMapper _mapper;

  public MovieController(ILogger<MovieController> logger, IItemService<Movie> service, IMapper mapper)
  {
    _logger = logger;
    _service = service;
    _mapper = mapper;
  }

  [HttpGet("movie/{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    MovieDTO movie = _mapper.Map<MovieDTO>(await _service.GetByIdAsync(id));
    return Ok(movie);
  }

  [HttpGet("movies")]
  public async Task<IActionResult> GetAllByIds([FromQuery] IEnumerable<string> ids)
  {
    IEnumerable<MovieDTO> movies = _mapper.Map<IEnumerable<MovieDTO>>(await _service.GetAllByIdsAsync(ids));
    return Ok(movies);
  }

  [HttpGet("movie/title")]
  public async Task<IActionResult> GetByExactTitle([FromQuery] string title, [FromQuery] string year)
  {
    MovieDTO movie = _mapper.Map<MovieDTO>(await _service.GetByExactTitleAsync(Movie.GetFullTitle(title, year)));
    return Ok(movie);
  }

  [HttpGet("movies/title")]
  public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxItems = 10)
  {
    IEnumerable<MovieDTO> movies = _mapper.Map<IEnumerable<MovieDTO>>(await _service.GetAllByTitleAsync(title, maxItems));
    return Ok(movies);
  }
}