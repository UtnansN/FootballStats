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

    public class Goal : Event
    {
        public int GoalID { get; set; }
        public KickType Type { get; set; }
        public virtual ICollection<Assist> Assists { get; private set; } = new ObservableCollection<Assist>();
    }
}
