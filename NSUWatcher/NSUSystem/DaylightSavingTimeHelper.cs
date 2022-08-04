using NSU.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NSUWatcher.NSUSystem
{
    public class DaylightSavingTimeHelper
    {
        private readonly string LogTag = "TimeHelper";
        private readonly Timer _timer;
        private DateTime _lastTime;

        public bool TimeIsOk { get; private set; } = false;
        public event EventHandler DaylightTimeChanged;

        public DaylightSavingTimeHelper()
        {
            _timer = new Timer(5000);
            _timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var diff = now - _lastTime;
            if(diff.TotalSeconds > 15)
            {
                if (now.Year >= 2018)
                {
                    TimeIsOk = true;
                    NSULog.Debug(LogTag, $"Time change detected. From {_lastTime}, to {now}");
                    DaylightTimeChanged?.Invoke(this, null);
                }
            }
            _lastTime = now;
        }

        public void Start()
        {
            if (!_timer.Enabled)
            {
                var now = DateTime.Now;
                if (now.Year >= 2018)
                {
                    TimeIsOk = true;
                }
                _lastTime = now;
                _timer.Enabled = true;
            }
        }

        public void Stop()
        {
            _timer.Enabled = false;
            _timer.Dispose();
        }
    }
}
