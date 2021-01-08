using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Swap : Event
    {
        public int SwapID { get; set; }

        public int SwapToId { get; set; }
        public virtual Player SwapTo { get; set; }
    }
}
