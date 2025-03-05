

using GhostOfJoe.Models;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Data;

public class ServerDataContext : DbContext {

    // there should be one of these for every table in the database.
    public DbSet<Categories> Categories { get; set; } = null!;

    public DbSet<Games> Games { get; set; } = null!;

    public DbSet<Scores> Scores { get; set; } = null!;

    public DbSet<Servers> Servers { get; set; } = null!;

    public DbSet<Titles> Titles { get; set; } = null!;

    public DbSet<Users> Users { get; set; } = null!;


    // hard coding this is a bad idea
    private const string DatabasePath = @"C:\Users\evanriker\Desktop\GhostOfJoe\GhostOfJoe\GhostOfJoe\bin\ServerData.db";
    
    private const string ServerDataConnection = $"Data Source={DatabasePath};";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite(ServerDataConnection);
    }
    
    public void InitializeDatabase() {
        // Ensure that the database file is created if it doesn't exist
        Database.EnsureCreated();
    }
}   