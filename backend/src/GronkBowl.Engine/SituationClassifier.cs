using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

public static class SituationClassifier
{
    public static SituationalBucket Classify(GameState state)
    {
        if (state.FieldPosition >= 95) return SituationalBucket.GoalLine;
        if (state.FieldPosition >= 80) return SituationalBucket.RedZone;
        if (state.FieldPosition <= 10) return SituationalBucket.BackedUp;

        var inTwoMinuteWindow = state.PlaysRemainingInQuarter <= 4 && (state.Quarter == 2 || state.Quarter == 4);
        if (inTwoMinuteWindow) return SituationalBucket.TwoMinuteDrill;

        return state.Down switch
        {
            2 when state.DistanceToGo <= 3 => SituationalBucket.SecondAndShort,
            2 => SituationalBucket.SecondAndLong,
            3 when state.DistanceToGo <= 3 => SituationalBucket.ThirdAndShort,
            3 or 4 => SituationalBucket.ThirdAndLong,
            _ => SituationalBucket.Standard,
        };
    }
}
