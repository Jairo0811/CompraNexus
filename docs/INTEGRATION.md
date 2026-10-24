# Contrato propuesto — Interfaz con Contabilidad

**Importante:** La presentación del profesor enumera los datos de los asientos, pero **no proporciona** una URL, verbos HTTP, estructura JSON, autenticación, catálogo de cuentas ni reglas de respuesta. El contrato siguiente es **provisional** y deberá adaptarse al servicio real del otro grupo.

## Configuración

```text
ACCOUNTING_WS_URL=https://servidor-real.example/api/asientos
ACCOUNTING_WS_TOKEN=<token-del-grupo>
ACCOUNTING_INVENTORY_TYPE_ID=1
ACCOUNTING_DEBIT_ACCOUNT=1401
ACCOUNTING_CREDIT_ACCOUNT=2101
```

Los valores de cuentas de ejemplo y el tipo de inventario son **supuestos técnicos**, no cuentas autorizadas por el profesor. No activar la transmisión sin validarlos con Contabilidad.

## Flujo

1. Registrar una orden con una o más líneas, con departamento, empleado y proveedor.
2. Cambiar el estado a **Confirmada**; no se contabilizan borradores o anuladas.
3. Seleccionar «Contabilizar». Se crean un asiento DB y otro CR por el **total** de la orden. Se preservan los UUID en reintentos.
4. Si `ACCOUNTING_WS_URL` existe, se hace un `POST` por asiento con encabezado `Idempotency-Key`.
5. Respuesta HTTP 2xx → **Enviado**; respuesta de error/transporte → **Error**; sin URL → **Pendiente**, sin solicitar red.
6. Se permiten reintentos; nunca se reenvía un asiento ya marcado como Enviado.

## Solicitud JSON provisional

```json
{
  "identificadorAsiento": "<uuid>",
  "descripcion": "Orden de compra OC-1001",
  "identificadorTipoInventario": "1",
  "cuentaContable": "1401",
  "tipoMovimiento": "DB",
  "fechaAsiento": "2026-10-24",
  "montoAsiento": 1800.0,
  "estado": "Activo"
}
```

**Conciliación:** para una orden de RD$1,800 se generan dos mensajes del mismo monto, uno DB y uno CR; cada asiento es una fila distinta. Esto no implica que el receptor acepte el contrato propuesto. Ajustar nombres de propiedades, tipo de moneda, formato de fecha, firma/autenticación y códigos según el contrato firmado por ambos grupos.

## Riesgos pendientes

- Cuando un WS recibe el mensaje pero la respuesta se pierde, el reintento exige que el receptor reconozca `Idempotency-Key`; no se debe asumir soporte sin comprobarlo.
- La app evita editar órdenes que ya tengan asientos, pero no sustituye un sistema contable transaccional ni un libro mayor.
- La transmisión no equivale a contabilización confirmada hasta que el sistema receptor valide semánticamente el asiento.
- El alcance de otras integraciones con grupos distintos de Contabilidad no se conoce todavía.
