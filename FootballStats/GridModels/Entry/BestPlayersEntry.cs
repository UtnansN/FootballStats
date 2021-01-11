using System.Collections.Generic;

namespace FootballStats.GridModels
{
    public class BestPlayersEntry
    {
        public int Place { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Team { get; set; }
        public int Number { get; set; }
        public int GoalCount { get; set; }
        public int AssistCount { get; set; }
    }
}
