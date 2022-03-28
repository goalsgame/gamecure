namespace Gamecure.Core.Common;

public class Querystring
{
    private readonly List<(string Key, string Value)> _values = new();
    public Querystring Param(string name, string value)
    {
        _values.Add((name, value));
        return this;
    }

    public Querystring Param(string name, object value) => Param(name, value?.ToString() ?? string.Empty);
    public override string ToString() => string.Join('&', _values.Select(tuple => $"{tuple.Key}={Uri.EscapeDataString(tuple.Value)}"));
}