# Guía de despliegue cloud — CompraNexus

**Esto es una guía de preparación, no evidencia de que CompraNexus esté publicado.**

## Opciones de alojamiento

- Host compatible con Docker (VPS, PaaS, Azure Container Apps, etc.).
- Puerto interno 8000; configurar el puerto de escucha de plataforma mediante `PORT`.
- HTTPS gestionado por proxy/PaaS (nunca exponer autenticación básica en HTTP).
- Almacenamiento **persistente** para SQLite o, preferentemente para un entorno compartido, una base de datos PostgreSQL administrada.

## Variables obligatorias de producción

```text
APP_ENV=production
APP_USERNAME=<usuario_admin>
APP_PASSWORD=<clave_fuerte_y_unica>
SESSION_SECRET=<valor_aleatorio_largo>
DATABASE_URL=<url_sqlite_con_volumen_o_postgres>
```

Nunca subir credenciales a Git. Rotarlas al compartir acceso o finalizar la evaluación.

## Variables opcionales de contabilidad

Ver [INTEGRATION.md](INTEGRATION.md). No indicar una URL ficticia o placeholder como servicio real. Mantener `ACCOUNTING_WS_URL` vacío hasta disponer del contrato y credenciales auténticos.

## Validación posterior a publicar

1. `GET /health` responde `{"status":"ok"}`.
2. Abrir web vía HTTPS, iniciar sesión y verificar los catálogos.
3. Crear orden con al menos una línea, filtrarla por departamento y empleado.
4. Crear asientos para orden confirmada, confirmar DB/CR y total.
5. Solo con otro grupo disponible: probar envío, reintento seguro y verificar en su sistema receptor.
6. Guardar URL y capturas de la solución para la entrega final.

## Limitaciones conocidas

- El modelo no incluye aún gestión avanzada de roles, autorización granular ni recuperación de contraseña.
- `create_all` crea tablas al inicio, pero una modificación del esquema exige migraciones reales (por ejemplo, Alembic).
- SQLite requiere volumen permanente en cloud; algunos proveedores eliminan el disco entre despliegues.
- Para despliegues concurrentes, usar PostgreSQL y considerar migraciones/versionado.
