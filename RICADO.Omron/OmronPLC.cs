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
        private string _remoteHost;
        private int _port = 9600;
        private int _timeout;
        private int _retries;

        private enPLCType _plcType;
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

        public OmronPLC(byte localNodeId, byte remoteNodeId, string remoteHost, int port = 9600, int timeout = 2000, int retries = 1)
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

        public async Task<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            // Initialize the Channel

            // Identify the PLC Type

            // Set Is Initialized

            // Return True
        }

        public void Dispose()
        {
            if(_channel != null)
            {
                // Dispose of the Channel
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

            // TODO: Validate Address based on the Data Type and PLC Series

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

            // TODO: Validate Address based on the Data Type and PLC Series

            // TODO: Validate Length based on Data Type and PLC Series (+ Start Address Location)

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

            // TODO: Validate Address based on the Data Type and PLC Series

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

            // TODO: Validate Address based on the Data Type and PLC Series

            // TODO: Validate Values Length based on Data Type and PLC Series (+ Start Address Location)

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


        #endregion
    }
}
