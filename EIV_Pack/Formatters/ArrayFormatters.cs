namespace EIV_Pack.Formatters;

public sealed class ArrayFormatter<T> : BaseFormatter<T?[]>
{
    public override void Serialize(ref PackWriter writer, scoped ref readonly T?[]? value)
    {
        writer.WriteArray(value);
    }

    public override void Deserialize(ref PackReader reader, scoped ref T?[]? value)
    {
        reader.ReadArray(ref value);
    }
}

public sealed class ArraySegmentFormatter<T> : BaseFormatter<ArraySegment<T?>>
{
    public override void Serialize(ref PackWriter writer, scoped ref readonly ArraySegment<T?> value)
    {
        writer.WriteSpan(value.AsSpan());
    }

    public override void Deserialize(ref PackReader reader, scoped ref ArraySegment<T?> value)
    {
        T?[] array = reader.ReadArray<T>()!;
#if NET8_0_OR_GREATER
        value = (ArraySegment<T?>)array;
#else
        value = new ArraySegment<T?>(array);
#endif
    }
}

public sealed class MemoryFormatter<T> : BaseFormatter<Memory<T?>>
{
    public override void Serialize(ref PackWriter writer, scoped ref readonly Memory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    public override void Deserialize(ref PackReader reader, scoped ref Memory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class ReadOnlyMemoryFormatter<T> : BaseFormatter<ReadOnlyMemory<T?>>
{
    public override void Serialize(ref PackWriter writer, scoped ref readonly ReadOnlyMemory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    public override void Deserialize(ref PackReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}