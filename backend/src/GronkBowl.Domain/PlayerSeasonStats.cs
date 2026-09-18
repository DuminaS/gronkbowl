namespace GronkBowl.Domain;

/// <summary>
/// One row per player, per year, per team - a player traded mid-season gets a separate row
/// for each team, so a career readout is just "every row for this PlayerId, ordered by Year"
/// and it already shows exactly who they played for and when.
/// </summary>
public sealed record PlayerSeasonStats(Guid PlayerId, int Year, Guid TeamId, int GamesPlayed, PlayerGameStats Totals)
{
    public static PlayerSeasonStats StartingWith(Guid playerId, int year, Guid teamId, PlayerGameStats firstGame) =>
        new(playerId, year, teamId, GamesPlayed: 1, firstGame);

    public PlayerSeasonStats AddGame(PlayerGameStats game) =>
        this with { GamesPlayed = GamesPlayed + 1, Totals = Totals + game };
}
