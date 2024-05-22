using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoWebApp.Foundation.Shared.Converters;

public sealed class IPAddressJsonConverter : JsonConverter<IPAddress>
{
  public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    IPAddress.TryParse(reader.GetString(), out var result);
    return result;
  }

  public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) =>
    writer.WriteStringValue(value.ToString());
}