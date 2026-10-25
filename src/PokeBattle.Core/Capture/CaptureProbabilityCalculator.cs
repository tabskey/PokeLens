namespace PokeBattle.Core.Capture;

/// <summary>
/// Probabilities derived from the shake threshold. These are calculated probabilities, not
/// estimates: the game's four checks are independent 16-bit random numbers compared against Y.
/// </summary>
public static class CaptureProbabilityCalculator
{
    private const int ShakeChecks = 4;
    private const double RandomRange = 65536;

    /// <summary>(Y / 65536)^4: chance that all four shake checks pass.</summary>
    public static double ChancePerBall(int shakeThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(shakeThreshold);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(shakeThreshold, 65535);

        return Math.Pow(shakeThreshold / RandomRange, ShakeChecks);
    }

    /// <summary>1 − (1 − p)^n: chance of at least one success in n throws.</summary>
    public static double CumulativeChance(double chancePerBall, int balls)
    {
        ValidateChance(chancePerBall);
        ArgumentOutOfRangeException.ThrowIfLessThan(balls, 0);

        return 1 - Math.Pow(1 - chancePerBall, balls);
    }

    /// <summary>
    /// Smallest n with 1 − (1 − p)^n ≥ q. Starts from ceil(ln(1 − q) / ln(1 − p)) and corrects by
    /// checking the cumulative chance, so floating-point noise never adds or removes a ball.
    /// </summary>
    public static int BallsForConfidence(double chancePerBall, double confidence)
    {
        ValidateChance(chancePerBall);
        if (confidence is <= 0 or >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(confidence), confidence, "Confidence must be between 0 and 1 (exclusive).");
        }

        if (chancePerBall >= 1)
        {
            return 1;
        }

        var balls = Math.Max(1, (int)Math.Ceiling(Math.Log(1 - confidence) / Math.Log(1 - chancePerBall)));
        while (balls > 1 && CumulativeChance(chancePerBall, balls - 1) >= confidence)
        {
            balls--;
        }

        while (CumulativeChance(chancePerBall, balls) < confidence)
        {
            balls++;
        }

        return balls;
    }

    private static void ValidateChance(double chancePerBall)
    {
        if (chancePerBall is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chancePerBall), chancePerBall, "Chance per ball must be in (0, 1].");
        }
    }
}