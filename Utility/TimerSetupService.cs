using UserListsAPI.DataLayer.Entities;
using UserListsAPI.ServiceLayer;

namespace UserListsAPI.Utility;

public class TimerSetupService
{
  private readonly IItemService<Game> _service;
  public TimerSetupService(IItemService<Game> service)
  {
    _service = service;
  }

  public async Task UpdateGameList()
  {
    await _service.UpdateAll();
  }
}
