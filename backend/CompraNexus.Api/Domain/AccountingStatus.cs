namespace CompraNexus.Api.Domain;

/// <summary>Estados de un asiento: Pendiente → Enviado | Error (reintentable) | Rechazado (4xx; revisión manual).</summary>
public static class AccountingStatus
{
    public const string Pending = "Pendiente";
    public const string Sent = "Enviado";
    public const string Error = "Error";
    public const string Rejected = "Rechazado";

    public static bool IsRetryable(string status, int attempts, int maxAttempts) =>
        (status is Pending or Error) && attempts < maxAttempts;
}
