using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class HexStringConverter : JsonConverter<UInt32>
{
    public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hexString = reader.GetString()!;
        if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hexString = hexString[2..];

        return Convert.ToUInt32(hexString, 16);
    }

    public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"0x{value:X8}");
    }
}