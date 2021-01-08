using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class TeamPlay
    {
        public int TeamPlayId { get; set; }

        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        public virtual ICollection<Swap> Swaps { get; private set; } = new ObservableCollection<Swap>();
        public virtual ICollection<Penalty> Penalties { get; private set; } = new ObservableCollection<Penalty>();
        public virtual ICollection<Goal> Goals { get; private set; } = new ObservableCollection<Goal>();
    }
}
