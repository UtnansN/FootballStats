using FootballStats.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    public class TeamAveragesOverview : IOverview
    {

        private readonly StatsContext dbContext;
        public ObservableCollection<TeamAveragesEntry> Stats = new ObservableCollection<TeamAveragesEntry>();

        public TeamAveragesOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<TeamAveragesEntry> localStats = new List<TeamAveragesEntry>();

            foreach (var team in dbContext.Teams)
            {
                List<Matchup> matchups = team.Matchups.ToList();

                List<int> goalCounts = matchups.Select(m => m.Goals.Count).ToList();
                List<int> opponentGoalCounts = matchups.Select(m => m.OpponentMatchup.Goals.Count).ToList();
                List<int> nonBaseSwapCounts = matchups.Select(m => m.Swaps.Where(s => s.Time != TimeSpan.Zero).ToList().Count).ToList();
                List<int> penalties = matchups.Select(m => m.Penalties.Count).ToList();

                var entry = new TeamAveragesEntry
                {
                    Team = team.Name,

                    AverageGoals = goalCounts.Average(),
                    MostGoals = goalCounts.Max(),

                    AverageConcessions = opponentGoalCounts.Average(),
                    MostConcessions = opponentGoalCounts.Max(),

                    AverageSwaps = nonBaseSwapCounts.Average(),
                    AveragePenalties = penalties.Average(),
                };

                localStats.Add(entry);
            }

            Stats.Clear();
            localStats.OrderBy(en => en.Team).ToList().ForEach(Stats.Add);
        }
    }
}
