using System;

namespace Arbor.SyslogServer.Areas.Syslog
{
    public class Clock : IClock
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}