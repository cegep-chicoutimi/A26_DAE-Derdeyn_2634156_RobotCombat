using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
    public class BusyException : Exception
    {
        public BusyException(string message, Exception? cause = null) : base(message, cause)
        {
        }
    }
}
