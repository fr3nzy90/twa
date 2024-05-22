using TodoWebApp.Foundation.Scheduling.Models.Configuration;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;

namespace TodoWebApp.Foundation.Scheduling.Extensions;

internal static class SimpleSchedulerJobConfigurationExtensions
{
  public static void Convert(this SimpleSchedulerJobConfiguration obj, out ScheduleOptions options)
  {
    options = new()
    {
      AbsoluteStart = obj.AbsoluteStart.HasValue ? DateTime.SpecifyKind(obj.AbsoluteStart.Value, obj.AbsoluteStartType) : null,
      RelativeStartDelay = obj.RelativeStartDelay,
      Interval = obj.Interval,
      PeriodicType = obj.PeriodicType
    };
  }
}