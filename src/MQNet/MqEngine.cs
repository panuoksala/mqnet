using System.Runtime.InteropServices;
using MQNet.Interop;

namespace MQNet;

/// <summary>
/// Wraps the mq native engine. Create once, call Eval multiple times, then Dispose.
/// For single-query convenience, use <see cref="Mq.Query"/> instead.
/// </summary>
public sealed class MqEngine : IDisposable
{
    private IntPtr _enginePtr;
    private bool _disposed;

    /// <summary>Creates a new mq engine instance.</summary>
    /// <exception cref="InvalidOperationException">If the native engine could not be created.</exception>
    public MqEngine()
    {
        _enginePtr = NativeMethods.MqCreate();
        if (_enginePtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create mq native engine.");
    }

    /// <summary>
    /// Evaluates a mq query against Markdown input.
    /// </summary>
    /// <param name="query">The mq query string (e.g. ".h1", ".code(\"rust\")").</param>
    /// <param name="input">The input content to query.</param>
    /// <param name="format">Input format (default: Markdown).</param>
    /// <returns>The query result.</returns>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="MqException">If the query fails.</exception>
    public MqResult Eval(string query, string input, InputFormat format = InputFormat.Markdown)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(input);

        var nativeResult = NativeMethods.MqEval(_enginePtr, query, input, format.ToNativeString());
        try
        {
            if (nativeResult.ErrorMsg != IntPtr.Zero)
            {
                var errorMsg = Marshal.PtrToStringUTF8(nativeResult.ErrorMsg) ?? "Unknown error";
                throw new MqException(errorMsg);
            }

            return MarshalResult(nativeResult);
        }
        finally
        {
            NativeMethods.MqFreeResult(nativeResult);
        }
    }

    /// <summary>
    /// Converts HTML to Markdown.
    /// </summary>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>The Markdown representation of the HTML.</returns>
    /// <exception cref="MqException">If conversion fails.</exception>
    public static string HtmlToMarkdown(string html, ConversionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(html);

        var nativeOptions = options is null
            ? default
            : new MqConversionOptionsNative
            {
                ExtractScriptsAsCodeBlocks = options.ExtractScriptsAsCodeBlocks,
                GenerateFrontMatter        = options.GenerateFrontMatter,
                UseTitleAsH1               = options.UseTitleAsH1
            };

        var resultPtr = NativeMethods.MqHtmlToMarkdown(html, nativeOptions, out var errorMsgPtr);

        if (resultPtr == IntPtr.Zero)
        {
            var errorMsg = errorMsgPtr != IntPtr.Zero
                ? Marshal.PtrToStringUTF8(errorMsgPtr) ?? "HTML conversion failed"
                : "HTML conversion failed";
            if (errorMsgPtr != IntPtr.Zero)
                NativeMethods.MqFreeString(errorMsgPtr);
            throw new MqException(errorMsg);
        }

        var markdown = Marshal.PtrToStringUTF8(resultPtr) ?? "";
        NativeMethods.MqFreeString(resultPtr);
        return markdown;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_enginePtr != IntPtr.Zero)
        {
            NativeMethods.MqDestroy(_enginePtr);
            _enginePtr = IntPtr.Zero;
        }
    }

    private static MqResult MarshalResult(MqResultNative native)
    {
        var count = (int)native.ValuesLen;
        if (count == 0 || native.Values == IntPtr.Zero)
            return new MqResult([]);

        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            var ptrToStr = Marshal.ReadIntPtr(native.Values, i * IntPtr.Size);
            values[i] = Marshal.PtrToStringUTF8(ptrToStr) ?? "";
        }
        return new MqResult(values);
    }
}
