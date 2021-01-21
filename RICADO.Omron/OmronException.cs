using System;

namespace RICADO.Omron
{
    public class OmronException : Exception
    {
        #region Constructors

        internal OmronException(string message) : base(message)
        {
        }

        internal OmronException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}
