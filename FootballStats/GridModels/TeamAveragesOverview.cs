using FootballStats.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    class TeamAveragesOverview : ITableData
    {
        public class Entry
        {
            public string Team { get; set; }

            public double AverageGoals { get; set; }
            public int MostGoals { get; set; }

            public double AverageConcessions { get; set; }
            public int MostConcessions { get; set; }

            public double AverageSwaps { get; set; }
            public int MostSwaps { get; set; }

            public double AveragePenalties { get; set; }
            public int MostPenalties { get; set; }
            public int YellowCardsReceived { get; set; }
            public int RedCardsReceived { get; set; }
        }

        private readonly StatsContext dbContext;
        public ObservableCollection<Entry> Stats = new ObservableCollection<Entry>();

        public TeamAveragesOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<Entry> localStats = new List<Entry>();

            foreach (var team in dbContext.Teams)
            {
                List<Matchup> matchups = team.Matchups.ToList();

                List<int> goalCounts = matchups.Select(m => m.Goals.Count).ToList();
                List<int> opponentGoalCounts = matchups.Select(m => m.OpponentMatchup.Goals.Count).ToList();
                List<int> nonBaseSwapCounts = matchups.Select(m => m.Swaps.Where(s => s.Time != TimeSpan.Zero).ToList().Count).ToList();

                List<List<Penalty>> penalties = matchups.Select(m => m.Penalties.ToList()).ToList();

                var entry = new Entry
                {
                    Team = team.Name,

                    AverageGoals = goalCounts.Average(),
                    MostGoals = goalCounts.Max(),

                    AverageConcessions = opponentGoalCounts.Average(),
                    MostConcessions = opponentGoalCounts.Max(),

                    AverageSwaps = nonBaseSwapCounts.Average(),
                    MostSwaps = nonBaseSwapCounts.Max(),

                    AveragePenalties = penalties.Select(p => p.Count).Average(),
                    MostPenalties = penalties.Select(p => p.Count).Max(),
                };

                localStats.Add(entry);
            }

            Stats.Clear();
            localStats.OrderBy(en => en.Team).ToList().ForEach(Stats.Add);
        }
    }
}
