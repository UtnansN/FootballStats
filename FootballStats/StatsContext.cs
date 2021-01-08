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
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Penalty> Penalties { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Swap> Swaps { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamPlay> TeamPlays { get; set; }
        public DbSet<Assist> Assists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assist>().HasKey(o => new { o.GoalId, o.PlayerId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=stats.db");
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

    }
}
