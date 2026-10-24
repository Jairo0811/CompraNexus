# Despliegue y configuración

## Segundo parcial (local)

Copie `.env.example` a `.env`, cambie `MSSQL_SA_PASSWORD` por una contraseña propia y ejecute:

```powershell
docker compose up --build -d
docker compose ps
```

Interfaz: http://localhost:8088 · API: http://localhost:5098 · Health: http://localhost:5098/health. SQL Server en SSMS: `localhost,1433`, autenticación SQL, usuario `sa`, contraseña del `.env`, base `CompraNexus`. La API crea tablas y datos de demostración en modo Development al conectarse por primera vez.

Alternativa con SQL Server ya instalado: configure `ConnectionStrings__SqlServer` en entorno, ejecute `dotnet run --project backend/CompraNexus.Api --launch-profile http`, luego en otra consola `cd frontend; npm install; npm run dev`. API: http://localhost:5098, interfaz: http://localhost:5173.

**Advertencia:** la contraseña de ejemplo NO debe utilizarse sin cambiarla. El puerto de SQL Server está restringido a loopback. No ejecutar `docker compose down -v` salvo que se quiera perder el volumen de datos.

## Examen final (cloud)

Aún sin proveedor ni sitio público. Antes de publicar: implementar autenticación real y roles, secretos gestionados, HTTPS, migraciones EF Core, usuario SQL mínimo privilegio, backups y cadena cifrada con certificado verificable; configurar contrato/URL externa de Contabilidad, validar idempotencia y realizar pruebas de interoperabilidad contra los otros grupos. No afirmar integración remota ni despliegue hasta obtener evidencia.

## Alcance MVC

La solución usa React como capa Vista, controladores ASP.NET Core y entidades EF Core. Validar con el profesor si acepta SPA+Web API como interpretación del patrón MVC o exige vistas de ASP.NET Core MVC.
