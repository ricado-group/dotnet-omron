using System;

namespace RICADO.Omron
{
    public struct WriteWordsResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
    }
}
