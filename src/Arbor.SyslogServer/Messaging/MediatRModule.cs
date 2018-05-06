using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Variance;
using MediatR;
using Module = Autofac.Module;

namespace Arbor.SyslogServer.Messaging
{
    public class MediatRModule : Module
    {
        private readonly ImmutableArray<Assembly> _scanAssemblies;

        public MediatRModule(ImmutableArray<Assembly> scanAssemblies)
        {
            _scanAssemblies = scanAssemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterSource(new ContravariantRegistrationSource());

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder
                .Register<SingleInstanceFactory>(ctx =>
                {
                    var context = ctx.Resolve<IComponentContext>();
                    return serviceType => context.TryResolve(serviceType, out object o) ? o : null;
                })
                .InstancePerLifetimeScope();

            builder
                .Register<MultiInstanceFactory>(ctx =>
                {
                    var context = ctx.Resolve<IComponentContext>();
                    return serviceType =>
                        (IEnumerable<object>)context.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType));
                })
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(_scanAssemblies.ToArray())
                .Where(type =>
                    type.IsPublic
                    && !type.IsAbstract
                    && type.Name.EndsWith("Handler", StringComparison.Ordinal))
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
