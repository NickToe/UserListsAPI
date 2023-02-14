using Application.Abstractions;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class MovieController : ControllerBase
{
    private readonly ILogger<MovieController> _logger;
    private readonly IItemService<MovieDTO> _service;

    public MovieController(ILogger<MovieController> logger, IItemService<MovieDTO> service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("movie/{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        MovieDTO? movie = await _service.GetByIdAsync(id);
        if (movie == default) return NotFound();
        return Ok(movie);
    }

    [HttpGet("movies")]
    public async Task<IActionResult> GetAllByIds([FromQuery] IEnumerable<string> ids)
    {
        IEnumerable<MovieDTO> movies = await _service.GetAllByIdsAsync(ids);
        return Ok(movies);
    }

    [HttpGet("movie/title")]
    public async Task<IActionResult> GetByExactTitle([FromQuery] string title, [FromQuery] string year)
    {
        MovieDTO? movie = await _service.GetByExactTitleAsync($"{title} ({year})");
        if (movie == default) return NotFound();
        return Ok(movie);
    }

    [HttpGet("movies/title")]
    public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxItems = 10)
    {
        IEnumerable<MovieDTO> movies = await _service.GetAllByTitleAsync(title, maxItems);
        return Ok(movies);
    }
}