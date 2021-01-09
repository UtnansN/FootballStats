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
    public class TeamOverview : ITableData
    {

        public class Entry
        {
            public string TeamName { get; set; }
            public int Points { get; set; }
            public int BaseWins { get; set; }
            public int BaseLosses { get; set; }
            public int ExtendedWins { get; set; }
            public int ExtendedLosses { get; set; }
            public int GoalCount { get; set; }
            public int ConcessionCount { get; set; }
        }

        private readonly StatsContext dbContext;
        public ObservableCollection<Entry> Stats = new ObservableCollection<Entry>();

        public TeamOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<Entry> localStats = new List<Entry>();
            foreach (Team team in dbContext.Teams)
            {
                List<Matchup> plays = dbContext.TeamPlays.Where(tp => tp.Team == team).ToList();
                List<Matchup> opponentPlays = plays.Select(tp => tp.OpponentMatchup).ToList();

                Entry curr = new Entry()
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
            localStats.OrderByDescending(en => en.Points).ToList().ForEach(Stats.Add);
        }


    }


}
