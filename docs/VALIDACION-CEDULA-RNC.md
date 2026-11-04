# Validación de identificadores fiscales dominicanos

## Alcance

CompraNexus valida **cédulas dominicanas (11 dígitos)** y **RNC (9 dígitos)** al crear o editar proveedores.
El formulario permite introducir dígitos con o sin guiones/espacios, muestra una comprobación previa
mediante el botón **Validar**, y la API **vuelve a validar al guardar** (no se confía en el cliente).
El valor se almacena normalizado, solo dígitos, en `Suppliers.TaxId`; no se añade ni modifica ninguna tabla.

## Algoritmos y referencias

- **Cédula:** 11 dígitos, checksum **Luhn**, siguiendo la lógica de `validLuhn` del proyecto
  [OGTIC Cuenta Única Registry](https://github.com/ogticrd/cuenta-unica-registry).
- **RNC:** 9 dígitos; checksum **módulo 11**, pesos `[7,9,8,6,5,4,3,2]`;
  resto 0 corresponde a dígito 2, resto 1 a dígito 1 y los restantes a `11 - resto`.
  El repositorio [OGTIC DGII Services API](https://github.com/ogticrd/dgii-services-api)
  contiene un wrapper que consulta contribuyentes en DGII, pero su `RncValidatorGuard`
  solo comprueba la longitud. CompraNexus añade una comprobación matemática local propia.

## Limitaciones y privacidad

**Validación matemática NO es validación de identidad o registro real**:
una cédula que pasa Luhn no significa que pertenezca a alguien, y un RNC cuyo checksum
es correcto no significa que esté inscrito, habilitado o al día en DGII.
La API **no consulta automáticamente JCE o DGII**, no descarga bases de datos ni remite
cédulas a servicios externos: estos requieren configuración, permisos y verificación de
condiciones de acceso. Para una consulta oficial se necesitará una integración futura
autorizada con los servicios gubernamentales.

**Excepciones de cédulas históricas:** el proyecto de Cuenta Única advierte que algunas
cédulas legítimas no superan Luhn y emplea un servicio externo para validar esas excepciones.
Por eso el mensaje al usuario reconoce esta limitación; la presente versión no incorpora
un listado no verificado de identificadores personales ni lo utiliza para omitir el checksum.
Las cédulas excepcionales se deben revisar mediante un canal autorizado antes de habilitar
un mecanismo de excepción administrado.

- Solo usuarios de roles `Administrador` y `Compras` pueden usar la comprobación previa.
- La comprobación previa usa `POST /api/identifications/validate` con token CSRF y sesión,
  no parámetros en la URL para evitar exponer el documento en registros de rutas.
- El backend exige el checksum en los métodos `POST /api/suppliers` y `PUT /api/suppliers/{id}`.
- Los registros previos se conservan intactos. Si un registro antiguo falla al editarlo,
  se debe corregir su identificación con evidencia antes de guardarlo.

## Pruebas

- `DominicanTaxIdTests.cs`: casos de RNC/cédula correctos, invalidaciones, normalización
  y rechazo de guardar un proveedor inválido.
- Prueba Playwright: verificación visible del botón de validación y su resultado.
- No requiere cambios en SQL Server, credenciales, autenticación ni Docker Compose.

**No ejecutar `docker compose down -v`** en el entorno con datos académicos.
