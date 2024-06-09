using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TodoWebApp.Foundation.Scheduling.Extensions;
using TodoWebApp.Foundation.Shared.Extensions.Logging;
using TodoWebApp.Foundation.Shared.Extensions.Threading;

namespace TodoWebApp.Foundation.Scheduling.Jobs;

public abstract class SimpleSchedulerJobBase<T> : BackgroundService
  where T : Models.Configuration.SimpleSchedulerJobConfiguration
{
  protected readonly ILogger _logger;
  protected readonly Services.ISchedulerService _schedulerService;
  protected readonly string _name;
  protected readonly string _configurationName;
  protected readonly IOptionsMonitor<T> _configurationMonitor;
  private readonly IDisposable? _configurationChangeListenerHandle;
  private readonly Shared.Threading.Synchronizer _synchronizer;
  private T? _currentConfiguration;
  private Guid _schedulerWorkId;

  protected SimpleSchedulerJobBase(ILogger logger,
    Services.ISchedulerService schedulerService,
    string name,
    string configurationName,
    IOptionsMonitor<T> configurationMonitor)
  {
    _logger = logger;
    _schedulerService = schedulerService;
    _name = name;
    _configurationName = configurationName;
    _configurationMonitor = configurationMonitor;
    _configurationChangeListenerHandle = _configurationMonitor.OnChange(OnConfigurationChange);
    _synchronizer = new();
    _schedulerWorkId = _schedulerService.Add(ExecuteWork, _name, Services.ISchedulerService.InfiniteOperationTimeout);
  }

  public virtual new void Dispose() =>
    _logger.ExecuteWithLogging(() =>
    {
      base.Dispose();
      _configurationChangeListenerHandle?.Dispose();
      Cancel();
      var success = _schedulerService.Remove(_schedulerWorkId, Services.ISchedulerService.InfiniteOperationTimeout);
      if (!success)
      {
        _logger.LogCritical("{0} removing failed", _name);
      }
    });

  protected abstract Task Work(T configuration, CancellationToken cancellationToken);

  protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
    _logger.ExecuteWithLoggingAsync(() =>
    {
      _currentConfiguration = _configurationMonitor.Get(_configurationName);
      Schedule();
      return Task.CompletedTask;
    });

  private void Schedule() =>
    _logger.ExecuteWithLogging(() =>
    {
      if (null == _currentConfiguration || !_currentConfiguration.Enabled) return;
      _synchronizer.Execute(() =>
      {
        _currentConfiguration.Convert(out var options);
        var success = _schedulerService.Schedule(_schedulerWorkId, options, Services.ISchedulerService.InfiniteOperationTimeout);
        if (!success)
        {
          _logger.LogCritical("{0} scheduling failed", _name);
        }
      });
    });

  private void Cancel() =>
    _logger.ExecuteWithLogging(() =>
    {
      var success = _schedulerService.Cancel(_schedulerWorkId, Services.ISchedulerService.InfiniteOperationTimeout);
      if (!success)
      {
        _logger.LogCritical("{0} cancelling failed", _name);
      }
    });

  private void OnConfigurationChange(T configuration, string? name) =>
    _logger.ExecuteWithLogging(() =>
    {
      if (_configurationName != name) return;
      _synchronizer.Execute(() =>
      {
        if (true == _currentConfiguration?.Equals(configuration)) return;
        Cancel();
        _currentConfiguration = configuration;
        Schedule();
      });
    },
    new()
    {
      Parameters = [
        new(nameof(configuration), configuration),
        new(nameof(name), name)
      ]
    });

  private Task ExecuteWork(CancellationToken cancellationToken) =>
    _logger.ExecuteWithLoggingAsync(() =>
    {
      if (null == _currentConfiguration) throw new ApplicationException("Configuration is null");
      return Work(_currentConfiguration, cancellationToken);
    });
}