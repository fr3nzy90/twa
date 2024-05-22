using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoWebApp.Foundation.Scheduling.Impl.Services;

namespace TodoWebApp.Foundation.Scheduling.Impl.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddFoundationSchedulingImpl(this IServiceCollection services, IConfiguration configuration) =>
    services
      .AddSingleton<SchedulerService>()
      .AddSingleton<Scheduling.Services.ISchedulerService>(serviceProvider => serviceProvider.GetRequiredService<SchedulerService>())
      .AddHostedService(serviceProvider => serviceProvider.GetRequiredService<SchedulerService>());
}