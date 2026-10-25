namespace CompraNexus.Api.Domain;

/// <summary>
/// Reglas de estado de las órdenes. Propuesta técnica: los requisitos del curso no definen
/// transiciones, por lo que deben confirmarse con el docente antes de considerarse oficiales.
/// </summary>
public static class OrderRules
{
    public static bool CanCreateAs(string status) => status is "Borrador" or "Confirmada";

    /// <summary>Borrador→Confirmada/Anulada, Confirmada→Anulada. Anulada es final.</summary>
    public static bool CanTransition(string from, string to) => (from, to) switch
    {
        ("Borrador", "Borrador" or "Confirmada" or "Anulada") => true,
        ("Confirmada", "Confirmada" or "Anulada") => true,
        _ => false
    };
}
