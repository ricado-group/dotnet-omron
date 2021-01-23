using System;
using System.Collections.Generic;
using System.Linq;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class ReadMemoryAreaWordResponse
    {
        #region Internal Methods

        internal static short[] ExtractValues(ReadMemoryAreaWordRequest request, FINSResponse response)
        {
            if (response.Data.Length < request.Length * 2)
            {
                throw new FINSException("The Response Data Length of '" + response.Data.Length.ToString() + "' was too short - Expecting a Length of '" + (request.Length * 2).ToString() + "'");
            }

            List<short> values = new List<short>();

            for(int i = 0; i < request.Length * 2; i += 2)
            {
                values.Add(BitConverter.ToInt16(new byte[] { response.Data[i + 1], response.Data[i] }));
            }

            return values.ToArray();
        }

        #endregion
    }
}
