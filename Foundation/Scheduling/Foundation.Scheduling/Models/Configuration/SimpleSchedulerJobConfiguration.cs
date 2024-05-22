using System.Text.Json.Serialization;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;

namespace TodoWebApp.Foundation.Scheduling.Models.Configuration;

public record SimpleSchedulerJobConfiguration
{
  public bool Enabled { get; init; } = false;
  public DateTime? AbsoluteStart { get; init; }
  [JsonConverter(typeof(JsonStringEnumConverter<DateTimeKind>))]
  public DateTimeKind AbsoluteStartType { get; init; } = DateTimeKind.Utc;
  public TimeSpan? RelativeStartDelay { get; init; }
  public TimeSpan? Interval { get; init; }
  [JsonConverter(typeof(JsonStringEnumConverter<PeriodicBehaviour>))]
  public PeriodicBehaviour PeriodicType { get; init; } = PeriodicBehaviour.FixedRate;
}