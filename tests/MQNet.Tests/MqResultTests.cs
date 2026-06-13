using MQNet;

namespace MQNet.Tests;

public class MqResultTests
{
    [Fact]
    public void Values_ReturnsAllValues()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal(["# H1", "## H2"], result.Values);
    }

    [Fact]
    public void Text_JoinsValuesWithNewline()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal("# H1\n## H2", result.Text);
    }

    [Fact]
    public void Indexer_ReturnsValueAtIndex()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal("# H1", result[0]);
        Assert.Equal("## H2", result[1]);
    }

    [Fact]
    public void Count_ReturnsNumberOfValues()
    {
        var result = new MqResult(["a", "b", "c"]);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void EmptyResult_HasZeroCount()
    {
        var result = new MqResult([]);
        Assert.Empty(result);
    }

    [Fact]
    public void EmptyResult_TextIsEmpty()
    {
        var result = new MqResult([]);
        Assert.Equal("", result.Text);
    }

    [Fact]
    public void Enumeration_IteratesAllValues()
    {
        var result = new MqResult(["x", "y"]);
        Assert.Equal(["x", "y"], result.ToList());
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var result = new MqResult(["a"]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = result[5]);
    }

    // --- PlainValues / PlainText ---

    [Fact]
    public void PlainValues_StripsMarkdownFromEachValue()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal(["H1", "H2"], result.PlainValues);
    }

    [Fact]
    public void PlainText_JoinsPlainValuesWithNewline()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal("H1\nH2", result.PlainText);
    }

    [Fact]
    public void PlainValues_EmptyResult_ReturnsEmpty()
    {
        var result = new MqResult([]);
        Assert.Empty(result.PlainValues);
    }

    [Fact]
    public void PlainText_EmptyResult_ReturnsEmptyString()
    {
        var result = new MqResult([]);
        Assert.Equal("", result.PlainText);
    }

    [Fact]
    public void PlainValues_BoldAndItalic_AreStripped()
    {
        var result = new MqResult(["**bold** and *italic*"]);
        Assert.Equal(["bold and italic"], result.PlainValues);
    }
}
