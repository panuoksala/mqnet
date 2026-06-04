using MQNet;

namespace MQNet.Tests;

public class MqEngineTests
{
    // --- Lifecycle ---

    [Fact]
    public void Constructor_CreatesEngineWithoutError()
    {
        using var engine = new MqEngine();
        Assert.NotNull(engine);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var engine = new MqEngine();
        engine.Dispose();
        engine.Dispose(); // must not throw
    }

    [Fact]
    public void Eval_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.Eval(".h1", "# Hello"));
    }

    // --- Query evaluation ---

    [Fact]
    public void Eval_ExtractsH1Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "# Hello\n\n## World\n\n# Another");
        Assert.Equal(["# Hello", "# Another"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsH2Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(2)", "# Title\n\n## Section A\n\n## Section B");
        Assert.Equal(["## Section A", "## Section B"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsCodeBlocks()
    {
        using var engine = new MqEngine();
        var markdown = "# Title\n\n```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code", markdown);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Eval_FiltersByCodeLanguage()
    {
        using var engine = new MqEngine();
        var markdown = "```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code(\"rust\")", markdown);
        Assert.Single(result);
        Assert.Contains("rust", result[0]);
    }

    [Fact]
    public void Eval_PipeQuery_SelectContains()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h | select(contains(\"Foo\"))", "# Foo\n\n## Bar\n\n## FooBar");
        Assert.Equal(2, result.Count);
        Assert.All(result.Values, v => Assert.Contains("Foo", v));
    }

    [Fact]
    public void Eval_EmptyInput_ReturnsEmptyResult()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h1", "");
        Assert.Empty(result);
    }

    [Fact]
    public void Eval_InvalidQuery_ThrowsMqException()
    {
        using var engine = new MqEngine();
        var ex = Assert.Throws<MqException>(() => engine.Eval(".!!invalid!!", "# Hello"));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Eval_DefaultFormat_IsMarkdown()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>");
        Assert.Empty(result);
    }

    [Fact]
    public void Eval_HtmlFormat_ConvertsBeforeQuerying()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>", InputFormat.Html);
        Assert.Single(result);
        Assert.Contains("Hello", result[0]);
    }

    [Fact]
    public void Eval_TextFormat_SplitsByLines()
    {
        using var engine = new MqEngine();
        var result = engine.Eval("select(contains(\"B\"))", "Line A\nLine B\nLine C",
            InputFormat.Text);
        Assert.Single(result);
        Assert.Equal("Line B", result[0]);
    }

    // --- HTML conversion ---

    [Fact]
    public void HtmlToMarkdown_BasicConversion()
    {
        var markdown = MqEngine.HtmlToMarkdown("<h1>Hello</h1><p>World</p>");
        Assert.Contains("# Hello", markdown);
        Assert.Contains("World", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_WithNullOptions_UsesDefaults()
    {
        var markdown = MqEngine.HtmlToMarkdown("<p>Simple</p>");
        Assert.Contains("Simple", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_UseTitleAsH1_IncludesTitle()
    {
        var html = "<html><head><title>My Title</title></head><body><p>Body</p></body></html>";
        var markdown = MqEngine.HtmlToMarkdown(html, new ConversionOptions { UseTitleAsH1 = true });
        Assert.Contains("# My Title", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_EmptyHtml_ReturnsEmptyOrWhitespace()
    {
        var markdown = MqEngine.HtmlToMarkdown("");
        Assert.NotNull(markdown);
    }
}
