using System.Security.Claims;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("SqlServer")
    ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException(
        "Falta ConnectionStrings__SqlServer. Configure una cadena SQL Server en variables de entorno o utilice Docker Compose.");

builder.Services.AddDbContext<CompraNexusDbContext>(options =>
    options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(4),
        errorNumbersToAdd: null)));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "CompraNexus.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async ctx =>
    {
        var id = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var stamp = ctx.Principal?.FindFirstValue("SecurityStamp");
        if (!int.TryParse(id, out var userId))
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
        var db = ctx.HttpContext.RequestServices.GetRequiredService<CompraNexusDbContext>();
        var user = await db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null || !user.IsActive || user.SecurityStamp != stamp
            || user.Role != ctx.Principal?.FindFirstValue(ClaimTypes.Role))
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CompraNexus.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<IAccountingGateway, AccountingGateway>(client => client.Timeout = TimeSpan.FromSeconds(12));
builder.Services.AddCors(options => options.AddPolicy("frontend", p => p
    .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173")
    .AllowAnyHeader().AllowAnyMethod()));

var keysPath = builder.Configuration["Auth:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection().SetApplicationName("CompraNexus")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

var app = builder.Build();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapGet("/health", async (CompraNexusDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ok", service = "CompraNexus", database = "SQL Server" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapControllers();

// Desarrollo: aprovisionamiento aditivo, conservando el volumen y los datos existentes.
// Pendiente del final: migraciones EF Core baseline y configuración de producción.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CompraNexusDbContext>();
    for (var i = 0; i < 40; i++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            await DevelopmentSeed.RunAsync(db);
            await SchemaPatches.ApplyAsync(db, app.Logger);
            await AdminBootstrap.EnsureInitialAdminAsync(db, app.Configuration, app.Logger);
            break;
        }
        catch (Exception ex) when (i < 39 && (ex is Microsoft.Data.SqlClient.SqlException))
        {
            app.Logger.LogWarning("SQL Server aun no esta disponible (intento {Attempt}/40).", i + 1);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
app.Run();

public partial class Program { }
