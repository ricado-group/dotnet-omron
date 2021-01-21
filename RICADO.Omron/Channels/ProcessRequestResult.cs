using System;
using RICADO.Omron.Responses;

namespace RICADO.Omron.Channels
{
    internal struct ProcessRequestResult
    {
        internal int BytesSent;
        internal int PacketsSent;
        internal int BytesReceived;
        internal int PacketsReceived;
        internal double Duration;
        internal FINSResponse Response;
    }
}
