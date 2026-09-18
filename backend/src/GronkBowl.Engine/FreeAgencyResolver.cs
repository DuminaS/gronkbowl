using GronkBowl.Domain;

namespace GronkBowl.Engine;

public sealed record FreeAgentOffer(Guid TeamId, Contract Contract);

/// <summary>
/// The highest offer usually wins, but not deterministically - a small preference roll keeps
/// one deep-pocketed team from mechanically out-bidding the market every single time. See
/// Master Design Doc s7.
/// </summary>
public static class FreeAgencyResolver
{
    private const int PreferenceNoiseRange = 50;

    public static FreeAgentOffer? ResolveSigning(IReadOnlyList<FreeAgentOffer> offers, Random rng)
    {
        if (offers.Count == 0)
        {
            return null;
        }

        return offers
            .Select(offer => (Offer: offer, Score: OfferValue(offer.Contract) + rng.Next(-PreferenceNoiseRange, PreferenceNoiseRange + 1)))
            .OrderByDescending(scored => scored.Score)
            .First()
            .Offer;
    }

    private static int OfferValue(Contract contract) => contract.AnnualGold + contract.SigningBonus;
}
