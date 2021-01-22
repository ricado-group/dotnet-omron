using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Omron.Channels;
using RICADO.Omron.Requests;
using RICADO.Omron.Responses;

namespace RICADO.Omron
{
    public class OmronPLC : IDisposable
    {
        #region Private Properties

        private byte _localNodeId;
        private byte _remoteNodeId;
        private enConnectionMethod _connectionMethod;
        private string _remoteHost;
        private int _port = 9600;
        private int _timeout;
        private int _retries;

        private enPLCType _plcType = enPLCType.Unknown;
        private bool _isInitialized;

        private EthernetChannel _channel;

        #endregion


        #region Internal Properties

        internal EthernetChannel Channel
        {
            get
            {
                return _channel;
            }
        }

        internal int MaximumReadWordLength
        {
            get
            {
                // TODO: Expand on the Channel Type (TCP vs. UDP) and PLC Type to determine the Maximum Packet Length (and thus the maximum Word Length)

                //return _plcType == enPLCType.NJ_NX_Series ? 9999 : 9999;
                return 999;
            }
        }

        internal int MaximumWriteWordLength
        {
            get
            {
                // TODO: Expand on the Channel Type (TCP vs. UDP) and PLC Type to determine the Maximum Packet Length (and thus the maximum Word Length)

                //return _plcType == enPLCType.NJ_NX_Series ? 9999 : 9999;
                return 996;
            }
        }

        #endregion


        #region Public Properties

        public byte LocalNodeID
        {
            get
            {
                return _localNodeId;
            }
        }

        public byte RemoteNodeID
        {
            get
            {
                return _remoteNodeId;
            }
        }

        public enConnectionMethod ConnectionMethod
        {
            get
            {
                return _connectionMethod;
            }
        }

        public string RemoteHost
        {
            get
            {
                return _remoteHost;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public int Retries
        {
            get
            {
                return _retries;
            }
            set
            {
                _retries = value;
            }
        }

        public enPLCType PLCType
        {
            get
            {
                return _plcType;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        #endregion


        #region Constructors

        public OmronPLC(byte localNodeId, byte remoteNodeId, enConnectionMethod connectionMethod, string remoteHost, int port = 9600, int timeout = 2000, int retries = 1)
        {
            if(localNodeId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(localNodeId), "The Local Node ID cannot be set to 0");
            }

            if(localNodeId == 255)
            {
                throw new ArgumentOutOfRangeException(nameof(localNodeId), "The Local Node ID cannot be set to 255");
            }

            _localNodeId = localNodeId;

            if (remoteNodeId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remoteNodeId), "The Remote Node ID cannot be set to 0");
            }

            if (remoteNodeId == 255)
            {
                throw new ArgumentOutOfRangeException(nameof(remoteNodeId), "The Remote Node ID cannot be set to 255");
            }

            if(remoteNodeId == localNodeId)
            {
                throw new ArgumentException("The Remote Node ID cannot be the same as the Local Node ID", nameof(remoteNodeId));
            }

            _remoteNodeId = remoteNodeId;

            _connectionMethod = connectionMethod;

            if (remoteHost == null)
            {
                throw new ArgumentNullException(nameof(remoteHost), "The Remote Host cannot be Null");
            }

            if(remoteHost.Length == 0)
            {
                throw new ArgumentException("The Remote Host cannot be Empty", nameof(remoteHost));
            }

            _remoteHost = remoteHost;

            if(port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "The Port cannot be less than 1");
            }

            _port = port;

            if(timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "The Timeout Value cannot be less than 1");
            }

            _timeout = timeout;

