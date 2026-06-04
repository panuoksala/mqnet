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
}
