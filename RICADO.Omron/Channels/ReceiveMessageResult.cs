using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Omron.Channels
{
    internal struct ReceiveMessageResult
    {
        internal Memory<byte> Message;
        internal int Bytes;
        internal int Packets;
    }
}
