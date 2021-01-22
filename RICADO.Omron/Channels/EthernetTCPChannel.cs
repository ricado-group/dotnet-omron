using System;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;

namespace RICADO.Omron.Channels
{
    internal class EthernetTCPChannel : EthernetChannel
    {
        #region Private Properties

        private System.Net.Sockets.TcpClient _client;

        #endregion


        #region Constructors

        internal EthernetTCPChannel(string remoteHost, int port) : base(remoteHost, port)
        {
        }

        #endregion


        #region Public Methods

        public override void Dispose()
        {
            try
            {
                if (_client != null)
                {
                    _client.Dispose();
                }

                _client = null;
            }
            catch
            {
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
                if (_client != null)
                {
                    _client.Dispose();
                }

                _client = null;
            }
            catch
            {
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

        protected override async Task<SendMessageResult> SendMessageAsync(ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken)
        {
            await Task.Delay(50);

            return new SendMessageResult();
        }

        protected override async Task<ReceiveMessageResult> ReceiveMessageAsync(int timeout, CancellationToken cancellationToken)
        {
            await Task.Delay(50);

            return new ReceiveMessageResult();
        }

        #endregion


        #region Private Methods

        private async Task initializeClient(int timeout, CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                return;
            }

            await Task.Delay(50);

            //_client = new TcpClient(base.RemoteHost, base.Port);

            // TODO: Connect the Client
        }

        #endregion
    }
}
