using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class WriteClockRequest : FINSRequest
    {
        #region Private Properties

        private DateTime _dateTime;
        private byte _dayOfWeek;

        #endregion


        #region Internal Properties

        internal DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
            }
        }

        internal byte DayOfWeek
        {
            get
            {
                return _dayOfWeek;
            }
            set
            {
                _dayOfWeek = value;
            }
        }

        #endregion


        #region Constructor

        private WriteClockRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static WriteClockRequest CreateNew(OmronPLC plc, DateTime dateTime, byte dayOfWeek)
        {
            return new WriteClockRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.TimeData,
                SubFunctionCode = (byte)enTimeDataFunctionCode.WriteClock,
                DateTime = dateTime,
                DayOfWeek = dayOfWeek,
            };
        }

        #endregion


        #region Protected Methods

        protected override List<byte> BuildRequestData()
        {
            List<byte> data = new List<byte>();

            // Year (Last 2 Digits)
            data.Add(BCDConverter.GetBCDByte((byte)(_dateTime.Year % 100)));

            // Month
            data.Add(BCDConverter.GetBCDByte((byte)_dateTime.Month));

            // Day
            data.Add(BCDConverter.GetBCDByte((byte)_dateTime.Day));

            // Hour
            data.Add(BCDConverter.GetBCDByte((byte)_dateTime.Hour));

            // Minute
            data.Add(BCDConverter.GetBCDByte((byte)_dateTime.Minute));

            // Second
            data.Add(BCDConverter.GetBCDByte((byte)_dateTime.Second));

            // Day of Week
            data.Add(BCDConverter.GetBCDByte(_dayOfWeek));

            return data;
        }

        #endregion
    }
}
