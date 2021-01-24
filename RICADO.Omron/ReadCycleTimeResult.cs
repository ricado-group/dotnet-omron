using System;

namespace RICADO.Omron
{
    public struct ReadCycleTimeResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
        public double MinimumCycleTime;
        public double MaximumCycleTime;
        public double AverageCycleTime;
    }
}
