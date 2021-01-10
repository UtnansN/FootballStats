using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.GridModels
{
    public interface IOverview
    {
        void HandleDatabaseUpdate(object sender, EventArgs e);
    }
}
