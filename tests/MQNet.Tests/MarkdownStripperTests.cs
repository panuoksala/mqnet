using MQNet;

namespace MQNet.Tests;

public class MarkdownStripperTests
{
    // --- Headings ---

    [Theory]
    [InlineData("# Hello", "Hello")]
    [InlineData("## Section", "Section")]
    [InlineData("### Sub", "Sub")]
    [InlineData("###### Deep", "Deep")]
    public void Strip_RemovesAtxHeadings(string input, string expected)
    {
        Assert.Equal(expected, MarkdownStripper.Strip(input));
    }

    // --- Emphasis ---

    [Theory]
    [InlineData("**bold**", "bold")]
    [InlineData("__bold__", "bold")]
    [InlineData("*italic*", "italic")]
    [InlineData("_italic_", "italic")]
    [InlineData("***bold italic***", "bold italic")]
    [InlineData("~~strike~~", "strike")]
    public void Strip_RemovesEmphasis(string input, string expected)
    {
        Assert.Equal(expected, MarkdownStripper.Strip(input));
    }

    // --- Inline code ---

    [Fact]
    public void Strip_RemovesInlineCode()
    {
        Assert.Equal("use var x", MarkdownStripper.Strip("use `var x`"));
    }

    // --- Links ---

    [Fact]
    public void Strip_RemovesInlineLinks_KeepsText()
    {
        Assert.Equal("click here", MarkdownStripper.Strip("[click here](https://example.com)"));
    }

    [Fact]
    public void Strip_RemovesImages_KeepsAltText()
    {
        Assert.Equal("logo", MarkdownStripper.Strip("![logo](https://example.com/logo.png)"));
    }

    [Fact]
    public void Strip_RemovesReferenceLinks_KeepsText()
    {
        Assert.Equal("click here", MarkdownStripper.Strip("[click here][ref]"));
    }

    // --- Fenced code blocks ---

    [Fact]
    public void Strip_FencedCodeBlock_KeepsInnerContent()
    {
        var input = "```csharp\nvar x = 1;\n```";
        Assert.Equal("var x = 1;", MarkdownStripper.Strip(input));
    }

    // --- Blockquotes ---

    [Fact]
    public void Strip_RemovesBlockquoteMarker()
    {
        Assert.Equal("quoted text", MarkdownStripper.Strip("> quoted text"));
    }

    // --- List markers ---

    [Theory]
    [InlineData("- item", "item")]
    [InlineData("* item", "item")]
    [InlineData("+ item", "item")]
    [InlineData("1. item", "item")]
    [InlineData("42. item", "item")]
    public void Strip_RemovesListMarkers(string input, string expected)
    {
        Assert.Equal(expected, MarkdownStripper.Strip(input));
    }

    // --- HTML tags ---

    [Fact]
    public void Strip_RemovesHtmlTags()
    {
        Assert.Equal("hello world", MarkdownStripper.Strip("<b>hello</b> world"));
    }

    // --- Empty / null-like ---

    [Fact]
    public void Strip_EmptyString_ReturnsEmpty()
    {
        Assert.Equal("", MarkdownStripper.Strip(""));
    }

    [Fact]
    public void Strip_PlainText_IsUnchanged()
    {
        Assert.Equal("just plain text", MarkdownStripper.Strip("just plain text"));
    }
}
