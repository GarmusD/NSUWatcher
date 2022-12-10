namespace TransportDataContracts
{
    internal class Constants
    {
        // SerialClient app exit codes
        public const int ExitCodeOk = 0;
        public const int ExitCodeArgsError = -1;
        public const int ExitCodeDisconnected = -2;
        public const int ExitCodeCommError = -3;
        public const int ExitCodeTcpError = -4;
        public const int ExitCodeUnknown = -100;

        // Command line arguments
        public const string ArgComPort = "com=";
        public const string ArgBaudRate = "baudRate=";
        public const string ArgRebootDtrPulseOnly = "rbtDtrOnly=";
        public const string ArgRebootBaudrate = "rbtBaudRate=";
        public const string ArgRebootDelay = "rbtDelay=";
        public const string ArgRebootReconnectDelay = "rcntDelay=";

        // Supported targets
        public const string DestinationSystem = "system";
        public const string DestinationMcu = "mcu";
        public const string DestinationLogger = "ilogger";

        // Actions to serialClient
        public const string ActionData = "data";
        public const string ActionConnect = "connect";
        public const string ActionDisconnect = "disconnect";
        public const string ActionRebootMcu = "reboot";
        public const string ActionQuit = "quit";

        // Actions from serialClient
        public const string ActionConnected = "connected";
        public const string ActionConnectFailed = "connectFailed";
        public const string ActionDisconnected = "disconnected";
        public const string ActionMcuHalted = "mcuHalted";
        public const string ActionComAppCrash = "comAppCrashed";

        // Log actions from serialClient
        public const string ActionLogDebug = "debug";
        public const string ActionLogError = "error";
        public const string ActionLogFatal = "fatal";
        public const string ActionLogInfo = "info";
        public const string ActionLogWarning = "warn";
    }
}
