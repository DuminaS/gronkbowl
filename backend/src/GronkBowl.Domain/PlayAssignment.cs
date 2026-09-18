using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public sealed record PlayAssignment(FootballPosition Slot, AssignmentRole Role);
