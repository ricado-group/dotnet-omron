using System;

namespace RICADO.Omron
{
    public struct WriteClockResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
    }
}
