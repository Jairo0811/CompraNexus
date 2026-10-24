<div align="center">

# 🛒 CompraNexus

### Sistema de Compras e Interfaz con Contabilidad

**UNAPEC · ISO-810 + ISO-815 · Septiembre–Diciembre 2026**

<p>
  <img src="https://img.shields.io/badge/UNAPEC-ISO--810%20%7C%20ISO--815-003B70?style=for-the-badge" alt="UNAPEC ISO-810 e ISO-815" />
  <img src="https://img.shields.io/badge/Estado-Desarrollo%20acad%C3%A9mico-F59E0B?style=for-the-badge" alt="Desarrollo académico" />
  <a href="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml"><img src="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml/badge.svg" alt="CI" /></a>
</p>

**Compras · MVC · Integración HTTP · Contabilidad**

</div>

---

## 📌 Descripción

**CompraNexus** es el proyecto académico unificado de **ISO-810** e **ISO-815**, impartidas por **Juan Pablo Valdez Reyes** en la Universidad APEC (UNAPEC). Implementa un sistema web de compras bajo patrón MVC y prepara la interoperabilidad HTTP con el sistema de Contabilidad.

La solución cubre catálogos, órdenes de compra, consultas por criterios y generación/transmisión de asientos contables. El código distingue explícitamente lo que está implementado de aquello que todavía requiere **contratos externos, pruebas entre grupos o despliegue cloud verificable**.

---
## 🎓 Información académica

| Campo | Información |
|---|---|
| Institución | Universidad APEC (UNAPEC) |
| Asignaturas | ISO-810 e ISO-815 |
| Profesor | Juan Pablo Valdez Reyes |
| Período | Septiembre–diciembre de 2026 |
| Proyecto | Sistema de Compras e Interfaz con Contabilidad |
| Segundo parcial | Menú y CRUDs |
| Examen final | Proyecto completo, publicado en la nube e integrado con los demás grupos |

### 👥 Equipo académico original

Las membresías se registran **por asignatura** y están ordenadas ascendentemente por matrícula UNAPEC.

| Integrante | Matrícula | ISO-810 | ISO-815 |
|---|---|:---:|:---:|
| 👨🏻‍💻 Jorge Alexander Minier Terrero | A00105678 | ✅ | ✅ |
| 👨🏻‍💻 Eliandres Rodríguez Cepeda | A00112070 | — | ✅ |
| 👨🏻‍💻 Francis Jairo Matías Rosario | A00115261 | ✅ | ✅ |
| 👨🏻‍💻 Enmanueli Alfonso Rondón Marrero | A00115575 | ✅ | ✅ |

**Importante:** Eliandres participa exclusivamente en ISO-815 y no pertenece al equipo de ISO-810.

## 🧭 Continuidad académica

CompraNexus unifica el **proyecto de segundo parcial y examen final** para ambas asignaturas, reutilizando una solución técnica compartida sin confundir la pertenencia académica de cada integrante.

