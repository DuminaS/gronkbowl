namespace GronkBowl.Domain;

public class Team
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public Roster Roster { get; init; } = new();
    public List<Play> Playbook { get; init; } = new();
    public CallSheet? CurrentCallSheet { get; set; }

    public int Gold { get; set; }
    public int CapSpace { get; set; }

    // v1 facilities: Medical and Scouting only (see Master Design Doc s0.5).
    public int MedicalFacilityLevel { get; set; }
    public int ScoutingFacilityLevel { get; set; }
}
