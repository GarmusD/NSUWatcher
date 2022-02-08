using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSUWatcher.Notifications
{
    public class PushNotifications
    {
        public bool ValidateDeviceType(string type)
        {
            if(type.Equals("android", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
