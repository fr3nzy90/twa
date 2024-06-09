using System.Text.Json.Serialization;

namespace TodoWebApp.Foundation.Scheduling.Models.Configuration;

public record SimpleSchedulerJobConfiguration
{
  public bool Enabled { get; init; } = false;
  public DateTime? AbsoluteStart { get; init; }
  [JsonConverter(typeof(JsonStringEnumConverter<DateTimeKind>))]
  public DateTimeKind AbsoluteStartType { get; init; } = DateTimeKind.Utc;
  public TimeSpan? RelativeStartDelay { get; init; }
  public TimeSpan? Interval { get; init; }
  [JsonConverter(typeof(JsonStringEnumConverter<DTOs.PeriodicBehaviour>))]
  public DTOs.PeriodicBehaviour PeriodicType { get; init; } = DTOs.PeriodicBehaviour.FixedRate;
}