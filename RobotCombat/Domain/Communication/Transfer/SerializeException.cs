using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Communication.Transfer
{
    /**
     * Exception levée lorsqu'une erreur de sérialisation ou de désérialisation se produit.
     */
    public class SerializeException : Exception
    {
        public SerializeException(string message, Exception? cause = null) : base(message, cause)
        {
        }
    }
}
