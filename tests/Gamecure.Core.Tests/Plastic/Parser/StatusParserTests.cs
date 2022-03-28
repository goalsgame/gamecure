using Gamecure.Core.Plastic.Parser;
using NUnit.Framework;

namespace Gamecure.Core.Tests.Plastic.Parser;

internal class StatusParserTests
{
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Parse_EmptyOrNull_ReturnNull(string? value)
    {
        var result = StatusParser.Parse(value);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Parse_BranchDetails_ReturnChangeset()
    {
        var result = StatusParser.Parse("/main@Game@Goals@Cloud (head:10)");

        Assert.That(result!.Head, Is.EqualTo(10));
    }

    [TestCase("/main@Game@Goals@Cloud (head:1)", "/main")]
    [TestCase("/main/1/2@Game@Goals@Cloud (head:1)", "/main/1/2")]
    [TestCase("/main/ad-hoc-123@Game@Goals@Cloud (head:1)", "/main/ad-hoc-123")]
    public void Parse_BranchDetails_ReturnBranch(string value, string branch)
    {
        var result = StatusParser.Parse(value);

        Assert.That(result!.Branch, Is.EqualTo(branch));
    }

    [Test]
    public void Parse_ChangesetDetails_ReturnChangeset()
    {
        var result = StatusParser.Parse("cs:4@Game@Goals@Cloud (head:4)");

        Assert.That(result!.Head, Is.EqualTo(4));
    }

    [Test]
    public void Parse_ChangesetDetails_ReturnNullBranch()
    {
        var result = StatusParser.Parse("cs:1@Game@Goals@Cloud (head:1)");

        Assert.That(result!.Branch, Is.Null);
    }


    [TestCase("/main@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("/main/a/b@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("cs:31@Game@Goals@Cloud (head:200)")]
    public void Parse_AnyStatus_SetRepository(string value)
    {
        var result = StatusParser.Parse(value);

        Assert.That(result!.Repository, Is.EqualTo("Game"));
    }

    [TestCase("/main@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("/main/a/b@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("cs:31@Game@Goals@Cloud (head:200)")]
    public void Parse_AnyStatus_SetServer(string value)
    {
        var result = StatusParser.Parse(value);

        Assert.That(result!.Server, Is.EqualTo("Goals"));
    }

    [TestCase("/main@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("/main/a/b@Game@Goals@Cloud (cs:-1 - head:800)")]
    [TestCase("cs:31@Game@Goals@Cloud (head:200)")]
    [Test]
    public void Parse_AnyStatus_SetServerType(string value)
    {
        var result = StatusParser.Parse(value);

        Assert.That(result!.ServerType, Is.EqualTo("Cloud"));
    }
}