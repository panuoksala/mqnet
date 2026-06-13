namespace MQNet;

/// <summary>Entry point for the fluent mq query API.</summary>
public static class Mq
{
    /// <summary>Starts a fluent query chain.</summary>
    /// <param name="query">The mq query string (e.g. ".h1", ".code(\"rust\")").</param>
    public static MqQueryBuilder Query(string query) => new(query);
}

/// <summary>
/// Fluent builder for mq queries. Obtain via <see cref="Mq.Query"/>.
/// For bulk queries over the same content, use <see cref="MqEngine"/> directly.
/// </summary>
public sealed class MqQueryBuilder
{
    private readonly string _query;
    private string? _input;
    private InputFormat _format = InputFormat.Markdown;
    private bool _plainText;

    internal MqQueryBuilder(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
    }

    /// <summary>Sets the input content to query.</summary>
    public MqQueryBuilder On(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;
        return this;
    }

    /// <summary>Sets the input format (default: <see cref="InputFormat.Markdown"/>).</summary>
    public MqQueryBuilder WithFormat(InputFormat format)
    {
        _format = format;
        return this;
    }

    /// <summary>
    /// Appends <c>| to_text</c> to the query so that results are returned as plain text
    /// with all Markdown formatting removed. This delegates to the native mq <c>to_text()</c>
    /// built-in, which operates on the parsed AST rather than the raw string.
    /// </summary>
    public MqQueryBuilder WithPlainText()
    {
        _plainText = true;
        return this;
    }

    /// <summary>Executes the query and returns the result.</summary>
    /// <exception cref="InvalidOperationException">If <see cref="On"/> was not called.</exception>
    /// <exception cref="MqException">If the query fails.</exception>
    public MqResult Run()
    {
        if (_input is null)
            throw new InvalidOperationException("Call On(input) before Run().");

        var query = _plainText ? $"{_query} | to_text" : _query;
        using var engine = new MqEngine();
        return engine.Eval(query, _input, _format);
    }
}
