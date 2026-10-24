<p align="center">
  <img src="https://img.shields.io/badge/UNAPEC-ISO--810%20%7C%20ISO--815-0057B8?style=for-the-badge" alt="UNAPEC ISO-810 e ISO-815">
  <img src="https://img.shields.io/badge/Estado-Desarrollo%20acad%C3%A9mico-f59e0b?style=for-the-badge" alt="Desarrollo académico">
  <a href="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml"><img src="https://github.com/Jairo0811/CompraNexus/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
</p>

# CompraNexus

> **Sistema de Compras e Interfaz con Contabilidad** · UNAPEC · Septiembre–diciembre de 2026

**CompraNexus** es el proyecto académico único de **ISO-810** e **ISO-815**, bajo la docencia de **Juan Pablo Valdez Reyes**. Implementa los módulos de compras solicitados con una aplicación web MVC open source y prepara la comunicación HTTP con el sistema de Contabilidad.

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

## 📋 Estado de entregables

| Entrega | Requisito | Estado de código |
|---|---|---|
| Segundo parcial | Menú y CRUDs | Implementados, pendientes de aceptación académica |
| Final | Órdenes completas y consultas | Implementados como base operativa |
| Final | WS Contabilidad | Adaptador configurable implementado; **sin contrato/endpoint externo verificado** |
| Final | Integración con los demás grupos | **Pendiente de contratos y pruebas entre grupos** |
| Final | Publicación en la nube | Dockerfile y guía disponibles; **no desplegado** |

> No confundir funciones implementadas con despliegue ni validación integrada en vivo.

## 🧱 Stack tecnológico

### Backend y API

![Python](https://img.shields.io/badge/Python-3.12%2B-3776AB?logo=python&logoColor=white) ![FastAPI](https://img.shields.io/badge/FastAPI-0.115%2B-009688?logo=fastapi&logoColor=white) ![Uvicorn](https://img.shields.io/badge/Uvicorn-ASGI-4051B5) ![SQLAlchemy](https://img.shields.io/badge/SQLAlchemy-2.x-D71F00)

### Interfaz MVC

![Jinja2](https://img.shields.io/badge/Jinja2-Templates-B41717) ![HTML5](https://img.shields.io/badge/HTML5-E34F26?logo=html5&logoColor=white) ![CSS3](https://img.shields.io/badge/CSS3-1572B6?logo=css&logoColor=white) ![JavaScript](https://img.shields.io/badge/JavaScript-ES6-F7DF1E?logo=javascript&logoColor=black)

### Datos e integración

![SQLite](https://img.shields.io/badge/SQLite-Local-003B57?logo=sqlite&logoColor=white) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Configurable-4169E1?logo=postgresql&logoColor=white) ![HTTP](https://img.shields.io/badge/HTTP-WS%20provisional-64748B) ![httpx](https://img.shields.io/badge/httpx-Cliente%20HTTP-2563EB)

### Calidad y despliegue

![pytest](https://img.shields.io/badge/pytest-8%2B-0A9EDC?logo=pytest&logoColor=white) ![Docker](https://img.shields.io/badge/Docker-Preparado-2496ED?logo=docker&logoColor=white) ![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI-2088FF?logo=githubactions&logoColor=white)

**Alcance:** PostgreSQL es configurable, no se declara como despliegue productivo verificado; Docker prepara el empaquetado, no acredita publicación cloud. El contrato HTTP contable es provisional.

### Patrón MVC

- **Modelos:** SQLAlchemy 2 (departamentos, unidades, proveedores, empleados, artículos, órdenes, líneas y asientos).
- **Controladores:** FastAPI / Python (rutas HTTP, validaciones, operaciones y WS).
- **Vistas:** Jinja2 HTML, CSS responsive y JavaScript para líneas de detalle.
- **Persistencia:** SQLite para evaluación local; `DATABASE_URL` acepta PostgreSQL para producción.
- **Seguridad:** CSRF en formularios; autenticación HTTP Basic configurable y obligatoria en producción; secretos por entorno.

Se usa tecnología open source conforme al requisito, priorizando un monolito MVC reproducible. La elección de SQLite/PostgreSQL y Python es una **decisión técnica del equipo**, no una imposición del enunciado.

## Requisitos del profesor

- Departamentos: ID, nombre, estado.
- Unidades de medida: ID, descripción, estado.
- Proveedores: ID, cédula/RNC, nombre comercial, estado.
- Artículos: ID, descripción, marca, unidad, existencia, estado.
- Órdenes: número, fecha, estado y líneas de artículo, cantidad, unidad, costo unitario.
- Consulta por criterios: departamento y empleado, además de estado.
- Envío por WS de asientos contables con identificador, descripción, tipo inventario, cuenta, DB/CR, fecha, monto y estado.

**Ampliaciones útiles:** catálogo de empleados, encabezado con departamento/proveedor, detalles múltiples, búsqueda y confirmación de órdenes, estado de transmisión contable.

## Iniciar en Windows (PowerShell)

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

## Menú del sistema

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

## Pruebas

```bash
pytest -q
python -m compileall -q app scripts
```

GitHub Actions ejecuta las pruebas al hacer push/PR.

## Deploy (preparación)

```bash
docker build -t compranexus .
docker run --rm -p 8000:8000 -e APP_ENV=production -e APP_USERNAME=admin -e APP_PASSWORD='CAMBIE_ESTA_CLAVE' -e SESSION_SECRET='CAMBIE_ESTE_SECRETO_LARGO' -v compranexus-data:/code/data compranexus
```

Para publicar en una plataforma cloud, usar **HTTPS**, un servicio Docker con almacenamiento persistente (o PostgreSQL en `DATABASE_URL`) y secretos configurados. El contenedor no publica por sí mismo el proyecto. Sin URL ni contrato del WS, los asientos se guardan como Pendiente y **no se envían**.

Documentación: [trazabilidad académica](docs/REQUIREMENTS.md) · [contrato de integración](docs/INTEGRATION.md) · [despliegue](docs/DEPLOYMENT.md).

