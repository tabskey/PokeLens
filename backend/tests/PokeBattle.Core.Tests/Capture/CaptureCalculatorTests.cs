using PokeBattle.Core.Capture;

namespace PokeBattle.Core.Tests.Capture;

// Reference: Cmd_handleballthrow in pret/pokefirered (src/battle_script_commands.c):
//   X = catchRate * ball / 10;  X = X * (3M - 2H) / (3M);  sleep/freeze X*2, poison/burn/paralysis X*15/10
//   X > 254 -> caught;  else Y = 1048560 / Sqrt(Sqrt(16711680 / X)), Sqrt = GBA BIOS integer square root.
// Catch rates (Gen III): Bulbasaur 45, Pikachu 190, Snorlax 25, Mewtwo 3.
// Each case was also checked with an independent Python implementation of the same source.
public class CaptureCalculatorTests
{
    // Bulbasaur, Poké Ball, full HP (30/30), no status:
    // X = 45*10/10 = 45; 45*(90-60)/90 = 15.  16711680/15 = 1114112; sqrt = 1055; sqrt = 32; Y = 1048560/32 = 32767.
    // p = (32767/65536)^4 = 0.062492; balls 11 / 36 / 47 / 72.
    [Fact]
    public void Calculate_BulbasaurFullHpPokeBall()
    {
        var result = CaptureCalculator.Calculate(45, 30, 30, PokeBall.PokeBall, CaptureStatus.None);

        Assert.Equal(15, result.FinalCaptureRate);
        Assert.Equal(32767, result.ShakeThreshold);
        Assert.Equal(0.062492, result.ChancePerBall, precision: 6);
        Assert.Equal((11, 36, 47, 72), (result.BallsFor50Percent, result.BallsFor90Percent, result.BallsFor95Percent, result.BallsFor99Percent));
    }

    // Pikachu, Great Ball, 30/35 HP, paralysed:
    // X = 190*15/10 = 285; 285*(105-60)/105 = 122; paralysis 122*15/10 = 183.
    // 16711680/183 = 91320; sqrt = 302; sqrt = 17; Y = 1048560/17 = 61680. p = 0.784617; balls 1 / 2 / 2 / 3.
    [Fact]
    public void Calculate_PikachuParalysedGreatBall()
    {
        var result = CaptureCalculator.Calculate(190, 35, 30, PokeBall.GreatBall, CaptureStatus.Paralysis);

        Assert.Equal(183, result.FinalCaptureRate);
        Assert.Equal(61680, result.ShakeThreshold);
        Assert.Equal(0.784617, result.ChancePerBall, precision: 6);
        Assert.Equal((1, 2, 2, 3), (result.BallsFor50Percent, result.BallsFor90Percent, result.BallsFor95Percent, result.BallsFor99Percent));
    }

    // Snorlax, Ultra Ball, 95/190 HP, asleep:
    // X = 25*20/10 = 50; 50*(570-190)/570 = 33; sleep 66.
    // 16711680/66 = 253207; sqrt = 503; sqrt = 22; Y = 47661. p = 0.279726; balls 3 / 8 / 10 / 15.
    [Fact]
    public void Calculate_SnorlaxAsleepUltraBall()
    {
        var result = CaptureCalculator.Calculate(25, 190, 95, PokeBall.UltraBall, CaptureStatus.Sleep);

        Assert.Equal(66, result.FinalCaptureRate);
        Assert.Equal(47661, result.ShakeThreshold);
        Assert.Equal(0.279726, result.ChancePerBall, precision: 6);
        Assert.Equal((3, 8, 10, 15), (result.BallsFor50Percent, result.BallsFor90Percent, result.BallsFor95Percent, result.BallsFor99Percent));
    }

    // Mewtwo, Ultra Ball, 1/250 HP, no status:
    // X = 3*20/10 = 6; 6*(750-2)/750 = 5.  16711680/5 = 3342336; sqrt = 1828; sqrt = 42; Y = 24965.
    // p = 0.021057; balls 33 / 109 / 141 / 217.
    [Fact]
    public void Calculate_MewtwoOneHpUltraBall()
    {
        var result = CaptureCalculator.Calculate(3, 250, 1, PokeBall.UltraBall, CaptureStatus.None);

        Assert.Equal(5, result.FinalCaptureRate);
        Assert.Equal(24965, result.ShakeThreshold);
        Assert.Equal(0.021057, result.ChancePerBall, precision: 6);
        Assert.Equal((33, 109, 141, 217), (result.BallsFor50Percent, result.BallsFor90Percent, result.BallsFor95Percent, result.BallsFor99Percent));
    }

