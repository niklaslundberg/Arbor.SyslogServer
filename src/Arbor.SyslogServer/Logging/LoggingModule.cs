using Autofac;
using Serilog;

namespace Arbor.SyslogServer.Logging
{
    public class LoggingModule : Module
    {
        private readonly ILogger _logger;

        public LoggingModule(ILogger logger)
        {
            _logger = logger;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => _logger).SingleInstance().AsImplementedInterfaces();
        }
    }
}
