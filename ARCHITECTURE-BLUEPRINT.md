# Architecture Blueprint — Clean Architecture + CQRS Blazor App

A reusable blueprint distilled from **dWebShopX**. Follow it to stand up a new
.NET 9 / Blazor Server project with the same layering, patterns, and conventions.

> **Stack:** .NET 9 · Blazor Server (Interactive Server render mode) · EF Core 9
> (Pomelo MySQL) · MediatR · FluentValidation · Mapster · ASP.NET Identity · Serilog

---

## 1. Guiding Principles

The whole solution is built around a small set of rules. Keep them and the rest
falls into place.

1. **Dependencies point inward.** `Web`/`Admin` → `Infrastructure` → `Application` → `Domain`.
   The Domain knows nothing about the outside world; the outer layers depend on
   abstractions the inner layers define.
2. **CQRS everywhere.** Every use case is a `Command` (writes) or a `Query`
   (reads), dispatched through MediatR. UI never touches the `DbContext`.
3. **Vertical slices.** Code is organised by *feature*, not by technical type.
   A feature folder holds its own commands, queries, handlers, validators, and DTOs.
4. **The Application layer owns interfaces; Infrastructure implements them.**
   `IAppDbContext`, `IAppDbContextFactory`, `IEmailService`, `IPricingService`
   live in Application. Their concrete types live in Infrastructure.
5. **Records for messages and DTOs.** Commands, queries, and DTOs are immutable
   `record` types. Handlers use primary constructors.
6. **Validation is a pipeline concern.** FluentValidation runs automatically via a
   MediatR `IPipelineBehavior`, so handlers assume valid input.

---

## 2. Solution Structure

Five projects under `/src`, plus a `/tools` project for one-off jobs. Use an
`.slnx` (or `.sln`) that references them.

```
src/
  YourApp.Domain/          # Entities, enums, base classes, domain interfaces. No dependencies.
  YourApp.Application/      # CQRS features, DTOs, behaviours, service interfaces. Refs Domain.
  YourApp.Infrastructure/   # EF Core DbContext, migrations, service impls, Identity. Refs Application.
  YourApp.Web/              # Public Blazor front-end. Refs Application + Infrastructure.
  YourApp.Admin/            # Admin Blazor front-end (optional second host). Refs Application + Infrastructure.
tools/
  YourApp.DataMigration/    # Console tool for data import/migration.
```

`.slnx` example:

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/YourApp.Domain/YourApp.Domain.csproj" />
    <Project Path="src/YourApp.Application/YourApp.Application.csproj" />
    <Project Path="src/YourApp.Infrastructure/YourApp.Infrastructure.csproj" />
    <Project Path="src/YourApp.Web/YourApp.Web.csproj" />
    <Project Path="src/YourApp.Admin/YourApp.Admin.csproj" />
  </Folder>
  <Folder Name="/tools/">
    <Project Path="tools/YourApp.DataMigration/YourApp.DataMigration.csproj" />
  </Folder>
</Solution>
```

### Why two front-ends?
`Web` is the public-facing site; `Admin` is a separately-hosted back office
gated by `[Authorize(Roles = "Admin")]`. Both are thin — they only send MediatR
messages. Split them if you need independent deployment/auth; otherwise a single
Blazor host with role-guarded pages is fine.

---

## 3. The Domain Layer

Pure C#. No NuGet packages beyond the framework. Two base classes and two marker
interfaces underpin every entity.

```csharp
// Common/Interfaces/IEntity.cs
public interface IEntity { int Id { get; set; } }

// Common/Interfaces/IAuditableEntity.cs
public interface IAuditableEntity : IEntity
{
    int? CreatedBy { get; set; }
    DateTime? CreatedDate { get; set; }
    int? UpdatedBy { get; set; }
    DateTime? UpdatedDate { get; set; }
}

// Common/BaseEntity.cs
public abstract class BaseEntity : IEntity { public int Id { get; set; } }

// Common/BaseAuditableEntity.cs
public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
```

Entities are plain classes with navigation properties. Group them into
sub-namespaces by aggregate (`Entities/Products`, `Entities/Orders`, …). Enums
live next to the entity that owns them.

```csharp
namespace YourApp.Domain.Entities.Products;

public enum ProductStatus { Draft = 0, Active = 1, Archived = 2 }

public class Product : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<Category>? Categories { get; set; }
}
```

**Conventions**
- `int` surrogate keys via `BaseEntity`.
- Non-null string props default to `string.Empty`; optional relations are nullable.
- Keep behaviour thin here — business rules that need data access belong in
  Application handlers or a domain service interface.

---

## 4. The Application Layer

This is the heart of the app. Reference only Domain plus these packages:

```xml
<PackageReference Include="MediatR" Version="14.1.0" />
<PackageReference Include="FluentValidation" Version="12.1.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
<PackageReference Include="Mapster" Version="10.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.4" />
```

### 4.1 Folder layout

```
Application/
  Common/
    Behaviours/ValidationBehaviour.cs
    Interfaces/IAppDbContext.cs
    Interfaces/IAppDbContextFactory.cs
  Features/
    Products/
      Commands/CreateProductCommand.cs
      Queries/GetProductsQuery.cs
    Orders/
      Commands/...
      Queries/...
  Services/            # Cross-cutting service *interfaces* (IEmailService, IPricingService)
  DependencyInjection.cs
