namespace MQNet;

/// <summary>
/// Controls the AST optimizer that runs before each mq query evaluation.
/// Higher levels trade compilation time for faster evaluation of complex queries.
/// </summary>
public enum MqOptimizationLevel
{
    /// <summary>No AST optimization (fastest compilation, slowest evaluation for complex queries).</summary>
    None = 0,

    /// <summary>Basic constant-folding and dead-code elimination.</summary>
    Basic = 1,

    /// <summary>Full multi-pass optimization pipeline.</summary>
    Full = 2,
}
