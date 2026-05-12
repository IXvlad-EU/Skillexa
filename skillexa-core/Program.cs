using System.Net.Http.Headers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;
using Skillexa.Core.Data;
using Skillexa.Core.Infrastructure.TheirStack;
using Skillexa.Core.Modules;
using Skillexa.Core.Queries;
using Skillexa.Core.Queries.SearchJobListings;
using Skillexa.Core.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule());
    container.RegisterModule(new CqrsModule());
    // container.RegisterModule(new MessagingModule(builder.Configuration));
    // container.RegisterModule(new StorageModule(builder.Configuration));
    // container.RegisterModule(new MappingModule());
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ITheirStackClient, TheirStackClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TheirStack:BaseUrl"]!);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["TheirStack:ApiKey"]);
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

app.MapGet("/", () => "Hello World!")
    .RequireAuthorization()
    .WithName("GetRoot")
    .WithSummary("Root endpoint")
    .WithDescription("Returns a greeting message from Skillexa-Core");

app.MapPost("/job-listings/search", async (
    SearchJobListingsRequest req,
    IQueryHandler<SearchJobListingsQuery, IReadOnlyList<SearchJobListingsResult>> handler,
    CancellationToken cancellationToken) =>
{
    var query = new SearchJobListingsQuery(req.Skills, req.SourceDomains, req.Page, req.PageSize);
    var results = await handler.HandleAsync(query, cancellationToken);
    return TypedResults.Ok(results);
})
// TODO: .RequireAuthorization()
.WithName("SearchJobListings")
.WithSummary("Search job listings")
.WithDescription("Proxies to TheirStack POST /v1/jobs/search and returns matching job listings.")
.WithOpenApi();

app.Run();

