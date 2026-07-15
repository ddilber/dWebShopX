using System.Text;
using System.Xml;
using dWebShop.Application;
using dWebShop.Application.Features.Brands.Queries;
using dWebShop.Application.Features.Inspirations.Queries;
using dWebShop.Application.Features.Products.Queries;
using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.Users;
using dWebShop.Infrastructure;
using dWebShop.Infrastructure.Persistence;
using dWebShop.Web;
using dWebShop.Web.Components;
using dWebShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/dwebshop-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ShopCatalogService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    await initializer.MigrateAsync();
    await initializer.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

var sharedUploads = app.Configuration["SharedUploadsPath"];
if (!string.IsNullOrWhiteSpace(sharedUploads))
{
    var absPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, sharedUploads));
    Directory.CreateDirectory(absPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(absPath),
        RequestPath = ""
    });
}

app.MapStaticAssets();
app.MapOgImage();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout endpoint — GET so Blazor interactive components can navigate to it directly
app.MapGet("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

app.MapGet("/sitemap.xml", async (IMediator mediator, IConfiguration config) =>
{
    var baseUrl = (config["SiteUrl"] ?? "https://asgifiks.ba").TrimEnd('/');
    var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

    var entries = new List<(string Loc, string? LastMod, string ChangeFreq, string Priority)>
    {
        ($"{baseUrl}/",          today, "weekly",  "1.0"),
        ($"{baseUrl}/about",     today, "monthly", "0.6"),
        ($"{baseUrl}/services",  today, "monthly", "0.8"),
        ($"{baseUrl}/projects",  today, "monthly", "0.7"),
        ($"{baseUrl}/brands",    today, "monthly", "0.8"),
        ($"{baseUrl}/contact",   today, "yearly",  "0.5"),
        ($"{baseUrl}/shop",      today, "weekly",  "0.9"),
        ($"{baseUrl}/privacy",   today, "yearly",  "0.2"),
        ($"{baseUrl}/terms",     today, "yearly",  "0.2"),
    };

    // Pull brands and products from the DB. If either query fails (e.g.
    // first-run before initial migration), the catch below still emits a
    // partial sitemap rather than 500ing the route.
    try
    {
        var brands = await mediator.Send(new GetBrandsQuery());
        foreach (var brand in brands)
        {
            entries.Add(($"{baseUrl}/brands/{brand.Slug}",             today, "monthly", "0.8"));
            entries.Add(($"{baseUrl}/shop/{brand.Slug}",               today, "weekly",  "0.7"));
            entries.Add(($"{baseUrl}/brands/{brand.Slug}/inspiracija", today, "weekly",  "0.6"));
        }

        var products = await mediator.Send(new GetProductsQuery(
            StartIndex: 0,
            Count: int.MaxValue,
            Status: ProductStatus.Active));
        foreach (var product in products.Items)
            entries.Add(($"{baseUrl}/shop/p/{product.Slug}", today, "weekly", "0.7"));
    }
    catch
    {
        // Brands/products unavailable — skip those entries.
    }

    try
    {
        var articles = await mediator.Send(new GetInspirationsQuery(Published: true));
        foreach (var a in articles)
            entries.Add(($"{baseUrl}/brands/{a.BrandSlug}/inspiracija/{a.Slug}", a.PublishedAt, "monthly", "0.6"));
    }
    catch
    {
        // If the DB is unavailable, still serve a partial sitemap rather than 500.
    }

    using var ms = new MemoryStream();
    // A StringBuilder-backed XmlWriter stamps encoding="utf-16" in the prolog
    // (because .NET strings are UTF-16). Served as UTF-8 that mismatch makes
    // parsers reject the file — Google Search Console: "Unsupported file
    // format". Write UTF-8 bytes (no BOM) so the declaration matches the body.
    var xmlSettings = new XmlWriterSettings
    {
        Indent = true,
        Async = false,
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
    };
    using (var writer = XmlWriter.Create(ms, xmlSettings))
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
        foreach (var (loc, lastMod, changeFreq, priority) in entries)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", loc);
            if (!string.IsNullOrWhiteSpace(lastMod))
                writer.WriteElementString("lastmod", lastMod);
            writer.WriteElementString("changefreq", changeFreq);
            writer.WriteElementString("priority", priority);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    return Results.Bytes(ms.ToArray(), "application/xml");
});

app.Run();
