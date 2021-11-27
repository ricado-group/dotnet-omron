using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class WriteMemoryAreaBitRequest : FINSRequest
    {
        #region Private Properties

        private ushort _address;
        private byte _startBitIndex;
        private enMemoryBitDataType _dataType;
        private bool[] _values;

        #endregion


        #region Internal Properties

        internal ushort Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
            }
        }

        internal byte StartBitIndex
        {
            get
            {
                return _startBitIndex;
            }
            set
            {
                _startBitIndex = value;
            }
        }

        internal enMemoryBitDataType DataType
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

        internal bool[] Values
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

        private WriteMemoryAreaBitRequest(OmronPLC plc) : base(plc)
        {
        }

        #endregion


        #region Internal Methods

        internal static WriteMemoryAreaBitRequest CreateNew(OmronPLC plc, ushort address, byte startBitIndex, enMemoryBitDataType dataType, bool[] values)
        {
            return new WriteMemoryAreaBitRequest(plc)
            {
                FunctionCode = (byte)enFunctionCode.MemoryArea,
                SubFunctionCode = (byte)enMemoryAreaFunctionCode.Write,
                Address = address,
                StartBitIndex = startBitIndex,
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
            data.AddRange(BitConverter.GetBytes(_address).Reverse());

            // Bit Index
            data.Add(_startBitIndex);

            // Length
            data.AddRange(BitConverter.GetBytes((ushort)_values.Length).Reverse());

            // Bit Values
            data.AddRange(_values.Select<bool, byte>(value => value == true ? (byte)1 : (byte)0));

            return data;
        }

        #endregion
    }
}
