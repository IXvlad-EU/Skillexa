var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/", () => "Hello World!")
    .WithName("GetRoot")
    .WithSummary("Root endpoint")
    .WithDescription("Returns a greeting message from Skillexa-Core");

app.Run();
