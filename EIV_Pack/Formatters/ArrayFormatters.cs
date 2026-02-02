namespace EIV_Pack.Formatters;

public sealed class ArrayFormatter<T> : IFormatter<T?[]>
{
    public void Serialize(ref PackWriter writer, scoped ref readonly T?[]? value)
    {
        writer.WriteArray(value);
    }

    public void Deserialize(ref PackReader reader, scoped ref T?[]? value)
    {
        reader.ReadArray(ref value);
    }
}

public sealed class ArraySegmentFormatter<T> : IFormatter<ArraySegment<T?>>
{
    public void Serialize(ref PackWriter writer, scoped ref readonly ArraySegment<T?> value)
    {
        writer.WriteSpan(value.AsSpan());
    }

    public void Deserialize(ref PackReader reader, scoped ref ArraySegment<T?> value)
    {
        T?[] array = reader.ReadArray<T>()!;
        value = (ArraySegment<T?>)array;
    }
}

public sealed class MemoryFormatter<T> : IFormatter<Memory<T?>>
{
    public void Serialize(ref PackWriter writer, scoped ref readonly Memory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    public void Deserialize(ref PackReader reader, scoped ref Memory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class ReadOnlyMemoryFormatter<T> : IFormatter<ReadOnlyMemory<T?>>
{
    public void Serialize(ref PackWriter writer, scoped ref readonly ReadOnlyMemory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    public void Deserialize(ref PackReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}