using System;
using System.Linq;
using Gamecure.Core.Plastic.Parser;
using NUnit.Framework;

namespace Gamecure.Core.Tests.Plastic.Parser;

// NOTE(Jens): This only tests happy case where the output from plastic is well formatted. It should/will crash on malformed input, which I think is what we want.
internal class LogParserTests
{
    private static readonly Guid Guid = Guid.NewGuid();
    private static readonly string[] Input =
    {
        "ChangesetId:2",
        "Owner:a@b.c",
        "Branch:/main",
        "Date:2022-01-02 03:04:05",
        $"Guid:{Guid}",
        "CommentBegin:",
        "first line",
        "second line",
        "CommentEnd:",
        "ItemsBegin:",
        "Afile1",
        "Cfile2",
        "Dfile3",
        "Mfile4",
        "ItemsEnd:"
    };

    [Test]
    public void Parse_Always_SetChangesetId()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Id, Is.EqualTo(2));
    }

    [Test]
    public void Parse_Always_SetDate()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Date, Is.EqualTo(new DateTime(2022, 01, 02, 03, 04, 05)));
    }

    [Test]
    public void Parse_Always_SetGuid()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Guid, Is.EqualTo(Guid));
    }

    [Test]
    public void Parse_Always_SetBranch()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Branch, Is.EqualTo("/main"));
    }

    [Test]
    public void Parse_Always_SetOwner()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Owner, Is.EqualTo("a@b.c"));
    }

    [Test]
    public void Parse_Always_SetComment()
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Comment, Is.EqualTo($"first line{Environment.NewLine}second line{Environment.NewLine}"));
    }

    [TestCase(0, PlasticChangeType.Added, "file1")]
    [TestCase(1, PlasticChangeType.Changed, "file2")]
    [TestCase(2, PlasticChangeType.Deleted, "file3")]
    [TestCase(3, PlasticChangeType.Moved, "file4")]
    public void Parse_Always_SetItems(int index, PlasticChangeType type, string filename)
    {
        var result = LogParser.Parse(Input).Single();

        Assert.That(result.Items[index].Type, Is.EqualTo(type));
        Assert.That(result.Items[index].Filename, Is.EqualTo(filename));
    }
}
