using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public enum PlayerRole
    {
        Goalkeeper,
        Attacker,
        Defender
    }

    public class Player
    {
        public int PlayerID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Number { get; set; }
        public PlayerRole Role { get; set; }

        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public virtual ICollection<Penalty> Penalties { get; private set; } = new ObservableCollection<Penalty>();
        public virtual ICollection<Goal> Goals { get; private set; } = new ObservableCollection<Goal>();
        public virtual ICollection<Assist> Assists { get; private set; } = new ObservableCollection<Assist>();
    }
}
