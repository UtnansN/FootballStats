using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballStats.Exceptions
{
    public class GameExistsException : Exception
    {

        public GameExistsException()
        {

        }

        public GameExistsException(string message) : base(message)
        {

        }

        public GameExistsException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
