using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class ReadCPUUnitDataResponse
    {
        #region Constants

        internal const int CONTROLLER_MODEL_LENGTH = 20;
        internal const int CONTROLLER_VERSION_LENGTH = 20;
        internal const int SYSTEM_RESERVED_LENGTH = 40;
        internal const int AREA_DATA_LENGTH = 12;

        #endregion


        #region Internal Methods

        internal static CPUUnitDataResult ExtractData(FINSResponse response)
        {
            int expectedLength = CONTROLLER_MODEL_LENGTH + CONTROLLER_VERSION_LENGTH + SYSTEM_RESERVED_LENGTH + AREA_DATA_LENGTH;

            if (response.Data.Length < expectedLength)
            {
                throw new FINSException("The Response Data Length of '" + response.Data.Length.ToString() + "' was too short - Expecting a Length of '" + expectedLength.ToString() + "'");
            }

            List<byte> data = response.Data.ToList();

            CPUUnitDataResult result = new CPUUnitDataResult();

            result.ControllerModel = ASCIIEncoding.ASCII.GetString(data.GetRange(0, CONTROLLER_MODEL_LENGTH).ToArray()).Trim();

            data.RemoveRange(0, CONTROLLER_MODEL_LENGTH);

            result.ControllerVersion = ASCIIEncoding.ASCII.GetString(data.GetRange(0, CONTROLLER_VERSION_LENGTH).ToArray()).Trim();

            data.RemoveRange(0, CONTROLLER_VERSION_LENGTH);

            data.RemoveRange(0, SYSTEM_RESERVED_LENGTH);

            result.DataMemoryWordCount = BitConverter.ToUInt16(new byte[] { data[4], data[3] });

            return result;
        }

        #endregion


        #region Structs

        internal struct CPUUnitDataResult
        {
            internal string ControllerModel;
            internal string ControllerVersion;
            internal ushort DataMemoryWordCount;
        }

        #endregion
    }
}
