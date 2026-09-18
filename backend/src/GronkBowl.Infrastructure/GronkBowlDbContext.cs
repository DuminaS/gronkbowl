using GronkBowl.Domain;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Infrastructure;

public class GronkBowlDbContext : DbContext
{
    public GronkBowlDbContext(DbContextOptions<GronkBowlDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Play> Plays => Set<Play>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<CallSheet> CallSheets => Set<CallSheet>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<DraftState> DraftStates => Set<DraftState>();
    public DbSet<PlaybookFolder> PlaybookFolders => Set<PlaybookFolder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePlayer(modelBuilder);
        ConfigurePlay(modelBuilder);
        ConfigureTeam(modelBuilder);
        ConfigureCallSheet(modelBuilder);
        ConfigureSeason(modelBuilder);
        ConfigureMatch(modelBuilder);
        ConfigureLeague(modelBuilder);
        ConfigureDraftState(modelBuilder);
        ConfigurePlaybookFolder(modelBuilder);
    }

    private static void ConfigurePlayer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Race).HasConversion<string>();
            entity.Property(p => p.Position).HasConversion<string>();
            entity.Property(p => p.InjuryStatus).HasConversion<string>();
            entity.Property(p => p.Attributes).AsJson();
            entity.Property(p => p.Traits).AsJson();
            entity.Property(p => p.Contract).AsJson();
            entity.Property(p => p.Skills).AsJson();
            entity.Property(p => p.DevelopmentPotential).HasConversion<string>();
        });
    }

    private static void ConfigurePlay(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Play>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Category).HasConversion<string>();
            entity.Property(p => p.PrimaryPosition).HasConversion<string>();
            entity.Property(p => p.Assignments).AsJson();
            entity.Property(p => p.Tags).AsJson();
        });
    }

    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Roster).AsJson();

            // Implicit many-to-many: a Play (the shared, permanently-public starter plays
            // especially) can be installed on more than one team's Playbook at once.
            entity.HasMany(t => t.Playbook).WithMany();
        });
    }

    private static void ConfigureCallSheet(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CallSheet>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.OffensiveSituationalPlays).AsJson();
            entity.Property(c => c.DefensiveSituationalPlays).AsJson();
        });
    }

    private static void ConfigureSeason(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Schedule).AsJson();
            entity.Property(s => s.Standings).AsJson();
            entity.Property(s => s.DraftOrder).AsJson();

            // CompletedMatches lives in its own table (Matches, keyed by SeasonId) since it
            // grows every week and gets queried on its own - not folded into this row's json.
            entity.Ignore(s => s.CompletedMatches);
        });
    }

    private static void ConfigureMatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.EventLog).AsJson();
            entity.HasIndex(m => new { m.SeasonId, m.Week });
        });
    }

    private static void ConfigureLeague(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(l => l.Id);

            // v1 simplification: League<->Team<->Season relational wiring isn't needed yet -
            // the API loads Teams/Seasons directly rather than through League navigation.
            // Revisit once multiple concurrent leagues are a real product requirement.
            entity.Ignore(l => l.Franchises);
            entity.Ignore(l => l.SeasonHistory);
        });
    }

    private static void ConfigureDraftState(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DraftState>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.PickOrder).AsJson();
            entity.Property(d => d.ProspectPlayerIds).AsJson();
            entity.Property(d => d.DraftedProspectPlayerIds).AsJson();
        });
    }

    private static void ConfigurePlaybookFolder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaybookFolder>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Category).HasConversion<string>();
            entity.Property(f => f.PlayIds).AsJson();
            entity.HasIndex(f => f.TeamId);
        });
    }
}
