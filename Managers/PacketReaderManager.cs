using System;
using System.Text;

public class PacketReaderManager
{
    private readonly byte[] buffer;
    private int offset;

    public PacketReaderManager(byte[] data)
    {
        buffer = data ?? throw new ArgumentNullException(nameof(data));
        offset = 0;
    }

    public int ReadInt()
    {
        EnsureAvailable(4);

        int value = BitConverter.ToInt32(buffer, offset);
        offset += 4;

        return value;
    }

    public float ReadFloat()
    {
        EnsureAvailable(4);

        float value = BitConverter.ToSingle(buffer, offset);
        offset += 4;

        return value;
    }

    public long ReadLong()
    {
        EnsureAvailable(8);

        long value = BitConverter.ToInt64(buffer, offset);
        offset += 8;

        return value;
    }

    public double ReadDouble()
    {
        EnsureAvailable(8);

        double value = BitConverter.ToDouble(buffer, offset);
        offset += 8;

        return value;
    }

    public bool ReadBool()
    {
        EnsureAvailable(1);

        bool value = BitConverter.ToBoolean(buffer, offset);
        offset += 1;

        return value;
    }

    public string ReadString()
    {
        int length = ReadInt();

        if (length < 0)
            throw new InvalidOperationException("Invalid string length.");

        if (length == 0)
            return string.Empty;

        EnsureAvailable(length);

        string value = Encoding.UTF8.GetString(buffer, offset, length);
        offset += length;

        return value;
    }

    private void EnsureAvailable(int count)
    {
        if (count < 0 || offset > buffer.Length - count)
            throw new InvalidOperationException($"Packet buffer overflow. Offset={offset}, Required={count}, Length={buffer.Length}");
    }
}