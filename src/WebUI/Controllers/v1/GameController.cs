using Application.Abstractions;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IItemService<GameDTO> _service;
    public GameController(ILogger<GameController> logger, IItemService<GameDTO> service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("game/{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        GameDTO? game = await _service.GetByIdAsync(id);
        if (game == default) return NotFound();
        return Ok(game);
    }

    [HttpGet("games")]
    public async Task<IActionResult> GetAllByIds([FromQuery] IEnumerable<string> ids)
    {
        IEnumerable<GameDTO> games = await _service.GetAllByIdsAsync(ids);
        return Ok(games);
    }

    [HttpGet("game/title")]
    public async Task<IActionResult> GetByExactTitle([FromQuery] string title)
    {
        GameDTO? game = await _service.GetByExactTitleAsync(title);
        if (game == default) return NotFound();
        return Ok(game);
    }

    [HttpGet("games/title")]
    public async Task<IActionResult> GetAllByTitle([FromQuery] string title, [FromQuery] int maxItems = 10)
    {
        IEnumerable<GameDTO> games = await _service.GetAllByTitleAsync(title, maxItems);
        return Ok(games);
    }
}