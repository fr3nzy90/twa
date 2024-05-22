using Microsoft.Extensions.DependencyInjection;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;
using TodoWebApp.Foundation.Scheduling.Services;

namespace Foundation.Scheduling.Tools.ProtoConsole.Testing;

internal static class StressTester
{
  private record Schedulation(string ExternalId, ScheduleOptions Options, TimeSpan WorkDuration)
  {
    public Guid Id { get; set; }
  }

  private static readonly List<Schedulation> Schedulations;

  static StressTester()
  {
    var commonOptions = new ScheduleOptions
    {
      RelativeStartDelay = TimeSpan.FromSeconds(5),
      Interval = TimeSpan.Zero
    };
    var commonWorkDuration = TimeSpan.Zero;
    Schedulations = new()
    {
      new("TSK_A", commonOptions, commonWorkDuration),
      new("TSK_B", commonOptions, commonWorkDuration),
      new("TSK_C", commonOptions, commonWorkDuration),
      new("TSK_D", commonOptions, commonWorkDuration)
    };
  }

  public static void Run(IServiceProvider serviceProvider)
  {
    var schedulerService = serviceProvider.GetRequiredService<ISchedulerService>();
    Schedulations.ForEach(schedulerService.Add);
    Schedulations.ForEach(schedulerService.Schedule);
  }

  private static void Add(this ISchedulerService schedulerService, Schedulation schedulation)
  {
    schedulation.Id = schedulerService.Add(cancellationToken =>
    {
      var cancelled = cancellationToken.WaitHandle.WaitOne(schedulation.WorkDuration);
      if (cancelled)
      {
        Console.WriteLine($"Schedulation '{schedulation.ExternalId}' cancelled");
      }
      return Task.CompletedTask;
    }, schedulation.ExternalId);
  }

  private static void Schedule(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.Schedule(schedulation.Id, schedulation.Options);
}