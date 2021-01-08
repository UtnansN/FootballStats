using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Entities
{
    public class Judge
    {
        public int JudgeId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public virtual ICollection<JudgeGame> Games { get; private set; } = new ObservableCollection<JudgeGame>();
    }
}
