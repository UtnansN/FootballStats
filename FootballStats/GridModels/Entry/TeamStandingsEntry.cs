using System.Collections.Generic;

namespace FootballStats.GridModels
{
    public class TeamStandingsEntry
    {
        public int Place { get; set; }
        public string TeamName { get; set; }
        public int Points { get; set; }
        public int BaseWins { get; set; }
        public int BaseLosses { get; set; }
        public int ExtendedWins { get; set; }
        public int ExtendedLosses { get; set; }
        public int GoalCount { get; set; }
        public int ConcessionCount { get; set; }
    }
}
