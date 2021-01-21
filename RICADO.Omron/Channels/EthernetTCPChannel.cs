using System;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;

namespace RICADO.Omron.Channels
{
    internal class EthernetTCPChannel : EthernetChannel
    {
        #region Private Properties

        private TcpClient _client;

        #endregion


        #region Constructors

        internal EthernetTCPChannel(string remoteHost, int port) : base(remoteHost, port)
        {
            initializeClient();
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


        #region Protected Methods

        protected override void DestroyAndInitializeClient()
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

            initializeClient();
        }

        protected override async Task<SendMessageResult> SendMessageAsync(ReadOnlyMemory<byte> message, int timeout, CancellationToken cancellationToken)
        {

        }

        protected override async Task<ReceiveMessageResult> ReceiveMessageAsync(int timeout, CancellationToken cancellationToken)
        {

        }

        #endregion


        #region Private Methods

        private void initializeClient()
        {
            if (_client != null)
            {
                return;
            }

            _client = new TcpClient(base.RemoteHost, base.Port);

            // TODO: Connect the Client
        }

        #endregion
    }
}
