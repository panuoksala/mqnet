using MQNet;

namespace MQNet.Tests;

public class MarkdownTagTests
{
    // ── Well-known selector strings ──────────────────────────────────────────

    [Fact]
    public void H1_Selector_IsCorrect() => Assert.Equal(".h(1)", MarkdownTag.H1.Selector);

    [Fact]
    public void H2_Selector_IsCorrect() => Assert.Equal(".h(2)", MarkdownTag.H2.Selector);

    [Fact]
    public void H3_Selector_IsCorrect() => Assert.Equal(".h(3)", MarkdownTag.H3.Selector);

    [Fact]
    public void H4_Selector_IsCorrect() => Assert.Equal(".h(4)", MarkdownTag.H4.Selector);

    [Fact]
    public void H5_Selector_IsCorrect() => Assert.Equal(".h(5)", MarkdownTag.H5.Selector);

    [Fact]
    public void H6_Selector_IsCorrect() => Assert.Equal(".h(6)", MarkdownTag.H6.Selector);

    [Fact]
    public void Heading_Selector_IsCorrect() => Assert.Equal(".h", MarkdownTag.Heading.Selector);

    [Fact]
    public void Paragraph_Selector_IsCorrect() => Assert.Equal(".text", MarkdownTag.Paragraph.Selector);

    [Fact]
    public void Text_IsAliasForParagraph() => Assert.Equal(MarkdownTag.Paragraph, MarkdownTag.Text);

    [Fact]
    public void List_Selector_IsCorrect() => Assert.Equal(".list", MarkdownTag.List.Selector);

    [Fact]
    public void Code_Selector_IsCorrect() => Assert.Equal(".code", MarkdownTag.Code.Selector);

    [Fact]
    public void InlineCode_Selector_IsCorrect() => Assert.Equal(".code_inline", MarkdownTag.InlineCode.Selector);

    [Fact]
    public void Link_Selector_IsCorrect() => Assert.Equal(".link", MarkdownTag.Link.Selector);

    [Fact]
    public void Image_Selector_IsCorrect() => Assert.Equal(".image", MarkdownTag.Image.Selector);

    [Fact]
    public void HorizontalRule_Selector_IsCorrect() => Assert.Equal(".horizontal_rule", MarkdownTag.HorizontalRule.Selector);

    [Fact]
    public void LineBreak_Selector_IsCorrect() => Assert.Equal(".break", MarkdownTag.LineBreak.Selector);

    [Fact]
    public void Blockquote_Selector_IsCorrect() => Assert.Equal(".blockquote", MarkdownTag.Blockquote.Selector);

    [Fact]
    public void Table_Selector_IsCorrect() => Assert.Equal(".table", MarkdownTag.Table.Selector);

    [Fact]
    public void Footnote_Selector_IsCorrect() => Assert.Equal(".footnote", MarkdownTag.Footnote.Selector);

    [Fact]
    public void MathInline_Selector_IsCorrect() => Assert.Equal(".math_inline", MarkdownTag.MathInline.Selector);

    [Fact]
    public void Html_Selector_IsCorrect() => Assert.Equal(".html", MarkdownTag.Html.Selector);

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsSelector() => Assert.Equal(".h(1)", MarkdownTag.H1.ToString());

    [Fact]
    public void ToString_Paragraph_ReturnsSelector() => Assert.Equal(".text", MarkdownTag.Paragraph.ToString());

    [Fact]
    public void Default_ToString_IsNull()
        => Assert.Null(default(MarkdownTag).ToString());

    // ── Equality ─────────────────────────────────────────────────────────────

    [Fact]
    public void TwoH1Values_AreEqual()
    {
        var a = MarkdownTag.H1;
        var b = MarkdownTag.H1;
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void H1_NotEqualTo_H2()
    {
        Assert.NotEqual(MarkdownTag.H1, MarkdownTag.H2);
        Assert.True(MarkdownTag.H1 != MarkdownTag.H2);
        Assert.False(MarkdownTag.H1 == MarkdownTag.H2);
    }

    [Fact]
    public void GetHashCode_SameForEqualTags()
    {
        Assert.Equal(MarkdownTag.H1.GetHashCode(), MarkdownTag.H1.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentForDifferentTags()
    {
        // Not a contract requirement — smoke-check that two distinct values don't trivially collide.
        Assert.NotEqual(MarkdownTag.H1.GetHashCode(), MarkdownTag.H2.GetHashCode());
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        object obj = MarkdownTag.H1;
        Assert.True(MarkdownTag.H1.Equals(obj));
        Assert.False(MarkdownTag.H2.Equals(obj));
    }

    // ── Default value ─────────────────────────────────────────────────────────

    [Fact]
    public void Default_Selector_IsNull()
    {
        var d = default(MarkdownTag);
        Assert.Null(d.Selector);
    }

    [Fact]
    public void Default_NotEqualToH1()
    {
        var d = default(MarkdownTag);
        Assert.NotEqual(d, MarkdownTag.H1);
    }

    [Fact]
    public void Default_NotEqualToParagraph()
    {
        var d = default(MarkdownTag);
        Assert.NotEqual(d, MarkdownTag.Paragraph);
    }

    [Fact]
    public void Default_EqualToAnotherDefault()
    {
        var d1 = default(MarkdownTag);
        var d2 = default(MarkdownTag);
        Assert.Equal(d1, d2);
    }
}