| Asignatura | Trabajo anterior | Proyecto unificado |
|---|---|---|
| ISO-810 | [SOA-Forge](https://github.com/Jairo0811/SOA-Forge) — Akana SOA | CompraNexus |
| ISO-815 | [BonitaSoft](https://github.com/Jairo0811/BonitaSoft) — Bonita BPM | CompraNexus |

Los repositorios anteriores son antecedentes académicos, **no dependencias técnicas**. El proyecto unificado usa un stack open source propio y no implica que SOA-Forge pertenezca a ISO-815 ni que BonitaSoft pertenezca a ISO-810.

---

## 📊 Estado académico y entregables

| Entrega | Requisito | Estado de código |
|---|---|---|
| Segundo parcial | Menú y CRUDs | Implementados, pendientes de aceptación académica |
| Final | Órdenes completas y consultas | Implementados como base operativa |
| Final | WS Contabilidad | Adaptador configurable implementado; **sin contrato/endpoint externo verificado** |
| Final | Integración con los demás grupos | **Pendiente de contratos y pruebas entre grupos** |
| Final | Publicación en la nube | Dockerfile y guía disponibles; **no desplegado** |

> No confundir funciones implementadas con despliegue ni validación integrada en vivo.

---

## 🧱 Stack tecnológico

### ⚙️ Backend y API

<p>
  <img src="https://skillicons.dev/icons?i=python,fastapi" alt="Python y FastAPI" />
  <img src="https://img.shields.io/badge/Uvicorn-ASGI-4051B5?style=flat-square" alt="Uvicorn ASGI" />
  <img src="https://img.shields.io/badge/SQLAlchemy-2.x-D71F00?style=flat-square" alt="SQLAlchemy 2" />
</p>

- **Python 3.12+**;
- **FastAPI** para rutas HTTP, validación y controladores;
- **Uvicorn** como servidor ASGI;
- **SQLAlchemy 2** como ORM;
- **httpx** para integración HTTP saliente;
- autenticación HTTP Basic configurable y obligatoria en producción.

### 🎨 Vistas y experiencia web

<p>
  <img src="https://skillicons.dev/icons?i=html,css,js" alt="HTML5, CSS3 y JavaScript" />
  <img src="https://img.shields.io/badge/Jinja2-Templates-B41717?style=flat-square" alt="Jinja2 Templates" />
</p>

- **Jinja2** para renderizado server-side;
- **HTML5**;
- **CSS responsive**;
- **JavaScript ES6** para interacción y líneas de detalle;
- formularios protegidos con CSRF.

### 🗄️ Datos e integración

<p>
  <img src="https://skillicons.dev/icons?i=sqlite,postgres" alt="SQLite y PostgreSQL" />
  <img src="https://img.shields.io/badge/httpx-HTTP%20Client-2563EB?style=flat-square" alt="httpx" />
  <img src="https://img.shields.io/badge/WS%20Contabilidad-Contrato%20provisional-64748B?style=flat-square" alt="WS Contabilidad provisional" />
</p>

- **SQLite** para evaluación y ejecución local;
- DATABASE_URL permite **PostgreSQL** para una evolución productiva;
- adaptador HTTP configurable para Contabilidad;
- idempotencia y registro persistente de intentos;
- contrato del WS todavía **provisional**, pendiente del servicio real del otro grupo.

### 🧪 Calidad, control de versiones y despliegue

<p>
  <img src="https://skillicons.dev/icons?i=docker,git,github,githubactions" alt="Docker, Git, GitHub y GitHub Actions" />
  <img src="https://img.shields.io/badge/pytest-8%2B-0A9EDC?style=flat-square&logo=pytest&logoColor=white" alt="pytest" />
</p>

- **pytest** para pruebas automatizadas;
- compileall como validación adicional del código Python;
- **Git / GitHub**;
- **GitHub Actions** para CI;
- **Dockerfile** para empaquetado reproducible;
- preparación para cloud sin declarar un despliegue todavía no verificado.

> **Alcance técnico:** que PostgreSQL sea configurable o que exista un Dockerfile no significa que CompraNexus ya esté desplegado en producción. La publicación cloud y la integración real entre grupos siguen requiriendo evidencia externa.

### 🧩 Patrón MVC

| Capa | Implementación |
|---|---|
| **Modelos** | SQLAlchemy 2: departamentos, unidades, proveedores, empleados, artículos, órdenes, líneas y asientos |
| **Controladores** | FastAPI / Python: rutas HTTP, validaciones, operaciones y comunicación con WS |
| **Vistas** | Jinja2 + HTML + CSS + JavaScript |
| **Persistencia** | SQLite local; PostgreSQL configurable mediante DATABASE_URL |
| **Seguridad** | CSRF, HTTP Basic configurable y secretos por entorno |

Se utiliza tecnología **open source** conforme al requisito académico. La selección de Python, FastAPI, SQLite/PostgreSQL y Jinja2 es una decisión técnica del equipo, no una imposición del enunciado.

---

## 🏗️ Arquitectura

```text
Navegador
   │
   │ HTTP
   ▼
FastAPI / Controllers
   │
   ├──────────────► Jinja2 Views
   │
   ▼
SQLAlchemy Models
   │
   ├──────────────► SQLite (local)
   │
   └──────────────► PostgreSQL (configurable)
   │
   ▼
Accounting Adapter / httpx
   │
   ▼
WS Contabilidad
(contrato externo pendiente)
```

## 📋 Requisitos del profesor

- Departamentos: ID, nombre, estado.
- Unidades de medida: ID, descripción, estado.
- Proveedores: ID, cédula/RNC, nombre comercial, estado.
- Artículos: ID, descripción, marca, unidad, existencia, estado.
- Órdenes: número, fecha, estado y líneas de artículo, cantidad, unidad, costo unitario.
- Consulta por criterios: departamento y empleado, además de estado.
- Envío por WS de asientos contables con identificador, descripción, tipo inventario, cuenta, DB/CR, fecha, monto y estado.

**Ampliaciones útiles:** catálogo de empleados, encabezado con departamento/proveedor, detalles múltiples, búsqueda y confirmación de órdenes, estado de transmisión contable.

---

## 🚀 Ejecución local

### Windows (PowerShell)

Requiere **Python 3.12+**.

```powershell
git clone https://github.com/Jairo0811/CompraNexus.git
cd CompraNexus
py -3.12 -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python -m scripts.seed
uvicorn app.main:app --reload
```

Abrir **http://localhost:8000**. Opcional: copiar `.env.example` a `.env` y definir variables de entorno desde PowerShell (el programa no carga `.env` automáticamente). No se requiere la carga de demostración: la base se crea al iniciar.

**Linux/macOS:**

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
python -m scripts.seed
uvicorn app.main:app --reload
```

---

## 🧭 Módulos y navegación

| Módulo | Ruta |
|---|---|
| Dashboard | `/` |
| Departamentos | `/catalogs/departments` |
| Unidades de medida | `/catalogs/units` |
| Proveedores | `/catalogs/suppliers` |
| Artículos | `/catalogs/articles` |
| Empleados (soporte a consultas) | `/catalogs/employees` |
| Órdenes de compra | `/orders` |
| Consultas por criterios | `/reports` |
| Interfaz contable | `/accounting` |
| Health | `/health` |

Para registrar una orden primero cree un departamento, una unidad, un proveedor, un empleado asignado al departamento y al menos un artículo.

---

## 🧪 Pruebas y CI

```bash
pytest -q
python -m compileall -q app scripts
```

GitHub Actions ejecuta las pruebas al hacer push/PR.

---

## ☁️ Despliegue — preparación

```bash
docker build -t compranexus .
docker run --rm -p 8000:8000 -e APP_ENV=production -e APP_USERNAME=admin -e APP_PASSWORD='CAMBIE_ESTA_CLAVE' -e SESSION_SECRET='CAMBIE_ESTE_SECRETO_LARGO' -v compranexus-data:/code/data compranexus
```

Para publicar en una plataforma cloud, usar **HTTPS**, un servicio Docker con almacenamiento persistente (o PostgreSQL en `DATABASE_URL`) y secretos configurados. El contenedor no publica por sí mismo el proyecto. Sin URL ni contrato del WS, los asientos se guardan como Pendiente y **no se envían**.

## 📚 Documentación

- [Trazabilidad académica](docs/REQUIREMENTS.md)
- [Contrato propuesto de integración con Contabilidad](docs/INTEGRATION.md)
- [Guía de despliegue](docs/DEPLOYMENT.md)

---

<div align="center">

**CompraNexus · Sistema de Compras e Interfaz con Contabilidad**  
**UNAPEC · ISO-810 + ISO-815 · 2026**

</div>

