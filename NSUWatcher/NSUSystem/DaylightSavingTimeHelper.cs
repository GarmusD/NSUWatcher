using Serilog;
using System;
using System.Timers;

namespace NSUWatcher.NSUSystem
{
    public class DaylightSavingTimeHelper : IDisposable
    {
        public event EventHandler<EventArgs>? DaylightTimeChanged;
        public bool TimeIsOk => CheckTimeIsOk();


        private readonly ILogger _logger;
        private readonly Timer _timer;
        private DateTime _lastTime;
        private bool _timeIsOk = false;


        public DaylightSavingTimeHelper(ILogger logger, bool autoStart = true)
        {
            _logger = logger.ForContext<DaylightSavingTimeHelper>() ?? throw new ArgumentNullException(nameof(logger), "Instance of ILogger cannot be null.");
            _timer = new Timer(5000);
            _timer.Elapsed += Timer_Elapsed;
            if (autoStart) Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeIsOk)
            {
                TimeSpan diff;
                if ((diff = DateTime.Now - _lastTime).TotalSeconds > 15)
                {                    
                    _logger.Debug($"DST change detected. From {_lastTime}, to {DateTime.Now}");
                    _lastTime = DateTime.Now;
                    var evt = DaylightTimeChanged;
                    evt?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool CheckTimeIsOk()
        {
            if (!_timeIsOk)
            {
                _timeIsOk = DateTime.Now.Year >= 2022;
                if (_timeIsOk) _lastTime = DateTime.Now;
            }
            return _timeIsOk;
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                CheckTimeIsOk();
                _timer.Enabled = true;
            }
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
