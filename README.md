# CompraNexus

**Sistema de Compras e Interfaz con Contabilidad** | Universidad APEC | Septiembre–diciembre 2026

Proyecto académico compartido por **ISO-810** (Integración de Aplicaciones con Tecnología Propietaria) e **ISO-815** (Integración de Aplicaciones con Tecnología Open Source). Profesor: **Juan Pablo Valdez Reyes**.

## Integrantes (respetando su asignatura)

| Integrante | Matrícula | ISO-810 | ISO-815 |
|---|---|:---:|:---:|
| Jorge Alexander Minier Terrero | A00105678 | ✓ | ✓ |
| Francis Jairo Matías Rosario | A00115261 | ✓ | ✓ |
| Enmanueli Alfonso Rondón Marrero | A00115575 | ✓ | ✓ |
| Eliandres Rodríguez Cepeda | A00112070 | — | ✓ |

## Entregables del profesor

- **Segundo parcial:** menú de navegación y CRUDs de departamentos, unidades de medida, proveedores, artículos y órdenes de compra.
- **Examen final:** sistema completo, publicado en la nube e integrado con los otros sistemas/grupos, incluyendo intercambio de asientos mediante WS Contabilidad.

## Requisitos del enunciado

Departamentos (identificador, nombre, estado), unidades (identificador, descripción, estado), proveedores (identificador, cédula/RNC, nombre comercial, estado), artículos (identificador, descripción, marca, unidad, existencia, estado), órdenes (número, fecha, estado, artículo, cantidad, unidad y costo unitario), una consulta por criterios (por ejemplo, departamento y empleado), y envío de asientos (ID, descripción, tipo de inventario, cuenta contable, DB/CR, fecha, monto, estado).

> El repositorio se inicializará con implementación, pruebas y manual de despliegue. La integración con servicios de terceros requiere sus URLs, credenciales y contratos; no se declara completada antes de una prueba real.
