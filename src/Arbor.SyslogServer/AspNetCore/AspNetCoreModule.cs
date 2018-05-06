using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.SyslogServer.AspNetCore
{
    public class AspNetCoreModule : Module
    {
        private readonly IServiceCollection _services;

        public AspNetCoreModule([NotNull] IServiceCollection services)
        {
            services.AddMemoryCache();

            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Populate(_services);
        }
    }
}
