using System.Text;

namespace Gamecure.Core.Plastic.Parser;

public enum PlasticChangeType
{
    Added,
    Deleted,
    Changed,
    Moved,
    Unknown,
}

public record PlasticChanges(string Filename, PlasticChangeType Type);
public record PlasticChangeset(int Id, string Branch, string Owner, DateTime Date, Guid Guid, string Comment, PlasticChanges[] Items);
internal class LogParser
{
    private const string ChangesetId = "ChangesetId:";
    private const string Owner = "Owner:";
    private const string Branch = "Branch:";
    private const string Date = "Date:";
    private const string GuidSeparator = "Guid:";
    private const string CommentBegin = "CommentBegin:";
    private const string CommentEnd = "CommentEnd:";
    private const string ItemsBegin = "ItemsBegin:";
    private const string ItemsEnd = "ItemsEnd:";

    public static readonly string LogFormat = @$"{ChangesetId}{{changesetid}}{{newline}}{Owner}{{owner}}{{newline}}{Branch}{{branch}}{{newline}}{Date}{{date}}{{newline}}{GuidSeparator}{{guid}}{{newline}}{CommentBegin}{{newline}}{{comment}}{{newline}}{CommentEnd}{{newline}}{ItemsBegin}{{newline}}{{items}}{ItemsEnd}";
    public const string ItemsFormat = "{shortstatus}{path}{newline}";
    public static IEnumerable<PlasticChangeset> Parse(string[] lines)
    {
        var i = 0;
        while (i < lines.Length)
        {
            yield return ParseChangeset(lines, ref i);
        }
    }

    private static PlasticChangeset ParseChangeset(string[] lines, ref int i)
    {
        var changesetId = ParseChangesetId(lines[i++]);
        var owner = ParseOwner(lines[i++]);
        var branch = ParseBranch(lines[i++]);
        var date = ParseDate(lines[i++]);
        var guid = ParseGuid(lines[i++]);
        var comment = ParseComment(lines, ref i);
        var items = ParseItems(lines, ref i);

        return new PlasticChangeset(changesetId, branch, owner, date, guid, comment, items);
    }

    private static int ParseChangesetId(ReadOnlySpan<char> line)
    {
        if (line.StartsWith(ChangesetId) && int.TryParse(line[ChangesetId.Length..], out var changesetId))
        {
            return changesetId;
        }
        throw new ParserException("Failed to parse changeset ID", new string(line));
    }

    private static string ParseOwner(ReadOnlySpan<char> line)
    {
        if (line.StartsWith(Owner))
        {
            return new string(line[Owner.Length..]);
        }
        throw new ParserException("Failed to parse Owner", new string(line));
    }

    private static string ParseBranch(ReadOnlySpan<char> line)
    {
        if (line.StartsWith(Branch))
        {
            return new string(line[Branch.Length..]);
        }
        throw new ParserException("Failed to parse Branch", new string(line));
    }

    private static DateTime ParseDate(ReadOnlySpan<char> line)
    {
        if (line.StartsWith(Date) && DateTime.TryParse(line[Date.Length..], out var date))
        {
            return date;
        }
        throw new ParserException("Failed to parse Date", new string(line));
    }

    private static Guid ParseGuid(ReadOnlySpan<char> line)
    {
        if (line.StartsWith(GuidSeparator) && Guid.TryParse(line[GuidSeparator.Length..], out var guid))
        {
            return guid;
        }
        throw new ParserException("Failed to parse GUID", new string(line));
    }

    private static string ParseComment(string[] lines, ref int index)
    {
        if (lines[index] != CommentBegin)
        {
            throw new ParserException($"Expected {CommentBegin}", lines[index]);
        }

        var comment = new StringBuilder();
        string line;
        // NOTE(Jens): should we handle cases where we run out of bounds before finding the end? Should not happen unless plastic cm log returns an invalid result.
        while ((line = lines[++index]) != CommentEnd)
        {
            comment.AppendLine(line);
        }
        index++;
        return comment.ToString();
    }

    private static PlasticChanges[] ParseItems(string[] lines, ref int index)
    {
        List<PlasticChanges> changes = new();
        if (lines[index] != ItemsBegin)
        {
            throw new ParserException($"Expected {ItemsBegin}", lines[index]);
        }

        string line;
        while ((line = lines[++index]) != ItemsEnd)
        {
            changes.Add(ParseChange(line));
        }

        index++;
        return changes.ToArray();

        static PlasticChanges ParseChange(string line)
        {
            if (line.Length < 2)
            {
                throw new ParserException("The items line was not in correct format. Expected format '{ADMC}{FileName}'", line);
            }
            var type = line[0] switch
            {
                'A' => PlasticChangeType.Added,
                'D' => PlasticChangeType.Deleted,
                'C' => PlasticChangeType.Changed,
                'M' => PlasticChangeType.Moved,
                _ => PlasticChangeType.Unknown
            };
            return new PlasticChanges(line[1..], type);
        }
    }
}