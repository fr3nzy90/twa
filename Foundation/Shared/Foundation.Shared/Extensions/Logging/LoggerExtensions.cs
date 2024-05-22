using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using TodoWebApp.Foundation.Shared.Models.Logging;

namespace TodoWebApp.Foundation.Shared.Extensions.Logging;

public static class LoggerExtensions
{
  private record Messages
  {
    public string? MethodMessage { get; private set; }
    public string? TypeParametersMessage { get; private set; }
    public string? ParametersMessage { get; private set; }

    public void ConditionallyUpdateMethodMessage(string? value)
    {
      if (null == value) return;
      MethodMessage ??= value;
    }

    public void ConditionallyUpdateTypeParametersMessage(string? value)
    {
      if (null == value) return;
      TypeParametersMessage ??= value;
    }

    public void ConditionallyUpdateParametersMessage(string? value)
    {
      if (null == value) return;
      ParametersMessage ??= value;
    }

    public static string GenerateMethodMessage(bool isAsync) =>
      isAsync ? "async method" : "method";

    public static string GenerateTypeParametersMessage(Type[]? parameters) =>
      null == parameters ? "" : $"<{parameters
        .Select(obj => obj.Name)
        .Aggregate((obj1, obj2) => $"{obj1},{obj2}")}>";

    public static string GenerateParametersMessage(FunctionParameter[]? parameters) =>
      null == parameters ? "" : $"({parameters
        .Select(obj => $"{obj.Name}={obj.Value}")
        .Aggregate((obj1, obj2) => $"{obj1},{obj2}")})";
  }

  private static readonly ExecuteBlockOptions DefaultOptions = new();

  public static void ConditionallyLog(this ILogger logger, LogLevel logLevel, string? message, Func<object?[]>? argsGetter)
  {
    if (!logger.IsEnabled(logLevel)) return;
    var args = argsGetter?.Invoke() ?? [];
    logger.Log(logLevel, message, args);
  }

  public static void ConditionallyLog(this ILogger logger,
    LogLevel logLevel,
    Exception? exception,
    string? message,
    Func<object?[]>? argsGetter)
  {
    if (!logger.IsEnabled(logLevel)) return;
    var args = argsGetter?.Invoke() ?? [];
    logger.Log(logLevel, exception, message, args);
  }

  public static void ExecuteWithLogging(this ILogger logger,
    Action action,
    ExecuteBlockOptions? options = default,
    [CallerMemberName] string caller = "") =>
    Execute(logger, () => { action.Invoke(); return true; }, caller, options);

  public static T ExecuteWithLogging<T>(this ILogger logger,
    Func<T> action,
    ExecuteBlockOptions? options = default,
    [CallerMemberName] string caller = "") =>
    Execute(logger, action, caller, options);

  public static async Task ExecuteWithLoggingAsync(this ILogger logger,
    Func<Task> action,
    ExecuteBlockOptions? options = default,
    [CallerMemberName] string caller = "") =>
    await ExecuteAsync(logger, async () => { await action.Invoke(); return true; }, caller, options);

  public static async Task<T> ExecuteWithLoggingAsync<T>(this ILogger logger,
    Func<Task<T>> action,
    ExecuteBlockOptions? options = default,
    [CallerMemberName] string caller = "") =>
    await ExecuteAsync(logger, action, caller, options);

  private static T Execute<T>(ILogger logger, Func<T> action, string caller, ExecuteBlockOptions? options = default)
  {
    options ??= DefaultOptions;
    var messages = new Messages();
    try
    {
      LogMethodEnter(logger, false, caller, options, ref messages);
      return action.Invoke();
    }
    catch (Exception e)
    {
      LogMethodError(logger, false, caller, e, options, ref messages);
      throw;
    }
    finally
    {
      LogMethodExit(logger, false, caller, options, ref messages);
    }
  }

  private static async Task<T> ExecuteAsync<T>(ILogger logger, Func<Task<T>> action, string caller, ExecuteBlockOptions? options = default)
  {
    options ??= DefaultOptions;
    var messages = new Messages();
    try
    {
      LogMethodEnter(logger, true, caller, options, ref messages);
      return await action.Invoke();
    }
    catch (Exception e)
    {
      LogMethodError(logger, true, caller, e, options, ref messages);
      throw;
    }
    finally
    {
      LogMethodExit(logger, true, caller, options, ref messages);
    }
  }

  private static void LogMethodEnter(ILogger logger, bool isAsync, string caller, ExecuteBlockOptions options, ref Messages messages)
  {
    var methodMessage = messages.MethodMessage;
    var typeParametersMessage = messages.TypeParametersMessage;
    var parametersMessage = messages.ParametersMessage;
    logger.ConditionallyLog(options.EnterLogLevel, "Entering {0} {1}{2}{3}", () =>
    {
      methodMessage ??= Messages.GenerateMethodMessage(isAsync);
      typeParametersMessage ??= Messages.GenerateTypeParametersMessage(options.TypeParameters);
      parametersMessage ??= Messages.GenerateParametersMessage(options.Parameters);
      return [methodMessage, caller, typeParametersMessage, parametersMessage];
    });
    messages.ConditionallyUpdateMethodMessage(methodMessage);
    messages.ConditionallyUpdateTypeParametersMessage(typeParametersMessage);
    messages.ConditionallyUpdateParametersMessage(parametersMessage);
  }

  private static void LogMethodError(ILogger logger,
    bool isAsync,
    string caller,
    Exception exception,
    ExecuteBlockOptions options,
    ref Messages messages)
  {
    var methodMessage = messages.MethodMessage;
    logger.ConditionallyLog(options.ErrorLogLevel, exception, "Error while executing {0} {1}", () =>
    {
      methodMessage ??= Messages.GenerateMethodMessage(isAsync);
      return [methodMessage, caller];
    });
    messages.ConditionallyUpdateMethodMessage(methodMessage);
  }

  private static void LogMethodExit(ILogger logger, bool isAsync, string caller, ExecuteBlockOptions options, ref Messages messages)
  {
    var methodMessage = messages.MethodMessage;
    var typeParametersMessage = messages.TypeParametersMessage;
    logger.ConditionallyLog(options.ExitLogLevel, "Finished {0} {1}{2}", () =>
    {
      methodMessage ??= Messages.GenerateMethodMessage(isAsync);
      typeParametersMessage ??= Messages.GenerateTypeParametersMessage(options.TypeParameters);
      return [methodMessage, caller, typeParametersMessage];
    });
    messages.ConditionallyUpdateMethodMessage(methodMessage);
    messages.ConditionallyUpdateTypeParametersMessage(typeParametersMessage);
  }
}