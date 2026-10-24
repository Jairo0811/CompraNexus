<p align="center">
  <img src="https://img.shields.io/badge/UNAPEC-ISO--810%20%7C%20ISO--815-0057B8?style=for-the-badge" alt="UNAPEC ISO-810 e ISO-815" />
  <a href="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml"><img src="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml/badge.svg" alt="CI"/></a>
</p>

# CompraNexus

> **Sistema de Compras e Interfaz con Contabilidad** · Universidad APEC · Septiembre–diciembre de 2026.

**CompraNexus** es la solución única de **ISO-810** e **ISO-815**, con entregables de segundo parcial (menú y CRUDs) y examen final (sistema completo en la nube e integrado con los demás grupos). Docente: **Juan Pablo Valdez Reyes**.

## Stack oficial acordado

| Capa | Tecnología |
|---|---|
| Frontend | **React 19, TypeScript, Vite** |
| Backend | **ASP.NET Core 10 / C#** |
| Persistencia | **SQL Server 2022 + EF Core 10** |
| Servicios web | REST / JSON / OpenAPI |
| Integraciones | Adaptadores HttpClient (WS de Contabilidad) |
| Pruebas | xUnit, Vitest y Playwright |
| Contenedores | Docker Compose |
| CI/CD | GitHub Actions |
| Nube | Proveedor pendiente de seleccionar |

**Esta es la arquitectura del proyecto.** No se utiliza Python/FastAPI/Jinja2/SQLite como implementación vigente. SQL Server se utiliza tanto para la API como para la ejecución de desarrollo con Docker. Los modelos, controladores API y vistas SPA mantienen responsabilidades separadas; el equipo confirmará con el profesor si acepta esta separación como MVC o exige Razor MVC tradicional.

## Empezar: Docker + SQL Server

1. Tener Docker Desktop con soporte para contenedores Linux.
2. Copiar `.env.example` a `.env` y reemplazar `MSSQL_SA_PASSWORD` por una contraseña privada y fuerte.
3. Ejecutar:

```powershell
git clone https://github.com/Jairo0811/CompraNexus.git
cd CompraNexus
Copy-Item .env.example .env
# Editar .env y cambiar MSSQL_SA_PASSWORD
docker compose up --build -d
docker compose ps
```

| Servicio | Dirección |
|---|---|
| Aplicación React | http://localhost:8088 |
| API ASP.NET Core | http://localhost:5098 |
| Health | http://localhost:5098/health |
| SQL Server | localhost,1433 (SSMS; usuario sa y contraseña del archivo .env) |
| Base de datos | CompraNexus |

Se generan las tablas automáticamente solo en Development mediante `EnsureCreated`, y se agrega un conjunto pequeño de registros de demostración si la base está vacía. **No usar `docker compose down -v`** salvo que se quiera destruir deliberadamente la base persistida.

## Desarrollo separado (sin contenedores para código)

Con SQL Server ya instalado y accesible:

```powershell
$env:ConnectionStrings__SqlServer = 'Server=localhost,1433;Database=CompraNexus;User Id=sa;Password=SU_CLAVE;Encrypt=True;TrustServerCertificate=True'
dotnet run --project backend/CompraNexus.Api --launch-profile http
```

En otra consola:
```powershell
cd frontend
npm install
npm run dev
```

Frontend: http://localhost:5173 · API: http://localhost:5098. No guardar credenciales reales en el repositorio.

## Requerimientos del profesor

| Módulo | Alcance |
|---|---|
| Departamentos | ID, nombre, estado; CRUD |
| Unidades de medida | ID, descripción, estado; CRUD |
| Proveedores | ID, cédula/RNC, nombre comercial, estado; CRUD |
| Artículos | ID, descripción, marca, unidad, existencia, estado; CRUD |
| Órdenes | Número, fecha, estado; detalles con artículos, cantidades, unidad y costo |
| Consultas | Por departamento, empleado y estado |
| Contabilidad | Interfaz REST provisional para asientos DB/CR |
| Final | Publicación en nube e integración real con otros grupos (pendiente) |

El catálogo de empleados, las relaciones a departamentos y la separación encabezado/detalle son ampliaciones propuestas para satisfacer las consultas y la trazabilidad.

## Pruebas y calidad

```powershell
dotnet test backend/CompraNexus.slnx
cd frontend
npm install
npm run build
npm run test
npx playwright install chromium
npm run test:e2e
```

GitHub Actions valida xUnit, build .NET, compilación TypeScript, Vitest y navegación UI con Playwright.

## Integrantes por asignatura

| Integrante | Matrícula | ISO-810 | ISO-815 |
|---|---|:---:|:---:|
| Jorge Alexander Minier Terrero | A00105678 | ✅ | ✅ |
| Eliandres Rodríguez Cepeda | A00112070 | — | ✅ |
| Francis Jairo Matías Rosario | A00115261 | ✅ | ✅ |
| Enmanueli Alfonso Rondón Marrero | A00115575 | ✅ | ✅ |

**Nota:** Eliandres pertenece exclusivamente a ISO-815. SOA-Forge (ISO-810) y BonitaSoft (ISO-815) son trabajos previos independientes.

## Documentación

- [Arquitectura y datos](docs/ARCHITECTURE.md)
- [Despliegue en Docker y futura nube](docs/DEPLOYMENT.md)
- [Contrato de Contabilidad](docs/INTEGRATION.md)
- [Trazabilidad del enunciado](docs/REQUIREMENTS.md)

**Estado de evaluación:** el segundo parcial requiere menú y CRUDs. El final necesita además validación de servicios externos y despliegue real; el código no constituye por sí solo evidencia de esos requisitos.
