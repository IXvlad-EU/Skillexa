using Autofac;
using Skillexa.Engine.Commands;
using Skillexa.Engine.Queries;

namespace Skillexa.Engine.Modules;

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
