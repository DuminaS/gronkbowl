namespace GronkBowl.Domain;

public sealed record Contract(int AnnualGold, int Years, int GuaranteedGold, int SigningBonus)
{
    public int DeadCapIfReleasedNow => GuaranteedGold;
}
