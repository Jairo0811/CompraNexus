# CompraNexus

> **Sistema de Compras e Interfaz con Contabilidad** · UNAPEC · Septiembre–diciembre de 2026

**CompraNexus** es el proyecto académico único de **ISO-810** e **ISO-815**, bajo la docencia de **Juan Pablo Valdez Reyes**. Implementa los módulos de compras solicitados con una aplicación web MVC open source y prepara la comunicación HTTP con el sistema de Contabilidad.

## Estado de entregables

| Entrega | Requisito | Estado de código |
|---|---|---|
| Segundo parcial | Menú y CRUDs | Implementados, pendientes de aceptación académica |
| Final | Órdenes completas y consultas | Implementados como base operativa |
| Final | WS Contabilidad | Adaptador configurable implementado; **sin contrato/endpoint externo verificado** |
| Final | Integración con los demás grupos | **Pendiente de contratos y pruebas entre grupos** |
| Final | Publicación en la nube | Dockerfile y guía disponibles; **no desplegado** |

> No confundir funciones implementadas con despliegue ni validación integrada en vivo.

## Stack y patrón MVC

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

## Equipo académico (membresías independientes)

| Integrante | Matrícula | ISO-810 | ISO-815 |
|---|---|:---:|:---:|
| Jorge Alexander Minier Terrero | A00105678 | ✓ | ✓ |
| Francis Jairo Matías Rosario | A00115261 | ✓ | ✓ |
| Enmanueli Alfonso Rondón Marrero | A00115575 | ✓ | ✓ |
| Eliandres Rodríguez Cepeda | A00112070 | — | ✓ |

Repositorios de primer parcial anteriores, sin dependencia técnica: [SOA-Forge](https://github.com/Jairo0811/SOA-Forge) (ISO-810) y [BonitaSoft](https://github.com/Jairo0811/BonitaSoft) (ISO-815).
