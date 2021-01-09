using FootballStats.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats
{
    public class StatsContext : DbContext
    {
        public DbSet<Game> Games { get; set; }

        public DbSet<Judge> Judges { get; set; }
        public DbSet<JudgeGame> JudgeGames { get; set; }

        public DbSet<Goal> Goals { get; set; }
        public DbSet<Penalty> Penalties { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Swap> Swaps { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Matchup> TeamPlays { get; set; }
        public DbSet<Assist> Assists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys for some many-to-many linking tables
            // TeamPlay has its own ID despite it being an many-to-many table due to having a couple of other tables linked to it.
            modelBuilder.Entity<Assist>().HasKey(o => new { o.GoalId, o.PlayerId });
            modelBuilder.Entity<JudgeGame>().HasKey(o => new { o.JudgeId, o.GameId });

            // Indices to speed up lookup by specific columns
            modelBuilder.Entity<Judge>().HasIndex(j => new { j.Name, j.Surname });
            modelBuilder.Entity<Player>().HasIndex(p => p.Number);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=StatsDatabase.db");
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

    }
}
