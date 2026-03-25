using System;
using System.Text;

public class PacketReaderManager
{
    private byte[] buffer;
    private int offset;

    public PacketReaderManager(byte[] data)
    {
        buffer = data;
        offset = 0;
    }

    public int ReadInt()
    {
        int value = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        return value;
    }

    public float ReadFloat()
    {
        float value = BitConverter.ToSingle(buffer, offset);
        offset += 4;
        return value;
    }

    public long ReadLong()
    {
        long value = BitConverter.ToInt64(buffer, offset);
        offset += 8;
        return value;
    }

    public double ReadDouble()
    {
        double value = BitConverter.ToDouble(buffer, offset);
        offset += 8;
        return value;
    }

    public bool ReadBool()
    {
        bool value = BitConverter.ToBoolean(buffer, offset);
        offset += 1;
        return value;
    }

    public string ReadString()
    {
        int length = ReadInt();

        string value = Encoding.UTF8.GetString(buffer, offset, length);

        offset += length;

        return value;
    }
}