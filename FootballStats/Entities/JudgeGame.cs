using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public enum JudgeType
    {
        Regular,
        Lead
    }

    public class JudgeGame
    {
        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        public int JudgeId { get; set; }
        public virtual Judge Judge { get; set; }

        public JudgeType Type { get; set; }
    }
}
