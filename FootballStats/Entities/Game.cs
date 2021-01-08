using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public virtual ICollection<TeamPlay> Teams { get; private set; } = new ObservableCollection<TeamPlay>();
    }
}
