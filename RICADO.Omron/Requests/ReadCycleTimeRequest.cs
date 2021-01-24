using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class ReadCycleTimeRequest : FINSRequest
    {
        #region Constructor

        private ReadCycleTimeRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static ReadCycleTimeRequest CreateNew(OmronPLC plc)
        {
            return new ReadCycleTimeRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.Status,
                SubFunctionCode = (byte)enStatusFunctionCode.ReadCycleTime,
            };
        }

        #endregion


        #region Protected Methods

        protected override List<byte> BuildRequestData()
        {
            List<byte> data = new List<byte>();

            // Read Cycle Time
            data.Add(01);

            return data;
        }

        #endregion
    }
}
