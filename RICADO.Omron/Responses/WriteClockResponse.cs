using System;
using System.Collections.Generic;
using System.Linq;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class WriteClockResponse
    {
        #region Internal Methods

        internal static void Validate(WriteClockRequest request, FINSResponse response)
        {
            // TODO: Consider if any Checks can be made on the FINS Response
        }

        #endregion
    }
}
