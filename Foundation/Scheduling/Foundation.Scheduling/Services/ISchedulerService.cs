using System.Linq.Expressions;
using TodoWebApp.Foundation.Scheduling.Models.DTOs;

namespace TodoWebApp.Foundation.Scheduling.Services;

public interface ISchedulerService
{
  static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromMilliseconds(500);
  static readonly TimeSpan InfiniteOperationTimeout = TimeSpan.FromMilliseconds(-1);

  Guid Add(Func<CancellationToken, Task> work, string externalId, TimeSpan? lockTimeout = default);
  bool Remove(Guid id, TimeSpan? lockTimeout = default);
  SchedulerWork? Get(Guid id, TimeSpan? lockTimeout = default);
  IEnumerable<SchedulerWork> GetAll(Expression<Func<SchedulerWork, bool>>? filter = default, TimeSpan? lockTimeout = default);
  bool Cancel(Guid id, TimeSpan? lockTimeout = default);
  bool ExecuteNow(Guid id, TimeSpan? lockTimeout = default);
  bool Schedule(Guid id, ScheduleOptions? options = default, TimeSpan? lockTimeout = default);
}