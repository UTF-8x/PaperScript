using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class ByteHexStringConverter : JsonConverter<byte>
{
    public override byte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hexString = reader.GetString()!;
        if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hexString = hexString[2..];

        return Convert.ToByte(hexString, 16);
    }

    public override void Write(Utf8JsonWriter writer, byte value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"0x{value:X8}");
    }
}