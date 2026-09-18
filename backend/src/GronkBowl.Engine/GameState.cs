namespace GronkBowl.Engine;

/// <summary>
/// Plays-remaining stands in for a real game clock in v1 - simple, readable, and enough to
/// terminate a game and drive the TwoMinuteDrill situational bucket without simulating seconds.
/// </summary>
public sealed class GameState
{
    public required Guid PossessionTeamId { get; set; }
    public required Guid DefendingTeamId { get; set; }

    public int Quarter { get; set; } = 1;
    public int PlaysRemainingInQuarter { get; set; } = 12;

    public int Down { get; set; } = 1;
    public int DistanceToGo { get; set; } = 10;

    /// <summary>0 = the possessing team's own goal line, 100 = the opponent's goal line.</summary>
    public int FieldPosition { get; set; } = 25;

    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public bool IsGameOver => Quarter > 4;

    public void FlipPossession(int newFieldPosition)
    {
        (PossessionTeamId, DefendingTeamId) = (DefendingTeamId, PossessionTeamId);
        Down = 1;
        DistanceToGo = 10;
        FieldPosition = Math.Clamp(newFieldPosition, 1, 99);
    }

    public void AdvanceClock()
    {
        PlaysRemainingInQuarter--;
        if (PlaysRemainingInQuarter <= 0 && !IsGameOver)
        {
            Quarter++;
            PlaysRemainingInQuarter = 12;
        }
    }
}
