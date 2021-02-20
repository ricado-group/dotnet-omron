using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;
using RICADO.Omron.Responses;

namespace RICADO.Omron.Channels
{
    internal class EthernetTCPChannel : EthernetChannel
    {
        #region Enums

        internal enum enTCPCommandCode : byte
        {
            NodeAddressToPLC = 0,
            NodeAddressFromPLC = 1,
            FINSFrame = 2,
        }

        #endregion


        #region Constants

        internal const int TCP_HEADER_LENGTH = 16;

        #endregion


        #region Private Properties

        private TcpClient _client;

        private byte _localNodeId;
        private byte _remoteNodeId;

        #endregion


        #region Internal Properties

        internal byte LocalNodeID => _localNodeId;

        internal byte RemoteNodeID => _remoteNodeId;

        #endregion


        #region Constructors

        internal EthernetTCPChannel(string remoteHost, int port) : base(remoteHost, port)
        {
        }

        #endregion


        #region Public Methods

        public override void Dispose()
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                _client.Dispose();
            }
            finally
            {
                _client = null;
            }
        }

        #endregion


        #region Internal Methods

        internal override Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            return initializeClient(timeout, cancellationToken);
        }

        #endregion


        #region Protected Methods

        protected override async Task DestroyAndInitializeClient(int timeout, CancellationToken cancellationToken)
        {
            try
            {
                _client?.Dispose();
            }
            finally
            {
                _client = null;
            }

            try
            {
                await initializeClient(timeout, cancellationToken);
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Re-Connect within the Timeout Period to Omron PLC '" + base.RemoteHost + ":" + base.Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Re-Connect to Omron PLC '" + base.RemoteHost + ":" + base.Port + "'", e);
            }
        }

        protected override Task<SendMessageResult> SendMessageAsync(ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken)
        {
            return sendMessageAsync(enTCPCommandCode.FINSFrame, message, timeout, cancellationToken);
        }

        protected override Task<ReceiveMessageResult> ReceiveMessageAsync(int timeout, CancellationToken cancellationToken)
        {
            return receiveMessageAsync(enTCPCommandCode.FINSFrame, timeout, cancellationToken);
        }

        #endregion


        #region Private Methods

        private async Task initializeClient(int timeout, CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                return;
            }

            _client = new TcpClient(base.RemoteHost, base.Port);

            await _client.ConnectAsync(timeout, cancellationToken);

            try
            {
                // Send Auto-Assign Client Node Request
                SendMessageResult sendResult = await sendMessageAsync(enTCPCommandCode.NodeAddressToPLC, new byte[4], timeout, cancellationToken);

                // Receive Client Node ID
                ReceiveMessageResult receiveResult = await receiveMessageAsync(enTCPCommandCode.NodeAddressFromPLC, timeout, cancellationToken);

                if(receiveResult.Message.Length < 8)
                {
                    throw new OmronException("Failed to Negotiate a TCP Connection with Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - TCP Negotiation Message Length was too Short");
                }

                byte[] tcpNegotiationMessage = receiveResult.Message.Slice(0, 8).ToArray();

                if(tcpNegotiationMessage[3] == 0 || tcpNegotiationMessage[3] == 255)
                {
                    throw new OmronException("Failed to Negotiate a TCP Connection with Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - TCP Negotiation Message contained an Invalid Local Node ID");
                }

                _localNodeId = tcpNegotiationMessage[3];

                if (tcpNegotiationMessage[7] == 0 || tcpNegotiationMessage[7] == 255)
                {
                    throw new OmronException("Failed to Negotiate a TCP Connection with Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - TCP Negotiation Message contained an Invalid Remote Node ID");
                }

                _remoteNodeId = tcpNegotiationMessage[7];
            }
            catch (OmronException e)
            {
                throw new OmronException("Failed to Negotiate a TCP Connection with Omron PLC '" + base.RemoteHost + ":" + base.Port + "'", e);
            }
        }

        private async Task<SendMessageResult> sendMessageAsync(enTCPCommandCode command, ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken)
        {
            SendMessageResult result = new SendMessageResult
            {
                Bytes = 0,
                Packets = 0,
            };

            ReadOnlyMemory<byte> tcpMessage = buildFinsTcpMessage(command, message);

            try
            {
                result.Bytes += await _client.SendAsync(tcpMessage, timeout, cancellationToken);
                result.Packets += 1;
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Send FINS Message within the Timeout Period to Omron PLC '" + base.RemoteHost + ":" + base.Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Send FINS Message to Omron PLC '" + base.RemoteHost + ":" + base.Port + "'", e);
            }

            return result;
        }

        private async Task<ReceiveMessageResult> receiveMessageAsync(enTCPCommandCode command, int timeout, CancellationToken cancellationToken)
        {
            ReceiveMessageResult result = new ReceiveMessageResult
            {
                Bytes = 0,
                Packets = 0,
                Message = new Memory<byte>(),
            };

            try
            {
                List<byte> receivedData = new List<byte>();
                DateTime startTimestamp = DateTime.UtcNow;

                while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receivedData.Count < TCP_HEADER_LENGTH)
                {
                    Memory<byte> buffer = new byte[4096];
                    TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(timeout).Subtract(DateTime.UtcNow.Subtract(startTimestamp));

                    if (receiveTimeout.TotalMilliseconds >= 50)
                    {
                        int receivedBytes = await _client.ReceiveAsync(buffer, receiveTimeout, cancellationToken);

                        if (receivedBytes > 0)
                        {
                            receivedData.AddRange(buffer.Slice(0, receivedBytes).ToArray());

                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }
                    }
                }

                if (receivedData.Count == 0)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - No Data was Received");
                }

                if (receivedData.Count < TCP_HEADER_LENGTH)
                {
                    throw new OmronException("Failed to Receive FINS Message within the Timeout Period from Omron PLC '" + base.RemoteHost + ":" + base.Port + "'");
                }

                if (receivedData[0] != 'F' || receivedData[1] != 'I' || receivedData[2] != 'N' || receivedData[3] != 'S')
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - The TCP Header was Invalid");
                }

                byte[] tcpHeader = receivedData.GetRange(0, TCP_HEADER_LENGTH).ToArray();

                int tcpMessageDataLength = (int)BitConverter.ToUInt32(new byte[] { receivedData[7], receivedData[6], receivedData[5], receivedData[4] }) - 8;

                if(tcpMessageDataLength <= 0 || tcpMessageDataLength > short.MaxValue)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - The TCP Message Length was Invalid");
                }

                if(receivedData[11] == 3 || receivedData[15] != 0)
                {
                    switch(receivedData[15])
                    {
                        case 1:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The FINS Identifier (ASCII Code) was Invalid.");

                        case 2:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The Data Length is too Long.");

                        case 3:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The Command is not Supported.");

                        case 20:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: All Connections are in Use.");

                        case 21:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The Specified Node is already Connected.");

                        case 22:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: Attempt to Access a Protected Node from an Unspecified IP Address.");

                        case 23:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The Client FINS Node Address is out of Range.");

                        case 24:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: The same FINS Node Address is being used by the Client and Server.");

                        case 25:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: All the Node Addresses Available for Allocation have been Used.");

                        default:
                            throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - Omron TCP Error: Unknown Code '" + receivedData[15] + "'");
                    }
                }

                if(receivedData[8] != 0 || receivedData[9] != 0 || receivedData[10] != 0 || receivedData[11] != (byte)command)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - The TCP Command Received '" + receivedData[11] + "' did not match Expected Command '" + (byte)command + "'");
                }

                if(command == enTCPCommandCode.FINSFrame && tcpMessageDataLength < FINSResponse.HEADER_LENGTH + FINSResponse.COMMAND_LENGTH + FINSResponse.RESPONSE_CODE_LENGTH)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - The TCP Message Length was too short for a FINS Frame");
                }

                receivedData.RemoveRange(0, TCP_HEADER_LENGTH);

                if (receivedData.Count < tcpMessageDataLength)
                {
                    startTimestamp = DateTime.UtcNow;

                    while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receivedData.Count < tcpMessageDataLength)
                    {
                        Memory<byte> buffer = new byte[4096];
                        TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(timeout).Subtract(DateTime.UtcNow.Subtract(startTimestamp));

                        if (receiveTimeout.TotalMilliseconds >= 50)
                        {
                            int receivedBytes = await _client.ReceiveAsync(buffer, receiveTimeout, cancellationToken);

                            if (receivedBytes > 0)
                            {
                                receivedData.AddRange(buffer.Slice(0, receivedBytes).ToArray());
                            }

                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }
                    }
                }

                if (receivedData.Count == 0)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - No Data was Received after TCP Header");
                }

                if (receivedData.Count < tcpMessageDataLength)
                {
                    throw new OmronException("Failed to Receive FINS Message within the Timeout Period from Omron PLC '" + base.RemoteHost + ":" + base.Port + "'");
                }

                if (command == enTCPCommandCode.FINSFrame && receivedData[0] != 0xC0 && receivedData[0] != 0xC1)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "' - The FINS Header was Invalid");
                }

                result.Message = receivedData.ToArray();
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Receive FINS Message within the Timeout Period from Omron PLC '" + base.RemoteHost + ":" + base.Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + base.RemoteHost + ":" + base.Port + "'", e);
            }

            return result;
        }

        private ReadOnlyMemory<byte> buildFinsTcpMessage(enTCPCommandCode command, ReadOnlyMemory<byte> message)
        {
            List<byte> tcpMessage = new List<byte>();

            // FINS Message Identifier
            tcpMessage.Add((byte)'F');
            tcpMessage.Add((byte)'I');
            tcpMessage.Add((byte)'N');
            tcpMessage.Add((byte)'S');

            // Length of Message
            tcpMessage.AddRange(BitConverter.GetBytes(Convert.ToUInt32(4 + 4 + message.Length)).Reverse()); // Command + Error Code + Message Data

            // Command
            tcpMessage.Add(0);
            tcpMessage.Add(0);
            tcpMessage.Add(0);
            tcpMessage.Add((byte)command);

            // Error Code
            tcpMessage.Add(0);
            tcpMessage.Add(0);
            tcpMessage.Add(0);
            tcpMessage.Add(0);

            tcpMessage.AddRange(message.ToArray());

            return tcpMessage.ToArray();
        }

        #endregion
    }
}
