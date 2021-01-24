using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class WriteMemoryAreaWordRequest : FINSRequest
    {
        #region Private Properties

        private ushort _startAddress;
        private enMemoryWordDataType _dataType;
        private short[] _values;

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

        internal short[] Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value;
            }
        }

        #endregion


        #region Constructor

        private WriteMemoryAreaWordRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static WriteMemoryAreaWordRequest CreateNew(OmronPLC plc, ushort startAddress, enMemoryWordDataType dataType, short[] values)
        {
            return new WriteMemoryAreaWordRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.MemoryArea,
                SubFunctionCode = (byte)enMemoryAreaFunctionCode.Write,
                StartAddress = startAddress,
                DataType = dataType,
                Values = values,
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
            data.AddRange(BitConverter.GetBytes((ushort)_values.Length).Reverse());

            // Word Values
            foreach(short value in _values)
            {
                data.AddRange(BitConverter.GetBytes(value).Reverse());
            }

            return data;
        }

        #endregion
    }
}
