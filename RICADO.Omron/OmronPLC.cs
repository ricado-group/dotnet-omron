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
    // TODO: Add Documentation to all Classes, Interfaces, Structs and Enums

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

        private string _controllerModel;
        private string _controllerVersion;

        #endregion


        #region Internal Properties

        internal EthernetChannel Channel => _channel;

        internal bool IsNSeries => _plcType switch
        {
            enPLCType.NJ101 => true,
            enPLCType.NJ301 => true,
            enPLCType.NJ501 => true,
            enPLCType.NX1P2 => true,
            enPLCType.NX102 => true,
            enPLCType.NX701 => true,
            enPLCType.NY512 => true,
            enPLCType.NY532 => true,
            enPLCType.NJ_NX_NY_Series => true,
            _ => false,
        };

        internal bool IsCSeries => _plcType switch
        {
            enPLCType.CP1 => true,
            enPLCType.CJ2 => true,
            enPLCType.C_Series => true,
            _ => false,
        };

        #endregion


        #region Public Properties

        public byte LocalNodeID => _localNodeId;

        public byte RemoteNodeID => _remoteNodeId;

        public enConnectionMethod ConnectionMethod => _connectionMethod;

        public string RemoteHost => _remoteHost;

        public int Port => _port;

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

        public enPLCType PLCType => _plcType;

        public bool IsInitialized => _isInitialized;

        public string ControllerModel => _controllerModel;

        public string ControllerVersion => _controllerVersion;

        public int MaximumReadWordLength => _plcType == enPLCType.CP1 ? 499 : 999;

        public int MaximumWriteWordLength => _plcType == enPLCType.CP1 ? 496 : 996;

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

            await requestControllerInformation(cancellationToken);

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

            if (validateBitDataType(dataType) == false)
            {
                throw new ArgumentException("The Data Type '" + Enum.GetName(typeof(enMemoryBitDataType), dataType) + "' is not Supported on this PLC", nameof(dataType));
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

            if (validateWordDataType(dataType) == false)
            {
                throw new ArgumentException("The Data Type '" + Enum.GetName(typeof(enMemoryWordDataType), dataType) + "' is not Supported on this PLC", nameof(dataType));
            }

            if (validateWordStartAddress(startAddress, length, dataType) == false)
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

        public Task<WriteBitsResult> WriteBitAsync(bool value, ushort address, byte bitIndex, enMemoryBitDataType dataType, CancellationToken cancellationToken)
        {
            return WriteBitsAsync(new bool[] { value }, address, bitIndex, dataType, cancellationToken);
        }

        public async Task<WriteBitsResult> WriteBitsAsync(bool[] values, ushort address, byte startBitIndex, enMemoryBitDataType dataType, CancellationToken cancellationToken)
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

            if (validateBitDataType(dataType) == false)
            {
                throw new ArgumentException("The Data Type '" + Enum.GetName(typeof(enMemoryBitDataType), dataType) + "' is not Supported on this PLC", nameof(dataType));
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

        public Task<WriteWordsResult> WriteWordAsync(short value, ushort address, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            return WriteWordsAsync(new short[] { value }, address, dataType, cancellationToken);
        }

        public async Task<WriteWordsResult> WriteWordsAsync(short[] values, ushort startAddress, enMemoryWordDataType dataType, CancellationToken cancellationToken)
        {
            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array cannot be Empty");
            }

            if(values.Length > MaximumWriteWordLength)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The Values Array Length cannot be greater than " + MaximumWriteWordLength.ToString());
            }

            if (validateWordDataType(dataType) == false)
            {
                throw new ArgumentException("The Data Type '" + Enum.GetName(typeof(enMemoryWordDataType), dataType) + "' is not Supported on this PLC", nameof(dataType));
            }

            if (validateWordStartAddress(startAddress, values.Length, dataType) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "The Start Address and Values Array Length combined are greater than the Maximum Address for the '" + Enum.GetName(typeof(enMemoryWordDataType), dataType) + "' Data Type");
            }

            WriteMemoryAreaWordRequest request = WriteMemoryAreaWordRequest.CreateNew(this, startAddress, dataType, values);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            WriteMemoryAreaWordResponse.Validate(request, requestResult.Response);

            return new WriteWordsResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
            };
        }

        public async Task<ReadClockResult> ReadClockAsync(CancellationToken cancellationToken)
        {
            ReadClockRequest request = ReadClockRequest.CreateNew(this);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            ReadClockResponse.ClockResult result = ReadClockResponse.ExtractClock(request, requestResult.Response);

            return new ReadClockResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
                Clock = result.ClockDateTime,
                DayOfWeek = result.DayOfWeek
            };
        }

        public Task<WriteClockResult> WriteClockAsync(DateTime newDateTime, CancellationToken cancellationToken)
        {
            return WriteClockAsync(newDateTime, (int)newDateTime.DayOfWeek, cancellationToken);
        }

        public async Task<WriteClockResult> WriteClockAsync(DateTime newDateTime, int newDayOfWeek, CancellationToken cancellationToken)
        {
            DateTime minDateTime = new DateTime(1998, 1, 1, 0, 0, 0);

            if (newDateTime < minDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(newDateTime), "The Date Time Value cannot be less than '" + minDateTime.ToString() + "'");
            }

            DateTime maxDateTime = new DateTime(2069, 12, 31, 23, 59, 59);

            if (newDateTime > maxDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(newDateTime), "The Date Time Value cannot be greater than '" + maxDateTime.ToString() + "'");
            }

            if(newDayOfWeek < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newDayOfWeek), "The Day of Week Value cannot be less than 0");
            }

            if(newDayOfWeek > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(newDayOfWeek), "The Day of Week Value cannot be greater than 6");
            }
            
            WriteClockRequest request = WriteClockRequest.CreateNew(this, newDateTime, (byte)newDayOfWeek);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            WriteClockResponse.Validate(request, requestResult.Response);

            return new WriteClockResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
            };
        }

        public async Task<ReadCycleTimeResult> ReadCycleTimeAsync(CancellationToken cancellationToken)
        {
            if(IsNSeries == true && _plcType != enPLCType.NJ101 && _plcType != enPLCType.NJ301 && _plcType != enPLCType.NJ501)
            {
                throw new OmronException("Read Cycle Time is not Supported on the NX/NY Series PLC");
            }

            ReadCycleTimeRequest request = ReadCycleTimeRequest.CreateNew(this);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            ReadCycleTimeResponse.CycleTimeResult result = ReadCycleTimeResponse.ExtractCycleTime(request, requestResult.Response);

            return new ReadCycleTimeResult
            {
                BytesSent = requestResult.BytesSent,
                PacketsSent = requestResult.PacketsSent,
                BytesReceived = requestResult.BytesReceived,
                PacketsReceived = requestResult.PacketsReceived,
                Duration = requestResult.Duration,
                MinimumCycleTime = result.MinimumCycleTime,
                MaximumCycleTime = result.MaximumCycleTime,
                AverageCycleTime = result.AverageCycleTime,
            };
        }

        #endregion


        #region Private Methods

        private bool validateBitAddress(ushort address, enMemoryBitDataType dataType)
        {
            return dataType switch
            {
                enMemoryBitDataType.DataMemory => address < (_plcType == enPLCType.NX1P2 ? 16000 : 32768),
                enMemoryBitDataType.CommonIO => address < 6144,
                enMemoryBitDataType.Work => address < 512,
                enMemoryBitDataType.Holding => address < 1536,
                enMemoryBitDataType.Auxiliary => address < (_plcType == enPLCType.CJ2 ? 11536 : 960),
                _ => false,
            };
        }

        private bool validateBitDataType(enMemoryBitDataType dataType)
        {
            return dataType switch
            {
                enMemoryBitDataType.DataMemory => _plcType != enPLCType.CP1,
                enMemoryBitDataType.CommonIO => true,
                enMemoryBitDataType.Work => true,
                enMemoryBitDataType.Holding => true,
                enMemoryBitDataType.Auxiliary => !IsNSeries,
                _ => false,
            };
        }

        private bool validateWordStartAddress(ushort startAddress, int length, enMemoryWordDataType dataType)
        {
            return dataType switch
            {
                enMemoryWordDataType.DataMemory => startAddress + (length - 1) < (_plcType == enPLCType.NX1P2 ? 16000 : 32768),
                enMemoryWordDataType.CommonIO => startAddress + (length - 1) < 6144,
                enMemoryWordDataType.Work => startAddress + (length - 1) < 512,
                enMemoryWordDataType.Holding => startAddress + (length - 1) < 1536,
                enMemoryWordDataType.Auxiliary => startAddress + (length - 1) < (_plcType == enPLCType.CJ2 ? 11536 : 960),
                _ => false,
            };
        }

        private bool validateWordDataType(enMemoryWordDataType dataType)
        {
            return dataType switch
            {
                enMemoryWordDataType.DataMemory => true,
                enMemoryWordDataType.CommonIO => true,
                enMemoryWordDataType.Work => true,
                enMemoryWordDataType.Holding => true,
                enMemoryWordDataType.Auxiliary => !IsNSeries,
                _ => false,
            };
        }

        private async Task requestControllerInformation(CancellationToken cancellationToken)
        {
            ReadCPUUnitDataRequest request = ReadCPUUnitDataRequest.CreateNew(this);

            ProcessRequestResult requestResult = await _channel.ProcessRequestAsync(request, _timeout, _retries, cancellationToken);

            ReadCPUUnitDataResponse.CPUUnitDataResult result = ReadCPUUnitDataResponse.ExtractData(requestResult.Response);

            if(result.ControllerModel != null && result.ControllerModel.Length > 0)
            {
                _controllerModel = result.ControllerModel;

                if (_controllerModel.StartsWith("NJ101"))
                {
                    _plcType = enPLCType.NJ101;
                }
                else if (_controllerModel.StartsWith("NJ301"))
                {
                    _plcType = enPLCType.NJ301;
                }
                else if (_controllerModel.StartsWith("NJ501"))
                {
                    _plcType = enPLCType.NJ501;
                }
                else if (_controllerModel.StartsWith("NX1P2"))
                {
                    _plcType = enPLCType.NX1P2;
                }
                else if (_controllerModel.StartsWith("NX102"))
                {
                    _plcType = enPLCType.NX102;
                }
                else if (_controllerModel.StartsWith("NX701"))
                {
                    _plcType = enPLCType.NX701;
                }
                else if(_controllerModel.StartsWith("NJ") || _controllerModel.StartsWith("NX") || _controllerModel.StartsWith("NY"))
                {
                    _plcType = enPLCType.NJ_NX_NY_Series;
                }
                else if(_controllerModel.StartsWith("CJ2"))
                {
                    _plcType = enPLCType.CJ2;
                }
                else if(_controllerModel.StartsWith("CP1"))
                {
                    _plcType = enPLCType.CP1;
                }
                else if(_controllerModel.StartsWith("C"))
                {
                    _plcType = enPLCType.C_Series;
                }
                else
                {
                    _plcType = enPLCType.Unknown;
                }
            }

            if(result.ControllerVersion != null && result.ControllerVersion.Length > 0)
            {
                _controllerVersion = result.ControllerVersion;
            }
        }

        #endregion
    }
}
