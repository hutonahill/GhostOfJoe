using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

public partial class ServerDataContext : DbContext
{
    public ServerDataContext()
    {
    }

    public ServerDataContext(DbContextOptions<ServerDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillHasMetric> BillHasMetrics { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricHasWeight> MetricHasWeights { get; set; }

    public virtual DbSet<MetricScoreSet> MetricScoreSets { get; set; }

    public virtual DbSet<MetricWeightSet> MetricWeightSets { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<Party> Parties { get; set; }

    public virtual DbSet<Querk> Querks { get; set; }

    public virtual DbSet<Race> Races { get; set; }

    public virtual DbSet<Religon> Religons { get; set; }

    public virtual DbSet<RepHasQuerk> RepHasQuerks { get; set; }

    public virtual DbSet<Representitive> Representitives { get; set; }

    public virtual DbSet<RepresentitiveHasBill> RepresentitiveHasBills { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Title> Titles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VoteType> VoteTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=C:\\Users\\evanriker\\Desktop\\GhostOfJoe\\GhostOfJoe\\GhostOfJoe\\bin\\ServerData.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("bills");

            entity.Property(e => e.BillId)
                .ValueGeneratedNever()
                .HasColumnName("bill_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ShortName).HasColumnName("short_name");
        });

        modelBuilder.Entity<BillHasMetric>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("bill_has_metric");

            entity.Property(e => e.BillId).HasColumnName("bill_id");
            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.ScoreSetId).HasColumnName("score_set_id");

            entity.HasOne(d => d.Bill).WithMany().HasForeignKey(d => d.BillId);

            entity.HasOne(d => d.Metric).WithMany().HasForeignKey(d => d.MetricId);

            entity.HasOne(d => d.ScoreSet).WithMany().HasForeignKey(d => d.ScoreSetId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.HigherBetter).HasColumnName("higherBetter");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Unit).HasColumnName("unit");

            entity.HasOne(d => d.Game).WithMany(p => p.Categories)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("games");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.ServerId).HasColumnName("server_id");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Server).WithMany(p => p.Games)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.ToTable("gender");

            entity.Property(e => e.GenderId).HasColumnName("gender_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity.ToTable("metric");

            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<MetricHasWeight>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("metric_has_weights");

            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.WeightSetId).HasColumnName("weight_set_id");

            entity.HasOne(d => d.Metric).WithMany().HasForeignKey(d => d.MetricId);

            entity.HasOne(d => d.WeightSet).WithMany()
                .HasForeignKey(d => d.WeightSetId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MetricScoreSet>(entity =>
        {
            entity.HasKey(e => e.ScoreSetId);

            entity.ToTable("metric_score_set");

            entity.Property(e => e.ScoreSetId).HasColumnName("score_set_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<MetricWeightSet>(entity =>
        {
            entity.HasKey(e => e.WeightSetId);

            entity.ToTable("metric_weight_set");

            entity.Property(e => e.WeightSetId).HasColumnName("weight_set_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.ToTable("office");

            entity.Property(e => e.OfficeId).HasColumnName("office_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("party");

            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Querk>(entity =>
        {
            entity.ToTable("querks");

            entity.Property(e => e.QuerkId)
                .ValueGeneratedNever()
                .HasColumnName("querk_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.ToTable("race");

            entity.Property(e => e.RaceId).HasColumnName("race_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Religon>(entity =>
        {
            entity.ToTable("religon");

            entity.Property(e => e.ReligonId)
                .ValueGeneratedNever()
                .HasColumnName("religon_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<RepHasQuerk>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("rep_has_querk");

            entity.Property(e => e.QuerkId).HasColumnName("querk_id");
            entity.Property(e => e.RepId).HasColumnName("rep_id");

            entity.HasOne(d => d.Querk).WithMany().HasForeignKey(d => d.QuerkId);

            entity.HasOne(d => d.Rep).WithMany().HasForeignKey(d => d.RepId);
        });

        modelBuilder.Entity<Representitive>(entity =>
        {
            entity.HasKey(e => e.RepId);

            entity.ToTable("representitives");

            entity.Property(e => e.RepId).HasColumnName("rep_id");
            entity.Property(e => e.BirthYear).HasColumnName("birth_year");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.FirstElected).HasColumnName("first_elected");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.GenderId).HasColumnName("gender_id");
            entity.Property(e => e.Ipa).HasColumnName("ipa");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Lgbt).HasColumnName("lgbt");
            entity.Property(e => e.OfficeId).HasColumnName("office_id");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.Phonetic).HasColumnName("phonetic");
            entity.Property(e => e.RaceId).HasColumnName("race_id");
            entity.Property(e => e.ReligonId).HasColumnName("religon_id");

            entity.HasOne(d => d.Gender).WithMany(p => p.Representitives)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Office).WithMany(p => p.Representitives)
                .HasForeignKey(d => d.OfficeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Party).WithMany(p => p.Representitives)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Race).WithMany(p => p.Representitives).HasForeignKey(d => d.RaceId);

            entity.HasOne(d => d.Religon).WithMany(p => p.Representitives)
                .HasForeignKey(d => d.ReligonId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<RepresentitiveHasBill>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("representitive_has_bills");

            entity.Property(e => e.BillId).HasColumnName("bill_id");
            entity.Property(e => e.RepId).HasColumnName("rep_id");
            entity.Property(e => e.VoteTypeId).HasColumnName("vote_type_id");

            entity.HasOne(d => d.Bill).WithMany()
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Rep).WithMany()
                .HasForeignKey(d => d.RepId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VoteType).WithMany().HasForeignKey(d => d.VoteTypeId);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.ToTable("scores");

            entity.Property(e => e.ScoreId).HasColumnName("score_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Value)
                .HasColumnType("NUMERIC (10, 1)")
                .HasColumnName("value");

            entity.HasOne(d => d.Category).WithMany(p => p.Scores)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Scores)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.ToTable("servers");

            entity.Property(e => e.ServerId).HasColumnName("server_id");
            entity.Property(e => e.SafeFlow)
                .HasDefaultValue(1)
                .HasColumnName("safeFlow");
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.ToTable("titles");

            entity.Property(e => e.TitleId).HasColumnName("title_id");
            entity.Property(e => e.Title1).HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Titles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DiscordUserId).HasColumnName("discordUser_id");
            entity.Property(e => e.PreferedScoreSetId).HasColumnName("prefered_score_set_id");
            entity.Property(e => e.PreferedWeightSetId).HasColumnName("prefered_weight_set_id");
            entity.Property(e => e.ServerId).HasColumnName("server_id");

            entity.HasOne(d => d.PreferedScoreSet).WithMany(p => p.Users)
                .HasForeignKey(d => d.PreferedScoreSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.PreferedWeightSet).WithMany(p => p.Users)
                .HasForeignKey(d => d.PreferedWeightSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Server).WithMany(p => p.Users)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VoteType>(entity =>
        {
            entity.ToTable("vote_type");

            entity.Property(e => e.VoteTypeId).HasColumnName("vote_type_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
