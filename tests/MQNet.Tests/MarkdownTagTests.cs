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
    public void AllHeadings_Selector_IsCorrect() => Assert.Equal(".h", MarkdownTag.AllHeadings.Selector);

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

    // ── HeadingLevel factory ──────────────────────────────────────────────────

    [Fact]
    public void HeadingLevel_1_SelectorIsCorrect()
        => Assert.Equal(".h(1)", MarkdownTag.HeadingLevel(1).Selector);

    [Fact]
    public void HeadingLevel_3_SelectorIsCorrect()
        => Assert.Equal(".h(3)", MarkdownTag.HeadingLevel(3).Selector);

    [Fact]
    public void HeadingLevel_6_SelectorIsCorrect()
        => Assert.Equal(".h(6)", MarkdownTag.HeadingLevel(6).Selector);

    [Fact]
    public void HeadingLevel_0_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.HeadingLevel(0));

    [Fact]
    public void HeadingLevel_7_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.HeadingLevel(7));

    // ── HeadingRange factory ──────────────────────────────────────────────────

    [Fact]
    public void HeadingRange_1_3_SelectorIsCorrect()
        => Assert.Equal(".h(1..3)", MarkdownTag.HeadingRange(1, 3).Selector);

    [Fact]
    public void HeadingRange_2_5_SelectorIsCorrect()
        => Assert.Equal(".h(2..5)", MarkdownTag.HeadingRange(2, 5).Selector);

    [Fact]
    public void HeadingRange_1_1_SelectorIsCorrect()
        => Assert.Equal(".h(1..1)", MarkdownTag.HeadingRange(1, 1).Selector);

    [Fact]
    public void HeadingRange_2_2_SelectorIsCorrect()
        => Assert.Equal(".h(2..2)", MarkdownTag.HeadingRange(2, 2).Selector);

    [Fact]
    public void HeadingRange_FromGreaterThanTo_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.HeadingRange(3, 1));

    [Fact]
    public void HeadingRange_FromOutOfRange_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.HeadingRange(0, 3));

    [Fact]
    public void HeadingRange_ToOutOfRange_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.HeadingRange(1, 7));

    // ── Heading(Range) factory ────────────────────────────────────────────────

    [Fact]
    public void Heading_Range_1_3_SelectorIsCorrect()
        => Assert.Equal(".h(1..2)", MarkdownTag.Heading(1..3).Selector);

    [Fact]
    public void Heading_Range_2_5_SelectorIsCorrect()
        => Assert.Equal(".h(2..4)", MarkdownTag.Heading(2..5).Selector);

    [Fact]
    public void Heading_Range_1_2_CollapsesToSingleLevel()
        => Assert.Equal(".h(1)", MarkdownTag.Heading(1..2).Selector);

    [Fact]
    public void Heading_Range_OpenStart_3_SelectorIsCorrect()
        => Assert.Equal(".h(1..2)", MarkdownTag.Heading(..3).Selector);

    [Fact]
    public void Heading_Range_1_OpenEnd_SelectorIsCorrect()
        => Assert.Equal(".h(1..6)", MarkdownTag.Heading(1..).Selector);

    [Fact]
    public void Heading_Range_FullOpen_SelectorIsCorrect()
        => Assert.Equal(".h(1..6)", MarkdownTag.Heading(..).Selector);

    [Fact]
    public void Heading_Range_FromEnd3_OpenEnd_SelectorIsCorrect()
        => Assert.Equal(".h(3..6)", MarkdownTag.Heading(^3..).Selector);

    [Fact]
    public void Heading_Range_OpenStart_FromEnd1_SelectorIsCorrect()
        => Assert.Equal(".h(1..4)", MarkdownTag.Heading(..^1).Selector);

    [Theory]
    [InlineData(3, 3)]   // empty range: excl=3, incl=2 < from=3
    [InlineData(3, 1)]   // end resolves to inclusiveTo=0 which is out of bounds (triggers bounds guard, not inversion guard)
    [InlineData(1, 8)]   // end out of bounds: incl=7 > 6
    public void Heading_Range_Invalid_ThrowsArgumentOutOfRangeException(int start, int end)
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.Heading(start..end));

    [Fact]
    public void Heading_Range_Inverted_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarkdownTag.Heading(4..2));

    // Note: 0..3 is indistinguishable from ..3 at the Range level (both produce Range(Index(0,false), Index(3,false))),
    // so 0..3 is treated as an open-start range resolving to from=1, not as an error.
    [Fact]
    public void Heading_Range_0_3_TreatedAsOpenStart()
        => Assert.Equal(".h(1..2)", MarkdownTag.Heading(0..3).Selector);

    // ── CodeBlock factory ─────────────────────────────────────────────────────

    [Fact]
    public void CodeBlock_Rust_SelectorIsCorrect()
        => Assert.Equal(".code(\"rust\")", MarkdownTag.CodeBlock("rust").Selector);

    [Fact]
    public void CodeBlock_Python_SelectorIsCorrect()
        => Assert.Equal(".code(\"python\")", MarkdownTag.CodeBlock("python").Selector);

    [Fact]
    public void CodeBlock_Null_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => MarkdownTag.CodeBlock(null!));

    [Fact]
    public void CodeBlock_Empty_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => MarkdownTag.CodeBlock(""));

    [Fact]
    public void CodeBlock_Whitespace_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => MarkdownTag.CodeBlock(" "));

    [Fact]
    public void CodeBlock_EmbeddedDoubleQuote_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => MarkdownTag.CodeBlock("a\"b"));
}
