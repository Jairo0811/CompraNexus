# Recuperación local de acceso administrativo (CompraNexus)

Procedimiento excepcional para restablecer la contraseña **del administrador existente**.
No crea usuarios, no cambia otros roles, no borra la base de datos y **no expone un endpoint HTTP**.

## Requisitos

- Acceso de operador al equipo que ejecuta Docker Compose y su contenedor `api`.
- Ambiente `Development` (comando deshabilitado en `Production`).
- SQL Server encendido, base de datos `CompraNexus` ya inicializada.
- `COMPRANEXUS_ADMIN_EMAIL` debe coincidir con un usuario administrador activo.
- **No se necesita** introducir la contraseña antigua ni modificar `COMPRANEXUS_ADMIN_PASSWORD`.

## Ejecución en Windows PowerShell 5.1

Desde la raíz del repositorio, con `main` actualizado:

```powershell
git switch main
git pull --ff-only origin main
docker compose up --build --no-deps -d api
docker compose exec api dotnet CompraNexus.Api.dll reset-admin
```

**No utilices `-T`** en el comando de recuperación: se necesita una terminal interactiva.
El programa pide:
1. Confirmar escribiendo el correo completo del administrador configurado.
2. Introducir una contraseña nueva de **14 a 128 caracteres** (no se muestra).
3. Repetirla (tampoco se muestra).

No se pasan contraseñas en argumentos de CLI, archivos de comandos ni rutas HTTP. El comando finaliza con código `0` si cambió la contraseña; `1` por error y `2` por validación o cancelación. En caso de error de validación no se modifican datos.

El restablecimiento actualiza **únicamente `dbo.AppUsers`** para el administrador seleccionado: `PasswordHash` (mediante `PasswordHasher<AppUser>`), `FailedAttempts = 0`, `LockoutEndUtc = NULL` y `SecurityStamp` rotado. Las sesiones anteriores dejan de ser válidas en la próxima solicitud. El resto del sistema y los datos de compras se conservan.

Abre `http://localhost:8088/login` e inicia sesión con el correo y la nueva contraseña. Evita publicar capturas con contraseñas, hashes, cadenas de conexión o el archivo `.env`.

**No ejecutes** `docker compose down -v`, ni elimines SQL Server, ni cambies el password de `sa` para recuperar un usuario de aplicación.

Cuando se haya confirmado el login, elimina de `.env` el valor `COMPRANEXUS_ADMIN_PASSWORD` (deja la variable vacía si el despliegue lo requiere), porque ese secreto de bootstrap ya no sirve para una cuenta existente.