    // Truncation order matters. Catch rate 45, Great Ball, 1/10 HP:
    // game: 45*15/10 = 67 (67.5 truncated); 67*(30-2)/30 = 62.
    // A single-expression formula ((3M-2H) * C * 1.5 / 3M = 28*67.5/30 = 63) would give 63.
    [Fact]
    public void FinalCaptureRate_TruncatesBallMultiplierBeforeHpFraction()
    {
        Assert.Equal(62, CaptureCalculator.FinalCaptureRate(45, 10, 1, PokeBall.GreatBall, CaptureStatus.None));
    }

    // Catch rate 45, Poké Ball, full HP 30/30: X before status = 15.
    [Theory]
    [InlineData(CaptureStatus.None, 15)]
    [InlineData(CaptureStatus.Sleep, 30)]
    [InlineData(CaptureStatus.Freeze, 30)]
    [InlineData(CaptureStatus.Paralysis, 22)]
    [InlineData(CaptureStatus.Poison, 22)]
    [InlineData(CaptureStatus.Burn, 22)]
    public void FinalCaptureRate_AppliesStatusAfterHpFraction(CaptureStatus status, int expected)
    {
        Assert.Equal(expected, CaptureCalculator.FinalCaptureRate(45, 30, 30, PokeBall.PokeBall, status));
    }

    // The illustrative example in docs/arquitetura.md §23: Pikachu at 7/35 HP, Ultra Ball, asleep.
    // X = 190*20/10 = 380; 380*(105-14)/105 = 329; sleep 658 > 254 -> guaranteed (no shake checks).
    [Fact]
    public void Calculate_AboveThreshold_IsGuaranteed()
    {
        var result = CaptureCalculator.Calculate(190, 35, 7, PokeBall.UltraBall, CaptureStatus.Sleep);

        Assert.Equal(658, result.FinalCaptureRate);
        Assert.Null(result.ShakeThreshold);
        Assert.Equal(1.0, result.ChancePerBall);
        Assert.Equal((1, 1, 1, 1), (result.BallsFor50Percent, result.BallsFor90Percent, result.BallsFor95Percent, result.BallsFor99Percent));
    }

    [Fact]
    public void Calculate_MasterBall_AlwaysCatches()
    {
        var result = CaptureCalculator.Calculate(3, 250, 250, PokeBall.MasterBall, CaptureStatus.None);

        Assert.Equal(255, result.FinalCaptureRate);
        Assert.Null(result.ShakeThreshold);
        Assert.Equal(1.0, result.ChancePerBall);
    }

    // X = 254 is the highest value that still rolls shake checks:
    // 16711680/254 = 65794; sqrt = 256; sqrt = 16; Y = 65535 (just below 65536).
    [Fact]
    public void ShakeThreshold_AtTheLimit()
    {
        Assert.Equal(65535, CaptureCalculator.ShakeThreshold(254));
        Assert.Null(CaptureCalculator.ShakeThreshold(255));
    }

    // X = 1: 16711680; sqrt = 4087; sqrt = 63; Y = 16643.
    [Fact]
    public void ShakeThreshold_LowestRate()
    {
        Assert.Equal(16643, CaptureCalculator.ShakeThreshold(1));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(15, 3)]
    [InlineData(16, 4)]
    [InlineData(17, 4)]
    [InlineData(1114112, 1055)]
    [InlineData(16711680, 4087)]
    [InlineData(int.MaxValue, 46340)]
    public void IntegerSqrt_IsTruncated(int value, int expected)
    {
        Assert.Equal(expected, CaptureCalculator.IntegerSqrt(value));
    }

    [Theory]
    [InlineData(2, 30, 30)]
    [InlineData(256, 30, 30)]
    [InlineData(45, 0, 0)]
    [InlineData(45, 30, 0)]
    [InlineData(45, 30, 31)]
    public void FinalCaptureRate_InvalidInput_Throws(int catchRate, int maxHp, int currentHp)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CaptureCalculator.FinalCaptureRate(catchRate, maxHp, currentHp, PokeBall.PokeBall, CaptureStatus.None));
    }

    [Fact]
    public void InvalidEnumsAndRates_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureCalculator.FinalCaptureRate(45, 30, 30, (PokeBall)99, CaptureStatus.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureCalculator.FinalCaptureRate(45, 30, 30, PokeBall.PokeBall, (CaptureStatus)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureCalculator.ShakeThreshold(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CaptureCalculator.IntegerSqrt(-1));
    }
}