using System.Collections;

namespace MQNet;

/// <summary>The result of an mq query evaluation.</summary>
public sealed class MqResult : IReadOnlyList<string>
{
    private readonly IReadOnlyList<string> _values;

    /// <summary>Creates a new MqResult with the specified values.</summary>
    public MqResult(IReadOnlyList<string> values)
    {
        _values = values;
    }

    /// <summary>All matched values.</summary>
    public IReadOnlyList<string> Values => _values;

    /// <summary>All values joined by a newline character.</summary>
    public string Text => _values.Count == 0 ? "" : string.Join('\n', _values);

    /// <inheritdoc />
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= _values.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _values[index];
        }
    }

    /// <inheritdoc />
    public int Count => _values.Count;

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
