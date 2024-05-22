using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoWebApp.Foundation.Shared.Converters;

public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
  private readonly DateTimeKind _dateTimeKind;

  public DateTimeJsonConverter(DateTimeKind dateTimeKind = DateTimeKind.Utc)
  {
    _dateTimeKind = dateTimeKind;
  }

  public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var dateTimeString = reader.GetString() ?? throw new ApplicationException($"Cannot read {nameof(DateTime)}");
    var result = DateTime.Parse(dateTimeString);
    result = DateTime.SpecifyKind(result, _dateTimeKind);
    return result;
  }

  public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
  {
    var temp = _dateTimeKind switch
    {
      DateTimeKind.Utc => value.ToUniversalTime(),
      DateTimeKind.Local => value.ToLocalTime(),
      _ => value
    };
    writer.WriteStringValue(temp.ToString("o", CultureInfo.InvariantCulture));
  }
}