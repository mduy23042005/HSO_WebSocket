using System;
using System.Buffers;
using System.Text;

public class PacketWriterManager
{
    private const int DefaultCapacity = 4096;

    private byte[] buffer;
    private int length;
    private bool returnedToPool;

    public PacketWriterManager()
    {
        buffer = ArrayPool<byte>.Shared.Rent(DefaultCapacity);
        length = 0;
        returnedToPool = false;
    }

    public byte[] ToArray()
    {
        EnsureNotReturned();

        byte[] result = new byte[length];

        Buffer.BlockCopy(buffer, 0, result, 0, length);

        ArrayPool<byte>.Shared.Return(buffer);

        buffer = null;
        returnedToPool = true;

        return result;
    }

    public int Length()
    {
        return length;
    }

    public void WriteInt(int value)
    {
        EnsureCapacity(4);

        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(bytes, 0, buffer, length, 4);

        length += 4;
    }

    public void WriteBool(bool value)
    {
        EnsureCapacity(1);

        buffer[length] = value ? (byte)1 : (byte)0;
        length += 1;
    }

    public void WriteFloat(float value)
    {
        EnsureCapacity(4);

        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(bytes, 0, buffer, length, 4);

        length += 4;
    }

    public void WriteLong(long value)
    {
        EnsureCapacity(8);

        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(bytes, 0, buffer, length, 8);

        length += 8;
    }

    public void WriteDouble(double value)
    {
        EnsureCapacity(8);

        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(
            bytes,
            0,
            buffer,
            length,
            8
        );

        length += 8;
    }

    public void WriteString(string value)
    {
        if (value == null)
        {
            WriteInt(0);
            return;
        }

        if (value.Length == 0)
        {
            WriteInt(0);
            return;
        }

        int byteCount = Encoding.UTF8.GetByteCount(value);

        EnsureCapacity(4 + byteCount);

        WriteInt(byteCount);

        Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, length);

        length += byteCount;
    }

    public void WriteBytes(byte[] data)
    {
        if (data == null)
        {
            WriteInt(0);
            return;
        }

        EnsureCapacity(4 + data.Length);

        WriteInt(data.Length);

        Buffer.BlockCopy(data, 0, buffer, length, data.Length);

        length += data.Length;
    }

    public void WriteListCount(int count)
    {
        WriteInt(count);
    }

    private void EnsureCapacity(int additionalBytes)
    {
        EnsureNotReturned();

        int requiredLength = length + additionalBytes;

        if (requiredLength <= buffer.Length)
            return;

        int newCapacity = buffer.Length * 2;

        if (newCapacity < requiredLength)
            newCapacity = requiredLength;

        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

        Buffer.BlockCopy(buffer, 0, newBuffer, 0, length);

        ArrayPool<byte>.Shared.Return(buffer);

        buffer = newBuffer;
    }

    private void EnsureNotReturned()
    {
        if (returnedToPool)
            throw new ObjectDisposedException(nameof(PacketWriterManager));
    }
}