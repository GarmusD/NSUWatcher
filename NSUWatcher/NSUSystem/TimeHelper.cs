using NSU.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NSUWatcher.NSUSystem
{
    public class TimeHelper
    {
        private readonly string LogTag = "TimeHelper";
        private Timer timer;
        private DateTime lasttime;
        public bool TimeIsOk { get; private set; } = false;
        public event EventHandler TimeChanged;

        public TimeHelper()
        {
            timer = new Timer(5000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var diff = now - lasttime;
            if(diff.TotalSeconds > 15)
            {
                if (now.Year >= 2018)
                {
                    TimeIsOk = true;
                    NSULog.Debug(LogTag, $"Time change detected. From {lasttime.ToString()}, to {now.ToString()}");
                    TimeChanged?.Invoke(this, null);
                }
            }
            lasttime = now;
        }

        public void Start()
        {
            if (!timer.Enabled)
            {
                var now = DateTime.Now;
                if (now.Year >= 2018)
                {
                    TimeIsOk = true;
                }
                lasttime = now;
                timer.Enabled = true;
            }
        }

        public void Stop()
        {
            timer.Enabled = false;
            timer.Dispose();
        }
    }
}
