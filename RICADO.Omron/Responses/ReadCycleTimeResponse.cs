using System;
using System.Collections.Generic;
using System.Linq;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class ReadCycleTimeResponse
    {
        #region Constants

        internal const int CYCLE_TIME_ITEM_LENGTH = 4;

        #endregion


        #region Internal Methods

        internal static CycleTimeResult ExtractCycleTime(ReadCycleTimeRequest request, FINSResponse response)
        {
            if (response.Data.Length < CYCLE_TIME_ITEM_LENGTH * 3)
            {
                throw new FINSException("The Response Data Length of '" + response.Data.Length.ToString() + "' was too short - Expecting a Length of '" + (CYCLE_TIME_ITEM_LENGTH * 3).ToString() + "'");
            }

            ReadOnlyMemory<byte> data = response.Data;

            return new CycleTimeResult
            {
                AverageCycleTime = getCycleTime(data.Slice(0, CYCLE_TIME_ITEM_LENGTH).ToArray()),
                MaximumCycleTime = getCycleTime(data.Slice(CYCLE_TIME_ITEM_LENGTH, CYCLE_TIME_ITEM_LENGTH).ToArray()),
                MinimumCycleTime = getCycleTime(data.Slice(CYCLE_TIME_ITEM_LENGTH * 2, CYCLE_TIME_ITEM_LENGTH).ToArray()),
            };
        }

        #endregion


        #region Private Methods

        private static double getCycleTime(byte[] bytes)
        {
            if(bytes.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "The Cycle Time Bytes Array Length must be 4");
            }

            uint cycleTimeValue = BCDConverter.ToUInt32(bytes.Reverse().ToArray());

            if(cycleTimeValue > 0)
            {
                return cycleTimeValue / (double)10;
            }

            return 0;
        }

        #endregion


        #region Structs

        internal struct CycleTimeResult
        {
            internal double MinimumCycleTime;
            internal double MaximumCycleTime;
            internal double AverageCycleTime;
        }

        #endregion
    }
}
