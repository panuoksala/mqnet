using MQNet;

namespace MQNet.Tests;

public class MqQueryBuilderTests
{
    [Fact]
    public void Query_On_Run_ExtractsH1()
    {
        var result = Mq.Query(".h(1)").On("# Hello\n\n## World").Run();
        Assert.Single(result);
        Assert.Equal("# Hello", result[0]);
    }

    [Fact]
    public void Query_WithDefaultFormat_IsMarkdown()
    {
        var result = Mq.Query(".h(1)").On("# Title\n\n## Sub").Run();
        Assert.Single(result);
    }

    [Fact]
    public void Query_WithHtmlFormat_ConvertsFirst()
    {
        var result = Mq.Query(".h(1)")
            .On("<h1>Title</h1>")
            .WithFormat(InputFormat.Html)
            .Run();
        Assert.Single(result);
        Assert.Contains("Title", result[0]);
    }

    [Fact]
    public void Query_WithTextFormat_QueriesLines()
    {
        var result = Mq.Query("select(contains(\"match\"))")
            .On("no\nmatch this\nno")
            .WithFormat(InputFormat.Text)
            .Run();
        Assert.Single(result);
        Assert.Equal("match this", result[0]);
    }

    [Fact]
    public void Query_ChainedCalls_ReturnSameBuilder()
    {
        var builder = Mq.Query(".h(1)");
        var withContent = builder.On("# Hello");
        var withFormat = withContent.WithFormat(InputFormat.Markdown);
        Assert.Same(builder, withContent);
        Assert.Same(builder, withFormat);
    }

    [Fact]
    public void Query_Run_DisposesEngineAfterCall()
    {
        var r1 = Mq.Query(".h(1)").On("# A").Run();
        var r2 = Mq.Query(".h(1)").On("# B").Run();
        Assert.Equal("# A", r1[0]);
        Assert.Equal("# B", r2[0]);
    }

    [Fact]
    public void Query_Run_WithoutOn_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Mq.Query(".h(1)").Run());
    }

    [Fact]
    public void Query_InvalidQuery_ThrowsMqException()
    {
        Assert.Throws<MqException>(() => Mq.Query(".!!bad!!").On("# Hello").Run());
    }

    // --- WithPlainText ---

    [Fact]
    public void Query_WithPlainText_StripsHeadingMarkers()
    {
        var result = Mq.Query(".h(1)").On("# Hello\n\n## World").WithPlainText().Run();
        Assert.Single(result);
        Assert.Equal("Hello", result[0]);
    }

    [Fact]
    public void Query_WithPlainText_StripsMarkdownFormatting()
    {
        var result = Mq.Query(".h").On("# Hello **World**\n\n## *Section*").WithPlainText().Run();
        Assert.Equal(["Hello World", "Section"], result.Values);
    }

    [Fact]
    public void Query_WithPlainText_ChainReturnsSameBuilder()
    {
        var builder = Mq.Query(".h(1)");
        var withPlainText = builder.WithPlainText();
        Assert.Same(builder, withPlainText);
    }

    // --- Mq.Query(MarkdownTag) overload ---

    [Fact]
    public void Query_MarkdownTagH1_ExtractsH1()
    {
        var result = Mq.Query(MarkdownTag.H1).On("# Hello\n\n## World").Run();
        Assert.Single(result);
        Assert.Equal("# Hello", result[0]);
    }

    [Fact]
    public void Query_MarkdownTagH2_ExtractsH2()
    {
        var result = Mq.Query(MarkdownTag.H2).On("# Hello\n\n## World").Run();
        Assert.Single(result);
        Assert.Equal("## World", result[0]);
    }

    [Fact]
    public void Query_MarkdownTagCode_ExtractsCodeBlock()
    {
        var result = Mq.Query(MarkdownTag.Code).On("```rust\nfn main() {}\n```").Run();
        Assert.Single(result);
        Assert.Contains("fn main()", result[0]);
    }

    [Fact]
    public void Query_MarkdownTagH1_WithPlainText_ReturnsPlainText()
    {
        var result = Mq.Query(MarkdownTag.H1).On("# Hello\n\n## World").WithPlainText().Run();
        Assert.Single(result);
        Assert.Equal("Hello", result[0]);
    }

    [Fact]
    public void Query_MarkdownTagH1_WithFormat_ReturnsH1()
    {
        var result = Mq.Query(MarkdownTag.H1).On("# Hello").WithFormat(InputFormat.Markdown).Run();

        Assert.Single(result);
        Assert.Equal("# Hello", result[0]);
    }

    [Fact]
    public void Query_DefaultMarkdownTag_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Mq.Query(default(MarkdownTag)));
    }

    // --- Mq.Heading / Mq.Code convenience methods ---

    [Fact]
    public void Heading_Level1_MatchesQueryMarkdownTagH1()
    {
        var convenience = Mq.Heading(1).On("# Hello\n\n## World").Run();
        var direct = Mq.Query(MarkdownTag.H1).On("# Hello\n\n## World").Run();
        Assert.Equal(direct.Values, convenience.Values);
    }

    [Fact]
    public void CodeBlock_Rust_MatchesQueryMarkdownTagCodeBlockRust()
    {
        var convenience = Mq.CodeBlock("rust").On("```rust\nfn main() {}\n```").Run();
        var direct = Mq.Query(MarkdownTag.CodeBlock("rust")).On("```rust\nfn main() {}\n```").Run();
        Assert.Equal(direct.Values, convenience.Values);
    }

    [Fact]
    public void Heading_Level0_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(0));
    }

    [Fact]
    public void Heading_Level7_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(7));
    }

    // Mq.Heading(int from, int to) — inclusive range

    [Fact]
    public void Heading_InclusiveRange_MatchesQueryHeadingRange()
    {
        // Mq.Heading(1, 2) is shorthand for Query(MarkdownTag.HeadingRange(1, 2))
        var convenience = Mq.Heading(1, 2).On("# H1\n\n## H2\n\n### H3").Run();
        var direct = Mq.Query(MarkdownTag.HeadingRange(1, 2)).On("# H1\n\n## H2\n\n### H3").Run();
        Assert.Equal(direct.Values, convenience.Values);
    }

    [Fact]
    public void Heading_InclusiveRange_InvalidFrom_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(0, 3));
    }

    [Fact]
    public void Heading_InclusiveRange_InvalidTo_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(1, 7));
    }

    [Fact]
    public void Heading_InclusiveRange_Inverted_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(3, 1));
    }

    // Mq.Heading(Range) — C# exclusive-end

    [Fact]
    public void Heading_CSharpRange_MatchesQueryMarkdownTagHeading()
    {
        // Mq.Heading(1..3) is shorthand for Query(MarkdownTag.Heading(1..3))
        var convenience = Mq.Heading(1..3).On("# H1\n\n## H2\n\n### H3").Run();
        var direct = Mq.Query(MarkdownTag.Heading(1..3)).On("# H1\n\n## H2\n\n### H3").Run();
        Assert.Equal(direct.Values, convenience.Values);
    }

    [Fact]
    public void Heading_CSharpRange_EmptyRange_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(3..3));
    }

    [Fact]
    public void Heading_CSharpRange_Inverted_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mq.Heading(4..2));
    }

    [Fact]
    public void CodeBlock_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Mq.CodeBlock(""));
    }
}