```

### 4.2 Database abstractions

The Application layer never sees a concrete `DbContext`. It defines two
interfaces and picks the right one per use case.

```csharp
// Common/Interfaces/IAppDbContext.cs
public interface IAppDbContext : IAsyncDisposable
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    // ... one DbSet per aggregate root you query
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Common/Interfaces/IAppDbContextFactory.cs
public interface IAppDbContextFactory
{
    Task<IAppDbContext> CreateDbContextAsync(CancellationToken ct = default);
}
```

**Two access patterns — this matters for Blazor Server:**

| Use case | Inject | Why |
|----------|--------|-----|
| **Commands** (writes) | `IAppDbContext` | Scoped, change-tracked, one unit of work. |
| **Queries** (reads) | `IAppDbContextFactory` | Creates a short-lived context per query with `AsNoTracking()`. Avoids the "second operation on this DbContext" concurrency errors that Blazor Server's long-lived scope otherwise causes. |

### 4.3 A Command (write)

Command, validator, and handler live in one file. The command is a `record`; the
handler uses a primary constructor to receive `IAppDbContext`.

```csharp
public record CreateProductCommand(
    string Name, string Slug, ProductStatus Status,
    int? BrandId, List<int>? CategoryIds) : IRequest<int>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300);
    }
}

public class CreateProductCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Status = request.Status,
            BrandId = request.BrandId,
        };

        if (request.CategoryIds?.Count > 0)
            product.Categories = db.Categories
                .Where(c => request.CategoryIds.Contains(c.Id)).ToList();

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return product.Id;
    }
}
```

Deletes throw `KeyNotFoundException` when the target is missing — let the UI/host
translate exceptions to user feedback.

### 4.4 A Query (read)

Queries return **DTO records**, never entities. Project with `.Select(...)` so
EF Core builds efficient SQL. Reuse a generic `PagedResult<T>` for lists.

```csharp
public record ProductListItemDto(
    int Id, string Name, string Slug, ProductStatus Status,
    string? BrandName, string? PrimaryImage, decimal? MinPrice = null);

public record PagedResult<T>(List<T> Items, int TotalCount, int StartIndex, int Count);

public record GetProductsQuery(
    int StartIndex = 0, int Count = 20,
    int? BrandId = null, string? Search = null,
    ProductStatus? Status = null) : IRequest<PagedResult<ProductListItemDto>>;

public class GetProductsQueryHandler(IAppDbContextFactory dbFactory)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public async Task<PagedResult<ProductListItemDto>> Handle(
        GetProductsQuery request, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Products.AsNoTracking().AsSplitQuery()
            .Include(p => p.Brand)
            .AsQueryable();

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId);
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip(request.StartIndex).Take(request.Count)
            .Select(p => new ProductListItemDto(
                p.Id, p.Name, p.Slug, p.Status,
                p.Brand != null ? p.Brand.Name : null,
                null, null))
            .ToListAsync(ct);

        return new PagedResult<ProductListItemDto>(items, total, request.StartIndex, request.Count);
    }
}
```

**Query conventions:** always `AsNoTracking()`; add `AsSplitQuery()` when you
`Include` multiple collections; do filtering/paging in SQL (before
`ToListAsync`), never in memory.

### 4.5 Validation behaviour

Registered once, runs before every handler. Handlers therefore never validate.

```csharp
public class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0) throw new ValidationException(failures);
        return await next();
    }
}
```

### 4.6 Application DI

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
```

Assembly scanning means **new handlers and validators wire themselves up** — no
manual registration when you add a feature.

---

## 5. The Infrastructure Layer

Implements everything the Application layer declared. References Application and
brings in EF Core provider, Identity, and service implementations.

### 5.1 DbContext

`AppDbContext` implements `IAppDbContext` and (for Identity) extends
`IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`. Configure the model
with `IEntityTypeConfiguration<T>` classes or inline `OnModelCreating`.

### 5.2 The factory + scoped adapter pattern

Blazor Server holds a scope open for the life of a circuit, so a single scoped
`DbContext` gets hit by concurrent component renders. The fix: register a
**DbContextFactory** and expose both a factory (for queries) and a scoped
instance (for Identity/commands).

```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
    services.AddDbContextFactory<AppDbContext>(options =>
        options.UseMySql(connectionString, serverVersion,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

    // Scoped instance derived from the factory — for Identity & command handlers.
    services.AddScoped<AppDbContext>(sp =>
        sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

    services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // Bind the Application-layer interfaces to concrete types.
    services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    services.AddSingleton<IAppDbContextFactory, AppDbContextFactoryAdapter>();

    services.AddScoped<AppDbContextInitializer>();
    services.AddScoped<IEmailService, SmtpEmailService>();

    return services;
}
```

