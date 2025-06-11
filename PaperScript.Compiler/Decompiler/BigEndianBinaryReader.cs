using System.Text;

namespace PaperScript.Compiler.Decompiler;

public class BigEndianBinaryReader : IDisposable
{
    private readonly BinaryReader _reader;

    public BigEndianBinaryReader(Stream stream)
    {
        _reader = new BinaryReader(stream);
    }

    public UInt32 ReadUInt32()
    {
        byte[] bytes = _reader.ReadBytes(4);
        if(bytes.Length != 4) throw new EndOfStreamException();
        Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
    
    public ushort ReadUInt16()
    {
        byte[] bytes = _reader.ReadBytes(2);
        if (bytes.Length < 2) throw new EndOfStreamException();
        Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes, 0);
    }
    
    public float ReadSingle()
    {
        byte[] bytes = _reader.ReadBytes(4);
        if (bytes.Length < 4)
            throw new EndOfStreamException();

        Array.Reverse(bytes); // Convert BE → LE
        return BitConverter.ToSingle(bytes, 0);
    }

    public ushort ReadUInt16LE() => _reader.ReadUInt16();

    public ulong ReadUInt64()
    {
        byte[] bytes = _reader.ReadBytes(8);
        if (bytes.Length < 8) throw new EndOfStreamException();
        Array.Reverse(bytes);
        return BitConverter.ToUInt64(bytes, 0);
    }

    public byte ReadByte() => _reader.ReadByte();

    public string ReadNullTerminatedUtf16BEString()
    {
        using var ms = new MemoryStream();
        while (true)
        {
            byte b1 = _reader.ReadByte();
            byte b2 = _reader.ReadByte();
            if (b1 == 0 && b2 == 0)
                break;

            ms.WriteByte(b1);
            ms.WriteByte(b2);
        }

        return Encoding.BigEndianUnicode.GetString(ms.ToArray());
    }
    
    public string ReadNullTerminatedUtf8String()
    {
        var bytes = new List<byte>();
        byte b;
        while ((b = _reader.ReadByte()) != 0)
            bytes.Add(b);

        return Encoding.UTF8.GetString(bytes.ToArray());
    }
    
    public string ReadUtf8StringU8LengthWithTrailingNull()
    {
        byte length = _reader.ReadByte();                   // 1-byte length
        byte[] bytes = _reader.ReadBytes(length);           // UTF-8 string
        byte terminator = _reader.ReadByte();               // Null byte

        // if (terminator != 0)
        //     throw new InvalidDataException($"Expected null terminator (0x00) after string. (found {terminator:X})");

        return Encoding.UTF8.GetString(bytes);
    }

    public void Rewind(int bytes = 1)
    {
        _reader.BaseStream.Seek(-bytes, SeekOrigin.Current);
    }

    
    public void Dispose()
    {
        _reader.Dispose();
    }
}