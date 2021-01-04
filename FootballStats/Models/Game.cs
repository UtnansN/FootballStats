using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Game
    {
        public int GameID { get; set; }
        public DateTime date { get; set; } 
        public int viewerCount { get; set; }
        public String place { get; set; }

    }
}
