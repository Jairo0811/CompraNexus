-- CompraNexus: un solo asiento DB y un solo CR por orden (protege ante solicitudes concurrentes).
-- Aditivo e idempotente: no borra ni modifica datos. Ejecutar sobre la base CompraNexus.
-- Si existen duplicados previos, el índice NO se crea y se listan para revisión manual.
USE CompraNexus;
GO
IF EXISTS (SELECT 1 FROM dbo.AccountingEntries GROUP BY PurchaseOrderId, Movement HAVING COUNT(*) > 1)
BEGIN
    PRINT 'Hay asientos duplicados; resuélvalos manualmente antes de crear el índice:';
    SELECT PurchaseOrderId, Movement, COUNT(*) AS Total
    FROM dbo.AccountingEntries GROUP BY PurchaseOrderId, Movement HAVING COUNT(*) > 1;
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_AccountingEntries_PurchaseOrderId_Movement'
                      AND object_id = OBJECT_ID(N'dbo.AccountingEntries'))
BEGIN
    CREATE UNIQUE INDEX IX_AccountingEntries_PurchaseOrderId_Movement
        ON dbo.AccountingEntries (PurchaseOrderId, Movement);
    PRINT 'Índice creado.';
END
ELSE PRINT 'El índice ya existe.';
GO
