using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule());
    container.RegisterModule(new CqrsModule());
    // container.RegisterModule(new MessagingModule(builder.Configuration));
    // container.RegisterModule(new StorageModule(builder.Configuration));
    // container.RegisterModule(new MappingModule());
});

var jwtIssuer = builder.Configuration["JWT:Issuer"]
    ?? throw new InvalidOperationException("JWT:Issuer is required.");
var jwtAudience = builder.Configuration["JWT:Audience"]
    ?? throw new InvalidOperationException("JWT:Audience is required.");
var jwtSigningKey = LoadJwtSigningKey(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            IssuerSigningKey = jwtSigningKey,
            NameClaimType = "name",
            RoleClaimType = "roles",
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var subject = context.Principal?.FindFirstValue("sub");
                var email = context.Principal?.FindFirstValue("email");

                if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email))
                {
                    context.Fail("JWT must include sub and email claims.");
                }

                return Task.CompletedTask;
            },
        };
    });

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

app.MapPost("/provision", async (
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    return TypedResults.Ok(new { userId });
})
.RequireAuthorization()
.WithName("ProvisionUser")
.WithSummary("Provision current user")
.WithDescription("Returns the current Core user ID, provisioning the user from the validated Portal JWT when needed.")
.WithOpenApi();

app.MapPost("/job-listings/search", async (
    SearchJobListingsRequest request,
    IQueryHandler<SearchJobListingsQuery, IReadOnlyList<SearchJobListingsResult>> handler,
    CancellationToken cancellationToken) =>
{
    var query = new SearchJobListingsQuery(
        request.Skills,
        request.SourceDomains,
        request.Page,
        request.PageSize,
        request.JobTitles,
        request.DescriptionKeywords,
        request.Remote,
        request.Seniorities,
        request.EmploymentTypes,
        request.Countries,
        request.MinSalaryUsd,
        request.MaxSalaryUsd,
        request.PostedWithinDays,
        request.CompanyNames);
    var results = await handler.HandleAsync(query, cancellationToken);
    return TypedResults.Ok(results);
})
.RequireAuthorization()
.WithName("SearchJobListings")
.WithSummary("Search job listings")
.WithDescription("Proxies to TheirStack POST /v1/jobs/search and returns matching job listings.")
.WithOpenApi();

app.MapPost("/documents", async (
    CreateDocumentRequest request,
    ICommandHandler<CreateDocumentCommand, CreateDocumentResult> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    var command = new CreateDocumentCommand(userId, request.TemplateKey, request.TemplateVersion, request.PayloadJson);
    var result = await handler.HandleAsync(command, cancellationToken);
    return TypedResults.Created($"/documents/{result.DocumentId}", result);
})
.RequireAuthorization()
.WithName("CreateDocument")
.WithSummary("Create document")
.WithDescription("Creates a new document and enqueues a GeneratePdf command.")
.WithOpenApi();

app.MapGet("/documents", async (
    IQueryHandler<GetDocumentsQuery, IReadOnlyList<GetDocumentsResult>> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken,
    int page = 1,
    int pageSize = 20) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    var query = new GetDocumentsQuery(userId, page, pageSize);
    var results = await handler.HandleAsync(query, cancellationToken);
    return TypedResults.Ok(results);
})
.RequireAuthorization()
.WithName("GetDocuments")
.WithSummary("List documents")
.WithDescription("Returns a paginated list of documents for the current user.")
.WithOpenApi();

app.MapGet("/documents/{id}", async (
    long id,
    IQueryHandler<GetDocumentByIdQuery, GetDocumentByIdResult?> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    var query = new GetDocumentByIdQuery(id, userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.RequireAuthorization()
.WithName("GetDocumentById")
.WithSummary("Get document")
.WithDescription("Returns a single document by ID for the current user.")
.WithOpenApi();

app.MapPost("/documents/{id}/download-url", async (
    long id,
    IQueryHandler<GetDownloadUrlQuery, GetDownloadUrlResult> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    var query = new GetDownloadUrlQuery(id, userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return Results.Ok(result);
})
.RequireAuthorization()
.WithName("GetDownloadUrl")
.WithSummary("Get download URL")
.WithDescription("Generates a short-lived signed URL for downloading the PDF. Requires document status Succeeded.")
.WithOpenApi();

app.MapGet("/app/usage", async (
    IQueryHandler<GetUsageQuery, GetUsageResult?> handler,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = await GetCurrentUserIdAsync(httpContext, provisioner, cancellationToken);
    var query = new GetUsageQuery(userId);
    var result = await handler.HandleAsync(query, cancellationToken);
    return result is null ? Results.NoContent() : Results.Ok(result);
})
.RequireAuthorization()
.WithName("GetUsage")
.WithSummary("Get provider usage")
.WithDescription("Returns the current provider usage and quota for the current user.")
.WithOpenApi();

app.Run();

static async Task<long> GetCurrentUserIdAsync(
    HttpContext httpContext,
    ICommandHandler<ProvisionUserCommand, ProvisionUserResult> provisioner,
    CancellationToken cancellationToken)
{
    var uid = httpContext.User.FindFirstValue("uid");
    if (long.TryParse(uid, out var userId) && userId > 0)
    {
        return userId;
    }

    var email = httpContext.User.FindFirstValue("email")
        ?? throw new InvalidOperationException("Authenticated user is missing email claim.");
    var displayName = httpContext.User.FindFirstValue("name") ?? email;

    var result = await provisioner.HandleAsync(
        new ProvisionUserCommand(email, displayName), cancellationToken);

    return result.UserId;
}

static SecurityKey LoadJwtSigningKey(IConfiguration configuration)
{
    var publicKey = configuration["JWT:PublicKey"];
    if (string.IsNullOrWhiteSpace(publicKey))
    {
        throw new InvalidOperationException("JWT:PublicKey is required.");
    }

    var rsa = RSA.Create();
    rsa.ImportFromPem(NormalizePem(publicKey).AsSpan());

    return new RsaSecurityKey(rsa);
}

static string NormalizePem(string value)
{
    return value.Replace("\\n", "\n", StringComparison.Ordinal).Trim();
}
