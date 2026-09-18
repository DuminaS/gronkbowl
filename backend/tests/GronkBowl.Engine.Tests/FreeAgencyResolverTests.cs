using GronkBowl.Domain;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class FreeAgencyResolverTests
{
    [Fact]
    public void NoOffers_ResolvesToNull()
    {
        Assert.Null(FreeAgencyResolver.ResolveSigning(Array.Empty<FreeAgentOffer>(), new Random(1)));
    }

    [Fact]
    public void OneOffer_AlwaysWins()
    {
        var onlyOffer = new FreeAgentOffer(Guid.NewGuid(), new Contract(100, 3, 100, 0));
        var result = FreeAgencyResolver.ResolveSigning(new[] { onlyOffer }, new Random(1));

        Assert.Equal(onlyOffer, result);
    }

    [Fact]
    public void ARichEnoughOffer_UsuallyBeatsATinyOffer_AcrossManySeeds()
    {
        var bigSpender = Guid.NewGuid();
        var bigOffer = new FreeAgentOffer(bigSpender, new Contract(1000, 3, 1000, 0));
        var tinyOffer = new FreeAgentOffer(Guid.NewGuid(), new Contract(10, 1, 10, 0));

        var bigOfferWins = Enumerable.Range(0, 100)
            .Count(seed => FreeAgencyResolver.ResolveSigning(new[] { bigOffer, tinyOffer }, new Random(seed))?.TeamId == bigSpender);

        Assert.True(bigOfferWins > 90, $"Expected the much larger offer to win almost always, won {bigOfferWins}/100");
    }

    [Fact]
    public void CloseOffers_DoNotAlwaysGoToTheHighestBidder()
    {
        var slightlyBetter = new FreeAgentOffer(Guid.NewGuid(), new Contract(210, 3, 210, 0));
        var slightlyWorse = new FreeAgentOffer(Guid.NewGuid(), new Contract(200, 3, 200, 0));

        var winners = Enumerable.Range(0, 100)
            .Select(seed => FreeAgencyResolver.ResolveSigning(new[] { slightlyBetter, slightlyWorse }, new Random(seed))?.TeamId)
            .Distinct()
            .Count();

        Assert.True(winners > 1, "A close market should let the lower offer win sometimes.");
    }
}
