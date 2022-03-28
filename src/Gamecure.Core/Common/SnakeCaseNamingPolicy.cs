using System.Text.Json;

namespace Gamecure.Core.Common;

internal class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        Span<char> newName = stackalloc char[name.Length * 2];

        var count = 0;
        for (var i = 0; i < name.Length; ++i)
        {
            var c = name[i];
            if (i > 0 && (char.IsUpper(c) || char.IsNumber(c)))
            {
                newName[count++] = '_';
            }
            newName[count++] = char.ToLower(name[i]);
        }

        var result = new string(newName.Slice(0, count));
        return result;
    }
}