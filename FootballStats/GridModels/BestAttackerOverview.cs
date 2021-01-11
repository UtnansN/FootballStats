using FootballStats.Entities;
using FootballStats.GridModels.Entry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    public class BestAttackerOverview : IOverview
    {
        private readonly StatsContext dbContext;
        private int topCount = 10;
        public ObservableCollection<BestAttackerEntry> Stats = new ObservableCollection<BestAttackerEntry>();

        public BestAttackerOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<BestAttackerEntry> localStats = new List<BestAttackerEntry>();
            List<Player> players = dbContext.Players
                .Where(p => p.Role == PlayerRole.Attacker)
                .OrderByDescending(p => p.Goals.Count)
                .ThenByDescending(p => p.Assists.Count)
                .Take(topCount)
                .ToList();

            List<Swap> playerSwaps = dbContext.Swaps.ToList();

            for (int i = 1; i <= players.Count; i++)
            {
                Player player = players[i - 1];

                int participations = playerSwaps
                    .Where(s => s.SwapTo == player)
                    .Select(m => m.Matchup)
                    .Distinct().Count();

                int yellowCards = 0, redCards = 0;
                player.Penalties
                    .GroupBy(p => p.Matchup)
                    .Select(n => n.Count())
                    .ToList()
                    .ForEach(count => {
                        if (count > 0) {
                            yellowCards++;
                            if (count > 1)
                            {
                                redCards++;
                            }
                        }
                    });

                localStats.Add(new BestAttackerEntry
                {
                    Place = i,
                    Name = player.Name,
                    Surname = player.Surname,
                    Team = player.Team.Name,
                    Goals = player.Goals.Count,
                    Assists = player.Assists.Count,
                    GamesPlayed = participations,
                    YellowCards = yellowCards,
                    RedCards = redCards
                });
            }

            Stats.Clear();
            localStats.ForEach(Stats.Add);
        }


    }
}
