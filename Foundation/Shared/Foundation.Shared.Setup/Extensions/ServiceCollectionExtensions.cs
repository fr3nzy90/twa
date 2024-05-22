using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace TodoWebApp.Foundation.Shared.Setup.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddFoundationShared(this IServiceCollection services, IConfiguration configuration) =>
    services
      .AddLogging(loggingBuilder =>
        loggingBuilder
          .ClearProviders()
          .SetMinimumLevel(LogLevel.Trace)
          .AddNLog());
}