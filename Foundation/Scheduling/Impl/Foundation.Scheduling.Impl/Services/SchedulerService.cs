using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq.Expressions;
using TodoWebApp.Foundation.Scheduling.Impl.Extensions;
using TodoWebApp.Foundation.Scheduling.Impl.Shared;
using TodoWebApp.Foundation.Shared.Extensions.Logging;
using TodoWebApp.Foundation.Shared.Extensions.System;
using TodoWebApp.Foundation.Shared.Extensions.Threading;

namespace TodoWebApp.Foundation.Scheduling.Impl.Services;

internal sealed class SchedulerService : BackgroundService, Scheduling.Services.ISchedulerService
{
  private readonly ILogger _logger;
  private readonly Dictionary<Guid, SchedulerWork> _workStorage = new();
  private readonly SchedulerWorkQueue _scheduledWorkQueue = new();
  private readonly Foundation.Shared.Threading.Synchronizer _synchronizer = new();

  public SchedulerService(ILogger<SchedulerService> logger)
  {
    _logger = logger;
  }

  #region ISchedulerService
  public Guid Add(Func<CancellationToken, Task> work, string externalId, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var id = Guid.NewGuid();
        while (_workStorage.ContainsKey(id))
        {
          id = Guid.NewGuid();
        }
        var obj = SchedulerWork.Create(id, externalId, work);
        _workStorage.Add(id, obj);
        _logger.LogInformation("{0} added", obj);
        return id;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(externalId), externalId),
          new(nameof(timeout), timeout)
        ]
      });
  }

  public bool Cancel(Guid id, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
       _synchronizer.TryExecute(() =>
       {
         var success = _workStorage.TryGetValue(id, out var workObj);
         if (!success || null == workObj || !workObj.CanCancel())
         {
           return false;
         }
         _scheduledWorkQueue.Remove(id);
         workObj.Cancel();
         _logger.LogInformation("{0} {1}", workObj, workObj.State == Models.DTOs.SchedulerWorkState.Cancelled ? "cancelled" : "aborted");
         return true;
       }, timeout),
       new()
       {
         Parameters = [
           new(nameof(id), id),
          new(nameof(timeout), timeout)
         ]
       });
  }

  public bool ExecuteNow(Guid id, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var success = _workStorage.TryGetValue(id, out var workObj);
        if (!success || null == workObj)
        {
          return false;
        }
        var canSchedule = workObj.CanSchedule();
        if (canSchedule)
        {
          workObj.Schedule(new());
        }
        var canExecute = workObj.CanExecute();
        if (canExecute)
        {
          StartExecuteScheduledWork(id, Models.DTOs.ExecutionTrigger.APIRequest, CancellationToken.None);
          _logger.LogInformation("{0} execution requested via API", workObj);
          return true;
        }
        return false;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(id), id),
          new(nameof(timeout), timeout)
        ]
      });
  }

  public Models.DTOs.SchedulerWork? Get(Guid id, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var success = _workStorage.TryGetValue(id, out var temp);
        if (!success)
        {
          return null;
        }
        var result = SchedulerWorkFactory.CreateFrom(temp!);
        return result;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(id), id),
          new(nameof(timeout), timeout)
        ]
      });
  }

  public IEnumerable<Models.DTOs.SchedulerWork> GetAll(Expression<Func<Models.DTOs.SchedulerWork, bool>>? filter, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var temp = _workStorage
          .Values
          .Select(obj => obj as Models.DTOs.SchedulerWork);
        if (null != filter)
        {
          temp = temp.Where(filter.Compile());
        }
        var result = temp
          .Select(obj => SchedulerWorkFactory.CreateFrom((obj as SchedulerWork)!))
          .ToList();
        return result;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(filter), filter),
          new(nameof(timeout), timeout)
        ]
      });
  }

  public bool Remove(Guid id, TimeSpan? timeout)
  {
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var success = _workStorage.TryGetValue(id, out var workObj);
        if (!success || null == workObj || !workObj.State.IsDestroyableState())
        {
          return false;
        }
        _workStorage.Remove(id);
        _logger.LogInformation("{0} removed", workObj);
        workObj.Dispose();
        return true;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(id), id),
          new(nameof(timeout), timeout)
        ]
      });
  }

  public bool Schedule(Guid id, Models.DTOs.ScheduleOptions? options, TimeSpan? timeout)
  {
    options ??= new();
    timeout ??= Scheduling.Services.ISchedulerService.DefaultOperationTimeout;
    return _logger.ExecuteWithLogging(() =>
      _synchronizer.TryExecute(() =>
      {
        var success = _workStorage.TryGetValue(id, out var workObj);
        if (!success || null == workObj || !workObj.CanSchedule())
        {
          return false;
        }
        workObj.Schedule(options);
        _scheduledWorkQueue.Add(workObj.Id, workObj.ScheduledExecution!.Value);
        _synchronizer.PulseAll();
        _logger.LogInformation("{0} scheduled at {1}", workObj, workObj.ScheduledExecution);
        return true;
      }, timeout),
      new()
      {
        Parameters = [
          new(nameof(id), id),
          new(nameof(options), options),
          new(nameof(timeout), timeout)
        ]
      });
  }
  #endregion

  #region Task processing
  protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
    _logger.ExecuteWithLogging(() =>
    {
      cancellationToken.Register(() => _synchronizer.Execute(_synchronizer.PulseAll));
      var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      return Task.Factory.StartNew(new Action<object?>(state => ProcessScheduledWork((CancellationToken)state!)),
        cancellationTokenSource.Token,
        cancellationTokenSource.Token,
        TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness,
        TaskScheduler.Default);
    });

  private void ProcessScheduledWork(CancellationToken cancellationToken) =>
    _logger.ExecuteWithLogging(() =>
    {
      _logger.LogInformation("{0} started", nameof(SchedulerService));
      _synchronizer.Execute(() =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          var scheduledWorkId = _scheduledWorkQueue.NextScheduled?.Id;
          if (scheduledWorkId.HasValue)
          {
            StartExecuteScheduledWork(scheduledWorkId.Value, Models.DTOs.ExecutionTrigger.Internal, cancellationToken);
            continue;
          }
          _synchronizer.Wait(_scheduledWorkQueue.NextScheduledIn?.LimitLow(TimeSpan.Zero));
        }
      });
      _logger.LogInformation("{0} shutting down", nameof(SchedulerService));
    });

  // NOTE: Method must be executed inside synchronizer scope
  private void StartExecuteScheduledWork(Guid id, Models.DTOs.ExecutionTrigger trigger, CancellationToken cancellationToken) =>
    _logger.ExecuteWithLogging(() =>
    {
      _scheduledWorkQueue.Remove(id);
      var success = _workStorage.TryGetValue(id, out var workObj);
      if (!success)
      {
        _logger.LogError("Error while retrieving {0} for id={1}", nameof(SchedulerWork), id);
        return;
      }
      workObj!.Execute(trigger, cancellationToken);
      Task.Factory.StartNew(new Action<object?>(state => ExecuteWork((SchedulerWork)state!)), workObj, workObj.CancellationToken);
    },
    new()
    {
      Parameters = [
        new(nameof(id), id)
      ]
    });

  private void ExecuteWork(SchedulerWork workObj) =>
    _logger.ExecuteWithLogging(() =>
    {
      #region Pre-execution
      var stopwatch = new Stopwatch();
      DateTime completedExecution;

      string workStr;
      Func<CancellationToken, Task> work;
      CancellationToken cancellationToken;
      Models.DTOs.SchedulerWorkState state;

      (workStr, work, cancellationToken, state) =
        _synchronizer.Execute(() => (workObj.ToString(), workObj.Work, workObj.CancellationToken, workObj.State));
      #endregion
      #region Execution
      if (Models.DTOs.SchedulerWorkState.Executing != state) return;
      try
      {
        _logger.LogDebug("{0} execution started", workStr);
        stopwatch.Start();
        work(cancellationToken).Wait(cancellationToken);
        stopwatch.Stop();
        completedExecution = DateTime.UtcNow;
        _logger.LogInformation("{0} execution ended in {1} ms", workStr, stopwatch.ElapsedMilliseconds);
      }
      catch (Exception e)
      {
        completedExecution = DateTime.UtcNow;
        _logger.LogError("{0} execution failed: {1}", workStr, e.Message);
      }
      #endregion
      #region Post-execution
      _synchronizer.Execute(() =>
      {
        if (Models.DTOs.SchedulerWorkState.Executing != workObj.State) return;

        if (workObj.CancellationToken.IsCancellationRequested)
        {
          workObj.Cancel();
          _logger.LogInformation("{0} aborted", workObj);
          return;
        }

        workObj.Complete(completedExecution, TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
        _logger.LogInformation("{0} completed at {1}", workObj, workObj.LastCompletedExecution);

        if (!workObj.CanReschedule()) return;
        workObj.Reschedule();
        _scheduledWorkQueue.Add(workObj.Id, workObj.ScheduledExecution!.Value);
        _logger.LogInformation("{0} rescheduled at {1}", workObj, workObj.ScheduledExecution);
        _synchronizer.PulseAll();
      });
      #endregion
    },
    new()
    {
      Parameters = [
        new(nameof(workObj), workObj)
      ]
    });
  #endregion
}