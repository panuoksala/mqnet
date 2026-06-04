using System.Runtime.InteropServices;

namespace MQNet.Interop;

/// <summary>
/// Native struct matching mq-ffi's MqResult (repr(C)):
///   values:     *mut *mut c_char  (pointer to array of C string pointers)
///   values_len: usize             (number of strings)
///   error_msg:  *mut c_char       (null on success)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MqResultNative
{
    public IntPtr  Values;     // char**
    public UIntPtr ValuesLen;  // size_t
    public IntPtr  ErrorMsg;   // char*
}

/// <summary>
/// Native struct matching mq-ffi's MqConversionOptions (repr(C)).
/// Each bool is a single byte (Rust bool = 1 byte in C ABI).
/// With DisableRuntimeMarshalling, bool fields are marshalled as 1 byte.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MqConversionOptionsNative
{
    public bool ExtractScriptsAsCodeBlocks;
    public bool GenerateFrontMatter;
    public bool UseTitleAsH1;
}

internal static partial class NativeMethods
{
    // Library name — .NET resolves to mq_ffi.dll / libmq_ffi.so / libmq_ffi.dylib
    private const string LibName = "mq_ffi";

    /// <summary>Creates a new mq engine. Must be freed with mq_destroy.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_create")]
    internal static partial IntPtr MqCreate();

    /// <summary>Destroys an mq engine.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_destroy")]
    internal static partial void MqDestroy(IntPtr enginePtr);

    /// <summary>
    /// Evaluates a mq query. The returned MqResultNative MUST be freed with MqFreeResult.
    /// Strings are UTF-8 encoded.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "mq_eval",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial MqResultNative MqEval(
        IntPtr enginePtr,
        string code,
        string input,
        string inputFormat);

    /// <summary>Frees an MqResultNative returned by MqEval.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_free_result")]
    internal static partial void MqFreeResult(MqResultNative result);

    /// <summary>Frees a C string allocated by Rust.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_free_string")]
    internal static partial void MqFreeString(IntPtr str);

    /// <summary>
    /// Converts HTML to Markdown. Returns null on error and sets errorMsg.
    /// The returned string and errorMsg MUST be freed with MqFreeString.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "mq_html_to_markdown",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr MqHtmlToMarkdown(
        string html,
        MqConversionOptionsNative options,
        out IntPtr errorMsg);
}
