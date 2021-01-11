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

                List<List<Goal>> goals = matchups.Select(m => m.Goals.ToList()).ToList();

                List<int> goalCounts = goals.Select(g => g.Count).ToList();
                List<int> opponentGoalCounts = matchups.Select(m => m.OpponentMatchup.Goals.Count).ToList();
                List<int> nonBaseSwapCounts = matchups.Select(m => m.Swaps.Where(s => s.Time != TimeSpan.Zero).ToList().Count).ToList();
                List<int> penalties = matchups.Select(m => m.Penalties.Count).ToList();

                var goalGroupCounts = goals
                    .SelectMany(g => g)
                    .GroupBy(g => g.Type)
                    .Select(n => new
                    {
                        Type = n.Key,
                        Count = n.Count()
                    });

                var regularGoalGroup = goalGroupCounts.FirstOrDefault(g => g.Type == KickType.Regular);
                var penaltyGoalGroup = goalGroupCounts.FirstOrDefault(g => g.Type == KickType.Penalty);

                var entry = new TeamAveragesEntry
                {
                    Team = team.Name,
                    AverageGoals = Math.Round(goalCounts.Average(), 2),
                    AverageConcessions = Math.Round(opponentGoalCounts.Average(), 2),
                    RegularGoals = regularGoalGroup == null ? 0 : regularGoalGroup.Count,
                    PenaltyGoals = penaltyGoalGroup == null ? 0 : penaltyGoalGroup.Count,
                    AverageSwaps = Math.Round(nonBaseSwapCounts.Average(), 2),
                    AveragePenalties = Math.Round(penalties.Average(), 2),
                    LongestGame = matchups.Select(m => m.Game.Length).ToList().Max().ToString(),
                    MostGoalsInAGame = goalCounts.Max()
                };
                localStats.Add(entry);
            }

            Stats.Clear();
            localStats.OrderBy(en => en.Team).ToList().ForEach(Stats.Add);
        }
    }
}
