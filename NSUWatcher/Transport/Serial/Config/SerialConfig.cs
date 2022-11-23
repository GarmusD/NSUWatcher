namespace NSUWatcher.Transport.Serial.Config
{
    public class SerialConfig
    {
        public string ComPort { get; set; } = string.Empty;
        public int BaudRate { get; set; }
        public RebootMcuConfig RebootMcu { get; set; } = new RebootMcuConfig();
    }
}
