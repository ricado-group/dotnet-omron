using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class ReadMemoryAreaWordRequest : FINSRequest
    {
        #region Private Properties

        private ushort _startAddress;
        private ushort _length;
        private enMemoryWordDataType _dataType;

        #endregion


        #region Internal Properties

        internal ushort StartAddress
        {
            get
            {
                return _startAddress;
            }
            set
            {
                _startAddress = value;
            }
        }

        internal ushort Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        internal enMemoryWordDataType DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        #endregion


        #region Constructor

        private ReadMemoryAreaWordRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static ReadMemoryAreaWordRequest CreateNew(OmronPLC plc, ushort startAddress, ushort length, enMemoryWordDataType dataType)
        {
            return new ReadMemoryAreaWordRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.MemoryArea,
                SubFunctionCode = (byte)enMemoryAreaFunctionCode.Read,
                StartAddress = startAddress,
                Length = length,
                DataType = dataType,
            };
        }

        #endregion


        #region Protected Methods

        protected override List<byte> BuildRequestData()
        {
            List<byte> data = new List<byte>();

            // Memory Area Data Type
            data.Add((byte)_dataType);

            // Address
            data.AddRange(BitConverter.GetBytes(_startAddress).Reverse());

            // Reserved
            data.Add(0);

            // Length
            data.AddRange(BitConverter.GetBytes(_length).Reverse());

            return data;
        }

        #endregion
    }
}
