using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>Rookie contracts are slotted by draft position, not negotiated. See design doc s7.</summary>
public static class RookieWageScale
{
    private const int TopPickGold = 600;
    private const int GoldDeclinePerPick = 5;
    private const int MinimumGold = 60;

    public static Contract ForPick(int overallPickNumber)
    {
        var annual = Math.Max(MinimumGold, TopPickGold - (overallPickNumber - 1) * GoldDeclinePerPick);
        return new Contract(AnnualGold: annual, Years: 3, GuaranteedGold: annual, SigningBonus: 0);
    }
}
