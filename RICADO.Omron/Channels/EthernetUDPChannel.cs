using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;
using RICADO.Omron.Responses;

namespace RICADO.Omron.Channels
{
    internal class EthernetUDPChannel : EthernetChannel
    {
        #region Private Properties

        private UdpClient _client;

        #endregion


        #region Constructors

        internal EthernetUDPChannel(string remoteHost, int port) : base(remoteHost, port)
        {
        }

        #endregion


        #region Public Methods

        public override void Dispose()
        {
            try
            {
                _client?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _client = null;
            }
        }


        #endregion


        #region Internal Methods

        internal override async Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            try
            {
                if (!Semaphore.Wait(0))
                {
                    await Semaphore.WaitAsync(cancellationToken);
                }

                destroyClient();

                await initializeClient(timeout, cancellationToken);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        #endregion


        #region Protected Methods

        protected override async Task DestroyAndInitializeClient(int timeout, CancellationToken cancellationToken)
        {
            destroyClient();

            try
            {
                await initializeClient(timeout, cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                throw new OmronException("Failed to Re-Connect to Omron PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Re-Connect within the Timeout Period to Omron PLC '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Re-Connect to Omron PLC '" + RemoteHost + ":" + Port + "'", e);
            }
        }

        protected override async Task<SendMessageResult> SendMessageAsync(ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken)
        {
            SendMessageResult result = new SendMessageResult
            {
                Bytes = 0,
                Packets = 0,
            };
            
            try
            {
                result.Bytes += await _client.SendAsync(message, timeout, cancellationToken);
                result.Packets += 1;
            }
            catch (ObjectDisposedException)
            {
                throw new OmronException("Failed to Send FINS Message to Omron PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Send FINS Message within the Timeout Period to Omron PLC '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Send FINS Message to Omron PLC '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        protected override async Task<ReceiveMessageResult> ReceiveMessageAsync(int timeout, CancellationToken cancellationToken)
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

                while(DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receivedData.Count < FINSResponse.HEADER_LENGTH + FINSResponse.COMMAND_LENGTH + FINSResponse.RESPONSE_CODE_LENGTH)
                {
                    Memory<byte> buffer = new byte[4096];
                    TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(timeout).Subtract(DateTime.UtcNow.Subtract(startTimestamp));

                    if (receiveTimeout.TotalMilliseconds >= 50)
                    {
                        int receivedBytes = await _client.ReceiveAsync(buffer, receiveTimeout, cancellationToken);

                        if(receivedBytes > 0)
                        {
                            receivedData.AddRange(buffer.Slice(0, receivedBytes).ToArray());

                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }
                    }
                }

                if(receivedData.Count == 0)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + RemoteHost + ":" + Port + "' - No Data was Received");
                }

                if(receivedData.Count < FINSResponse.HEADER_LENGTH + FINSResponse.COMMAND_LENGTH + FINSResponse.RESPONSE_CODE_LENGTH)
                {
                    throw new OmronException("Failed to Receive FINS Message within the Timeout Period from Omron PLC '" + RemoteHost + ":" + Port + "'");
                }

                if(receivedData[0] != 0xC0 && receivedData[0] != 0xC1)
                {
                    throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + RemoteHost + ":" + Port + "' - The FINS Header was Invalid");
                }

                result.Message = receivedData.ToArray();
            }
            catch (ObjectDisposedException)
            {
                throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new OmronException("Failed to Receive FINS Message within the Timeout Period from Omron PLC '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new OmronException("Failed to Receive FINS Message from Omron PLC '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        protected override async Task PurgeReceiveBuffer(int timeout, CancellationToken cancellationToken)
        {
            try
            {
                if (_client.Available == 0)
                {
                    await Task.Delay(timeout / 4);
                }

                DateTime startTimestamp = DateTime.UtcNow;
                Memory<byte> buffer = new byte[2000];

                while (_client.Available > 0 && DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout)
                {
                    try
                    {
                        await _client.ReceiveAsync(buffer, timeout, cancellationToken);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        #endregion


        #region Private Methods

        private Task initializeClient(int timeout, CancellationToken cancellationToken)
        {
            _client = new UdpClient(RemoteHost, Port);

            return Task.CompletedTask;
        }

        private void destroyClient()
        {
            try
            {
                _client?.Dispose();
            }
            finally
            {
                _client = null;
            }
        }

        #endregion
    }
}
