# Trazabilidad académica · CompraNexus

**Fuente:** presentación «Proyecto Final Asignatura — Compras e interfaz con Contabilidad», Universidad APEC, profesor Juan P. Valdez (2020). Este documento distingue requisitos **explícitos** de decisiones del equipo.

## Funcionalidad solicitada y ubicación

| Enunciado | Implementación |
|---|---|
| Gestión de Departamentos (ID, nombre, estado) | `/catalogs/departments` |
| Gestión de Unidades de Medida (ID, descripción, estado) | `/catalogs/units` |
| Gestión de Proveedores (ID, cédula/RNC, nombre comercial, estado) | `/catalogs/suppliers` |
| Gestión de Artículos (ID, descripción, marca, unidad de medida, existencia, estado) | `/catalogs/articles` |
| Órdenes de Compra (número, fecha, estado, artículo, cantidad, unidad, costo) | `/orders` con encabezado y líneas |
| Una consulta por criterios (ej.: departamentos y empleados) | `/reports` |
| Tecnología Open Source bajo MVC | FastAPI (controladores), SQLAlchemy (modelos), Jinja2 (vistas) |
| Envío por WS Contabilidad (asiento, descripción, tipo inventario, cuenta, DB/CR, fecha, monto, estado) | `/accounting`; adaptador HTTP configurable |

## Extensiones de diseño (NO enumeradas expresamente)

- Empleados y asociación a departamento, para poder consultar por empleado.
- Proveedor y departamento en encabezado de orden, porque el sistema de compras los necesita.
- Varias líneas por orden, en lugar de un único artículo.
- Separación por controladores, modelos y vistas; protección CSRF y autenticación básica en producción.
- Dos asientos por orden (DB/CR) como propuesta de partida doble; cuentas contables y tipos **configurables**.
- Registro persistente de intentos y respuesta de la interfaz contable con clave de idempotencia.
- Campos básicos de estados: Activo/Inactivo y Borrador/Confirmada/Anulada.

## Entregas según indicación del profesor al equipo

**Segundo parcial:** menú y CRUDs.

**Examen final:** proyecto completo publicado en la nube e integrado con los demás grupos. La publicación y la interoperabilidad real **no se certifican** con el código solo; se requieren contratos reales, endpoints remotos, despliegue y pruebas extremo a extremo.

## Pendientes para entrega final

1. Obtener contrato/API y URL(s) de los demás grupos y del WS de Contabilidad.
2. Confirmar codificación de campos, cuentas, tipo de inventario y esquema de autenticación.
3. Desplegar instancia pública con TLS, almacenamiento persistente, secretos y URL de evidencia.
4. Probar el intercambio remoto real, capturar evidencias y documentar resultados.
