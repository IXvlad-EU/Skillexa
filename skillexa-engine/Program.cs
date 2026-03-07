using Microsoft.EntityFrameworkCore;
using Skillexa.Engine;
using Skillexa.Engine.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<EngineDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

if (builder.Environment.IsDevelopment())
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EngineDbContext>();
    await db.Database.MigrateAsync();
}

host.Run();
