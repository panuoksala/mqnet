using System.Text.RegularExpressions;

namespace MQNet;

/// <summary>Utility that removes common Markdown formatting from a string.</summary>
internal static partial class MarkdownStripper
{
    // Fenced code blocks: ```lang\n...\n```
    [GeneratedRegex(@"^```[^\n]*\n(.*?)\n```\s*$", RegexOptions.Singleline)]
    private static partial Regex FencedCodeBlock();

    // ATX headings: # text, ## text, etc.
    [GeneratedRegex(@"^#{1,6}\s+")]
    private static partial Regex Heading();

    // Setext heading underlines: ===... or ---...
    [GeneratedRegex(@"\n[=\-]{2,}\s*$")]
    private static partial Regex SetextUnderline();

    // Blockquote markers: > at start of line
    [GeneratedRegex(@"^>\s?", RegexOptions.Multiline)]
    private static partial Regex Blockquote();

    // Horizontal rules: ---, ***, ___
    [GeneratedRegex(@"^\s*[-*_]{3,}\s*$", RegexOptions.Multiline)]
    private static partial Regex HorizontalRule();

    // Images: ![alt](url) — must come before links
    [GeneratedRegex(@"!\[([^\]]*)\]\([^\)]*\)")]
    private static partial Regex Image();

    // Inline links: [text](url)
    [GeneratedRegex(@"\[([^\]]+)\]\([^\)]*\)")]
    private static partial Regex InlineLink();

    // Reference-style links: [text][id]
    [GeneratedRegex(@"\[([^\]]+)\]\[[^\]]*\]")]
    private static partial Regex ReferenceLink();

    // Bold+italic: ***text*** or ___text___
    [GeneratedRegex(@"(\*{3}|_{3})(.+?)\1")]
    private static partial Regex BoldItalic();

    // Bold: **text** or __text__
    [GeneratedRegex(@"(\*{2}|_{2})(.+?)\1")]
    private static partial Regex Bold();

    // Italic: *text* or _text_
    [GeneratedRegex(@"(\*|_)(.+?)\1")]
    private static partial Regex Italic();

    // Strikethrough: ~~text~~
    [GeneratedRegex(@"~~(.+?)~~")]
    private static partial Regex Strikethrough();

    // Inline code: `code`
    [GeneratedRegex(@"`+(.+?)`+")]
    private static partial Regex InlineCode();

    // Unordered list markers: - , * , + at line start
    [GeneratedRegex(@"^[\*\-\+]\s+", RegexOptions.Multiline)]
    private static partial Regex UnorderedList();

    // Ordered list markers: 1. at line start
    [GeneratedRegex(@"^\d+\.\s+", RegexOptions.Multiline)]
    private static partial Regex OrderedList();

    // HTML tags
    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTag();

    // Collapse multiple blank lines / trim
    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultipleBlankLines();

    /// <summary>Returns <paramref name="markdown"/> with all recognized markdown constructs removed.</summary>
    public static string Strip(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return markdown;

        var s = markdown;

        // Fenced code blocks — keep the inner content
        s = FencedCodeBlock().Replace(s, m => m.Groups[1].Value);

        // Block-level
        s = Heading().Replace(s, "");
        s = SetextUnderline().Replace(s, "");
        s = Blockquote().Replace(s, "");
        s = HorizontalRule().Replace(s, "");

        // Images before links (otherwise link pattern would match)
        s = Image().Replace(s, m => m.Groups[1].Value);
        s = InlineLink().Replace(s, m => m.Groups[1].Value);
        s = ReferenceLink().Replace(s, m => m.Groups[1].Value);

        // Emphasis — order matters: bold+italic before bold before italic
        s = BoldItalic().Replace(s, m => m.Groups[2].Value);
        s = Bold().Replace(s, m => m.Groups[2].Value);
        s = Italic().Replace(s, m => m.Groups[2].Value);

        // Other inline
        s = Strikethrough().Replace(s, m => m.Groups[1].Value);
        s = InlineCode().Replace(s, m => m.Groups[1].Value);

        // List markers
        s = UnorderedList().Replace(s, "");
        s = OrderedList().Replace(s, "");

        // HTML
        s = HtmlTag().Replace(s, "");

        // Clean up excess blank lines and trim
        s = MultipleBlankLines().Replace(s, "\n\n");
        s = s.Trim();

        return s;
    }
}
