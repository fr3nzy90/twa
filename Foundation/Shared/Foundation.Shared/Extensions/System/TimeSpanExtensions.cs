namespace TodoWebApp.Foundation.Shared.Extensions.System;

public static class TimeSpanExtensions
{
  public static TimeSpan LimitLow(this TimeSpan obj, TimeSpan limit) =>
    obj <= limit ? limit : obj;

  public static TimeSpan LimitHigh(this TimeSpan obj, TimeSpan limit) =>
    obj >= limit ? limit : obj;
}