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

        public void DeleteAllData()
        {
            // Apparently there is no good way to wipe the database. Could do this with reflection?
            Games.RemoveRange(Games);
            Judges.RemoveRange(Judges);
            JudgeGames.RemoveRange(JudgeGames);
            Goals.RemoveRange(Goals);
            Penalties.RemoveRange(Penalties);
            Players.RemoveRange(Players);
            Swaps.RemoveRange(Swaps);
            Teams.RemoveRange(Teams);
            TeamPlays.RemoveRange(TeamPlays);
            Assists.RemoveRange(Assists);

            SaveChanges();
        }

    }
}
