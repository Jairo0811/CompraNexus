# WS Contabilidad — Contrato provisional

La presentación de UNAPEC exige transmisión de: identificador del asiento, descripción, identificador de tipo de inventario, cuenta contable, movimiento DB/CR, fecha, monto y estado.

`AccountingGateway` prepara **dos registros por orden confirmada** (uno DB y otro CR, importes iguales), con UUID idempotente para evitar duplicados. `Accounting:ServiceUrl` y `Accounting:BearerToken` se configuran por entorno. En Docker, use `ACCOUNTING_SERVICE_URL` y `ACCOUNTING_BEARER_TOKEN`. Si la URL está vacía, conserva los asientos con estado «Pendiente» sin afirmar que han sido transmitidos.

Se necesita coordinar con el equipo de Contabilidad antes de declarar compatible el contrato. Acordar URL, método, autenticación, esquema JSON, cuentas de inventario/proveedores y formato de confirmación. Las cuentas `1401` y `2101` son **provisionales**, no asientos validados por contadores.

## JSON enviado (provisional)
```json
{
  "identificadorAsiento": "e08c0741-b257-4e28-9bb5-12f5d94e986a",
  "descripcion": "Orden de compra OC-1001",
  "identificadorTipoInventario": "1",
  "cuentaContable": "1401",
  "tipoMovimiento": "DB",
  "fechaAsiento": "2026-10-24",
  "montoAsiento": 1951.50,
  "estado": "Activo"
}
```

El receptor debe respetar `Idempotency-Key` para evitar duplicados cuando hay timeout o reintento. Un HTTP 2xx no demuestra aceptación contable semántica sin respuesta contractual validada.

## Contabilización: semántica de la respuesta y duplicados

`POST /api/purchase-orders/{id}/accounting` devuelve `{ created, pending, sent, configured }`:

- `created`: asientos **nuevos** creados en esa solicitud (2 la primera vez, 0 después).
- `pending`: asientos de la orden que siguen sin enviarse.
- `sent`: asientos transmitidos con éxito en esa solicitud.

Existe un índice único `(PurchaseOrderId, Movement)` para impedir más de un DB y un CR por orden aunque
lleguen solicitudes concurrentes. En bases ya creadas con `EnsureCreated` se aplica de forma aditiva al
arrancar en Development (`SchemaPatches`) o manualmente con `scripts/sql/001-accounting-unique-index.sql`.
Sigue siendo un contrato **provisional**: no hay integración externa verificada.

## Estados de asiento y política de reintentos

| Estado | Significado | ¿Se reintenta? |
|---|---|---|
| Pendiente | Creado, aún sin enviar o sin servicio configurado | Sí |
| Enviado | El servicio respondió 2xx | No |
| Error | 5xx, 408, 429, timeout o error de red | Sí, hasta `Accounting:MaxAttempts` (5 por defecto) |
| Rechazado | El servicio respondió otro 4xx | No: requiere revisión manual |

Cada reintento reutiliza la misma `Idempotency-Key` (el `JournalId` del asiento). Esa garantía solo es real si el
servicio contable de destino la respeta; **no está verificado**. Los envíos se serializan dentro del proceso.

### Servicio simulado vs. real
Las pruebas usan `FakeAccountingHandler` (un `HttpMessageHandler` en el proyecto de pruebas): simula 200, 4xx, 5xx y
timeout sin red. Demuestra el comportamiento de CompraNexus ante esas respuestas, **no** la compatibilidad con la
Contabilidad de otros grupos.
