namespace GronkBowl.Domain;

public class League
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<Team> Franchises { get; init; } = new();
    public List<Season> SeasonHistory { get; init; } = new();

    // Versioned so an in-progress season always resolves under the rules it started with,
    // even if a later season's ruleset changes. End-of-Season Vote is post-v1.
    public string CurrentRulesVersion { get; set; } = "v1";
}
