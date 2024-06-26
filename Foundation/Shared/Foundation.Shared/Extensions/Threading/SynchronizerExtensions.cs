﻿namespace TodoWebApp.Foundation.Shared.Extensions.Threading;

public static class SynchronizerExtensions
{
  public static bool TryEnter(this Shared.Threading.Synchronizer obj, TimeSpan? timeout) =>
    timeout.HasValue ? obj.TryEnter(timeout.Value) : obj.TryEnter();

  public static bool Wait(this Shared.Threading.Synchronizer obj, TimeSpan? timeout) =>
    timeout.HasValue ? obj.Wait(timeout.Value) : obj.Wait();

  public static void Execute(this Shared.Threading.Synchronizer obj, Action action) =>
    obj.Execute(() => { action(); return true; });

  public static T Execute<T>(this Shared.Threading.Synchronizer obj, Func<T> action) =>
    obj.Execute(obj.Enter, action);

  public static void TryExecute(this Shared.Threading.Synchronizer obj, Action action, TimeSpan? timeout = default) =>
    obj.TryExecute(() => { action(); return true; }, timeout);

  public static T TryExecute<T>(this Shared.Threading.Synchronizer obj, Func<T> action, TimeSpan? timeout = default) =>
    obj.Execute(() =>
    {
      var acquired = obj.TryEnter(timeout);
      if (!acquired)
      {
        var text = timeout?.TotalMilliseconds.ToString() ?? "infinite";
        throw new TimeoutException($"Lock acquisition timeout with {text} ms");
      }
    }, action);

  private static T Execute<T>(this Shared.Threading.Synchronizer obj, Action enterAction, Func<T> action)
  {
    enterAction();
    try
    {
      var result = action();
      return result;
    }
    finally
    {
      obj.Exit();
    }
  }
}