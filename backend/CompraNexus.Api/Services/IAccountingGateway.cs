using CompraNexus.Api.Domain;

namespace CompraNexus.Api.Services;

/// <summary>Contrato interno con el servicio contable. La implementación real es <see cref="AccountingGateway"/>;
/// las pruebas usan un HttpMessageHandler simulado, nunca el servicio de otro grupo.</summary>
public interface IAccountingGateway
{
    bool Configured { get; }
    Task<int> PrepareAsync(PurchaseOrder order, CancellationToken ct);
    Task<int> TransmitAsync(IEnumerable<AccountingEntry> entries, CancellationToken ct);
}
