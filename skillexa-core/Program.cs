using System.Net.Http.Headers;
using System.Security.Claims;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;
using Skillexa.Core.Commands;
using Skillexa.Core.Commands.CreateDocument;
using Skillexa.Core.Commands.ProvisionUser;
using Skillexa.Core.Data;
using Skillexa.Core.Infrastructure.TheirStack;
using Skillexa.Core.Modules;
using Skillexa.Core.Queries;
using Skillexa.Core.Queries.GetDocumentById;
using Skillexa.Core.Queries.GetDocuments;
using Skillexa.Core.Queries.GetDownloadUrl;
using Skillexa.Core.Queries.GetUsage;
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

app.MapPost("/documents", async (
    CreateDocumentRequest req,
    ICommandHandler<CreateDocumentCommand, CreateDocumentResult> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(ctx, provisioner, cancellationToken);
    var command = new CreateDocumentCommand(userId, req.TemplateKey, req.TemplateVersion, req.PayloadJson);
    var result = await handler.HandleAsync(command, cancellationToken);
    return TypedResults.Created($"/documents/{result.DocumentId}", result);
})
// TODO: .RequireAuthorization()
.WithName("CreateDocument")
.WithSummary("Create document")
.WithDescription("Creates a new document and enqueues a GeneratePdf command.")
.WithOpenApi();

app.MapGet("/documents", async (
    IQueryHandler<GetDocumentsQuery, IReadOnlyList<GetDocumentsResult>> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext ctx,
    CancellationToken cancellationToken,
    int page = 1,
    int pageSize = 20) =>
{
    var userId = await GetCurrentUserIdAsync(ctx, provisioner, cancellationToken);
    var query = new GetDocumentsQuery(userId, page, pageSize);
    var results = await handler.HandleAsync(query, cancellationToken);
    return TypedResults.Ok(results);
})
// TODO: .RequireAuthorization()
.WithName("GetDocuments")
.WithSummary("List documents")
.WithDescription("Returns a paginated list of documents for the current user.")
.WithOpenApi();

app.MapGet("/documents/{id}", async (
    long id,
    IQueryHandler<GetDocumentByIdQuery, GetDocumentByIdResult?> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(ctx, provisioner, cancellationToken);
    var query = new GetDocumentByIdQuery(id, userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
// TODO: .RequireAuthorization()
.WithName("GetDocumentById")
.WithSummary("Get document")
.WithDescription("Returns a single document by ID for the current user.")
.WithOpenApi();

app.MapPost("/documents/{id}/download-url", async (
    long id,
    IQueryHandler<GetDownloadUrlQuery, GetDownloadUrlResult> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(ctx, provisioner, cancellationToken);
    var query = new GetDownloadUrlQuery(id, userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return Results.Ok(result);
})
// TODO: .RequireAuthorization()
.WithName("GetDownloadUrl")
.WithSummary("Get download URL")
.WithDescription("Generates a short-lived signed URL for downloading the PDF. Requires document status Succeeded.")
.WithOpenApi();

app.MapGet("/app/usage", async (
    IQueryHandler<GetUsageQuery, GetUsageResult?> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(ctx, provisioner, cancellationToken);
    var query = new GetUsageQuery(userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return result is null ? Results.NoContent() : Results.Ok(result);
})
// TODO: .RequireAuthorization()
.WithName("GetUsage")
.WithSummary("Get provider usage")
.WithDescription("Returns the current provider usage and quota for the current user.")
.WithOpenApi();

app.Run();

static async Task<long> GetCurrentUserIdAsync(
    HttpContext ctx,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    CancellationToken ct)
{
    var entraObjectId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(entraObjectId))
    {
        return 0L; // stub while auth is public
    }

    var email = ctx.User.FindFirstValue("preferred_username") ?? string.Empty;
    var displayName = ctx.User.FindFirstValue("name") ?? string.Empty;

    var result = await provisioner.HandleAsync(
        new ProvisionUserCommand(entraObjectId, email, displayName), ct);

    return result.UserId;
}

