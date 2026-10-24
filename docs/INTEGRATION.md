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
