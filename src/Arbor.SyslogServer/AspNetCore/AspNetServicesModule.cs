using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.SyslogServer.AspNetCore
{
    internal class AspNetServicesModule : Module
    {
        private readonly IServiceCollection _services;

        public AspNetServicesModule([NotNull] IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Populate(_services);
        }
    }
}
