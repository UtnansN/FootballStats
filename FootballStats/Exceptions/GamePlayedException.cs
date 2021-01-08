using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Exceptions
{
    public class GamePlayedException : Exception
    {

        public GamePlayedException()
        {

        }

        public GamePlayedException(string message) : base(message)
        {

        }

        public GamePlayedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
