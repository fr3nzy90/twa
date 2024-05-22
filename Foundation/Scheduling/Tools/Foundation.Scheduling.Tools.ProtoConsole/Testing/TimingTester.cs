using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;
using TodoWebApp.Foundation.Scheduling.Services;

namespace Foundation.Scheduling.Tools.ProtoConsole.Testing;

internal static class TimingTester
{
  private class TimingTest : IDisposable
  {
    private int Iteration { get; set; } = 0;
    private long ExecutionBegin { get; set; }
    private long ExecutionEnd { get; set; }
    private StreamWriter ReportWriter { get; init; }
    private Stopwatch Stopwatch { get; init; } = Stopwatch.StartNew();

    public TimingTest(string path, bool writeHeader = true)
    {
      ReportWriter = new StreamWriter(path);
      if (writeHeader)
      {
        ReportWriter.WriteLine(Header());
        ReportWriter.Flush();
      }
    }

    public void Dispose()
    {
      ReportWriter.Dispose();
      Stopwatch.Stop();
    }

    public void ExecutionBegun()
    {
      ++Iteration;
      ExecutionBegin = Stopwatch.ElapsedMilliseconds;
    }

    public void ExecutionEnded()
    {
      ExecutionEnd = Stopwatch.ElapsedMilliseconds;
      ReportWriter.WriteLine(ReportLine());
      ReportWriter.Flush();
    }

    public static string Header() =>
      $"{nameof(Iteration)};{nameof(ExecutionBegin)};{nameof(ExecutionEnd)}";

    private string ReportLine() =>
      $"{Iteration};{ExecutionBegin};{ExecutionEnd}";
  }

  private record Schedulation(string ExternalId, ScheduleOptions Options, TimeSpan WorkDuration)
  {
    public Guid Id { get; set; }
  }

  private static readonly List<Schedulation> Schedulations;
  private static readonly string ReportsDirectory = Path.Combine(Path.Combine(Environment.CurrentDirectory, "reports"));

  static TimingTester()
  {
    ClearReports();
    Schedulations = new()
    {
      new("TSK_A_FixedDelay", new ScheduleOptions{
        RelativeStartDelay = TimeSpan.FromSeconds(5),
        Interval = TimeSpan.FromSeconds(3),
        PeriodicType = PeriodicBehaviour.FixedDelay
      }, TimeSpan.FromMilliseconds(500)),
      new("TSK_A_FixedRate", new ScheduleOptions{
        RelativeStartDelay = TimeSpan.FromSeconds(5),
        Interval = TimeSpan.FromSeconds(3),
        PeriodicType = PeriodicBehaviour.FixedRate
      }, TimeSpan.FromMilliseconds(500)),
      new("TSK_B_FixedDelay", new ScheduleOptions{
        RelativeStartDelay = TimeSpan.FromSeconds(5),
        Interval = TimeSpan.FromSeconds(2),
        PeriodicType = PeriodicBehaviour.FixedDelay
      }, TimeSpan.FromMilliseconds(250)),
      new("TSK_B_FixedRate", new ScheduleOptions{
        RelativeStartDelay = TimeSpan.FromSeconds(5),
        Interval = TimeSpan.FromSeconds(2),
        PeriodicType = PeriodicBehaviour.FixedRate
      }, TimeSpan.FromMilliseconds(250))
    };
  }

  public static void Run(IServiceProvider serviceProvider)
  {
    var schedulerService = serviceProvider.GetRequiredService<ISchedulerService>();
    Schedulations.ForEach(schedulerService.Add);
    Schedulations.ForEach(schedulerService.Schedule);
  }

  public static void ClearReports()
  {
    var directory = Directory.CreateDirectory(ReportsDirectory);
    foreach (var file in directory.GetFiles())
    {
      file.Delete();
    }
    foreach (var dir in directory.GetDirectories())
    {
      dir.Delete(true);
    }
  }

  private static void Add(this ISchedulerService schedulerService, Schedulation schedulation)
  {
    var testReporter = new TimingTest(Path.Combine(ReportsDirectory, $"{schedulation.ExternalId}_report.csv"));
    schedulation.Id = schedulerService.Add(cancellationToken =>
    {
      testReporter.ExecutionBegun();
      var cancelled = cancellationToken.WaitHandle.WaitOne(schedulation.WorkDuration);
      if (cancelled)
      {
        Console.WriteLine($"Task '{schedulation.ExternalId}' cancelled");
      }
      testReporter.ExecutionEnded();
      return Task.CompletedTask;
    }, schedulation.ExternalId);
  }

  private static void Schedule(this ISchedulerService schedulerService, Schedulation schedulation) =>
    schedulerService.Schedule(schedulation.Id, schedulation.Options);
}