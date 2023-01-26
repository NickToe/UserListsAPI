using UserListsAPI.DataLayer.Entities;
using UserListsAPI.ServiceLayer;
using Microsoft.AspNetCore.Mvc;

namespace UserListsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase
{
  private readonly ILogger<MovieController> _logger;
  private readonly IItemService<Movie> _service;

  public MovieController(ILogger<MovieController> logger, IItemService<Movie> service)
  {
    _logger = logger;
    _service = service;
  }

  [HttpGet("ById")]
  public async Task<IActionResult> GetById([FromQuery] string id)
  {
    return Ok(await _service.GetById(id));
  }

  [HttpGet("ByIds")]
  public async Task<IActionResult> GetAllByIds([FromQuery] IEnumerable<string> ids)
  {
    return Ok(await _service.GetAllByIds(ids));
  }

  [HttpGet("ByExactTitle")]
  public async Task<IActionResult> GetByExactTitle([FromQuery] string title, [FromQuery] string year)
  {
    return Ok(await _service.GetByExactTitle(Movie.GetFullTitle(title, year)));
  }

  [HttpGet("ByTitle")]
  public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxNumber = 30)
  {
    return Ok(await _service.GetAllByTitle(title, maxNumber));
  }
}