            if(retries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries), "The Retries Value cannot be Negative");
            }

            _retries = retries;
        }

        #endregion


        #region Public Methods

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if(_isInitialized == true)
            {
                return;
            }

            // Initialize the Channel
            if (_connectionMethod == enConnectionMethod.UDP)
            {
                try
                {
                    _channel = new EthernetUDPChannel(_remoteHost, _port);

                    await _channel.InitializeAsync(_timeout, cancellationToken);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    throw new OmronException("Failed to Create the Ethernet UDP Communication Channel for Omron PLC '" + _remoteHost + ":" + _port + "'", e);
                }
            }
            else if (_connectionMethod == enConnectionMethod.TCP)
            {
                try
                {
                    _channel = new EthernetTCPChannel(_remoteHost, _port);

                    await _channel.InitializeAsync(_timeout, cancellationToken);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    throw new OmronException("Failed to Create the Ethernet TCP Communication Channel for Omron PLC '" + _remoteHost + ":" + _port + "'", e);
                }
            }

            // TODO: Identify the PLC Type

            _isInitialized = true;
        }

        public void Dispose()
        {
            if(_channel != null)
            {
                _channel.Dispose();

                _channel = null;
            }

            if (_isInitialized == true)
            {
                _isInitialized = false;
            }
        }

        public Task<ReadBitsResult> ReadBitAsync(ushort address, byte bitIndex, enMemoryBitDataType dataType, CancellationToken cancellationToken)
        {
            return ReadBitsAsync(address, bitIndex, 1, dataType, cancellationToken);
        }

        public async Task<ReadBitsResult> ReadBitsAsync(ushort address, byte startBitIndex, byte length, enMemoryBitDataType dataType, CancellationToken cancellationToken)
        {
            if (startBitIndex > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(startBitIndex), "The Start Bit Index cannot be greater than 15");
            }

            if (length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Length cannot be Zero");
            }

            if (startBitIndex + length > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Start Bit Index and Length combined are greater than the Maximum Allowed of 16 Bits");
            }

            if (validateBitAddress(address, dataType) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The Address is greater than the Maximum Address for the '" + Enum.GetName(typeof(enMemoryBitDataType), dataType) + "' Data Type");
            }

            ReadMemoryAreaBitRequest request = ReadMemoryAreaBitRequest.CreateNew(this, address, startBitIndex, length, dataType);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            return new ReadBitsResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
                Values = ReadMemoryAreaBitResponse.ExtractValues(request, requestResult.Response),
            };
        }

        public Task<ReadWordsResult> ReadWordAsync(ushort address, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            return ReadWordsAsync(address, 1, dataType, cancellationToken);
        }

        public async Task<ReadWordsResult> ReadWordsAsync(ushort startAddress, ushort length, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            if (length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Length cannot be Zero");
            }

            if (length > MaximumReadWordLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Length cannot be greater than " + MaximumReadWordLength.ToString());
            }

            if(validateWordStartAddress(startAddress, length, dataType) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "The Start Address and Length combined are greater than the Maximum Address for the '" + Enum.GetName(typeof(enMemoryWordDataType), dataType) + "' Data Type");
            }

            ReadMemoryAreaWordRequest request = ReadMemoryAreaWordRequest.CreateNew(this, startAddress, length, dataType);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            return new ReadWordsResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
                Values = ReadMemoryAreaWordResponse.ExtractValues(request, requestResult.Response),
            };
        }

        public Task<WriteBitsResult> WriteBit(bool value, ushort address, byte bitIndex, enMemoryBitDataType dataType, CancellationToken cancellationToken)
        {
            return WriteBits(new bool[] { value }, address, bitIndex, dataType, cancellationToken);
        }

        public async Task<WriteBitsResult> WriteBits(bool[] values, ushort address, byte startBitIndex, enMemoryBitDataType dataType, CancellationToken cancellationToken)
        {
            if(startBitIndex > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(startBitIndex), "The Start Bit Index cannot be greater than 15");
            }
            
            if(values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array cannot be Empty");
            }

            if(startBitIndex + values.Length > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array Length was greater than the Maximum Allowed of 16 Bits");
            }

            if (validateBitAddress(address, dataType) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The Address is greater than the Maximum Address for the '" + Enum.GetName(typeof(enMemoryBitDataType), dataType) + "' Data Type");
            }

            WriteMemoryAreaBitRequest request = WriteMemoryAreaBitRequest.CreateNew(this, address, startBitIndex, dataType, values);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            WriteMemoryAreaBitResponse.Validate(request, requestResult.Response);

            return new WriteBitsResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
            };
        }

        public Task<WriteBitsResult> WriteWord(short value, ushort address, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            return WriteWords(new short[] { value }, address, dataType, cancellationToken);
        }

        public async Task<WriteBitsResult> WriteWords(short[] values, ushort startAddress, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array cannot be Empty");
            }

            if(values.Length > MaximumWriteWordLength)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array Length cannot be greater than " + MaximumWriteWordLength.ToString());
            }

            if (validateWordStartAddress(startAddress, values.Length, dataType) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "The Start Address and Values Array Length combined are greater than the Maximum Address for the '" + Enum.GetName(typeof(enMemoryWordDataType), dataType) + "' Data Type");
            }

            WriteMemoryAreaWordRequest request = WriteMemoryAreaWordRequest.CreateNew(this, startAddress, dataType, values);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            WriteMemoryAreaWordResponse.Validate(request, requestResult.Response);

            return new WriteBitsResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
            };
        }

        #endregion


        #region Private Methods

        private bool validateBitAddress(ushort address, enMemoryBitDataType dataType)
        {
            switch(dataType)
            {
                case enMemoryBitDataType.DataMemory:
                    return address <= (_plcType == enPLCType.NX1P2 ? 15999 : 32767);

                case enMemoryBitDataType.CommonIO:
                    return address <= 6143;

                case enMemoryBitDataType.Work:
                    return address <= 511;

                case enMemoryBitDataType.Holding:
                    return address <= 1535;

                case enMemoryBitDataType.Auxiliary:
                    return address <= (_plcType == enPLCType.CJ2 ? 11535 : 959);
            }

            return false;
        }

        private bool validateBitDataType(enMemoryBitDataType dataType)
        {
            switch (dataType)
            {
                case enMemoryBitDataType.DataMemory:
                    return _plcType != enPLCType.CP1;

                case enMemoryBitDataType.CommonIO:
                    return true;

                case enMemoryBitDataType.Work:
                    return true;

                case enMemoryBitDataType.Holding:
                    return true;

                case enMemoryBitDataType.Auxiliary:
                    return _plcType != enPLCType.NJ_NX_NY_Series && _plcType != enPLCType.NX1P2;
            }

            return false;
        }

        private bool validateWordStartAddress(ushort startAddress, int length, enMemoryWordDataType dataType)
        {
            switch (dataType)
            {
                case enMemoryWordDataType.DataMemory:
                    return startAddress + (length - 1) <= (_plcType == enPLCType.NX1P2 ? 15999 : 32767);

                case enMemoryWordDataType.CommonIO:
                    return startAddress + (length - 1) <= 6143;

                case enMemoryWordDataType.Work:
                    return startAddress + (length - 1) <= 511;

                case enMemoryWordDataType.Holding:
                    return startAddress + (length - 1) <= 1535;

                case enMemoryWordDataType.Auxiliary:
                    return startAddress + (length - 1) <= (_plcType == enPLCType.CJ2 ? 11535 : 959);
            }

            return false;
        }

        private bool validateWordDataType(enMemoryWordDataType dataType)
        {
            switch(dataType)
            {
                case enMemoryWordDataType.DataMemory:
                    return true;

                case enMemoryWordDataType.CommonIO:
                    return true;

                case enMemoryWordDataType.Work:
                    return true;

                case enMemoryWordDataType.Holding:
                    return true;

                case enMemoryWordDataType.Auxiliary:
                    return _plcType != enPLCType.NJ_NX_NY_Series && _plcType != enPLCType.NX1P2;
            }

            return false;
        }

        #endregion
    }
}
