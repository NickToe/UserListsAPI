using System;
using System.Timers;

namespace UserListsAPI.Utility;

public sealed class GlobalTimer
{
  private readonly ILogger _logger;
  private readonly string _filename;
  private readonly double _expirationTimeMin; // in minutes
  private System.Timers.Timer _timer = new System.Timers.Timer();
  private Func<Task> _timerHandler;

  public GlobalTimer(ILogger logger, string timerName, Func<Task> timerHandler)
  {
    _logger = logger;
    IConfiguration configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build().GetRequiredSection("Timers");
    _filename = configuration.GetValue<string>($"{timerName}:ConfigFile");
    _expirationTimeMin = configuration.GetValue<double>($"{timerName}:ExpirationTime");
    _timerHandler = timerHandler;

    SetupTimer();
    double expirationTime = ShouldBeUpdated() ? 0.01 : _expirationTimeMin - DateTime.Now.Subtract(LastUpdateTime).TotalMinutes;
    _logger.LogInformation("Expiration time: {0}", expirationTime);
    UpdateTimerInterval(TimeToMilliseconds(expirationTime));
  }

  private DateTime _lastUpdateTime;
  public DateTime LastUpdateTime
  {
    get
    {
      if (_lastUpdateTime == default)
      {
        _lastUpdateTime = GetUpdateTime();
      }
      return _lastUpdateTime;
    }
    private set { _lastUpdateTime = value; }
  }

  public bool ShouldBeUpdated()
  {
    if (LastUpdateTime == default)
    {
      _logger.LogInformation("Need to call the handler for the first time...");
      return true;
    }
    if (DateTime.Now.Subtract(LastUpdateTime).TotalMinutes > _expirationTimeMin)
    {
      _logger.LogInformation($"It's time to call handler... The last time was on {LastUpdateTime}");
      return true;
    }
    _logger.LogInformation($"It's too early to call handler... {_expirationTimeMin - DateTime.Now.Subtract(LastUpdateTime).TotalMinutes} minutes until update");
    return false;
  }

  public void UpdateConfigExpirationTime()
  {
    using (FileStream fs = new(_filename, FileMode.OpenOrCreate, FileAccess.Write))
    {
      using (StreamWriter sw = new(fs))
      {
        DateTime currentTime = DateTime.Now;
        sw.WriteLine(currentTime.ToString("G"));
        LastUpdateTime = currentTime;
      }
    }
  }

  private void SetupTimer()
  {
    _timer.Elapsed += _updateTimer_Elapsed;
    _timer.AutoReset = false;
    _timer.Enabled = true;
    _logger.LogInformation($"Timer was set up at {DateTime.Now}");
  }

  private void UpdateTimerInterval(double expirationTime)
  {
    _timer.Interval = expirationTime;
    _logger.LogInformation($"Timer was updated at {DateTime.Now} to {_timer.Interval} milliseconds");
  }

  private void _updateTimer_Elapsed(object? sender, ElapsedEventArgs e)
  {
    new Thread(() =>
    {
      _logger.LogInformation("Timer expired, call the handler");
      _timerHandler().Wait();
      _logger.LogInformation("Task completed!");
      UpdateConfigExpirationTime();
      UpdateTimerInterval(TimeToMilliseconds(_expirationTimeMin));
    }).Start();
  }

  private double TimeToMilliseconds(double expirationTime)
  {
    return expirationTime * 60 * 1000;
  }

  private DateTime GetUpdateTime()
  {
    DateTime updateDate;
    using (FileStream fs = new(_filename, FileMode.OpenOrCreate, FileAccess.Read))
    {
      using (StreamReader sr = new(fs))
      {
        string? updateDateStr = sr.ReadLine();
        try
        {
          updateDate = DateTime.Parse(updateDateStr);
        }
        catch
        {
          updateDate = DateTime.Now;
        }
      }
    }
    return updateDate;
  }
}