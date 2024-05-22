namespace TodoWebApp.Foundation.Scheduling.Impl.Shared;

internal static class SchedulerWorkFactory
{
  private sealed record SchedulerWork : Models.DTOs.SchedulerWork
  {
    // NOTE: This record is needed due to C# dumb implementation, since it does not allow to initialize base class inside derived when
    //       using protected set properties...
    public static Models.DTOs.SchedulerWork CreateFrom(Shared.SchedulerWork obj) =>
      new SchedulerWork()
      {
        Id = obj.Id,
        ExternalId = obj.ExternalId,
        State = obj.State,
        ScheduledExecution = obj.ScheduledExecution,
        LastCompletedExecution = obj.LastCompletedExecution,
        LastExecutionDuration = obj.LastExecutionDuration,
        LastExecutionTrigger = obj.LastExecutionTrigger,
        Interval = obj.Interval,
        PeriodicType = obj.PeriodicType
      };
  }

  public static Models.DTOs.SchedulerWork CreateFrom(Shared.SchedulerWork obj) =>
    SchedulerWork.CreateFrom(obj);
}