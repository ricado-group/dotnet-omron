using System;
using System.Linq;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class ReadMemoryAreaBitResponse
    {
        #region Internal Methods

        internal static bool[] ExtractValues(ReadMemoryAreaBitRequest request, FINSResponse response)
        {
            // TODO: Review the Main and Sub Response Codes and handle them accordingly

            if(response.Data.Length < request.Length)
            {
                throw new FINSException("The Response Data Length of '" + response.Data.Length.ToString() + "' was too short - Expecting a Length of '" + request.Length.ToString() + "'");
            }

            return response.Data.Select<byte, bool>(value => value == 0 ? false : true).ToArray();
        }

        #endregion
    }
}
