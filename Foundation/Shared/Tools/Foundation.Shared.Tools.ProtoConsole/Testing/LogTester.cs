using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoWebApp.Foundation.Shared.Extensions.Logging;

namespace Foundation.Shared.Tools.ProtoConsole.Testing;

internal class LogTester
{
  private readonly ILogger _logger;

  public static void Run(IServiceProvider serviceProvider)
  {
    var logger = serviceProvider.GetRequiredService<ILogger<LogTester>>();

    var tester = new LogTester(logger);

    var i = 1;

    Wrap(() => tester.Test(2 + i, $"test{i++}"));
    Wrap(() => tester.Test<int, ILogger>(2 + i, $"test{i++}"));
    Wrap(() => tester.TestThrow<int, ILogger>(2 + i, $"test{i++}"));
    Wrap(() => tester.TestNested<int, ILogger>(2 + i, $"test{i++}"));
    Wrap(() => tester.TestAsync<int, ILogger>(2 + i, $"test{i++}"));
    Wrap(() => tester.TestThrowAsync<int, ILogger>(2 + i, $"test{i++}"));
  }

  public static void Wrap(Action action)
  {
    try
    {
      action.Invoke();
    }
    catch (Exception)
    {
    }
  }

  public static void Wrap(Func<Task> action)
  {
    try
    {
      action.Invoke().Wait();
    }
    catch (Exception)
    {
    }
  }

  private LogTester(ILogger logger)
  {
    _logger = logger;
  }

  public void Test(int a, string b, TimeSpan? c = default) =>
    _logger.ExecuteWithLogging(() => { }, new()
    {
      EnterLogLevel = LogLevel.Information,
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });

  public void Test<T1, T2>(int a, string b, TimeSpan? c = default) =>
    _logger.ExecuteWithLogging(() => { }, new()
    {
      TypeParameters = [typeof(T1), typeof(T2)],
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });

  public void TestThrow<T1, T2>(int a, string b, TimeSpan? c = default) =>
    _logger.ExecuteWithLogging(() => throw new ApplicationException("Ooops something went wrong"), new()
    {
      TypeParameters = [typeof(T1), typeof(T2)],
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });

  public void TestNested<T1, T2>(int a, string b, TimeSpan? c = default) =>
    _logger.ExecuteWithLogging(() =>
    {
      Test(a, b);
      Test<T1, T2>(a, b);
      TestThrow<T1, T2>(a, b);
    }, new()
    {
      TypeParameters = [typeof(T1), typeof(T2)],
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });

  public async Task TestAsync<T1, T2>(int a, string b, TimeSpan? c = default) =>
    await _logger.ExecuteWithLoggingAsync(() => Task.CompletedTask, new()
    {
      TypeParameters = [typeof(T1), typeof(T2)],
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });

  public async Task TestThrowAsync<T1, T2>(int a, string b, TimeSpan? c = default) =>
    await _logger.ExecuteWithLoggingAsync(() => throw new ApplicationException("Ooops something went wrong"), new()
    {
      TypeParameters = [typeof(T1), typeof(T2)],
      Parameters = [
        new(nameof(a), a),
        new(nameof(b), b),
        new(nameof(c), c)
      ]
    });
}