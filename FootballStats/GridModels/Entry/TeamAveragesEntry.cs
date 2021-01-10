using System.Collections.Generic;

namespace FootballStats.GridModels
{
        public class TeamAveragesEntry
        {
            public string Team { get; set; }

            public double AverageGoals { get; set; }
            public int MostGoals { get; set; }

            public double AverageConcessions { get; set; }
            public int MostConcessions { get; set; }

            public double AverageSwaps { get; set; }
            public double AveragePenalties { get; set; }
        }
}
