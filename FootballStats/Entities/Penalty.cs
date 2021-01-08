using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Penalty
    {
        public int PenaltyID { get; set; }
        public TimeSpan Time { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int TeamPlayId { get; set; }
        public virtual TeamPlay TeamPlay { get; set; }
    }
}
