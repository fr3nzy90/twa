namespace TodoWebApp.Foundation.Scheduling.Models.DTOs;

[Flags]
public enum SchedulerWorkState
{
  Created     = 0b_0000_0001,
  Scheduled   = 0b_0000_0010,
  Executing   = 0b_0000_0100,
  Completed   = 0b_0000_1000,
  Rescheduled = 0b_0001_0000,
  Cancelled   = 0b_0010_0000,
  Aborted     = 0b_0100_0000
}