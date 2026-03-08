using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Skillexa.Engine;
using Skillexa.Engine.Data;
using Skillexa.Engine.Modules;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, lc) => lc.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddDbContext<EngineDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.ConfigureContainer(new AutofacServiceProviderFactory(), container =>
{
    container.RegisterModule(new DataModule());
    container.RegisterModule(new CqrsModule());
    // container.RegisterModule(new MessagingModule(builder.Configuration));
    // container.RegisterModule(new StorageModule(builder.Configuration));
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

if (builder.Environment.IsDevelopment())
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EngineDbContext>();
    await db.Database.MigrateAsync();
}

host.Run();
