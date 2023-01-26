namespace UserListsAPI.Utility;

public class TimerSetup
{
  private readonly TimerSetupService _service;
  private readonly ILogger _logger;
  public TimerSetup(ILogger logger, TimerSetupService service)
  {
    _logger = logger;
    _service = service;
  }

  public void StartAll()
  {
    UpdateGameListTimer();
  }

  public void UpdateGameListTimer()
  {
    GlobalTimer gameListTimer = new(_logger, "GameListTimer", _service.UpdateGameList);
  }
}