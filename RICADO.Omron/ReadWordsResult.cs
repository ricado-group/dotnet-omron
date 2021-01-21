using System;

namespace RICADO.Omron
{
    public struct ReadWordsResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
        public Int16[] Values;
    }
}
