using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/players/{playerId:guid}/levelup")]
public class PlayerProgressionController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public PlayerProgressionController(GronkBowlDbContext db) => _db = db;

    /// <summary>
    /// Rolls 2d6 and reports the outcome plus the eligible skill choices, without applying
    /// anything yet - the coach picks a specific skill or stat next, the same way a draft pick
    /// or a trade is a deliberate choice, not something the engine decides for you.
    /// </summary>
    [HttpPost("roll")]
    public async Task<ActionResult<LevelUpRollDto>> Roll(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player is null)
        {
            return NotFound();
        }

        if (!PlayerProgression.IsEligibleToLevelUp(player))
        {
            return BadRequest("This player is not eligible to level up (not enough SPP, or already at their potential's ceiling).");
        }

        var roll = LevelUpResolver.Roll(new Random());

        return new LevelUpRollDto(
            roll.Die1,
            roll.Die2,
            roll.IsDoubles,
            roll.BaseOutcome.ToString(),
            SkillAccess.AvailablePrimarySkills(player).Select(s => s.ToString()).ToList(),
            roll.CanTakeSecondarySkillInstead ? SkillAccess.AvailableSecondarySkills(player).Select(s => s.ToString()).ToList() : new List<string>());
    }

    /// <summary>
    /// Applies a roll's result. The outcome type is recomputed from the two dice server-side
    /// (a pure function of them, so nothing about the roll itself is trusted from the client) -
    /// only which specific skill or stat the coach picked is a real choice.
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<PlayerDto>> Apply(Guid playerId, [FromBody] ApplyLevelUpRequest request)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player is null)
        {
            return NotFound();
        }

        if (!PlayerProgression.IsEligibleToLevelUp(player))
        {
            return BadRequest("This player is not eligible to level up.");
        }

        var sum = request.Die1 + request.Die2;
        var isDoubles = request.Die1 == request.Die2;
        var baseOutcome = sum switch
        {
            10 => LevelUpOutcomeKind.SpeedOrArmorBoost,
            11 => LevelUpOutcomeKind.AgilityBoost,
            12 => LevelUpOutcomeKind.StrengthBoost,
            _ => LevelUpOutcomeKind.NewSkill,
        };

        if (request.UseSecondarySkill && !isDoubles)
        {
            return BadRequest("A secondary skill can only be taken when the roll came up doubles.");
        }

        if (request.UseSecondarySkill || baseOutcome == LevelUpOutcomeKind.NewSkill)
        {
            if (request.SkillChoice is null || !Enum.TryParse<Skill>(request.SkillChoice, out var chosenSkill))
            {
                return BadRequest("A valid skill choice is required for this result.");
            }

            var eligible = request.UseSecondarySkill
                ? SkillAccess.AvailableSecondarySkills(player)
                : SkillAccess.AvailablePrimarySkills(player);

            if (!eligible.Contains(chosenSkill))
            {
                return BadRequest($"{chosenSkill} is not an eligible skill for this player right now.");
            }

            LevelUpResolver.ApplySkill(player, chosenSkill);
        }
        else
        {
            if (baseOutcome == LevelUpOutcomeKind.SpeedOrArmorBoost)
            {
                if (request.StatChoice != nameof(BoostableStat.Speed) && request.StatChoice != nameof(BoostableStat.ArmorValue))
                {
                    return BadRequest("StatChoice must be Speed or ArmorValue for this result.");
                }

                LevelUpResolver.ApplyStatBoost(player, Enum.Parse<BoostableStat>(request.StatChoice));
            }
            else
            {
                var stat = baseOutcome == LevelUpOutcomeKind.AgilityBoost ? BoostableStat.Agility : BoostableStat.Strength;
                LevelUpResolver.ApplyStatBoost(player, stat);
            }
        }

        await _db.SaveChangesAsync();
        return DtoMapping.ToDto(player);
    }
}
