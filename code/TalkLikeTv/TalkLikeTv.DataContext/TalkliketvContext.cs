using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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

    public virtual DbSet<Phrase> Phrases { get; set; }

    public virtual DbSet<Title> Titles { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<Translate> Translates { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            SqlConnectionStringBuilder builder = new();
            builder.DataSource = "tcp:127.0.0.1,1433"; // SQL Edge in Docker.
            builder.InitialCatalog = "Talkliketv";
            builder.TrustServerCertificate = true;
            builder.MultipleActiveResultSets = true;
            // Because we want to fail faster. Default is 15 seconds.
            builder.ConnectTimeout = 3;
            // SQL Server authentication.
            builder.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
            builder.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");
            
            optionsBuilder.UseSqlServer(builder.ConnectionString);
            
            optionsBuilder.LogTo(TalkliketvContextLogger.WriteLine,
                new[] { Microsoft.EntityFrameworkCore
                    .Diagnostics.RelationalEventId.CommandExecuting });
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK_Language");
        });

        modelBuilder.Entity<Phrase>(entity =>
        {
            entity.HasKey(e => e.PhraseId).HasName("PK__Phrases__0DBA0E8236F54210");

            entity.HasOne(d => d.Title).WithMany(p => p.Phrases).HasConstraintName("FK__Phrases__TitleId__76969D2E");
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.HasOne(d => d.OriginalLanguage).WithMany(p => p.Titles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Titles_OriginalLanguage");
        });

        modelBuilder.Entity<Translate>(entity =>
        {
            entity.HasKey(e => new { e.PhraseId, e.LanguageId }).HasName("PK__Translat__A6298BD86E7573EA");

            entity.HasOne(d => d.Language).WithMany(p => p.Translates).HasConstraintName("FK__Translate__Langu__7A672E12");

            entity.HasOne(d => d.PhraseNavigation).WithMany(p => p.Translates).HasConstraintName("FK__Translate__Phras__797309D9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
