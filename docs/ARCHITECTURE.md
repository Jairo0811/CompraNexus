# CompraNexus — Arquitectura oficial

| Capa | Tecnología |
|---|---|
| Frontend | React 19 + TypeScript + Vite |
| Backend | ASP.NET Core 10 Web API / C# |
| Base de datos | Microsoft SQL Server 2022 + EF Core 10 |
| Servicios | REST / JSON / OpenAPI |
| Integraciones | HttpClient y adaptadores para WS externos |
| Calidad | xUnit, Vitest, Playwright |
| Contenedores | Docker Compose (SQL Server + API + frontend) |
| CI/CD | GitHub Actions |
| Nube | Pendiente de seleccionar y desplegar |

React es la **vista SPA**; controladores ASP.NET Core API y entidades EF Core implementan separación modelo/controlador. Debe confirmarse con el profesor si el requisito académico de MVC acepta una SPA con backend Web API o exige ASP.NET Core MVC con vistas Razor.

El proyecto es único para ISO-810 e ISO-815. No se fusionan los trabajos previos SOA-Forge ni BonitaSoft.

## Fases
Segundo parcial: menú y CRUDs. Final: flujos completos, consultas, asientos contables por WS, despliegue en nube e integración real con otros equipos.

## Modelo relacional
Departments (ID, Name, Status)
UnitMeasures (ID, Description, Status)
Suppliers (ID, TaxId, CommercialName, Status)
Employees (ID, DepartmentId, Name, Status)
Articles (ID, UnitMeasureId, Description, Brand, Stock, Status)
PurchaseOrders (ID, Number, OrderDate, Status, DepartmentId, EmployeeId, SupplierId)
PurchaseOrderLines (ID, PurchaseOrderId, ArticleId, UnitMeasureId, Quantity, UnitCost)
AccountingEntries (ID, JournalId, PurchaseOrderId, Description, InventoryTypeId, Account, Movement, EntryDate, Amount, Status, Attempts)

**Nota:** se mantiene historial mediante desactivación de catálogos. `EnsureCreated` sirve para la primera demo; para despliegue final debe añadirse control de migraciones EF Core y permisos de BD por ambiente.
