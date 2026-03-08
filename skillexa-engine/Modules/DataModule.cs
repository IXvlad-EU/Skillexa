using Autofac;
using Skillexa.Engine.Data.UnitOfWork.Implementations;
using Skillexa.Engine.Data.UnitOfWork.Interfaces;

namespace Skillexa.Engine.Modules;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Repositories — assembly scan for all classes ending with "Repository"
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // Unit of Work
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();
    }
}
