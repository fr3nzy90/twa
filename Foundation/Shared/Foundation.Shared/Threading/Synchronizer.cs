namespace TodoWebApp.Foundation.Shared.Threading;

public class Synchronizer
{
  protected readonly object _obj = new();

  public void Enter() =>
    Monitor.Enter(_obj);

  public void Exit() =>
    Monitor.Exit(_obj);

  public bool IsEntered() =>
    Monitor.IsEntered(_obj);

  public void Pulse() =>
    Monitor.Pulse(_obj);

  public void PulseAll() =>
    Monitor.PulseAll(_obj);

  public bool TryEnter() =>
    Monitor.TryEnter(_obj);

  public bool TryEnter(TimeSpan timeout) =>
    Monitor.TryEnter(_obj, timeout);

  public bool Wait() =>
    Monitor.Wait(_obj);

  public bool Wait(TimeSpan timeout) =>
    Monitor.Wait(_obj, timeout);
}