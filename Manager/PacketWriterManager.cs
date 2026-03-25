using System;
using System.Collections.Generic;
using System.Text;

public class PacketWriterManager
{
    private List<byte> buffer = new List<byte>();

    public byte[] ToArray()
    {
        return buffer.ToArray();
    }

    public int Length()
    {
        return buffer.Count;
    }

    public void WriteInt(int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void WriteBool(bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void WriteFloat(float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void WriteLong(long value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void WriteDouble(double value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void WriteString(string value)
    {
        if (value == null)
        {
            WriteInt(0);
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(value);

        WriteInt(bytes.Length);
        buffer.AddRange(bytes);
    }

    public void WriteBytes(byte[] data)
    {
        WriteInt(data.Length);
        buffer.AddRange(data);
    }

    public void WriteListCount(int count)
    {
        WriteInt(count);
    }
}