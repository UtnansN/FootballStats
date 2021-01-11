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
    class GameOverview : IOverview
    {
        private readonly StatsContext dbContext;
        public ObservableCollection<GameEntry> Stats = new ObservableCollection<GameEntry>();

        public GameOverview(StatsContext context)
        {
            dbContext = context;
        }

        public void HandleDatabaseUpdate(object sender, EventArgs e)
        {
            List<GameEntry> localStats = new List<GameEntry>();
            List<Game> games = dbContext.Games.OrderByDescending(g => g.ViewerCount).ToList();

            for (int i = 1; i <= games.Count; i++)
            {
                Game game = games[i-1];
                List<Matchup> matchups = game.Matchups.ToList();

                localStats.Add(new GameEntry
                {
                    Place = i,
                    Viewers = game.ViewerCount,
                    Date = game.Date.ToShortDateString(),
                    T1 = matchups[0].Team.Name,
                    T2 = matchups[1].Team.Name,
                    FinalResults = matchups[0].Goals.Count + "-" + matchups[1].Goals.Count,
                    FirstHalfResults = GetFirstHalfGoals(matchups[0]) + "-" + GetFirstHalfGoals(matchups[1]),
                    GameLength = game.Length.ToString()
                });
            }

            Stats.Clear();
            localStats.ForEach(Stats.Add);
        }

        private int GetFirstHalfGoals(Matchup matchup)
        {
            return matchup.Goals.Where(g => g.Time < new TimeSpan(0, 30, 0)).ToList().Count;
        }

    }
}
