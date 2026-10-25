# Pruebas

| Capa | Herramienta | Qué demuestra | Qué NO demuestra |
|---|---|---|---|
| Dominio y controladores | xUnit + EF InMemory | Reglas de negocio y respuestas HTTP | Persistencia, índices únicos, concurrencia |
| Contabilidad | xUnit + `FakeAccountingHandler` | Comportamiento ante 2xx/4xx/5xx/timeout **simulados** | Compatibilidad con el servicio real de otros grupos |
| SQL Server real | xUnit (`[SqlServerFact]`) | Persistencia, FKs, índice único, concurrencia de asientos | Servicio contable externo |
| Frontend | Vitest | Funciones puras y manejo de errores de `api.ts` | Interfaz completa |
| Navegación | Playwright (API simulada) | Rutas y formularios básicos | Persistencia real |

## Pruebas con SQL Server real
Se omiten (aparecen como *skipped*) mientras no exista la variable `COMPRANEXUS_TEST_SQL`.
Cada prueba crea una base temporal `CompraNexus_Test_<guid>` y la elimina al terminar; **nunca usa la base `CompraNexus`**.

```powershell
# Con SQL Server de Docker Compose activo en localhost,14330
$saPass = ((Get-Content .env | Where-Object { $_ -like 'MSSQL_SA_PASSWORD=*' }) -replace '^MSSQL_SA_PASSWORD=','').Trim('"')
$env:COMPRANEXUS_TEST_SQL = "Server=localhost,14330;User Id=sa;Password=$saPass;TrustServerCertificate=True;Encrypt=True"
dotnet test backend/CompraNexus.slnx -c Release --filter "FullyQualifiedName~CompraNexus.Tests.SqlServer"
Remove-Item Env:COMPRANEXUS_TEST_SQL
```

Si una ejecución se interrumpe, puede quedar alguna base temporal:
`SELECT name FROM sys.databases WHERE name LIKE 'CompraNexus_Test_%';` (elimínelas manualmente; nunca `CompraNexus`).

En GitHub Actions el job `dotnet` levanta un SQL Server efímero con una contraseña aleatoria generada en el propio job.
