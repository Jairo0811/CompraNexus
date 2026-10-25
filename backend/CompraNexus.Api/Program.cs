using CompraNexus.Api.Data;
using CompraNexus.Api.Services;
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

// Prototipo academico: inicializacion solo en Development.
// Antes de cloud: EF migrations, credenciales minimas, autenticacion y TLS.
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
            break;
        }
        catch (Exception ex) when (i < 39 && (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException))
        {
            app.Logger.LogWarning("SQL Server aun no esta disponible (intento {Attempt}/40).", i + 1);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
app.Run();

public partial class Program { }
