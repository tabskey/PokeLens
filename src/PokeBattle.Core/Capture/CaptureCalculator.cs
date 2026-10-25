namespace PokeBattle.Core.Capture;

/// <summary>
/// Generation III capture formula, following Cmd_handleballthrow in the FireRed decompilation
/// (pret/pokefirered, src/battle_script_commands.c). Integer arithmetic only, truncating at each
/// step in the game's order: ball multiplier, then HP fraction, then status.
/// </summary>
public static class CaptureCalculator
{
    /// <summary>X above this value means the Pokémon is caught without shake checks.</summary>
    public const int GuaranteedThreshold = 254;

    private const int GuaranteedRate = 255;

    public static int FinalCaptureRate(int catchRate, int maxHp, int currentHp, PokeBall ball, CaptureStatus status)
    {
        // 3 is the lowest Gen III catch rate; below it X can truncate to 0 and Y is undefined.
        ArgumentOutOfRangeException.ThrowIfLessThan(catchRate, 3);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(catchRate, 255);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHp, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(currentHp, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(currentHp, maxHp);

        // The game skips the formula entirely for the Master Ball.
        if (ball == PokeBall.MasterBall)
        {
            return GuaranteedRate;
        }

        var rate = catchRate * BallMultiplierTenths(ball) / 10;
        rate = rate * ((3 * maxHp) - (2 * currentHp)) / (3 * maxHp);

        return status switch
        {
            CaptureStatus.Sleep or CaptureStatus.Freeze => rate * 2,
            CaptureStatus.Paralysis or CaptureStatus.Poison or CaptureStatus.Burn => rate * 15 / 10,
            CaptureStatus.None => rate,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    /// <summary>
    /// Y = 1048560 / sqrt(sqrt(16711680 / X)), with the GBA BIOS integer square root. Returns null
    /// when X is above <see cref="GuaranteedThreshold"/> (no shake checks happen).
    /// </summary>
    public static int? ShakeThreshold(int finalCaptureRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(finalCaptureRate, 1);

        if (finalCaptureRate > GuaranteedThreshold)
        {
            return null;
        }

        return 1048560 / IntegerSqrt(IntegerSqrt(16711680 / finalCaptureRate));
    }

    public static CaptureResult Calculate(int catchRate, int maxHp, int currentHp, PokeBall ball, CaptureStatus status)
    {
        var finalCaptureRate = FinalCaptureRate(catchRate, maxHp, currentHp, ball, status);
        var shakeThreshold = ShakeThreshold(finalCaptureRate);
        var chancePerBall = shakeThreshold is { } y ? CaptureProbabilityCalculator.ChancePerBall(y) : 1.0;

        return new CaptureResult(
            finalCaptureRate,
            shakeThreshold,
            chancePerBall,
            CaptureProbabilityCalculator.BallsForConfidence(chancePerBall, 0.50),
            CaptureProbabilityCalculator.BallsForConfidence(chancePerBall, 0.90),
            CaptureProbabilityCalculator.BallsForConfidence(chancePerBall, 0.95),
            CaptureProbabilityCalculator.BallsForConfidence(chancePerBall, 0.99));
    }

    /// <summary>Floor of the square root, like the GBA BIOS Sqrt call.</summary>
    internal static int IntegerSqrt(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        var root = (int)Math.Sqrt(value);
        while ((long)root * root > value)
        {
            root--;
        }

        while ((long)(root + 1) * (root + 1) <= value)
        {
            root++;
        }

        return root;
    }

    private static int BallMultiplierTenths(PokeBall ball) => ball switch
    {
        PokeBall.PokeBall => 10,
        PokeBall.GreatBall => 15,
        PokeBall.UltraBall => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(ball), ball, null),
    };
}