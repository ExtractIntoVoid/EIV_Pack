namespace EIV_Pack.Formatters;

public sealed class StringFormatter : BaseFormatter<string>
{
    public override void Deserialize(ref PackReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }

    public override void Serialize(ref PackWriter writer, scoped ref readonly string? value)
    {
        writer.WriteString(value);
    }
}
