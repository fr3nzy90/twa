namespace TodoWebApp.Foundation.Scheduling.Models.DTOs;

public record ScheduleOptions
{
  public DateTime? AbsoluteStart { get; init; }
  public TimeSpan? RelativeStartDelay { get; init; }
  public TimeSpan? Interval { get; init; }
  public PeriodicBehaviour PeriodicType { get; init; } = PeriodicBehaviour.FixedRate;
}