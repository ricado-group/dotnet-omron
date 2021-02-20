using System;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Omron.Requests;
using RICADO.Omron.Responses;

namespace RICADO.Omron.Channels
{
    internal abstract class EthernetChannel : IDisposable
    {
        #region Private Properties

        private string _remoteHost;
        private int _port;

        private byte _requestId = 0;

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        #endregion


        #region Internal Properties

        internal string RemoteHost
        {
            get
            {
                return _remoteHost;
            }
        }

        internal int Port
        {
            get
            {
                return _port;
            }
        }

        #endregion


        #region Constructors

        internal EthernetChannel(string remoteHost, int port)
        {
            _remoteHost = remoteHost;
            _port = port;
        }

        #endregion


        #region Public Methods

        public abstract void Dispose();

        #endregion


        #region Internal Methods

        internal abstract Task InitializeAsync(int timeout, CancellationToken cancellationToken);
        
        internal async Task<ProcessRequestResult> ProcessRequestAsync(FINSRequest request, int timeout, int retries, CancellationToken cancellationToken)
        {
            int attempts = 0;
            Memory<byte> responseMessage = new Memory<byte>();
            int bytesSent = 0;
            int packetsSent = 0;
            int bytesReceived = 0;
            int packetsReceived = 0;
            DateTime startTimestamp = DateTime.Now;

            while (attempts <= retries)
            {
                try
                {
                    await _semaphore.WaitAsync(cancellationToken);
                    
                    if (attempts > 0)
                    {
                        await DestroyAndInitializeClient(timeout, cancellationToken);
                    }

                    // Build the Request into a Message we can Send
                    ReadOnlyMemory<byte> requestMessage = request.BuildMessage(getNextRequestId());

                    // Send the Message
                    SendMessageResult sendResult = await SendMessageAsync(requestMessage, timeout, cancellationToken);

                    bytesSent += sendResult.Bytes;
                    packetsSent += sendResult.Packets;

                    // Receive a Response
                    ReceiveMessageResult receiveResult = await ReceiveMessageAsync(timeout, cancellationToken);

                    bytesReceived += receiveResult.Bytes;
                    packetsReceived += receiveResult.Packets;
                    responseMessage = receiveResult.Message;

                    break;
                }
                catch (OmronException)
                {
                    if(attempts >= retries)
                    {
                        throw;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }

                // Increment the Attempts
                attempts++;
            }

            try
            {
                return new ProcessRequestResult
                {
                    BytesSent = bytesSent,
                    PacketsSent = packetsSent,
                    BytesReceived = bytesReceived,
                    PacketsReceived = packetsReceived,
                    Duration = DateTime.Now.Subtract(startTimestamp).TotalMilliseconds,
                    Response = FINSResponse.CreateNew(responseMessage, request),
                };
            }
            catch (FINSException e)
            {
                throw new OmronException("Received a FINS Error Response from Omron PLC '" + _remoteHost + ":" + _port + "'", e);
            }
        }

        #endregion


        #region Protected Methods

        protected abstract Task DestroyAndInitializeClient(int timeout, CancellationToken cancellationToken);

        protected abstract Task<SendMessageResult> SendMessageAsync(ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken);

        protected abstract Task<ReceiveMessageResult> ReceiveMessageAsync(int timeout, CancellationToken cancellationToken);

        #endregion


        #region Private Methods

        private byte getNextRequestId()
        {
            if (_requestId == byte.MaxValue)
            {
                _requestId = byte.MinValue;
            }
            else
            {
                _requestId++;
            }

            return _requestId;
        }

        #endregion
    }
}
