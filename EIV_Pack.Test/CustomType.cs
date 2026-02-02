using EIV_Pack.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EIV_Pack.Test;

internal class CustomType : IPackable<CustomType>, IFormatter<CustomType>
{
    public string? Value;

    public static void RegisterFormatter()
    {
        if (!FormatterProvider.IsRegistered<CustomType>())
        {
            FormatterProvider.Register(new CustomType());
        }

        if (!FormatterProvider.IsRegistered<List<CustomType>>())
        {
            FormatterProvider.Register(new ListFormatter<CustomType>());
        }
    }

    public static void DeserializePackable(ref PackReader reader, scoped ref CustomType? value)
    {
        if (!reader.TryReadSmallHeader(out byte len) || len != 1)
        {
            value = null;
            return;
        }

        value = new()
        {
            Value = reader.ReadString(),
        };
    }

    public static void SerializePackable(ref PackWriter writer, scoped ref readonly CustomType? value)
    {
        if (value == null)
        {
            writer.WriteSmallHeader();
            return;
        }

        writer.WriteSmallHeader(1);
        writer.WriteString(value.Value);
    }

    public void Deserialize(ref PackReader reader, scoped ref CustomType? value)
    {
        DeserializePackable(ref reader, ref value);
    }

    public void Serialize(ref PackWriter writer, scoped ref readonly CustomType? value)
    {
        SerializePackable(ref writer, in value);
    }
}