using FootballStats.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    public class BestPlayersOverview : IOverview
    {

        private readonly StatsContext dbContext;
        private int topCount = 10;
        public ObservableCollection<BestPlayersEntry> Stats = new ObservableCollection<BestPlayersEntry>();

        public BestPlayersOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<BestPlayersEntry> localStats = new List<BestPlayersEntry>();

            List<Player> players = dbContext.Players
                .OrderByDescending(p => p.Goals.Count)
                .ThenByDescending(p => p.Assists.Count)
                .Take(topCount)
                .ToList();

            foreach (var player in players)
            {
                localStats.Add(new BestPlayersEntry()
                {
                    Name = player.Name,
                    Surname = player.Surname,
                    Team = player.Team.Name,
                    Number = player.Number,
                    GoalCount = player.Goals.Count,
                    AssistCount = player.Assists.Count
                });
            }

            Stats.Clear();
            localStats.ForEach(Stats.Add);
        }
    }
}
