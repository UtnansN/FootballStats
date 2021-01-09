using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Matchup
    {
        public int MatchupId { get; set; }

        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        public virtual ICollection<Swap> Swaps { get; private set; } = new ObservableCollection<Swap>();
        public virtual ICollection<Penalty> Penalties { get; private set; } = new ObservableCollection<Penalty>();
        public virtual ICollection<Goal> Goals { get; private set; } = new ObservableCollection<Goal>();

        [NotMapped]
        public Matchup OpponentMatchup
        {
            get
            {
                return Game.Matchups.First(t => t != this);
            }
        }
    }
}
