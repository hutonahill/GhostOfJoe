using System;
using System.Collections.Generic;
using GhostOfJoe.Models;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Data;

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

    public virtual DbSet<Categories> Categories { get; set; }

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

    public virtual DbSet<Servers> Servers { get; set; }

    public virtual DbSet<Titles> Titles { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<VoteType> VoteTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=C:\\Users\\evanriker\\Desktop\\GhostOfJoe\\GhostOfJoe\\GhostOfJoe\\bin\\ServerData.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.Property(e => e.BillId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Categories>(entity =>
        {
            entity.HasOne(d => d.Game).WithMany(p => p.Categories).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasOne(d => d.Servers).WithMany(p => p.Games).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MetricHasWeight>(entity =>
        {
            entity.HasOne(d => d.WeightSet).WithMany().OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Querk>(entity =>
        {
            entity.Property(e => e.QuerkId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Religon>(entity =>
        {
            entity.Property(e => e.ReligonId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Representitive>(entity =>
        {
            entity.HasOne(d => d.Gender).WithMany(p => p.Representitives).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Office).WithMany(p => p.Representitives).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Party).WithMany(p => p.Representitives).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Religon).WithMany(p => p.Representitives).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<RepresentitiveHasBill>(entity =>
        {
            entity.HasOne(d => d.Bill).WithMany().OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Rep).WithMany().OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasOne(d => d.Categories).WithMany(p => p.Scores).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Users).WithMany(p => p.Scores).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Servers>(entity =>
        {
            entity.Property(e => e.SafeFlow).HasDefaultValue(1);
        });

        modelBuilder.Entity<Titles>(entity =>
        {
            entity.HasOne(d => d.Users).WithMany(p => p.Titles).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasOne(d => d.PreferredScoreSet).WithMany(p => p.Users).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.PreferredWeightSet).WithMany(p => p.Users).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Servers).WithMany(p => p.Users).OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
