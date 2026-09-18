namespace GronkBowl.Domain.Enums;

/// <summary>The buckets a Call Sheet organizes installed plays into, by game situation.</summary>
public enum SituationalBucket
{
    Standard,
    SecondAndShort,
    SecondAndLong,
    ThirdAndShort,
    ThirdAndLong,
    RedZone,
    GoalLine,
    TwoMinuteDrill,
    BackedUp
}
