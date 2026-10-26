using PokeBattle.Core.Capture;

namespace PokeBattle.Core.Tests.Capture;

public class CaptureProbabilityCalculatorTests
{
    // n = ceil(ln(1 - q) / ln(1 - p)) with p = 0.19752, ln(1 - p) = -0.220052:
    // 50%: 0.693147/0.220052 = 3.15 -> 4;  90%: 2.302585/0.220052 = 10.46 -> 11
    // 95%: 2.995732/0.220052 = 13.61 -> 14; 99%: 4.605170/0.220052 = 20.93 -> 21
    [Theory]
    [InlineData(0.50, 4)]
    [InlineData(0.90, 11)]
    [InlineData(0.95, 14)]
    [InlineData(0.99, 21)]
    public void BallsForConfidence_MatchesClosedForm(double confidence, int expected)
    {
        Assert.Equal(expected, CaptureProbabilityCalculator.BallsForConfidence(0.19752, confidence));
    }

    // p = 0.5, q = 0.75: exactly 2 balls (1 - 0.5^2 = 0.75), with no floating-point extra ball.
    [Fact]
    public void BallsForConfidence_ExactBoundary_DoesNotAddABall()
    {
        Assert.Equal(2, CaptureProbabilityCalculator.BallsForConfidence(0.5, 0.75));
    }

    [Fact]
    public void BallsForConfidence_CertainCapture_IsOneBall()
    {
        Assert.Equal(1, CaptureProbabilityCalculator.BallsForConfidence(1.0, 0.99));
    }

    [Fact]
    public void BallsForConfidence_HighChance_IsAtLeastOneBall()
    {
        Assert.Equal(1, CaptureProbabilityCalculator.BallsForConfidence(0.9, 0.5));
    }

    // 1 - (1 - 0.19752)^14 = 1 - 0.80248^14 = 0.95434...
    [Fact]
    public void CumulativeChance_IsOneMinusFailureToThePowerN()
    {
        Assert.Equal(1 - Math.Pow(0.80248, 14), CaptureProbabilityCalculator.CumulativeChance(0.19752, 14), precision: 12);
        Assert.Equal(0, CaptureProbabilityCalculator.CumulativeChance(0.19752, 0));
    }

    // Y = 32768 -> (1/2)^4 = 0.0625; Y = 0 -> 0.
    [Fact]
    public void ChancePerBall_IsShakeRatioToTheFourth()
    {
        Assert.Equal(0.0625, CaptureProbabilityCalculator.ChancePerBall(32768));
        Assert.Equal(0, CaptureProbabilityCalculator.ChancePerBall(0));
    }

    [Fact]
    public void InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.ChancePerBall(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.ChancePerBall(65536));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.CumulativeChance(0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.CumulativeChance(1.5, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.CumulativeChance(0.5, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.BallsForConfidence(0.5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureProbabilityCalculator.BallsForConfidence(0.5, 1));
    }
}