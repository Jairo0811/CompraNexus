# Login seguro de CompraNexus

## Alcance

- Se requiere sesión para todos los controladores `/api/*` salvo `POST /api/auth/login` y `GET /api/auth/csrf`. `/health` sigue público.
- Login por correo electrónico con hash ASP.NET Core PasswordHasher. No existe registro público.
- Sesión mediante cookie HttpOnly, SameSite Strict, Secure en Production; expiración de 8 horas con renovación de actividad.
- Antiforgery MVC obligatorio en POST/PUT/PATCH/DELETE: obtener token en `GET /api/auth/csrf` y enviarlo en `X-CSRF-TOKEN`.
- Tras autenticar se obtiene un token CSRF nuevo, ligado a la nueva identidad; las cookies y tokens no se guardan en localStorage.
- Tras cinco intentos fallidos se bloquea la cuenta durante quince minutos; los mensajes no identifican si existe el usuario.
- Roles propuestos (pendientes de confirmación académica): Administrador, Compras, Consultas y Contabilidad. El backend protege operaciones de escritura y contabilización.
- No se implementa aún el panel de administración de usuarios ni recuperación de contraseña.

## Creación de primer administrador (Development local)

1. Conserva tu archivo `.env` privado en la raíz del repositorio. No compartas su contenido.
2. Añade o actualiza `COMPRANEXUS_ADMIN_EMAIL` con tu correo elegido y `COMPRANEXUS_ADMIN_PASSWORD` con una contraseña propia de entre 14 y 128 caracteres.
3. Ejecuta en PowerShell 5.1 `docker compose up --build -d` (sin borrar volúmenes).
4. Abre `http://localhost:8088/login` y entra con tus credenciales.
5. Después de comprobar el acceso, borra el **valor** de `COMPRANEXUS_ADMIN_PASSWORD` de `.env` y ejecuta `docker compose up -d --force-recreate api` para no seguir pasando el secreto al contenedor.

El backend crea el primer administrador **solo si la tabla AppUsers está vacía**. Nunca altera los hashes existentes ni crea credenciales de fábrica. La contraseña no se almacena en el repositorio.
Si falta la configuración inicial, el login aparece pero ningún usuario puede iniciar sesión hasta aprovisionar el administrador.

## Persistencia sin destrucción de datos

La nueva entidad `AppUsers` se añade al modelo EF Core. Para la base ya creada mediante `EnsureCreated`, `SchemaPatches` crea únicamente la tabla de usuarios y su índice único si aún no existen.
El parche no elimina ni reinicia las tablas de compras. Las claves Data Protection de las cookies viven en el volumen independiente `auth_keys`.
El proyecto continúa usando EnsureCreated en Development; una migración baseline completa de las tablas de compras sigue pendiente.

## Endpoints

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/auth/csrf` | Token antiforgery anónimo/autenticado |
| POST | `/api/auth/login` | Crear sesión con correo y contraseña, requiere token CSRF |
| GET | `/api/auth/me` | Datos del usuario actual, 401 sin sesión |
| POST | `/api/auth/logout` | Cerrar sesión, requiere CSRF |

Todos los mantenimientos y órdenes requieren sesión. Crear/editar/desactivar catálogos y editar órdenes: rol Administrador o Compras. Contabilizar/reintentar asientos: Administrador o Contabilidad.

## Pruebas y limitaciones

- El CI usa bases temporales y SQL Server efímero para verificar la creación del usuario, unicidad y hash.
- El CI también ejecuta un smoke HTTP real con login, CSRF, cookies y logout. Playwright usa API simulada y no demuestra persistencia.
- Antes de desplegar en internet faltan TLS/proxy de confianza, cifrado/gestión de claves, políticas de recuperación y administración de usuarios, controles de rate limiting distribuidos, migraciones completas y revisión de autorización.
- La integración contable externa continúa sin contrato real verificado. No se debe considerar listo para producción.
