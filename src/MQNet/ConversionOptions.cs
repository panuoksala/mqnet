namespace MQNet;

/// <summary>Options for HTML to Markdown conversion.</summary>
public sealed class ConversionOptions
{
    /// <summary>Extract &lt;script&gt; tags as fenced code blocks.</summary>
    public bool ExtractScriptsAsCodeBlocks { get; init; }

    /// <summary>Generate YAML front matter from HTML &lt;head&gt; metadata.</summary>
    public bool GenerateFrontMatter { get; init; }

    /// <summary>Use the HTML &lt;title&gt; element as the H1 heading.</summary>
    public bool UseTitleAsH1 { get; init; }
}
