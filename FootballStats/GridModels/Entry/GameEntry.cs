using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels.Entry
{
    class GameEntry
    {
        public int Place { get; set; }
        public int Viewers { get; set; }
        public string Date { get; set; }
        public string T1 { get; set; }
        public string T2 { get; set; }
        public string FinalResults { get; set; }
        public string FirstHalfResults { get; set; }
        public string GameLength { get; set; }
    }
}
