using Arbor.SyslogServer.Areas.Syslog;
using Autofac;

namespace Arbor.SyslogServer.Application
{
    public class TimeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Clock>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
