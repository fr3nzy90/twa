using TodoWebApp.Foundation.Scheduling.Impl.Extensions;

namespace TodoWebApp.Foundation.Scheduling.Setup.Extensions;

public static class ServiceProviderExtensions
{
  public static IServiceProvider SetupFoundationScheduling(this IServiceProvider serviceProvider) =>
    serviceProvider.SetupFoundationSchedulingImpl();
}