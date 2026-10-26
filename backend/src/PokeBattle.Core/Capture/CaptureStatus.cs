namespace PokeBattle.Core.Capture;

/// <summary>Target status relevant to capture. Toxic counts as Poison, as in the game.</summary>
public enum CaptureStatus
{
    None,
    Sleep,
    Freeze,
    Paralysis,
    Poison,
    Burn,
}