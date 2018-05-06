using System;

namespace Arbor.SyslogServer.Areas.Syslog
{
    public interface IClock
    {
        DateTime UtcNow();
    }
}