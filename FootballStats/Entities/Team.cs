using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Team
    {
        [Key]
        public string Name { get; set; }
        public virtual ICollection<Player> Players { get; private set; } = new ObservableCollection<Player>();
        public virtual ICollection<TeamPlay> Games { get; private set; } = new ObservableCollection<TeamPlay>();
    }
}
