using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class ReadCPUUnitDataRequest : FINSRequest
    {
        #region Constructor

        private ReadCPUUnitDataRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static ReadCPUUnitDataRequest CreateNew(OmronPLC plc)
        {
            return new ReadCPUUnitDataRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.MachineConfiguration,
                SubFunctionCode = (byte)enMachineConfigurationFunctionCode.ReadCPUUnitData,
            };
        }

        #endregion


        #region Protected Methods

        protected override List<byte> BuildRequestData()
        {
            List<byte> data = new List<byte>();

            // Read Data
            data.Add(0);

            return data;
        }

        #endregion
    }
}
