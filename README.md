<div align="center">

<img src="frontend/public/favicon.svg" alt="CompraNexus" width="110" />

# CompraNexus

### Sistema de Compras e Interfaz con Contabilidad

**Universidad APEC (UNAPEC) · ISO-810 + ISO-815 · Septiembre–Diciembre 2026**

<p>
  <img src="https://img.shields.io/badge/UNAPEC-ISO--810%20%7C%20ISO--815-003B70?style=for-the-badge" alt="UNAPEC ISO-810 e ISO-815" />
  <img src="https://img.shields.io/badge/Estado-Desarrollo%20acad%C3%A9mico-F59E0B?style=for-the-badge" alt="Desarrollo académico" />
  <a href="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml"><img src="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml/badge.svg" alt="CI" /></a>
</p>

**Compras · React · ASP.NET Core · SQL Server · Integración REST**

</div>

---

## 📌 Descripción

**CompraNexus** es el proyecto académico unificado de **ISO-810** e **ISO-815**, impartidas por **Juan Pablo Valdez Reyes** en la Universidad APEC (UNAPEC).

La solución implementa un sistema web para administrar catálogos de compras, órdenes, consultas y la preparación de asientos contables. El frontend funciona como una SPA en React y consume una API REST desarrollada con ASP.NET Core 10, mientras SQL Server 2022 actúa como motor de persistencia mediante Entity Framework Core 10.

El proyecto distingue entre funcionalidad implementada y requisitos que todavía necesitan evidencia externa: **contrato real de Contabilidad, integración con otros grupos y publicación en la nube**.

---

## 🎓 Información académica

| Campo | Información |
|---|---|
| 🏫 Institución | **Universidad APEC (UNAPEC)** |
| 📘 Asignaturas | **ISO-810 e ISO-815** |
| 👨‍🏫 Profesor | **Juan Pablo Valdez Reyes** |
| 📅 Período académico | **Septiembre–Diciembre 2026** |
| 📁 Proyecto | **Sistema de Compras e Interfaz con Contabilidad** |
| 🧩 Segundo parcial | **Menú y CRUDs** |
| 🚀 Examen final | **Proyecto completo, publicado en la nube e integrado con los demás grupos** |

### 👥 Equipo académico

Las membresías se conservan por asignatura y se muestran ordenadas por matrícula UNAPEC.

| Integrante | Matrícula | ISO-810 | ISO-815 |
|---|---|:---:|:---:|
| 👨🏻‍💻 **Jorge Alexander Minier Terrero** | **A00105678** | ✅ | ✅ |
| 👨🏻‍💻 **Eliandres Rodríguez Cepeda** | **A00112070** | — | ✅ |
| 👨🏻‍💻 **Francis Jairo Matías Rosario** | **A00115261** | ✅ | ✅ |
| 👨🏻‍💻 **Enmanueli Alfonso Rondón Marrero** | **A00115575** | ✅ | ✅ |

> **Nota:** Eliandres pertenece exclusivamente al equipo de **ISO-815**.

---

## 🧭 Continuidad académica

CompraNexus reutiliza una única solución para los entregables actuales de ISO-810 e ISO-815, pero mantiene separada la procedencia académica de los trabajos anteriores.

