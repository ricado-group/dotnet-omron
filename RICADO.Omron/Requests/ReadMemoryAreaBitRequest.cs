using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Omron.Requests
{
    internal class ReadMemoryAreaBitRequest : FINSRequest
    {
        #region Private Properties

        private ushort _address;
        private byte _startBitIndex;
        private ushort _length;
        private enMemoryBitDataType _dataType;

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

        #endregion


        #region Constructor

        private ReadMemoryAreaBitRequest() : base()
        {
        }

        #endregion


        #region Internal Methods

        internal static ReadMemoryAreaBitRequest CreateNew(OmronPLC plc, ushort address, byte startBitIndex, ushort length, enMemoryBitDataType dataType)
        {
            return new ReadMemoryAreaBitRequest()
            {
                LocalNodeID = plc.LocalNodeID,
                RemoteNodeID = plc.RemoteNodeID,
                FunctionCode = (byte)enFunctionCode.MemoryArea,
                SubFunctionCode = (byte)enMemoryAreaFunctionCode.Read,
                Address = address,
                StartBitIndex = startBitIndex,
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
            data.AddRange(BitConverter.GetBytes(_address).Reverse());

            // Bit Index
            data.Add(_startBitIndex);

            // Length
            data.AddRange(BitConverter.GetBytes(_length).Reverse());

            return data;
        }

        #endregion
    }
}
