namespace Gamecure.Core.Plastic.Parser;

public record PlasticStatus(int Head, string? Branch, string Server, string ServerType, string Repository);

internal static class StatusParser
{
    public static PlasticStatus? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Only 2 known outputs at the moment are "cs:XX..." and "/branch...."
        if (!value.StartsWith('/') && !value.StartsWith("cs"))
        {
            throw new ParserException("Malformed input or unrecognized state of the repo.", value);
        }

        var branch = ParseBranch(value);
        var (repository, server, type) = ParseServerInfo(value);
        var changeset = ParseChangeset(value);

        return new PlasticStatus(changeset, branch, server, type, repository);
    }

    private static int ParseChangeset(string value)
    {
        const string Head = "head:";
        var headIndex = value.IndexOf(Head, StringComparison.Ordinal);
        if (headIndex == -1)
        {
            throw new ParserException($"Malformed input, can't find the '{Head}'", value);
        }

        var start = headIndex + Head.Length;
        var count = 0;
        while (char.IsNumber(value[start+count]))
        {
            count++;
        }

        if (count == 0)
        {
            throw new ParserException($"No numbers found after the {Head} tag.", value);
        }
        return int.Parse(value.AsSpan(start, count));
    }

    private static (string Repository, string Server, string Type) ParseServerInfo(string value)
    {
        var start = value.IndexOf('@');
        var end = value.IndexOf(' ', start);

        // Create a substring from the part where the server, repo and type are, excluding the starting @ and trailing whitespace.
        var values = value.Substring(start + 1, end - start - 1).Split('@');
        if (values.Length != 3)
        {
            throw new ParserException("Malformed repository/server/type input. Expected @Repo@Server@Type", value);
        }
        return (values[0], values[1], values[2]);
    }

    private static string? ParseBranch(string value)
    {
        if (!value.StartsWith('/'))
        {
            return null;
        }

        var endOfBranch = value.IndexOf('@');
        if (endOfBranch == -1)
        {
            throw new ParserException("Could not locate the end of the branch name. Probably malformed or unexepected input.", value);
        }

        return value[..endOfBranch];
    }
}