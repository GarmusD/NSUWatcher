using Serilog.Events;

namespace NSUWatcher.Logger
{
    public class TelegramLogCfg
    {
        public bool Enabled { get; set; } = false;
        public string BotToken { get; set; } = string.Empty;
        public long ChannelId { get; set; } = 0;
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Warning;
    }
}
