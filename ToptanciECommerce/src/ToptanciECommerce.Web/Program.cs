using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Infrastructure;
using ToptanciECommerce.Infrastructure.Data;

// Required for Npgsql: treat DateTime as unspecified (no UTC enforcement)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Support PORT environment variable (used by Render.com)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ── Infrastructure (EF Core, Repositories, ImageService) ──────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Identity ───────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie ──────────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Session (for Cart) ──────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// Large multipart uploads (product images)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
var showDetailedErrors = app.Configuration.GetValue<bool>("ShowDetailedErrors")
    || string.Equals(
        Environment.GetEnvironmentVariable("SHOW_DETAILED_ERRORS"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (app.Environment.IsDevelopment() || showDetailedErrors)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            var ex = feature?.Error;
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalException");

            var traceId = context.TraceIdentifier;
            if (ex != null)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception. TraceId={TraceId}, Path={Path}, Endpoint={Endpoint}",
                    traceId,
                    feature?.Path,
                    context.Request.Path);
            }
            else
            {
                logger.LogError(
                    "Exception handler invoked but no exception (TraceId={TraceId}, Path={Path})",
                    traceId,
                    context.Request.Path);
            }

            // New request gets a different TraceIdentifier; pass the logged id for log correlation.
            context.Response.Redirect(
                $"/Home/Error?rid={Uri.EscapeDataString(traceId)}");
            await Task.CompletedTask;
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ── Areas ────────────────────────────────────────────────────────────────────
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Seed admin user + demo data on first run ─────────────────────────────────
await SeedDataAsync(app); // Run migrations first
await SeedAsync(app);     // Then seed roles/users

app.Logger.LogInformation(
    "Application ready. Environment={Environment}, ShowDetailedErrors={ShowDetailed}",
    app.Environment.EnvironmentName,
    showDetailedErrors);

app.Run();

static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    foreach (var role in new[] { "Admin", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed default admin
    const string adminEmail = "admin@toptanci.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Super",
            LastName = "Admin",
            CompanyName = "ToptanciB2B",
            IsApproved = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // __EFMigrationsHistory may exist from a failed migration attempt and block EnsureCreated.
    // Dropping it allows EnsureCreated to detect missing tables and create the full schema.
    try
    {
        await db.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
    }
    catch { /* safe to ignore if it doesn't exist */ }

    // EnsureCreated creates all tables from the model. It is idempotent:
    // it checks if model tables exist before creating them, so existing data is never dropped.
    await db.Database.EnsureCreatedAsync();

    await DataSeeder.SeedAsync(db);
}
