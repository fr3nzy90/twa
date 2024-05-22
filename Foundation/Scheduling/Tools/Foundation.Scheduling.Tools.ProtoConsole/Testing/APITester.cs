using Microsoft.Extensions.DependencyInjection;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;
using TodoWebApp.Foundation.Scheduling.Services;

namespace Foundation.Scheduling.Tools.ProtoConsole.Testing;

internal static class APITester
{
  private record Schedulation(string ExternalId, ScheduleOptions Options, TimeSpan WorkDuration)
  {
    public Guid Id { get; set; }
  }

  private static readonly bool CheckSchedulerSchedulations = false;
  private static readonly List<Schedulation> Schedulations;
  private static readonly List<IEnumerable<SchedulerWork>> SchedulerSchedulations = new();

  static APITester()
  {
    var commonOptions = new ScheduleOptions
    {
      RelativeStartDelay = TimeSpan.FromSeconds(2),
      Interval = TimeSpan.FromSeconds(3),
      PeriodicType = PeriodicBehaviour.FixedRate
    };
    var commonWorkDuration = TimeSpan.FromMilliseconds(50);
    Schedulations = new()
    {
      new("TSK_A", commonOptions, commonWorkDuration)
      //new("TSK_B", commonOptions, commonWorkDuration),
      //new("TSK_C", commonOptions, commonWorkDuration),
      //new("TSK_D", commonOptions, commonWorkDuration)
    };
  }

  public static void Run(IServiceProvider serviceProvider)
  {
    var schedulerService = serviceProvider.GetRequiredService<ISchedulerService>();
    Schedulations.ForEach(schedulerService.Add);
    schedulerService.GetAll();
    Schedulations.ForEach(schedulerService.Schedule);
    schedulerService.GetAll();
    Task.Run(() =>
    {
      CancellationToken.None.WaitHandle.WaitOne(TimeSpan.FromSeconds(2.5));
      schedulerService.GetAll();
    });
    Task.Run(() =>
    {
      CancellationToken.None.WaitHandle.WaitOne(TimeSpan.FromSeconds(4));
      Schedulations.ForEach(schedulerService.ExecuteNow);
    });
    Task.Run(() =>
    {
      CancellationToken.None.WaitHandle.WaitOne(TimeSpan.FromSeconds(4.5));
      schedulerService.GetAll();
    });
    Task.Run(() =>
    {
      CancellationToken.None.WaitHandle.WaitOne(TimeSpan.FromSeconds(8));
      Schedulations.ForEach(schedulerService.Cancel);
      schedulerService.GetAll();
    });
    Task.Run(() =>
    {
      CancellationToken.None.WaitHandle.WaitOne(TimeSpan.FromSeconds(9));
      Schedulations.ForEach(schedulerService.Remove);
      schedulerService.GetAll();
    });
  }

  private static void Add(this ISchedulerService schedulerService, Schedulation schedulation)
  {
    schedulation.Id = schedulerService.Add(cancellationToken =>
    {
      Console.WriteLine($"Hello from {schedulation.ExternalId}");
      var cancelled = cancellationToken.WaitHandle.WaitOne(schedulation.WorkDuration);
      if (cancelled)
      {
        Console.WriteLine($"Schedulation '{schedulation.ExternalId}' cancelled");
      }
      return Task.CompletedTask;
    }, schedulation.ExternalId);
  }

  private static void GetAll(this ISchedulerService schedulerService)
  {
    if (CheckSchedulerSchedulations)
    {
      SchedulerSchedulations.Add(schedulerService.GetAll());
    }
  }

  private static void Schedule(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.Schedule(schedulation.Id, schedulation.Options);

  private static void ExecuteNow(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.ExecuteNow(schedulation.Id);

  private static void Cancel(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.Cancel(schedulation.Id);

  private static void Remove(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.Remove(schedulation.Id);
}