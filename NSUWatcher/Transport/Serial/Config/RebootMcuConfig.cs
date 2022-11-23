namespace NSUWatcher.Transport.Serial.Config
{
    public class RebootMcuConfig
    {
        public int BaudRate { get; set; }
        public bool DtrPulseOnly { get; set; }
        public int Delay { get; set; }
        public int ReconnectDelay { get; set; }
    }
}
