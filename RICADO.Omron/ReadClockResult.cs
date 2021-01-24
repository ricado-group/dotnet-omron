using System;

namespace RICADO.Omron
{
    public struct ReadClockResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
        public DateTime Clock;
        public int DayOfWeek;
    }
}
