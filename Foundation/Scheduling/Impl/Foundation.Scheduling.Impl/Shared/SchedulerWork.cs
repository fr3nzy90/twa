using TodoWebApp.Foundation.Scheduling.Impl.Extensions;

namespace TodoWebApp.Foundation.Scheduling.Impl.Shared;

internal sealed record SchedulerWork : Models.DTOs.SchedulerWork, IDisposable
{
  public required Func<CancellationToken, Task> Work { get; init; }
  public CancellationTokenSource? CancellationTokenSource { get; private set; }

  public CancellationToken CancellationToken =>
    CancellationTokenSource?.Token ?? CancellationToken.None;

  public void Dispose()
  {
    CancellationTokenSource?.Dispose();
    CancellationTokenSource = null;
  }

  public override string ToString() =>
    $"{nameof(SchedulerWork)}{{{ExternalId}, {Id}}}";

  #region Creation
  public static SchedulerWork Create(Guid id, string externalId, Func<CancellationToken, Task> work) =>
    new()
    {
      Id = id,
      ExternalId = externalId,
      State = Models.DTOs.SchedulerWorkState.Created,
      Work = work
    };
  #endregion

  #region State manipulation
  public bool CanSchedule() =>
    ChangableTo(Models.DTOs.SchedulerWorkState.Scheduled);

  public void Schedule(Models.DTOs.ScheduleOptions options)
  {
    SetState(Models.DTOs.SchedulerWorkState.Scheduled);
    Interval = options.Interval;
    PeriodicType = options.PeriodicType;
    ScheduledExecution = options switch
    {
      { AbsoluteStart: var temp } when temp.HasValue => CalculateScheduledExecution(temp.Value.ToUniversalTime(), Interval),
      { RelativeStartDelay: var temp } when temp.HasValue => DateTime.UtcNow + temp.Value,
      _ => DateTime.UtcNow
    };
  }

  public bool CanExecute() =>
    ChangableTo(Models.DTOs.SchedulerWorkState.Executing);

  public void Execute(Models.DTOs.ExecutionTrigger trigger, CancellationToken cancellationToken)
  {
    SetState(Models.DTOs.SchedulerWorkState.Executing);
    LastCompletedExecution = null;
    LastExecutionDuration = null;
    LastExecutionTrigger = trigger;
    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
  }

  public bool CanComplete() =>
    ChangableTo(Models.DTOs.SchedulerWorkState.Completed);

  public void Complete(DateTime completedExecution, TimeSpan executionDuration)
  {
    SetState(Models.DTOs.SchedulerWorkState.Completed);
    LastCompletedExecution = completedExecution;
    LastExecutionDuration = executionDuration;
    CancellationTokenSource!.Dispose();
    CancellationTokenSource = null;
  }

  public bool CanReschedule() =>
    ChangableTo(Models.DTOs.SchedulerWorkState.Rescheduled);

  public void Reschedule()
  {
    SetState(Models.DTOs.SchedulerWorkState.Rescheduled);
    if (Models.DTOs.PeriodicBehaviour.FixedRate == PeriodicType)
    {
      if (Models.DTOs.ExecutionTrigger.APIRequest != LastExecutionTrigger)
      {
        ScheduledExecution = ScheduledExecution + Interval!.Value;
      }
    }
    else
    {
      ScheduledExecution = LastCompletedExecution!.Value + Interval!.Value;
    }
  }

  public bool CanCancel() =>
    ChangableTo(Models.DTOs.SchedulerWorkState.Aborted) || ChangableTo(Models.DTOs.SchedulerWorkState.Cancelled);

  public void Cancel()
  {
    var cancelState = ChangableTo(Models.DTOs.SchedulerWorkState.Aborted) ?
      Models.DTOs.SchedulerWorkState.Aborted :
      Models.DTOs.SchedulerWorkState.Cancelled;
    SetState(cancelState);
    if (null != CancellationTokenSource)
    {
      CancellationTokenSource.Cancel();
      CancellationTokenSource.Dispose();
      CancellationTokenSource = null;
    }
  }
  #endregion

  #region Internal
  private SchedulerWork()
  {
  }

  private bool ChangableTo(Models.DTOs.SchedulerWorkState state) =>
    Assignable(state) && ValidateStateRequirements(state);

  private void SetState(Models.DTOs.SchedulerWorkState state)
  {
    if (!ChangableTo(state)) throw new InvalidOperationException($"{nameof(SchedulerWork)} invalid state");
    State = state;
  }

  private bool Assignable(Models.DTOs.SchedulerWorkState state) =>
    State.AssignableState().HasFlag(state);

  private bool ValidateStateRequirements(Models.DTOs.SchedulerWorkState state) =>
    state switch
    {
      Models.DTOs.SchedulerWorkState.Rescheduled => Interval.HasValue && PeriodicType.HasValue,
      _ => true
    };

  private static DateTime CalculateScheduledExecution(DateTime scheduledExecution, TimeSpan? period)
  {
    if (!period.HasValue || period == TimeSpan.Zero)
    {
      return scheduledExecution;
    }
    var temp = DateTime.UtcNow;
    if (scheduledExecution >= temp)
    {
      return scheduledExecution;
    }
    return scheduledExecution + Math.Ceiling((temp - scheduledExecution) / period.Value) * period.Value;
  }
  #endregion
}