using Microsoft.Extensions.Logging;

namespace TodoWebApp.Foundation.Shared.Models.Logging;

public record ExecuteBlockOptions
{
  public Type[]? TypeParameters { get; init; } = default;
  public FunctionParameter[]? Parameters { get; init; } = default;
  public LogLevel EnterLogLevel { get; init; } = LogLevel.Debug;
  public LogLevel ErrorLogLevel { get; init; } = LogLevel.Error;
  public LogLevel ExitLogLevel { get; init; } = LogLevel.Debug;
}