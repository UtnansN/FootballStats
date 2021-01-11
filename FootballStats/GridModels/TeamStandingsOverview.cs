using FootballStats.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FootballStats.GridModels
{
    public class TeamStandingsOverview : IOverview
    {

        private readonly StatsContext dbContext;
        public ObservableCollection<TeamStandingsEntry> Stats = new ObservableCollection<TeamStandingsEntry>();

        public TeamStandingsOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<TeamStandingsEntry> localStats = new List<TeamStandingsEntry>();
            foreach (Team team in dbContext.Teams)
            {
                List<Matchup> plays = dbContext.TeamPlays.Where(tp => tp.Team == team).ToList();
                List<Matchup> opponentPlays = plays.Select(tp => tp.OpponentMatchup).ToList();

                TeamStandingsEntry curr = new TeamStandingsEntry()
                {
                    TeamName = team.Name,
                    GoalCount = plays.Sum(tp => tp.Goals.Count),
                    ConcessionCount = opponentPlays.Sum(tp => tp.Goals.Count)
                };
                localStats.Add(curr);

                for (int i = 0; i < plays.Count; i++)
                {
                    // Current team won against opponent
                    if (plays[i].Goals.Count > opponentPlays[i].Goals.Count)
                    {
                        if (plays[i].Goals.Any(g => g.Time.Hours > 0))
                        {
                            curr.ExtendedWins++;
                        }
                        else
                        {
                            curr.BaseWins++;
                        }
                    }
                    // Current team lost against opponent
                    else
                    {
                        if (opponentPlays[i].Goals.Any(g => g.Time.Hours > 0))
                        {
                            curr.ExtendedLosses++;
                        }
                        else
                        {
                            curr.BaseLosses++;
                        }
                    }
                }
                curr.Points = 5 * curr.BaseWins + 3 * curr.ExtendedWins + 2 * curr.ExtendedLosses + curr.BaseLosses;
            }

            Stats.Clear();
            localStats = localStats.OrderByDescending(en => en.Points).ToList();

            for (int i = 0; i < localStats.Count; i++)
            {
                localStats[i].Place = i + 1;
                Stats.Add(localStats[i]);
            }
        }
    }
}
