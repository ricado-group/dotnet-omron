using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class ReadClockRequest : FINSRequest
    {
        #region Constructor

        private ReadClockRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static ReadClockRequest CreateNew(OmronPLC plc)
        {
            return new ReadClockRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.TimeData,
                SubFunctionCode = (byte)enTimeDataFunctionCode.ReadClock,
            };
        }

        #endregion


        #region Protected Methods

        protected override List<byte> BuildRequestData()
        {
            return new List<byte>();
        }

        #endregion
    }
}
