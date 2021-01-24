using System;
using System.Collections.Generic;
using System.Linq;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class ReadClockResponse
    {
        #region Constants

        internal const int DATE_LENGTH = 6;
        internal const int DAY_OF_WEEK_LENGTH = 1;

        #endregion


        #region Internal Methods

        internal static ClockResult ExtractClock(ReadClockRequest request, FINSResponse response)
        {
            if (response.Data.Length < DATE_LENGTH + DAY_OF_WEEK_LENGTH)
            {
                throw new FINSException("The Response Data Length of '" + response.Data.Length.ToString() + "' was too short - Expecting a Length of '" + (DATE_LENGTH + DAY_OF_WEEK_LENGTH).ToString() + "'");
            }

            ReadOnlyMemory<byte> data = response.Data;

            return new ClockResult
            {
                ClockDateTime = getClockDateTime(data.Slice(0, DATE_LENGTH).ToArray()),
                DayOfWeek = BCDConverter.ToByte(data.ToArray()[DATE_LENGTH]),
            };
        }

        #endregion


        #region Private Methods

        private static DateTime getClockDateTime(byte[] bytes)
        {
            byte year = BCDConverter.ToByte(bytes[0]);
            byte month = BCDConverter.ToByte(bytes[1]);
            byte day = BCDConverter.ToByte(bytes[2]);
            byte hour = BCDConverter.ToByte(bytes[3]);
            byte minute = BCDConverter.ToByte(bytes[4]);
            byte second = BCDConverter.ToByte(bytes[5]);

            if (year < 70)
            {
                return new DateTime(2000 + year, month, day, hour, minute, second);
            }
            else if (year < 100)
            {
                return new DateTime(1900 + year, month, day, hour, minute, second);
            }

            throw new FINSException("Invalid DateTime Values received from the PLC Clock");
        }

        #endregion


        #region Structs

        internal struct ClockResult
        {
            internal DateTime ClockDateTime;
            internal byte DayOfWeek;
        }

        #endregion
    }
}
