namespace EIV_Pack.Formatters;

/// <summary>
/// A <see langword="string"/> formatter.
/// </summary>
public sealed class StringFormatter : BaseFormatter<string>
{
    /// <inheritdoc />
    public override void Deserialize(ref PackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }

    /// <inheritdoc />
    public override void Serialize(ref PackWriter writer, scoped ref readonly string? value)
    {
        writer.WriteString(value);
    }
}
