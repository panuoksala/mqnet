namespace MQNet;

/// <summary>
/// Identifies a Markdown node type for use in mq queries.
/// This is a value type; two <see cref="MarkdownTag"/> values with the same
/// <see cref="Selector"/> string are considered equal.
/// </summary>
/// <remarks>
/// Use the static well-known properties (e.g. <see cref="H1"/>, <see cref="Paragraph"/>)
/// to obtain pre-built instances. The parameterless <c>default</c> value has a
/// <see langword="null"/> <see cref="Selector"/> and is not equal to any named tag.
/// </remarks>
public readonly struct MarkdownTag : IEquatable<MarkdownTag>
{
    // ── Private constructor ──────────────────────────────────────────────────

    private MarkdownTag(string selector)
    {
        Selector = selector;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// The mq selector string for this tag (e.g. <c>".h(1)"</c>, <c>".text"</c>).
    /// Is <see langword="null"/> for <c>default(MarkdownTag)</c>.
    /// </summary>
    public string? Selector { get; }

    // ── Well-known static members ────────────────────────────────────────────

    /// <summary>Selects level-1 headings (<c>.h(1)</c>).</summary>
    public static MarkdownTag H1 { get; } = new(".h(1)");

    /// <summary>Selects level-2 headings (<c>.h(2)</c>).</summary>
    public static MarkdownTag H2 { get; } = new(".h(2)");

    /// <summary>Selects level-3 headings (<c>.h(3)</c>).</summary>
    public static MarkdownTag H3 { get; } = new(".h(3)");

    /// <summary>Selects level-4 headings (<c>.h(4)</c>).</summary>
    public static MarkdownTag H4 { get; } = new(".h(4)");

    /// <summary>Selects level-5 headings (<c>.h(5)</c>).</summary>
    public static MarkdownTag H5 { get; } = new(".h(5)");

    /// <summary>Selects level-6 headings (<c>.h(6)</c>).</summary>
    public static MarkdownTag H6 { get; } = new(".h(6)");

    /// <summary>Selects all headings regardless of level (<c>.h</c>).</summary>
    public static MarkdownTag AllHeadings { get; } = new(".h");

    /// <summary>Selects paragraph (text) nodes (<c>.text</c>).</summary>
    public static MarkdownTag Paragraph { get; } = new(".text");

    /// <summary>
    /// Alias for <see cref="Paragraph"/>. Selects paragraph (text) nodes (<c>.text</c>).
    /// </summary>
    public static MarkdownTag Text => Paragraph;

    /// <summary>Selects list nodes (<c>.list</c>).</summary>
    public static MarkdownTag List { get; } = new(".list");

    /// <summary>Selects fenced code block nodes (<c>.code</c>).</summary>
    public static MarkdownTag Code { get; } = new(".code");

    /// <summary>Selects inline code spans (<c>.code_inline</c>).</summary>
    public static MarkdownTag InlineCode { get; } = new(".code_inline");

    /// <summary>Selects hyperlink nodes (<c>.link</c>).</summary>
    public static MarkdownTag Link { get; } = new(".link");

    /// <summary>Selects image nodes (<c>.image</c>).</summary>
    public static MarkdownTag Image { get; } = new(".image");

    /// <summary>Selects horizontal rule (thematic break) nodes (<c>.horizontal_rule</c>).</summary>
    public static MarkdownTag HorizontalRule { get; } = new(".horizontal_rule");

    /// <summary>Selects line-break nodes (<c>.break</c>).</summary>
    public static MarkdownTag LineBreak { get; } = new(".break");

    /// <summary>Selects blockquote nodes (<c>.blockquote</c>).</summary>
    public static MarkdownTag Blockquote { get; } = new(".blockquote");

    /// <summary>Selects table nodes (<c>.table</c>).</summary>
    public static MarkdownTag Table { get; } = new(".table");

    /// <summary>Selects footnote nodes (<c>.footnote</c>).</summary>
    public static MarkdownTag Footnote { get; } = new(".footnote");

    /// <summary>Selects inline math nodes (<c>.math_inline</c>).</summary>
    public static MarkdownTag MathInline { get; } = new(".math_inline");

    /// <summary>Selects raw HTML nodes (<c>.html</c>).</summary>
    public static MarkdownTag Html { get; } = new(".html");

    // ── Factory methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="MarkdownTag"/> that selects headings at the specified level.
    /// </summary>
    /// <param name="level">Heading level, 1–6 inclusive.</param>
    /// <returns>A <see cref="MarkdownTag"/> whose <see cref="Selector"/> is <c>.h(<paramref name="level"/>)</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="level"/> is less than 1 or greater than 6.
    /// </exception>
    public static MarkdownTag HeadingLevel(int level)
    {
        if (level < 1 || level > 6)
            throw new ArgumentOutOfRangeException(nameof(level), level, "Heading level must be between 1 and 6 inclusive.");
        return new MarkdownTag($".h({level})");
    }

    /// <summary>
    /// Creates a <see cref="MarkdownTag"/> that selects headings within an inclusive level range.
    /// </summary>
    /// <param name="from">First heading level in the range, 1–6 inclusive.</param>
    /// <param name="to">Last heading level in the range, 1–6 inclusive. Must be ≥ <paramref name="from"/>.</param>
    /// <returns>
    /// A <see cref="MarkdownTag"/> whose <see cref="Selector"/> is <c>.h(<paramref name="from"/>..<paramref name="to"/>)</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="from"/> or <paramref name="to"/> is outside the range 1–6.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="from"/> is greater than <paramref name="to"/>.
    /// </exception>
    public static MarkdownTag HeadingRange(int from, int to)
    {
        if (from < 1 || from > 6)
            throw new ArgumentOutOfRangeException(nameof(from), from, "Heading level must be between 1 and 6 inclusive.");
        if (to < 1 || to > 6)
            throw new ArgumentOutOfRangeException(nameof(to), to, "Heading level must be between 1 and 6 inclusive.");
        if (from > to)
            throw new ArgumentOutOfRangeException(nameof(from), from, "The 'from' level must not be greater than the 'to' level.");
        return new MarkdownTag($".h({from}..{to})");
    }

    /// <summary>
    /// Creates a <see cref="MarkdownTag"/> that selects headings within a C# <see cref="Range"/> of levels,
    /// using exclusive-end semantics (as C# ranges do) translated to mq's inclusive range selector.
    /// </summary>
    /// <param name="levels">
    /// A C# <see cref="Range"/> using exclusive-end semantics over heading levels 1–6.
    /// Examples: <c>1..3</c> selects H1–H2; <c>1..</c> selects H1–H6; <c>..3</c> selects H1–H2;
    /// <c>..</c> selects H1–H6; <c>^3..</c> selects H3–H6; <c>..^1</c> selects H1–H4.
    /// </param>
    /// <returns>
    /// A <see cref="MarkdownTag"/> whose <see cref="Selector"/> is <c>.h(from..to)</c> for a range,
    /// or <c>.h(n)</c> when the range resolves to a single level.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the resolved start or inclusive end is outside 1–6, or when start is greater than
    /// the inclusive end (empty or inverted range).
    /// </exception>
    public static MarkdownTag Heading(Range levels)
    {
        // Resolve start.
        // Open start (..x or ..) compiles to: IsFromEnd=false, Value=0 → treat as 1.
        // From-end (^n..)       compiles to: IsFromEnd=true,  Value=n → 6 - n.
        // Explicit (n..)        compiles to: IsFromEnd=false, Value=n → n.
        int from = levels.Start.IsFromEnd
            ? 6 - levels.Start.Value               // ^n → 6-n
            : (levels.Start.Value == 0 ? 1 : levels.Start.Value);  // open → 1, else n

        // Resolve end (exclusive).
        // Open end (x.. or ..) compiles to: IsFromEnd=true,  Value=0 → treat as 7.
        // From-end (..^n)       compiles to: IsFromEnd=true,  Value=n → 6 - n.
        // Explicit (..n)        compiles to: IsFromEnd=false, Value=n → n.
        int exclusiveEnd = levels.End.IsFromEnd
            ? (levels.End.Value == 0 ? 7 : 6 - levels.End.Value)  // open → 7, ^n → 6-n
            : levels.End.Value;                    // n

        int inclusiveTo = exclusiveEnd - 1;

        if (from < 1 || from > 6)
            throw new ArgumentOutOfRangeException(nameof(levels), levels, "Resolved start heading level must be between 1 and 6 inclusive.");
        if (inclusiveTo < 1 || inclusiveTo > 6)
            throw new ArgumentOutOfRangeException(nameof(levels), levels, "Resolved end heading level must be between 1 and 6 inclusive.");
        if (from > inclusiveTo)
            throw new ArgumentOutOfRangeException(nameof(levels), levels, "The resolved start level must not be greater than the resolved end level.");

        return from == inclusiveTo
            ? new MarkdownTag($".h({from})")
            : new MarkdownTag($".h({from}..{inclusiveTo})");
    }

    /// <summary>
    /// Creates a <see cref="MarkdownTag"/> that selects fenced code blocks with the specified language identifier.
    /// </summary>
    /// <param name="language">
    /// The language identifier (e.g. <c>"rust"</c>, <c>"python"</c>).
    /// Must be non-null, non-empty, non-whitespace, and must not contain a double-quote character.
    /// </param>
    /// <returns>
    /// A <see cref="MarkdownTag"/> whose <see cref="Selector"/> is <c>.code("<paramref name="language"/>")</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="language"/> is <see langword="null"/>, empty, whitespace-only,
    /// or contains a double-quote character.
    /// </exception>
    public static MarkdownTag CodeBlock(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language must be a non-empty, non-whitespace string.", nameof(language));
        if (language.Contains('"'))
            throw new ArgumentException("Language must not contain double-quote characters.", nameof(language));
        return new MarkdownTag($".code(\"{language}\")");
    }

    // ── Equality ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool Equals(MarkdownTag other) =>
        StringComparer.Ordinal.Equals(Selector, other.Selector);

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is MarkdownTag other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        Selector is null ? 0 : StringComparer.Ordinal.GetHashCode(Selector);

    /// <summary>Returns <see langword="true"/> if the two tags have the same selector.</summary>
    public static bool operator ==(MarkdownTag left, MarkdownTag right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> if the two tags have different selectors.</summary>
    public static bool operator !=(MarkdownTag left, MarkdownTag right) => !left.Equals(right);

    // ── ToString ──────────────────────────────────────────────────────────────

    /// <summary>Returns the <see cref="Selector"/> string.</summary>
    public override string? ToString() => Selector;
}
