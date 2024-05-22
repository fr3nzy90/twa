using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoWebApp.Foundation.Scheduling.Impl.Extensions;

namespace TodoWebApp.Foundation.Scheduling.Setup.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddFoundationScheduling(this IServiceCollection services, IConfiguration configuration) =>
    services.AddFoundationSchedulingImpl(configuration);
}