| Asignatura | Trabajo anterior | Proyecto actual |
|---|---|---|
| **ISO-810** | [SOA-Forge](https://github.com/Jairo0811/SOA-Forge) — Akana SOA | **CompraNexus** |
| **ISO-815** | [BonitaSoft](https://github.com/Jairo0811/BonitaSoft) — Bonita BPM | **CompraNexus** |

**SOA-Forge y BonitaSoft son antecedentes académicos independientes, no dependencias técnicas de CompraNexus.** Esto tampoco implica que SOA-Forge pertenezca a ISO-815 ni que BonitaSoft pertenezca a ISO-810.

---

## 📊 Estado académico y técnico

| Área | Estado |
|---|:---:|
| Menú principal y navegación | ✅ Implementado |
| CRUD de Departamentos | ✅ Implementado |
| CRUD de Unidades de medida | ✅ Implementado |
| CRUD de Proveedores | ✅ Implementado |
| CRUD de Artículos | ✅ Implementado |
| Catálogo de Empleados | ✅ Implementado como ampliación |
| Órdenes de compra y detalles | ✅ Base operativa implementada |
| Consultas por departamento, empleado y estado | ✅ Implementado |
| Preparación de asientos DB/CR | ✅ Implementado |
| Adaptador HTTP hacia Contabilidad | ✅ Implementado |
| Contrato real con Contabilidad | ⏳ Pendiente |
| Integración real con otros grupos | ⏳ Pendiente |
| Publicación en la nube | ⏳ Pendiente |

> El código implementado no sustituye la evidencia de integración remota ni de despliegue público requerida para el examen final.

---

## 🧱 Stack tecnológico

### ⚙️ Backend / API

<p>
  <img src="https://skillicons.dev/icons?i=dotnet,cs" alt=".NET y C#" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="ASP.NET Core 10" />
  <img src="https://img.shields.io/badge/EF%20Core-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="Entity Framework Core 10" />
  <img src="https://img.shields.io/badge/OpenAPI-REST%20%2F%20JSON-85EA2D?style=flat-square&logo=swagger&logoColor=black" alt="OpenAPI REST JSON" />
</p>

- **.NET 10 / ASP.NET Core Web API**;
- **C#**;
- **Entity Framework Core 10**;
- API REST con JSON;
- OpenAPI disponible en Development;
- CORS configurado para el frontend;
- `HttpClient` mediante `AccountingGateway` para servicios externos;
- endpoint `/health` con comprobación de conexión a SQL Server.

### 🎨 Frontend SPA

<p>
  <img src="https://skillicons.dev/icons?i=react,ts,vite" alt="React, TypeScript y Vite" />
  <img src="https://img.shields.io/badge/React%20Router-7-CA4245?style=flat-square&logo=reactrouter&logoColor=white" alt="React Router 7" />
  <img src="https://img.shields.io/badge/Lucide-React-0F172A?style=flat-square" alt="Lucide React" />
</p>

- **React 19**;
- **TypeScript 5.9**;
- **Vite 7**;
- **React Router 7**;
- Lucide React para iconografía;
- navegación responsive;
- dashboard;
- catálogos reutilizables;
- órdenes de compra;
- consultas;
- interfaz contable.

### 🗄️ Datos

<p>
  <img src="https://img.shields.io/badge/Microsoft%20SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server 2022" />
  <img src="https://img.shields.io/badge/EF%20Core-SQL%20Server-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="EF Core SQL Server" />
</p>

- **Microsoft SQL Server 2022** como motor oficial;
- persistencia mediante EF Core;
- volumen persistente en Docker Compose;
- `EnsureCreated` y seed únicamente para el prototipo académico en Development.

> Para el despliegue final deben incorporarse **EF Core Migrations**, estrategia de actualización de esquema y permisos de base de datos por ambiente.

### 🧪 Testing, CI y contenedores

<p>
  <img src="https://skillicons.dev/icons?i=docker,git,github,githubactions" alt="Docker, Git, GitHub y GitHub Actions" />
  <img src="https://img.shields.io/badge/xUnit-2.9-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="xUnit" />
  <img src="https://img.shields.io/badge/Vitest-3.2-6E9F18?style=flat-square&logo=vitest&logoColor=white" alt="Vitest" />
  <img src="https://img.shields.io/badge/Playwright-1.57-2EAD33?style=flat-square&logo=playwright&logoColor=white" alt="Playwright" />
</p>

- **xUnit** para pruebas del backend;
- **Vitest** para pruebas del frontend;
- **Playwright** para smoke tests de navegador;
- build de TypeScript + Vite;
- GitHub Actions;
- Docker Compose para **SQL Server + API + frontend**;
- Node.js 22 en CI.

---

## 🏗️ Arquitectura

```text
┌───────────────────────────────┐
│ React 19 + TypeScript + Vite  │
│          Frontend SPA         │
└──────────────┬────────────────┘
               │ REST / JSON
               ▼
┌───────────────────────────────┐
│ ASP.NET Core 10 Web API       │
│ Controllers + Services        │
└──────────────┬────────────────┘
               │ EF Core 10
               ▼
┌───────────────────────────────┐
│ Microsoft SQL Server 2022     │
└───────────────────────────────┘

               API
                │
                │ HttpClient
                ▼
┌───────────────────────────────┐
│ AccountingGateway             │
│ WS / API de Contabilidad      │
│ contrato externo pendiente    │
└───────────────────────────────┘
```

### Sobre el requisito MVC

La arquitectura mantiene responsabilidades separadas entre **modelo/persistencia**, **controladores API** y **vista SPA React**. Sin embargo, el requisito académico menciona MVC; por eso debe confirmarse con el profesor si esta separación SPA + Web API satisface el requisito o si exige específicamente **ASP.NET Core MVC con Razor Views**.

---

## 🧩 Módulos funcionales

| Módulo | Función |
|---|---|
| 📊 Dashboard | Vista general del sistema |
| 🏢 Departamentos | CRUD de departamentos |
| 📏 Unidades de medida | CRUD de unidades |
| 🚚 Proveedores | CRUD de proveedores |
| 📦 Artículos | CRUD de artículos y existencia |
| 👥 Empleados | Soporte para consultas por departamento/empleado |
| 🧾 Órdenes de compra | Encabezado, detalles y líneas |
| 📈 Consultas | Filtros por departamento, empleado y estado |
| 🔗 Contabilidad | Preparación y transmisión provisional de asientos |
| ❤️ Health | Verificación de API y conectividad con SQL Server |

---

## 📋 Requerimientos del profesor

| Requisito | Implementación |
|---|---|
| Departamentos: ID, nombre, estado | API + CRUD React |
| Unidades: ID, descripción, estado | API + CRUD React |
| Proveedores: ID, cédula/RNC, nombre comercial, estado | API + CRUD React |
| Artículos: ID, descripción, marca, unidad, existencia, estado | API + CRUD React |
| Órdenes y detalles | API + interfaz React |
| Consulta por criterios | Departamento, empleado y estado |
| WS Contabilidad | `AccountingGateway` + registros DB/CR |
| Tecnología open source / MVC | React + ASP.NET Core + EF Core; interpretación MVC pendiente de confirmación |
| Segundo parcial | Menú y CRUDs |
| Final | Sistema completo, nube e integración real |

---

## 🚀 Ejecución local

### Opción 1 — Docker Compose

Requiere Docker Desktop.

```powershell
git clone https://github.com/Jairo0811/CompraNexus.git
cd CompraNexus
Copy-Item .env.example .env
# Edite .env y configure una contraseña fuerte en MSSQL_SA_PASSWORD
docker compose up --build -d
docker compose ps
```

| Servicio | Dirección |
|---|---|
| Frontend React | http://localhost:8088 |
| API ASP.NET Core | http://localhost:5098 |
| Health | http://localhost:5098/health |
| SQL Server | localhost,1433 |
| Base de datos | CompraNexus |

> No use `docker compose down -v` salvo que desee eliminar deliberadamente el volumen de SQL Server.

### Opción 2 — Desarrollo separado

Con SQL Server accesible:

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

Frontend de desarrollo: **http://localhost:5173**  
API: **http://localhost:5098**

---

## 🧪 Pruebas y CI

### Backend

```powershell
dotnet restore backend/CompraNexus.slnx
dotnet build backend/CompraNexus.slnx -c Release
dotnet test backend/CompraNexus.slnx -c Release
```

### Frontend

```powershell
cd frontend
npm install
npm run build
npm run test
npx playwright install chromium
npm run test:e2e
```

El workflow `.github/workflows/ci.yml` valida automáticamente:

- restore, build y xUnit en **.NET 10**;
- compilación TypeScript y build de **Vite**;
- pruebas **Vitest**;
- instalación de Chromium;
- smoke tests con **Playwright**.

---

## 🔗 Integración con Contabilidad

CompraNexus genera dos registros contables por orden confirmada como propuesta académica: uno **DB** y otro **CR**, ambos con identificador UUID.

La integración se configura mediante:

```text
Accounting__ServiceUrl
Accounting__BearerToken
```

En Docker:

```text
ACCOUNTING_SERVICE_URL
ACCOUNTING_BEARER_TOKEN
```

Si no existe una URL configurada, los asientos permanecen en estado **Pendiente**. El contrato actual es provisional y no debe considerarse una integración certificada hasta acordar URL, autenticación, esquema JSON, cuentas y respuesta con el grupo de Contabilidad.

---

## ☁️ Estado del despliegue final

Docker Compose ya permite levantar localmente SQL Server, API y frontend. Sin embargo, para considerar cumplido el examen final todavía faltan:

- seleccionar proveedor cloud;
- configurar HTTPS y secretos;
- sustituir `EnsureCreated` por migraciones EF Core;
- desplegar una instancia pública;
- acordar contratos con los demás grupos;
- ejecutar pruebas extremo a extremo entre sistemas;
- documentar evidencia de integración y URL pública.

---

## 📚 Documentación

- [Arquitectura y modelo de datos](docs/ARCHITECTURE.md)
- [Trazabilidad del enunciado](docs/REQUIREMENTS.md)
- [Contrato provisional de Contabilidad](docs/INTEGRATION.md)
- [Despliegue local y futura nube](docs/DEPLOYMENT.md)

---

<div align="center">

**CompraNexus · Sistema de Compras e Interfaz con Contabilidad**  
**UNAPEC · ISO-810 + ISO-815 · 2026**

</div>
