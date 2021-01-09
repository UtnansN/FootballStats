using FootballStats.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    public class BestPlayersOverview : ITableData
    {
        public class Entry
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Team { get; set; }
            public int Number { get; set; }
            public int GoalCount { get; set; }
            public int AssistCount { get; set; }
        }

        private readonly StatsContext dbContext;
        private int topCount = 10;
        public ObservableCollection<Entry> Stats = new ObservableCollection<Entry>();

        public BestPlayersOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<Entry> localStats = new List<Entry>();

            List<Player> players = dbContext.Players
                .OrderByDescending(p => p.Goals.Count)
                .ThenByDescending(p => p.Assists.Count)
                .Take(topCount)
                .ToList();

            foreach (var player in players)
            {
                localStats.Add(new Entry()
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
