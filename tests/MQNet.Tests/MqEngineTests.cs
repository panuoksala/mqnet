using MQNet;

namespace MQNet.Tests;

public class MqEngineTests
{
    // --- Lifecycle ---

    [Fact]
    public void Constructor_CreatesEngineWithoutError()
    {
        using var engine = new MqEngine();
        Assert.NotNull(engine);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var engine = new MqEngine();
        engine.Dispose();
        engine.Dispose(); // must not throw
    }

    [Fact]
    public void Eval_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.Eval(".h1", "# Hello"));
    }

    // --- Query evaluation ---

    [Fact]
    public void Eval_ExtractsH1Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "# Hello\n\n## World\n\n# Another");
        Assert.Equal(["# Hello", "# Another"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsH2Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(2)", "# Title\n\n## Section A\n\n## Section B");
        Assert.Equal(["## Section A", "## Section B"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsCodeBlocks()
    {
        using var engine = new MqEngine();
        var markdown = "# Title\n\n```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code", markdown);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Eval_FiltersByCodeLanguage()
    {
        using var engine = new MqEngine();
        var markdown = "```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code(\"rust\")", markdown);
        Assert.Single(result);
        Assert.Contains("rust", result[0]);
    }

    [Fact]
    public void Eval_PipeQuery_SelectContains()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h | select(contains(\"Foo\"))", "# Foo\n\n## Bar\n\n## FooBar");
        Assert.Equal(2, result.Count);
        Assert.All(result.Values, v => Assert.Contains("Foo", v));
    }

    [Fact]
    public void Eval_EmptyInput_ReturnsEmptyResult()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h1", "");
        Assert.Empty(result);
    }

    [Fact]
    public void Eval_InvalidQuery_ThrowsMqException()
    {
        using var engine = new MqEngine();
        var ex = Assert.Throws<MqException>(() => engine.Eval(".!!invalid!!", "# Hello"));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Eval_DefaultFormat_IsMarkdown()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>");
        Assert.Empty(result);
    }

    [Fact]
    public void Eval_HtmlFormat_ConvertsBeforeQuerying()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>", InputFormat.Html);
        Assert.Single(result);
        Assert.Contains("Hello", result[0]);
    }

    [Fact]
    public void Eval_TextFormat_SplitsByLines()
    {
        using var engine = new MqEngine();
        var result = engine.Eval("select(contains(\"B\"))", "Line A\nLine B\nLine C",
            InputFormat.Text);
        Assert.Single(result);
        Assert.Equal("Line B", result[0]);
    }

    // --- to_text (plain text output) ---

    [Fact]
    public void Eval_ToText_RemovesHeadingMarkers()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h | to_text", "# Hello\n\n## World\n\n# Another");
        Assert.Equal(["Hello", "World", "Another"], result.Values);
    }

    [Fact]
    public void Eval_ToText_RemovesBoldFormatting()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".text | to_text", "**bold text**");
        Assert.Single(result);
        Assert.Equal("bold text", result[0]);
    }

    [Fact]
    public void Eval_ToText_H1Only_ReturnsPlainHeadingText()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1) | to_text", "# Hello **World**\n\n## Section");
        Assert.Single(result);
        Assert.Equal("Hello World", result[0]);
    }

    // --- HTML conversion ---

    [Fact]
    public void HtmlToMarkdown_BasicConversion()
    {
        var markdown = MqEngine.HtmlToMarkdown("<h1>Hello</h1><p>World</p>");
        Assert.Contains("# Hello", markdown);
        Assert.Contains("World", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_WithNullOptions_UsesDefaults()
    {
        var markdown = MqEngine.HtmlToMarkdown("<p>Simple</p>");
        Assert.Contains("Simple", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_UseTitleAsH1_IncludesTitle()
    {
        var html = "<html><head><title>My Title</title></head><body><p>Body</p></body></html>";
        var markdown = MqEngine.HtmlToMarkdown(html, new ConversionOptions { UseTitleAsH1 = true });
        Assert.Contains("# My Title", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_EmptyHtml_ReturnsEmptyOrWhitespace()
    {
        var markdown = MqEngine.HtmlToMarkdown("");
        Assert.NotNull(markdown);
    }

    // ── Engine configuration (v0.6.x) ────────────────────────────────────────

    [Fact]
    public void SetOptimizationLevel_None_DoesNotAffectEval()
    {
        using var engine = new MqEngine();
        engine.SetOptimizationLevel(MqOptimizationLevel.None);
        var result = engine.Eval(".h(1)", "# Hello");
        Assert.Single(result);
    }

    [Fact]
    public void SetOptimizationLevel_Basic_DoesNotAffectEval()
    {
        using var engine = new MqEngine();
        engine.SetOptimizationLevel(MqOptimizationLevel.Basic);
        var result = engine.Eval(".h(1)", "# Hello");
        Assert.Single(result);
    }

    [Fact]
    public void SetOptimizationLevel_Full_DoesNotAffectEval()
    {
        using var engine = new MqEngine();
        engine.SetOptimizationLevel(MqOptimizationLevel.Full);
        var result = engine.Eval(".h(1)", "# Hello");
        Assert.Single(result);
    }

    [Fact]
    public void SetOptimizationLevel_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.SetOptimizationLevel(MqOptimizationLevel.None));
    }

    [Fact]
    public void SetMaxCallStackDepth_LowLimit_CausesErrorOnDeepRecursion()
    {
        using var engine = new MqEngine();
        engine.SetMaxCallStackDepth(2);
        var ex = Assert.Throws<MqException>(() =>
            engine.Eval("def rec(): rec(); rec()", "test", InputFormat.Text));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void SetMaxCallStackDepth_HighLimit_AllowsNormalEval()
    {
        using var engine = new MqEngine();
        engine.SetMaxCallStackDepth(1000);
        // A simple non-recursive query must still work after adjusting the limit.
        var result = engine.Eval(".h(1)", "# Hello");
        Assert.Single(result);
    }

    [Fact]
    public void SetMaxCallStackDepth_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.SetMaxCallStackDepth(100));
    }

    [Fact]
    public void SetSearchPaths_EmptyList_DoesNotThrow()
    {
        using var engine = new MqEngine();
        engine.SetSearchPaths([]); // should be a no-op
    }

    [Fact]
    public void SetSearchPaths_NullList_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.SetSearchPaths(null!));
    }

    [Fact]
    public void SetSearchPaths_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.SetSearchPaths([]));
    }

    [Fact]
    public void DefineStringValue_InjectedVariable_AccessibleInQuery()
    {
        using var engine = new MqEngine();
        engine.DefineStringValue("greeting", "hello");
        var result = engine.Eval("greeting", "ignored", InputFormat.Text);
        Assert.Single(result);
        Assert.Equal("hello", result[0]);
    }

    [Fact]
    public void DefineStringValue_OverwritesPreviousValue()
    {
        using var engine = new MqEngine();
        engine.DefineStringValue("v", "first");
        engine.DefineStringValue("v", "second");
        var result = engine.Eval("v", "ignored", InputFormat.Text);
        Assert.Single(result);
        Assert.Equal("second", result[0]);
    }

    [Fact]
    public void DefineStringValue_NullName_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.DefineStringValue(null!, "value"));
    }

    [Fact]
    public void DefineStringValue_NullValue_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.DefineStringValue("name", null!));
    }

    [Fact]
    public void DefineStringValue_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.DefineStringValue("x", "y"));
    }

    [Fact]
    public void ImportModule_NonExistentModule_ThrowsMqException()
    {
        using var engine = new MqEngine();
        var ex = Assert.Throws<MqException>(() =>
            engine.ImportModule("definitely_nonexistent_module_12345"));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void ImportModule_NullName_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.ImportModule(null!));
    }

    [Fact]
    public void ImportModule_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.ImportModule("mod"));
    }

    [Fact]
    public void LoadModule_NonExistentModule_ThrowsMqException()
    {
        using var engine = new MqEngine();
        var ex = Assert.Throws<MqException>(() =>
            engine.LoadModule("definitely_nonexistent_module_12345"));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void LoadModule_NullName_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.LoadModule(null!));
    }

    [Fact]
    public void LoadModule_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.LoadModule("mod"));
    }

    [Fact]
    public void SetHttpAllowedDomains_EmptyList_DoesNotThrow()
    {
        using var engine = new MqEngine();
        engine.SetHttpAllowedDomains([]);
    }

    [Fact]
    public void SetHttpAllowedDomains_NullList_ThrowsArgumentNullException()
    {
        using var engine = new MqEngine();
        Assert.Throws<ArgumentNullException>(() => engine.SetHttpAllowedDomains(null!));
    }

    [Fact]
    public void SetHttpAllowedDomains_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.SetHttpAllowedDomains([]));
    }

    [Fact]
    public void ClearHttpCache_DoesNotThrowUnexpectedException()
    {
        // Either succeeds or throws MqException (when http-import feature is absent) —
        // both are valid. Any other exception type would be a bug.
        using var engine = new MqEngine();
        try { engine.ClearHttpCache(); }
        catch (MqException) { /* expected when http-import feature is not compiled in */ }
    }

    [Fact]
    public void ClearHttpCacheAll_DoesNotThrowUnexpectedException()
    {
        using var engine = new MqEngine();
        try { engine.ClearHttpCacheAll(); }
        catch (MqException) { /* expected when http-import feature is not compiled in */ }
    }

    [Fact]
    public void ClearHttpCache_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.ClearHttpCache());
    }

    [Fact]
    public void ClearHttpCacheAll_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.ClearHttpCacheAll());
    }

    // ── Module loading from file system (v0.6.x) ─────────────────────────────

    [Fact]
    public void LoadModule_FromSearchPath_ExposesDefinedFunction()
    {
        var tmpDir  = Path.GetTempPath();
        var modFile = Path.Combine(tmpDir, "mqnet_test_load_module.mq");
        File.WriteAllText(modFile, "def double(x): x * 2;");

        try
        {
            using var engine = new MqEngine();
            engine.SetSearchPaths([tmpDir]);
            engine.LoadModule("mqnet_test_load_module");

            // After LoadModule the function is in global scope — no namespace prefix.
            var result = engine.Eval("double(3)", "ignored", InputFormat.Text);
            Assert.Single(result);
            Assert.Equal("6", result[0]);
        }
        finally
        {
            File.Delete(modFile);
        }
    }

    [Fact]
    public void ImportModule_FromSearchPath_ExposesNamespacedFunction()
    {
        var tmpDir  = Path.GetTempPath();
        var modFile = Path.Combine(tmpDir, "mqnet_test_import_module.mq");
        File.WriteAllText(modFile, "def triple(x): x * 3;");

        try
        {
            using var engine = new MqEngine();
            engine.SetSearchPaths([tmpDir]);
            engine.ImportModule("mqnet_test_import_module");

            // After ImportModule the function is namespaced.
            var result = engine.Eval("mqnet_test_import_module::triple(2)", "ignored", InputFormat.Text);
            Assert.Single(result);
            Assert.Equal("6", result[0]);
        }
        finally
        {
            File.Delete(modFile);
        }
    }
}
