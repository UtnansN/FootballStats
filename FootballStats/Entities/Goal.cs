using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public enum KickType
    {
        Regular,
        Penalty
    }

    public class Goal
    {
        public int GoalID { get; set; }
        public TimeSpan Time { get; set; }
        public KickType Type { get; set; }
        public virtual ICollection<Assist> Assists { get; private set; } = new ObservableCollection<Assist>();

        public int ScorerId { get; set; }
        public virtual Player Scorer { get; set; }

        public int TeamPlayId { get; set; }
        public virtual TeamPlay TeamPlay { get; set; }
    }
}
