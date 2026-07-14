using MQNet;

namespace MQNet.Tests;

public class MqVersionTests
{
    [Fact]
    public void Version_IsNotNullOrEmpty()
    {
        Assert.NotNull(MqEngine.Version);
        Assert.NotEmpty(MqEngine.Version);
    }

    [Fact]
    public void Version_StartsWithDigit()
    {
        // Version strings from mq-ffi look like "0.6.5"
        Assert.True(char.IsDigit(MqEngine.Version[0]),
            $"Expected version to start with a digit, got: {MqEngine.Version}");
    }

    [Fact]
    public void MqStaticVersion_MatchesMqEngineVersion()
    {
        Assert.Equal(MqEngine.Version, Mq.Version);
    }

    [Fact]
    public void Version_IsStableAcrossMultipleCalls()
    {
        var first  = MqEngine.Version;
        var second = MqEngine.Version;
        Assert.Equal(first, second);
    }
}
