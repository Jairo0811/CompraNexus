# Trazabilidad CompraNexus / UNAPEC

**Profesor:** Juan Pablo Valdez Reyes. **Asignaturas:** ISO-810, ISO-815. El mismo proyecto se presenta en ambas, conservando los miembros de cada equipo académico por separado.

| Requisito PPT | Implementación propuesta |
|---|---|
| Departamentos: ID, nombre, estado | API `/api/departments` + React CRUD |
| Unidades: ID, descripción, estado | API `/api/units` + React CRUD |
| Proveedores: ID, cédula/RNC, nombre comercial, estado | API `/api/suppliers` + React CRUD |
| Artículos: ID, descripción, marca, unidad, existencia, estado | API `/api/articles` + React CRUD |
| Órdenes de compra y detalles | API `/api/purchase-orders` + React CRUD |
| Consulta por criterios (departamento y empleado) | Filtros en `/api/purchase-orders` y página Consultas |
| WS Contabilidad: ID, descripción, tipo de inventario, cuenta, DB/CR, fecha, monto, estado | `AccountingGateway`, outbox e interfaz React |
| MVC y tecnología open source | React + ASP.NET Core 10 Web API + Entity Framework Core |
| Segundo parcial | Menú y CRUDs |
| Examen final | Proyecto completo en la nube e integrado con otros equipos |

### Decisiones de diseño no explícitas en el enunciado
- Tabla de Empleados, para el criterio de consulta indicado.
- Encabezado/detalle y líneas múltiples en una orden.
- Desactivación lógica de catálogos para preservar integridad referencial.
- Generación de dos asientos (DB/CR) por orden: propuesta académica, pendiente de reglas del equipo contable.
- PostgreSQL/SQLite eliminados del stack principal para alinear SQL Server + EF Core con la arquitectura acordada.

### Pendientes del final
Contratos del WS receptor y los otros grupos, pruebas reales de comunicación, HTTPS/identidad/autorización, migraciones EF Core, cloud elegido, URL pública y evidencias.
