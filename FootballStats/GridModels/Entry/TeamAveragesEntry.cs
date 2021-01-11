using System;
using System.Collections.Generic;

namespace FootballStats.GridModels
{
        public class TeamAveragesEntry
        {
            public string Team { get; set; }

            public double AverageGoals { get; set; }
            public double AverageConcessions { get; set; }

            public int PenaltyGoals { get; set; }
            public int RegularGoals { get; set; }

            public double AverageSwaps { get; set; }
            public double AveragePenalties { get; set; }

            public string LongestGame { get; set; }
            public int MostGoalsInAGame { get; set; }
        }
}
