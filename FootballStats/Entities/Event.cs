﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public abstract class Event
    {
        public TimeSpan Time { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int MatchupId { get; set; }
        public virtual Matchup Matchup { get; set; }
    }
}
