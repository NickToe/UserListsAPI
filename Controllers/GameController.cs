using Microsoft.AspNetCore.Mvc;
using UserListsAPI.ServiceLayer;
using UserListsAPI.DataLayer.Entities;

namespace UserListsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
  private readonly ILogger<GameController> _logger;
  private readonly IItemService<Game> _service;
  public GameController(ILogger<GameController> logger, IItemService<Game> service)
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
  public async Task<IActionResult> GetAllByIds([FromQuery] string[] ids)
  {
    return Ok(await _service.GetAllByIds(ids));
  }

  [HttpGet("ByExactTitle")]
  public async Task<IActionResult> GetByExactTitle([FromQuery] string exactTitle)
  {
    return Ok(await _service.GetByExactTitle(exactTitle));
  }

  [HttpGet("ByTitle")]
  public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxNumber = 10)
  {
    return Ok(await _service.GetAllByTitle(title, maxNumber));
  }
}