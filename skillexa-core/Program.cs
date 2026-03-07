using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.MapOpenApi();

app.MapGet("/", () => "Hello World!")
    .WithName("GetRoot")
    .WithSummary("Root endpoint")
    .WithDescription("Returns a greeting message from Skillexa-Core");

app.Run();
