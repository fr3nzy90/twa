using static TodoWebApp.Foundation.Scheduling.Models.DTOs.SchedulerWorkState;

namespace TodoWebApp.Foundation.Scheduling.Impl.Extensions;

internal static class SchedulerWorkStateExtensions
{
  public static Scheduling.Models.DTOs.SchedulerWorkState AssignableState(this Scheduling.Models.DTOs.SchedulerWorkState obj) =>
    obj switch
    {
      Created     => Scheduled,
      Scheduled   => Executing | Cancelled,
      Executing   => Completed | Aborted,
      Completed   => Scheduled | Rescheduled,
      Rescheduled => Executing | Cancelled,
      Cancelled   => Scheduled,
      Aborted     => Scheduled,
      _           => throw new NotImplementedException()
    };

  public static bool IsDestroyableState(this Scheduling.Models.DTOs.SchedulerWorkState obj) =>
    obj switch
    {
      Created     => true,
      Scheduled   => false, // NOTE: Not destroyable, since being queued
      Executing   => false, // NOTE: Not destroyable, since being executed
      Completed   => true,
      Rescheduled => false, // NOTE: Not destroyable, since being queued
      Cancelled   => true,
      Aborted     => true,
      _           => false
    };
}