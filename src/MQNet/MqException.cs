namespace MQNet;

/// <summary>Thrown when the mq native engine reports an error.</summary>
public sealed class MqException : Exception
{
    /// <inheritdoc />
    public MqException(string message) : base(message) { }

    /// <inheritdoc />
    public MqException(string message, Exception innerException)
        : base(message, innerException) { }
}
