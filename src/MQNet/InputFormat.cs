namespace MQNet;

/// <summary>Input format for mq query evaluation.</summary>
public enum InputFormat
{
    /// <summary>CommonMark / GFM Markdown (default).</summary>
    Markdown,
    /// <summary>Markdown with JSX (MDX).</summary>
    Mdx,
    /// <summary>HTML — auto-converted to Markdown before querying.</summary>
    Html,
    /// <summary>Plain text, split by lines.</summary>
    Text,
    /// <summary>Raw string, no parsing.</summary>
    Raw
}

internal static class InputFormatExtensions
{
    internal static string ToNativeString(this InputFormat format) => format switch
    {
        InputFormat.Markdown => "markdown",
        InputFormat.Mdx      => "mdx",
        InputFormat.Html     => "html",
        InputFormat.Text     => "text",
        InputFormat.Raw      => "raw",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };
}
