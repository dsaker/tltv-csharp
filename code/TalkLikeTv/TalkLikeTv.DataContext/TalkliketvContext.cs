using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=tcp:127.0.0.1,1433;Initial Catalog=Talkliketv;User Id=sa;Password=s3cret-Ninja;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B938558BBAA6BB07");
        });

        modelBuilder.Entity<Personality>(entity =>
        {
            entity.HasKey(e => e.PersonalityId).HasName("PK__Personal__CD053C5491E3FF7A");
        });

        modelBuilder.Entity<Phrase>(entity =>
        {
            entity.HasKey(e => e.PhraseId).HasName("PK__Phrases__0DBA0EA27CBE7DFC");

            entity.HasOne(d => d.Title).WithMany(p => p.Phrases).HasConstraintName("FK__Phrases__TitleID__42B7D1CC");
        });

        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.ScenarioId).HasName("PK__Scenario__0DF6D1A34DDC40CD");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.StyleId).HasName("PK__Styles__8AD147A0A73623D5");
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.HasKey(e => e.TitleId).HasName("PK__Titles__757589E6A1AF53A0");

            entity.HasOne(d => d.OriginalLanguage).WithMany(p => p.Titles).HasConstraintName("FK__Titles__Original__3FDB6521");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Tokens__658FEE8AEEE06094");
        });

        modelBuilder.Entity<Translate>(entity =>
        {
            entity.HasKey(e => new { e.PhraseId, e.LanguageId }).HasName("PK__Translat__A6298BFA7F219216");

            entity.HasOne(d => d.Language).WithMany(p => p.Translates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Translate__Langu__468862B0");

            entity.HasOne(d => d.PhraseNavigation).WithMany(p => p.Translates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Translate__Phras__45943E77");
        });

        modelBuilder.Entity<Voice>(entity =>
        {
            entity.HasKey(e => e.VoiceId).HasName("PK__Voices__D870D587C6CFF3C9");

            entity.HasOne(d => d.Language).WithMany(p => p.Voices).HasConstraintName("FK__Voices__Language__2803DB90");

            entity.HasMany(d => d.Personalities).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoicePersonality",
                    r => r.HasOne<Personality>().WithMany()
                        .HasForeignKey("PersonalityId")
                        .HasConstraintName("FK__VoicePers__Perso__3C0AD43D"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoicePers__Voice__3B16B004"),
                    j =>
                    {
                        j.HasKey("VoiceId", "PersonalityId").HasName("PK__VoicePer__84A0864214AC939F");
                        j.ToTable("VoicePersonalities");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("PersonalityId").HasColumnName("PersonalityID");
                    });

            entity.HasMany(d => d.Scenarios).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoiceScenario",
                    r => r.HasOne<Scenario>().WithMany()
                        .HasForeignKey("ScenarioId")
                        .HasConstraintName("FK__VoiceScen__Scena__355DD6AE"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoiceScen__Voice__3469B275"),
                    j =>
                    {
                        j.HasKey("VoiceId", "ScenarioId").HasName("PK__VoiceSce__F8AFB89D2C1769F2");
                        j.ToTable("VoiceScenarios");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("ScenarioId").HasColumnName("ScenarioID");
                    });

            entity.HasMany(d => d.Styles).WithMany(p => p.Voices)
                .UsingEntity<Dictionary<string, object>>(
                    "VoiceStyle",
                    r => r.HasOne<Style>().WithMany()
                        .HasForeignKey("StyleId")
                        .HasConstraintName("FK__VoiceStyl__Style__2EB0D91F"),
                    l => l.HasOne<Voice>().WithMany()
                        .HasForeignKey("VoiceId")
                        .HasConstraintName("FK__VoiceStyl__Voice__2DBCB4E6"),
                    j =>
                    {
                        j.HasKey("VoiceId", "StyleId").HasName("PK__VoiceSty__C0DDC1FD1B7A5C74");
                        j.ToTable("VoiceStyles");
                        j.IndexerProperty<int>("VoiceId").HasColumnName("VoiceID");
                        j.IndexerProperty<int>("StyleId").HasColumnName("StyleID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
