using System.Runtime.InteropServices;
using MQNet.Interop;

namespace MQNet;

/// <summary>
/// Wraps the mq native engine. Create once, call Eval multiple times, then Dispose.
/// For single-query convenience, use <see cref="Mq.Query(string)"/> or <see cref="Mq.Query(MarkdownTag)"/> instead.
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

    // ── Version ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the version string of the native mq-ffi library (e.g. <c>"0.6.5"</c>).
    /// This is backed by a static C string; it is safe to call at any time.
    /// </summary>
    public static string Version
    {
        get
        {
            var ptr = NativeMethods.MqVersion();
            return ptr == IntPtr.Zero ? "" : Marshal.PtrToStringUTF8(ptr) ?? "";
        }
    }

    // ── Engine configuration ──────────────────────────────────────────────────

    /// <summary>
    /// Sets the AST optimization level applied before each query evaluation.
    /// Higher levels may improve evaluation speed at the cost of slightly longer compilation.
    /// </summary>
    /// <param name="level">The desired optimization level.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    public void SetOptimizationLevel(MqOptimizationLevel level)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        NativeMethods.MqSetOptimizationLevel(_enginePtr, (int)level);
    }

    /// <summary>
    /// Sets the maximum call stack depth for mq function calls.
    /// Use this to guard against runaway recursion in untrusted queries.
    /// </summary>
    /// <param name="maxDepth">The maximum number of nested function calls allowed.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    public void SetMaxCallStackDepth(uint maxDepth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        NativeMethods.MqSetMaxCallStackDepth(_enginePtr, maxDepth);
    }

    /// <summary>
    /// Sets the file-system search paths used to resolve module imports.
    /// Replaces any previously configured search paths.
    /// </summary>
    /// <param name="paths">One or more directory paths to search.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="paths"/> is null.</exception>
    public void SetSearchPaths(IReadOnlyList<string> paths)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(paths);
        MarshalStringArray(paths, (ptr, len) =>
            NativeMethods.MqSetSearchPaths(_enginePtr, ptr, (UIntPtr)len));
    }

    /// <summary>
    /// Defines a named string variable that mq code can reference by name.
    /// Calling this again with the same name overwrites the previous value.
    /// </summary>
    /// <param name="name">Variable name (must not be null).</param>
    /// <param name="value">String value to associate with the name.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="value"/> is null.</exception>
    public void DefineStringValue(string name, string value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        NativeMethods.MqDefineStringValue(_enginePtr, name, value);
    }

    /// <summary>
    /// Namespace-imports a module by name (equivalent to mq's <c>import name</c>).
    /// The module's definitions are then accessible as <c>name::function()</c>.
    /// </summary>
    /// <param name="moduleName">The module name to import.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="moduleName"/> is null.</exception>
    /// <exception cref="MqException">If the module cannot be found or fails to load.</exception>
    public void ImportModule(string moduleName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(moduleName);
        var errorPtr = NativeMethods.MqImportModule(_enginePtr, moduleName);
        ThrowIfError(errorPtr);
    }

    /// <summary>
    /// Loads a module into the calling scope (equivalent to mq's <c>include name</c>).
    /// The module's definitions are accessible without a namespace prefix.
    /// </summary>
    /// <param name="moduleName">The module name to load.</param>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="moduleName"/> is null.</exception>
    /// <exception cref="MqException">If the module cannot be found or fails to load.</exception>
    public void LoadModule(string moduleName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(moduleName);
        var errorPtr = NativeMethods.MqLoadModule(_enginePtr, moduleName);
        ThrowIfError(errorPtr);
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

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// If <paramref name="errorPtr"/> is non-zero, reads the error message, frees the native string,
    /// and throws <see cref="MqException"/>.
    /// </summary>
    private static void ThrowIfError(IntPtr errorPtr)
    {
        if (errorPtr == IntPtr.Zero) return;
        var message = Marshal.PtrToStringUTF8(errorPtr) ?? "Unknown error";
        NativeMethods.MqFreeString(errorPtr);
        throw new MqException(message);
    }

    /// <summary>
    /// Marshals a managed string list to a native <c>char**</c> and invokes <paramref name="action"/>.
    /// Pinned memory is released before this method returns.
    /// </summary>
    private static unsafe void MarshalStringArray(IReadOnlyList<string> items, Action<IntPtr, int> action)
    {
        int count = items.Count;
        if (count == 0)
        {
            action(IntPtr.Zero, 0);
            return;
        }

        // Encode each string as a null-terminated UTF-8 byte array
        var encodings = new byte[count][];
        for (int i = 0; i < count; i++)
            encodings[i] = System.Text.Encoding.UTF8.GetBytes(items[i] + '\0');

        // Pin each encoding and build the pointer array
        var handles = new System.Runtime.InteropServices.GCHandle[count];
        var ptrs    = new IntPtr[count];
        try
        {
            for (int i = 0; i < count; i++)
            {
                handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(
                    encodings[i], System.Runtime.InteropServices.GCHandleType.Pinned);
                ptrs[i] = handles[i].AddrOfPinnedObject();
            }
            fixed (IntPtr* ptrArray = ptrs)
            {
                action((IntPtr)ptrArray, count);
            }
        }
        finally
        {
            foreach (var h in handles)
                if (h.IsAllocated) h.Free();
        }
    }

    private static MqResult MarshalResult(MqResultNative native)
    {
        var rawLen = (ulong)native.ValuesLen;
        if (rawLen > (ulong)Array.MaxLength)
            throw new InvalidOperationException($"Native result length {rawLen} exceeds maximum.");
        var count = (int)rawLen;

        if (count == 0 || native.Values == IntPtr.Zero)
            return new MqResult([]);

        var values = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            var ptrToStr = Marshal.ReadIntPtr(native.Values, i * IntPtr.Size);
            var s = Marshal.PtrToStringUTF8(ptrToStr) ?? "";
            if (s.Length > 0)
                values.Add(s);
        }
        return new MqResult(values);
    }
}
