using System;

namespace RICADO.Omron
{
    public class FINSException : Exception
    {
        #region Constructors

        internal FINSException(string message) : base(message)
        {
        }

        internal FINSException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}