`AppDbContextFactoryAdapter` wraps `IDbContextFactory<AppDbContext>` and returns
it as the Application-layer `IAppDbContextFactory`, keeping Infrastructure types
out of Application.

### 5.3 Migrations, seeding & audit

- **Migrations** live in `Persistence/Migrations`; generate with
  `dotnet ef migrations add <Name> -p src/YourApp.Infrastructure -s src/YourApp.Web`.
- **`AppDbContextInitializer`** exposes `MigrateAsync()` and `SeedAsync()`, both
  called at startup so a fresh environment self-provisions.
- **Auditing:** `BaseAuditableEntity` gives every entity `CreatedDate`/`UpdatedDate`
  and `CreatedBy`/`UpdatedBy` fields. In the source these are set explicitly where
  needed. **Recommended enhancement for the new project:** override
  `SaveChangesAsync` in `AppDbContext` (or add an `ISaveChangesInterceptor`) to
  stamp them automatically for any `IAuditableEntity` in the change tracker.

---

## 6. The Presentation Layer (Blazor)

Components are thin. They inject `IMediator` and dispatch — no data access, no
business logic.

```razor
@page "/products/{Slug}"
@attribute [Authorize(Roles = "Admin")]
@rendermode InteractiveServer
@inject IMediator Mediator
@inject NavigationManager Navigation

@code {
    [Parameter] public string Slug { get; set; } = "";
    private PagedResult<ProductListItemDto>? _products;

    protected override async Task OnInitializedAsync()
        => _products = await Mediator.Send(new GetProductsQuery(Count: 50));

    private async Task Save(CreateProductCommand cmd)
    {
        var id = await Mediator.Send(cmd);
        Navigation.NavigateTo($"/products/{id}");
    }
}
```

### 6.1 `Program.cs` (host wiring)

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(/* appsettings */)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();                 // MediatR + validators
builder.Services.AddInfrastructureServices(builder.Configuration); // EF + Identity

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/login"; o.SlidingExpiration = true; });

var app = builder.Build();

// Self-provision the database on boot.
using (var scope = app.Services.CreateScope())
{
    var init = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    await init.MigrateAsync();
    await init.SeedAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### 6.2 Host-layer conventions
- **Auth:** cookie auth + `CascadingAuthenticationState`; guard admin pages with
  `[Authorize(Roles = "Admin")]`.
- **Thin services:** put UI-only helpers (e.g. `CurrentUser`, a catalog cache) in
  `Web/Services`, registered scoped. Keep domain logic out of them.
- **SEO / minimal APIs:** endpoints like `/sitemap.xml` or `/logout` can be mapped
  directly in `Program.cs` and use `IMediator` for their data.
- **Logging:** Serilog to console + rolling file.

---

## 7. Request Lifecycle (end to end)

```
Blazor component
     │  Mediator.Send(command / query)
     ▼
MediatR pipeline
     │  ValidationBehaviour  → FluentValidation → throws on failure
     ▼
Handler
     │  Commands: IAppDbContext (tracked, SaveChanges)
     │  Queries:  IAppDbContextFactory → short-lived AsNoTracking context → DTO projection
     ▼
EF Core (Pomelo MySQL)
     ▼
Result record flows back to the component
```

---

## 8. Naming & Conventions Cheat-Sheet

| Concern | Convention |
|---------|-----------|
| Command | `VerbNounCommand` — `record`, `IRequest<TResult>` |
| Query | `GetNoun…Query` — `record`, `IRequest<TResult>` |
| Handler | `<Message>Handler`, primary-constructor DI |
| Validator | `<Command>Validator : AbstractValidator<T>` |
| DTO | `NounListItemDto`, `NounDetailDto` — `record` |
| Paging | `PagedResult<T>(Items, TotalCount, StartIndex, Count)` |
| Feature folder | `Features/<Aggregate>/{Commands,Queries}` |
| Entity base | `BaseEntity` / `BaseAuditableEntity` |
| Write DB access | inject `IAppDbContext` |
| Read DB access | inject `IAppDbContextFactory`, `AsNoTracking()` |
| DI entry points | `AddApplicationServices()`, `AddInfrastructureServices(config)` |

---

## 9. Bootstrapping Checklist for the New Project

1. Create the five projects and set the inward-pointing project references.
2. Add `BaseEntity`, `BaseAuditableEntity`, and the two marker interfaces to Domain.
3. In Application: add MediatR + FluentValidation + Mapster, the
   `ValidationBehaviour`, the two DB interfaces, and `AddApplicationServices()`.
4. In Infrastructure: add `AppDbContext`, the factory + scoped adapter, Identity,
   `AppDbContextInitializer`, and `AddInfrastructureServices()`.
5. Wire `Program.cs`: Serilog, both `Add…Services`, Razor components, auth,
   migrate + seed on boot.
6. Build your first vertical slice (e.g. `Features/Products`) with one command,
   one query, and a validator — confirm assembly scanning picks them up.
7. Generate the initial migration and run.

Keep every new use case as a self-contained slice and the architecture scales
without central files turning into bottlenecks.
