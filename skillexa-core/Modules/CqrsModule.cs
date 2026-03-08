using Autofac;
using Skillexa.Core.Commands;
using Skillexa.Core.Queries;

namespace Skillexa.Core.Modules;

public class CqrsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = ThisAssembly;

        // Register all ICommandHandler<,> implementations
        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();

        // Register all IQueryHandler<,> implementations
        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();
    }
}
