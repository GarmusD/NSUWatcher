using System;

namespace NSUWatcher.NSUWatcherNet
{
    internal class Statistics
    {
        private long lastUncompressed;
        private long lastCompressed;
        private float lastRatio;
        private long totalUncompressed;
        private long totalCompressed;
        private float totalRatio;

        private long uSizeSession;
        private int clntCount;

        public long LastUncompressed
        {
            get { return lastUncompressed; }
        }
        public long LastCompressed
        {
            get { return lastCompressed; }
        }
        public float LastRatio
        {
            get { return lastRatio; }
        }
        public long TotalUncompressed
        {
            get { return totalUncompressed; }
        }
        public long TotalCompressed
        {
            get { return totalCompressed; }
        }
        public float TotalRatio
        {
            get { return totalRatio; }
        }

        public long TotalPacketsSent { get { return packetsSentCount; } } private long packetsSentCount;

        public Statistics()
        {
            lastUncompressed = 0;
            lastCompressed = 0;
            lastRatio = 0.0f;
            totalUncompressed = 0;
            totalCompressed = 0;
            totalRatio = 0.0f;
            packetsSentCount = 0;
        }

        public void BeginSession(long uncomressedSize)
        {
            uSizeSession = uncomressedSize;
            lastUncompressed = uSizeSession;
            clntCount = 0;
        }

        public void EndSession()
        {
            totalUncompressed += uSizeSession * clntCount;
            totalRatio = (float)((float)totalCompressed / (float)totalUncompressed) * 100.0f;
        }

        public void AddPerClient(int cmprL)
        {
            packetsSentCount++;
            totalCompressed += cmprL;
            clntCount++;
        }

        public void EndPerClient()
        {
            
        }

        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;        

        string ToPrettySize(long value, int decimalPlaces = 0)
        {
                var asTb = Math.Round((double)value / OneTb, decimalPlaces);
                var asGb = Math.Round((double)value / OneGb, decimalPlaces);
                var asMb = Math.Round((double)value / OneMb, decimalPlaces);
                var asKb = Math.Round((double)value / OneKb, decimalPlaces);
                string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
                    : asGb > 1 ? string.Format("{0}Gb", asGb)
                    : asMb > 1 ? string.Format("{0}Mb", asMb)
                    : asKb > 1 ? string.Format("{0}Kb", asKb)
                    : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
                return chosenValue;
        }
        
        public override string ToString()
        {
            return $"Total Uncompressed: {ToPrettySize(totalUncompressed)}, Compressed: {ToPrettySize(totalCompressed)}, Ratio: {totalRatio:0.0}%, Packets Sent: {packetsSentCount}";
        }

    }
}
