using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Swap
    {
        public int SwapID { get; set; }
        public TimeSpan Time { get; set; }

        public int? FromId { get; set; }
        public virtual Player From { get; set; }

        public int ToId { get; set; }
        public virtual Player To { get; set; }

        public int TeamPlayId { get; set; }
        public virtual TeamPlay TeamPlay { get; set; }
    }
}
