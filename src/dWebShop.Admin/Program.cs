using System.Globalization;
using System.Text;
using dWebShop.Application;
using dWebShop.Application.Features.Orders.Queries;
using dWebShop.Domain.Entities.Orders;
using dWebShop.Domain.Entities.Users;
using dWebShop.Infrastructure;
using dWebShop.Infrastructure.Persistence;
using dWebShop.Admin.Components;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/dwebshop-admin-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseStaticWebAssets();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    await initializer.MigrateAsync();
    await initializer.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login", async (
    HttpContext ctx,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var user = await userManager.FindByEmailAsync(email);
    var userName = user?.UserName ?? email;
    var result = await signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: true);

    if (result.Succeeded)
        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

    var error = result.IsLockedOut ? "Account is locked. Try again later." : "Invalid email or password.";
    return Results.Redirect($"/login?error={Uri.EscapeDataString(error)}");
}).DisableAntiforgery();

app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).RequireAuthorization();

// CSV export for the Orders list — takes the same filters as the page.
// Semicolon-separated with hr-HR numbers and a UTF-8 BOM so Excel in the local
// locale opens it straight into columns.
app.MapGet("/orders/export", async (
    IMediator mediator,
    string? status, string? search, string? channel, string? payment, DateTime? from) =>
{
    OrderStatus? statusFilter = Enum.TryParse<OrderStatus>(status, true, out var s) ? s : null;
    var result = await mediator.Send(new GetAllOrdersQuery(
        1, int.MaxValue, search?.Trim(), statusFilter, channel, payment, from));

    var hr = CultureInfo.GetCultureInfo("hr-HR");
    static string Cell(string? v) => "\"" + (v ?? "").Replace("\"", "\"\"") + "\"";

    var sb = new StringBuilder();
    sb.AppendLine("Order;Date;Customer;Email;Items;Total (KM);Payment;Status;Channel");
    foreach (var o in result.Items)
    {
        sb.AppendLine(string.Join(';',
            o.Id.ToString(),
            o.Created.ToString("yyyy-MM-dd HH:mm"),
            Cell(o.PartnerName),
            Cell(o.PartnerEmail),
            o.ItemCount.ToString(),
            o.Total.ToString("0.00", hr),
            Cell(o.PaymentStatus),
            o.Status.ToString(),
            Cell(o.Channel)));
    }

    var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    return Results.File(bytes, "text/csv", $"orders-{DateTime.Now:yyyyMMdd-HHmm}.csv");
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

app.Run();
