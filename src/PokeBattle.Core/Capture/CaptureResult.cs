namespace PokeBattle.Core.Capture;

/// <summary>
/// Capture odds for one throw. <see cref="ChancePerBall"/> is the chance of each throw;
/// the BallsFor* values are how many throws reach that cumulative chance. They are different metrics.
/// </summary>
/// <param name="FinalCaptureRate">X in the Generation III formula (255 means guaranteed).</param>
/// <param name="ShakeThreshold">Y, compared against four 16-bit random numbers; null when the capture is guaranteed.</param>
public sealed record CaptureResult(
    int FinalCaptureRate,
    int? ShakeThreshold,
    double ChancePerBall,
    int BallsFor50Percent,
    int BallsFor90Percent,
    int BallsFor95Percent,
    int BallsFor99Percent);