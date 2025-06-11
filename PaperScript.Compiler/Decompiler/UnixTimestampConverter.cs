using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class UnixTimestampConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Optional: support reading ISO 8601 back into ulong
        string str = reader.GetString()!;
        var dateTime = DateTimeOffset.Parse(str).ToUniversalTime();
        return (ulong)dateTime.ToUnixTimeSeconds();
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime;
        writer.WriteStringValue(dateTime.ToString("o")); // ISO 8601
    }
}