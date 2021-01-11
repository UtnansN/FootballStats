using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Game
    {
        public int GameId { get; set; }
        public DateTime Date { get; set; } 
        public int ViewerCount { get; set; }
        public string Place { get; set; }

        [NotMapped]
        public TimeSpan Length
        {
            get
            {
                Goal goal = Matchups.SelectMany(m => m.Goals).FirstOrDefault(g => g.Time.Hours > 0);
                if (goal == null)
                {
                    return new TimeSpan(1, 0, 0);
                }
                else
                {
                    return goal.Time;
                }
            } 
        }

        public virtual ICollection<JudgeGame> Judges { get; private set; } = new ObservableCollection<JudgeGame>();
        public virtual ICollection<Matchup> Matchups { get; private set; } = new ObservableCollection<Matchup>();
    }
}
