using Microsoft.AspNetCore.Mvc;
using UserListsAPI.DTOs;
using AutoMapper;
using UserListsAPI.Data.Entities;
using UserListsAPI.Services;

namespace UserListsAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class GameController : ControllerBase
{
  private readonly ILogger<GameController> _logger;
  private readonly IItemService<Game> _service;
  private readonly IMapper _autoMapper;
  public GameController(ILogger<GameController> logger, IItemService<Game> service, IMapper autoMapper)
  {
    _logger = logger;
    _service = service;
    _autoMapper = autoMapper;
  }

  [HttpGet("game/{id}")]
  public async Task<IActionResult> GetById([FromRoute] string id)
  {
    GameDTO game = _autoMapper.Map<GameDTO>(await _service.GetByIdAsync(id));
    return Ok(game);
  }

  [HttpGet("games")]
  public async Task<IActionResult> GetAllByIds([FromQuery] IEnumerable<string> ids)
  {
    IEnumerable<GameDTO> games = _autoMapper.Map<IEnumerable<GameDTO>>(await _service.GetAllByIdsAsync(ids));
    return Ok(games);
  }

  [HttpGet("game/title")]
  public async Task<IActionResult> GetByExactTitle([FromQuery] string title)
  {
    GameDTO game = _autoMapper.Map<GameDTO>(await _service.GetByExactTitleAsync(title));
    return Ok(game);
  }

  [HttpGet("games/title")]
  public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxItems = 10)
  {
    IEnumerable<GameDTO> games = _autoMapper.Map<IEnumerable<GameDTO>>(await _service.GetAllByTitleAsync(title, maxItems));
    return Ok(games);
  }
}