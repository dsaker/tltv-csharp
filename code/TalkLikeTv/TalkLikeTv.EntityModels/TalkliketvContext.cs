using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TalkLikeTv.EntityModels;

public partial class TalkliketvContext : DbContext
{
    public TalkliketvContext()
    {
    }

    public TalkliketvContext(DbContextOptions<TalkliketvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Personality> Personalities { get; set; }

    public virtual DbSet<Phrase> Phrases { get; set; }

    public virtual DbSet<Scenario> Scenarios { get; set; }

    public virtual DbSet<Style> Styles { get; set; }

    public virtual DbSet<Title> Titles { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<Translate> Translates { get; set; }

    public virtual DbSet<Voice> Voices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            Console.WriteLine("Not yet configured");
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine($"Environment: {environment}");

            // Try to find the solution directory first
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = currentDir;
        
            // Navigate up until we find the solution directory or reach the root
            while (!Directory.Exists(Path.Combine(solutionDir, "TalkLikeTv.Mvc")) && 
                   Directory.GetParent(solutionDir) != null)
            {
                solutionDir = Directory.GetParent(solutionDir).FullName;
            }

            // Construct the path to the MVC project's appsettings.json
            var configPath = Path.Combine(solutionDir, "TalkLikeTv.Mvc", "appsettings.json");
        
            Console.WriteLine($"Looking for appsettings.json at: {configPath}");
            Console.WriteLine($"File exists: {File.Exists(configPath)}");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file not found at {configPath}");
            }

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false);

            var configuration = configBuilder.Build();

            var connectionStrings = configuration.GetSection("ConnectionStrings");
            if (!connectionStrings.Exists())
            {
                throw new InvalidOperationException("ConnectionStrings section not found in configuration file");
            }

            // Try to get the specific connection string
            var sqlServerConnection = configuration.GetConnectionString("TalkliketvConnection");
            Console.WriteLine($"TalkliketvConnection found: {sqlServerConnection != null}");

            if (sqlServerConnection is not null)
            {
                Console.WriteLine("Using TalkliketvConnection from configuration");
                Console.WriteLine($"Connection string: {sqlServerConnection}");
                SqlConnectionStringBuilder sql = new(sqlServerConnection);
                sql.IntegratedSecurity = false;
                sql.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
                sql.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");

                optionsBuilder.UseSqlServer(sql.ConnectionString);
            }
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B938558B157F7949");
        });

        modelBuilder.Entity<Personality>(entity =>
        {
            entity.HasKey(e => e.PersonalityId).HasName("PK__Personal__CD053C54F187B8DC");
        });

        modelBuilder.Entity<Phrase>(entity =>
        {
            entity.HasKey(e => e.PhraseId).HasName("PK__Phrases__0DBA0EA20BE30369");

            entity.HasOne(d => d.Title).WithMany(p => p.Phrases)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Phrases__TitleID__5A254709");
        });

        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.ScenarioId).HasName("PK__Scenario__0DF6D1A39A1B5B0E");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.StyleId).HasName("PK__Styles__8AD147A057377767");
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.HasKey(e => e.TitleId).HasName("PK__Titles__757589E6D98CFFD1");

            entity.HasOne(d => d.OriginalLanguage).WithMany(p => p.Titles).HasConstraintName("FK__Titles__Original__5748DA5E");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Tokens__658FEE8A40067671");
        });

        modelBuilder.Entity<Translate>(entity =>
        {
            entity.HasKey(e => new { e.PhraseId, e.LanguageId }).HasName("PK__Translat__A6298BFA0F5D3F2A");

            entity.HasOne(d => d.Language).WithMany(p => p.Translates).HasConstraintName("FK__Translate__Langu__5DF5D7ED");

            entity.HasOne(d => d.PhraseNavigation).WithMany(p => p.Translates).HasConstraintName("FK__Translate__Phras__5D01B3B4");
        });

        modelBuilder.Entity<Voice>(entity =>
        {
            entity.HasKey(e => e.VoiceId).HasName("PK__Voices__D870D587186CF99C");

            entity.HasOne(d => d.Language).WithMany(p => p.Voices).HasConstraintName("FK__Voices__Language__3F7150CD");

            entity.HasMany(d => d.Personalities).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoicePersonality",
                    r => r.HasOne<Personality>().WithMany()
                        .HasForeignKey("PersonalityId")
                        .HasConstraintName("FK__VoicePers__Perso__5378497A"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoicePers__Voice__52842541"),
                    j =>
                    {
                        j.HasKey("VoiceId", "PersonalityId").HasName("PK__VoicePer__84A08642C51F02F2");
                        j.ToTable("VoicePersonalities");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("PersonalityId").HasColumnName("PersonalityID");
                    });

            entity.HasMany(d => d.Scenarios).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoiceScenario",
                    r => r.HasOne<Scenario>().WithMany()
                        .HasForeignKey("ScenarioId")
                        .HasConstraintName("FK__VoiceScen__Scena__4CCB4BEB"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoiceScen__Voice__4BD727B2"),
                    j =>
                    {
                        j.HasKey("VoiceId", "ScenarioId").HasName("PK__VoiceSce__F8AFB89D9B0E5462");
                        j.ToTable("VoiceScenarios");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("ScenarioId").HasColumnName("ScenarioID");
                    });

            entity.HasMany(d => d.Styles).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoiceStyle",
                    r => r.HasOne<Style>().WithMany()
                        .HasForeignKey("StyleId")
                        .HasConstraintName("FK__VoiceStyl__Style__461E4E5C"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoiceStyl__Voice__452A2A23"),
                    j =>
                    {
                        j.HasKey("VoiceId", "StyleId").HasName("PK__VoiceSty__C0DDC1FD59FFE951");
                        j.ToTable("VoiceStyles");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("StyleId").HasColumnName("StyleID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
