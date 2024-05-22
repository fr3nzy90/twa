namespace TodoWebApp.Foundation.Scheduling.Models.DTOs;

public record SchedulerWork
{
  public required Guid Id { get; init; }
  public required string ExternalId { get; init; }
  public SchedulerWorkState State { get; protected set; }
  public DateTime? ScheduledExecution { get; protected set; }
  public DateTime? LastCompletedExecution { get; protected set; }
  public TimeSpan? LastExecutionDuration { get; protected set; }
  public ExecutionTrigger? LastExecutionTrigger { get; protected set; }
  public TimeSpan? Interval { get; protected set; }
  public PeriodicBehaviour? PeriodicType { get; protected set; }
}