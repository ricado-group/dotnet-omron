using System;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class WriteMemoryAreaWordResponse
    {
        #region Internal Methods

        internal static void Validate(WriteMemoryAreaWordRequest request, FINSResponse response)
        {
            // TODO: Consider if any Checks can be made on the FINS Response
        }

        #endregion
    }
}
