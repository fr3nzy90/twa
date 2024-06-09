using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TodoWebApp.Foundation.Scheduling.Impl.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddFoundationSchedulingImpl(this IServiceCollection services, IConfiguration configuration) =>
    services
      .AddSingleton<Services.SchedulerService>()
      .AddSingleton<Scheduling.Services.ISchedulerService>(serviceProvider =>
        serviceProvider.GetRequiredService<Services.SchedulerService>())
      .AddHostedService(serviceProvider => serviceProvider.GetRequiredService<Services.SchedulerService>());
}