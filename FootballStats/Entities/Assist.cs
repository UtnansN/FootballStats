using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Assist
    {
        public int GoalId { get; set; }
        public virtual Goal goal { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

    }
}
