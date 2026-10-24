using CompraNexus.Api.Data;
using CompraNexus.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("SqlServer")
    ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException(
        "Falta ConnectionStrings__SqlServer. Configure una cadena SQL Server en variables de entorno o utilice docker compose.");

builder.Services.AddDbContext<CompraNexusDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<AccountingGateway>(client => client.Timeout = TimeSpan.FromSeconds(12));
builder.Services.AddCors(options => options.AddPolicy("frontend", p => p
    .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173")
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors("frontend");
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapGet("/health", async (CompraNexusDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ok", service = "CompraNexus", database = "SQL Server" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapControllers();

// Academic prototype bootstrap in DEVELOPMENT ONLY. Production requires
// a provisioned database, EF migrations, role-based auth and TLS.
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
            break;
        }
        catch (Exception ex) when (i < 39 && (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException))
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.Run();

public partial class Program { }
