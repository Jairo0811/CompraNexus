using Xunit;

namespace CompraNexus.Tests.SqlServer;

/// <summary>
/// Prueba contra SQL Server REAL. Se omite si no existe la variable de entorno COMPRANEXUS_TEST_SQL
/// (cadena de conexión al SERVIDOR, sin base de datos). Cada prueba crea y elimina su propia base temporal
/// «CompraNexus_Test_&lt;guid&gt;»; jamás toca la base «CompraNexus» del usuario.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string VariableName = "COMPRANEXUS_TEST_SQL";
    public static bool Enabled => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariableName));

    public SqlServerFactAttribute()
    {
        if (!Enabled) Skip = $"Defina {VariableName} para ejecutar las pruebas con SQL Server real (ver docs/TESTING.md).";
    }
}
