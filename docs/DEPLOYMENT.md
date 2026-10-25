# Despliegue, recuperación y configuración de CompraNexus

## Arranque recomendado en Windows (Docker Desktop)

Ejecute desde la **raíz** del proyecto:

```powershell
.\scripts\Start-CompraNexus.ps1
```

El script:
1. Crea `.env` si falta; conserva las otras variables.
2. Verifica `MSSQL_SA_PASSWORD`. Si es débil, vacía o una contraseña de ejemplo, genera una aleatoria fuerte en el archivo local **sin mostrarla**.
3. Arranca SQL Server en `127.0.0.1:14330`, conservando el volumen.
4. Espera a que SQL Server abra su puerto interno, levanta API .NET 10 y React y comprueba `/health`.
5. Comunica el diagnóstico si no hay conexión. No borra bases ni volúmenes.

| Servicio | Desde Windows | Dentro de Compose |
|---|---|---|
| SQL Server | `localhost,14330` | `sqlserver,1433` |
| ASP.NET Core | http://localhost:5098 | `api:8080` |
| React | http://localhost:8088 | `frontend:80` |
| Salud de API | http://localhost:5098/health | — |

La base `CompraNexus` y los datos de demostración se crean solo en Development con `EnsureCreated`. No use este mecanismo como sustituto de migraciones de producción.

### Si modifica el código y prefiere arranque manual

```powershell
docker compose up --build -d
docker compose ps
Invoke-RestMethod http://localhost:5098/health
```

**No utilice** `docker compose config` para compartir logs públicamente: expande la contraseña a texto claro. Si debe compartir registros, redacte secretos y tokens.

## Diagnóstico del error SQL Server 18456 / State 7

El inicio de SQL Server 2022 puede fallar con `Password validation failed` si la contraseña de `sa` no alcanza su política de complejidad. Cuando la instancia se reinicia o desaparece de la red Docker, la API también falla al resolver `sqlserver` y `/health` no responde. Para corregirlo desde Windows:

```powershell
.\scripts\Start-CompraNexus.ps1
docker compose ps
docker compose logs --tail 50 sqlserver
docker compose logs --tail 50 api
```

El script corrige automáticamente una **configuración débil o de ejemplo**. No cambia cuentas del servidor ya inicializado. La contraseña de `sa` existente en un volumen persistente **no se modifica** al cambiar `.env`.

### ¿El volumen ya estaba inicializado con una contraseña anterior?

1. No elimine el volumen `compranexus_sqlserver_data` ni ejecute `docker compose down -v`.
2. Si conoce la contraseña válida anterior, use SSMS para conectarse al servidor con esa contraseña.
3. Para rotarla, ejecute desde una conexión autorizada de SSMS `ALTER LOGIN sa WITH PASSWORD = 'NUEVA_CLAVE_FUERTE';` usando su clave privada real. No publique la consulta con el secreto. Actualice `.env` a esa **misma** contraseña y recree `api` y `sqlserver`.
4. Si no conoce la contraseña vigente o el estado de inicialización quedó inconsistente, deténgase: respalde el volumen y diagnostique los archivos y los registros antes de cualquier recuperación o reinicialización. La herramienta no destruirá datos automáticamente.

En un entorno académico sin datos previos, la instancia que falló **antes** de establecer una contraseña suele recuperarse al generar una nueva clave fuerte y recrear el contenedor, conservando el mismo volumen. Si no funciona, muestre el nuevo log **ocultando las credenciales** para decidir la recuperación adecuada.

## Desarrollo sin la API en Docker

Con SQL Server de Docker operativo, establezca `ConnectionStrings__SqlServer` usando `Server=localhost,14330` en PowerShell y ejecute:

```powershell
dotnet run --project backend/CompraNexus.Api --launch-profile http
```

En otra consola:

```powershell
cd frontend
npm install
npm run dev
```

No ejecute al mismo tiempo una API local y `api` de Docker en el puerto 5098.

## Entrega final (cloud)

Todavía sin proveedor ni sitio público. Antes de publicar: autenticación y autorización reales, HTTPS, migraciones EF Core, usuario SQL de privilegios mínimos, copias de seguridad y gestión de secretos. Confirmar contrato real de Contabilidad e interoperabilidad con los otros grupos. No afirmar despliegue cloud hasta disponer de URL y evidencias.

## Alcance MVC

La solución usa React como vistas SPA, ASP.NET Core API Controllers y entidades EF Core. Confirmar con el profesor si acepta SPA+Web API como su interpretación de MVC o requiere Razor MVC.
