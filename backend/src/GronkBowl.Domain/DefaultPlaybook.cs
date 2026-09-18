using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

/// <summary>
/// Every team starts with these five offensive and five defensive plays, installed at zero
/// cost against the weekly practice-rep cap and permanently public league-wide (no scouting
/// fog-of-war applies to them). See Master Design Doc s5.
/// </summary>
public static class DefaultPlaybook
{
    public static IReadOnlyList<Play> Offense { get; } = new List<Play>
    {
        new()
        {
            Name = "Inside Dive",
            Category = PlayCategory.Offense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.RB,
            Assignments =
            {
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.OL, AssignmentRole.Block),
            },
        },
        new()
        {
            Name = "Outside Zone",
            Category = PlayCategory.Offense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.RB,
            Assignments =
            {
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.TE, AssignmentRole.Block),
            },
        },
        new()
        {
            Name = "Slant-Flat",
            Category = PlayCategory.Offense,
            IsDefaultPlay = true,
            IsPassPlay = true,
            PrimaryPosition = FootballPosition.WR,
            Assignments =
            {
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.WR, AssignmentRole.Route),
                new(FootballPosition.RB, AssignmentRole.Route),
            },
        },
        new()
        {
            Name = "Screen Pass",
            Category = PlayCategory.Offense,
            IsDefaultPlay = true,
            IsPassPlay = true,
            PrimaryPosition = FootballPosition.RB,
            Assignments =
            {
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.RB, AssignmentRole.Route),
            },
        },
        new()
        {
            Name = "Four Verticals",
            Category = PlayCategory.Offense,
            IsDefaultPlay = true,
            IsPassPlay = true,
            PrimaryPosition = FootballPosition.WR,
            Assignments =
            {
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.OL, AssignmentRole.Block),
                new(FootballPosition.WR, AssignmentRole.Route),
                new(FootballPosition.WR, AssignmentRole.Route),
                new(FootballPosition.TE, AssignmentRole.Route),
            },
        },
    };

    public static IReadOnlyList<Play> Defense { get; } = new List<Play>
    {
        new()
        {
            Name = "Base Front",
            Category = PlayCategory.Defense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.LB,
            Assignments =
            {
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.LB, AssignmentRole.Rush),
            },
        },
        new()
        {
            Name = "Cover 1",
            Category = PlayCategory.Defense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.CB,
            Assignments =
            {
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.CB, AssignmentRole.CoverageMan),
                new(FootballPosition.CB, AssignmentRole.CoverageMan),
                new(FootballPosition.S, AssignmentRole.CoverageMan),
            },
        },
        new()
        {
            Name = "Cover 3",
            Category = PlayCategory.Defense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.S,
            Assignments =
            {
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.CB, AssignmentRole.CoverageZone),
                new(FootballPosition.CB, AssignmentRole.CoverageZone),
                new(FootballPosition.S, AssignmentRole.CoverageZone),
            },
        },
        new()
        {
            Name = "Blitz Package",
            Category = PlayCategory.Defense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.LB,
            Assignments =
            {
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.LB, AssignmentRole.Rush),
                new(FootballPosition.LB, AssignmentRole.Rush),
                new(FootballPosition.CB, AssignmentRole.CoverageMan),
            },
        },
        new()
        {
            Name = "Prevent",
            Category = PlayCategory.Defense,
            IsDefaultPlay = true,
            IsPassPlay = false,
            PrimaryPosition = FootballPosition.S,
            Assignments =
            {
                new(FootballPosition.DL, AssignmentRole.Rush),
                new(FootballPosition.CB, AssignmentRole.CoverageZone),
                new(FootballPosition.CB, AssignmentRole.CoverageZone),
                new(FootballPosition.S, AssignmentRole.CoverageZone),
                new(FootballPosition.S, AssignmentRole.CoverageZone),
            },
        },
    };
}
