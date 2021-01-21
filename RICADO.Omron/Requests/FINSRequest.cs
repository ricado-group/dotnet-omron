using System;
using System.Collections.Generic;

namespace RICADO.Omron.Requests
{
    internal abstract class FINSRequest
    {
        #region Constants

        internal const int HEADER_LENGTH = 10;
        internal const int COMMAND_LENGTH = 2;

        #endregion


        #region Private Properties

        private byte _localNodeId;
        private byte _remoteNodeId;

        private byte _functionCode;
        private byte _subFunctionCode;

        #endregion


        #region Internal Properties

        internal byte LocalNodeID
        {
            get
            {
                return _localNodeId;
            }
            set
            {
                _localNodeId = value;
            }
        }

        internal byte RemoteNodeID
        {
            get
            {
                return _remoteNodeId;
            }
            set
            {
                _remoteNodeId = value;
            }
        }

        internal byte FunctionCode
        {
            get
            {
                return _functionCode;
            }
            set
            {
                _functionCode = value;
            }
        }

        internal byte SubFunctionCode
        {
            get
            {
                return _subFunctionCode;
            }
            set
            {
                _subFunctionCode = value;
            }
        }

        #endregion


        #region Constructors

        protected FINSRequest()
        {
        }

        #endregion


        #region Internal Methods

        internal ReadOnlyMemory<byte> BuildMessage()
        {
            List<byte> message = new List<byte>();

            /**
             * Header Section
             */

            // Information Control Field
            message.Add(0x80);

            // Reserved by System
            message.Add(0);

            // Permissible Number of Gateways
            message.Add(0x02);

            // Destination Network Address
            message.Add(0); // Local Network

            // Destination Node Address
            // 0 = Local PLC Unit
            // 1 to 254 = Destination Node Address
            // 255 = Broadcasting
            message.Add(_remoteNodeId);

            // Destination Unit Address
            message.Add(0); // PLC (CPU Unit)

            // Source Network Address
            message.Add(0); // Local Network

            // Source Node Address
            message.Add(_localNodeId); // Local Server

            // Source Unit Address
            message.Add(0);

            // Service ID
            message.Add(0);


            /**
             * Command Section
             */

            // Main Function Code
            message.Add(_functionCode);

            // Sub Function Code
            message.Add(_subFunctionCode);


            /**
             * Data Section
             */

            // Request Data
            message.AddRange(BuildRequestData());


            return new ReadOnlyMemory<byte>(message.ToArray());
        }

        #endregion


        #region Protected Methods

        protected abstract List<byte> BuildRequestData();

        #endregion
    }
}
