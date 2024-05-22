namespace TodoWebApp.Foundation.Scheduling.Impl.Shared;

// NOTE: This class is intended only to contain scheduled work (ie. is in state Scheduled/Rescheduled and has ScheduledExecution)
internal sealed class SchedulerWorkQueue
{
  public record Item
  {
    public required Guid Id { get; init; }
    public required DateTime ScheduledExecution { get; init; }
  }

  private static readonly IComparer<Item> ComparerByScheduledExecution = Comparer<Item>
    .Create((obj1, obj2) => DateTime.Compare(obj1.ScheduledExecution, obj2.ScheduledExecution));

  private readonly List<Item> _queue = new();

  public TimeSpan? NextScheduledIn => _queue.FirstOrDefault()?.ScheduledExecution - DateTime.UtcNow;
  public Item? NextScheduled => _queue.Find(obj => DateTime.UtcNow >= obj.ScheduledExecution);

  public void Add(Guid id, DateTime scheduledExecution)
  {
    var item = new Item
    {
      Id = id,
      ScheduledExecution = scheduledExecution
    };
    var idx = _queue.BinarySearch(item, ComparerByScheduledExecution);
    if (0 > idx)
    {
      idx = ~idx;
    }
    else
    {
      for (++idx; idx < _queue.Count && 0 == ComparerByScheduledExecution.Compare(_queue[idx], item); ++idx)
      {
      }
    }
    _queue.Insert(idx, item);
  }

  public void Remove(Guid id)
  {
    var idx = _queue.FindIndex(obj => obj.Id == id);
    if (0 > idx) return;
    _queue.RemoveAt(idx);
  }
}