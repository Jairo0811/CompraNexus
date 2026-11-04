namespace CompraNexus.Api.Domain;

/// <summary>
/// Validación matemática local: Luhn (cédula) y módulo 11 (RNC).
/// No confirma identidad, titularidad, registro ni estado ante DGII/JCE.
/// </summary>
public static class DominicanTaxId
{
    private static readonly int[] RncWeights = [7, 9, 8, 6, 5, 4, 3, 2];

    public static TaxIdValidation Check(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Invalid("Introduce una cédula o RNC.");
        if (input.Length > 32)
            return Invalid("Identificador demasiado largo.");

        var digits = new System.Text.StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is >= '0' and <= '9')
                digits.Append(c);
            else if (c is not ('-' or ' '))
                return Invalid("Solo se permiten dígitos, espacios y guiones.");
        }

        var number = digits.ToString();
        if (number.Length is not (9 or 11))
            return Invalid("El RNC debe tener 9 dígitos y la cédula 11.");
        if (number.All(c => c == '0'))
            return Invalid("El documento no puede contener solamente ceros.");

        if (number.Length == 11)
        {
            if (!ValidCedula(number))
                return Invalid("Dígito verificador de cédula incorrecto. Algunas cédulas históricas requieren verificación oficial.", number, "cedula");

            return new(true, number, "cedula",
                "Cédula con formato y dígito verificador correctos. No confirma identidad ni vigencia.");
        }

        if (!ValidRnc(number))
            return Invalid("Dígito verificador de RNC incorrecto.", number, "rnc");
        return new(true, number, "rnc",
            "RNC con formato y dígito verificador correctos. No confirma inscripción ni estado ante DGII.");
    }

    private static bool ValidCedula(string number)
    {
        var sum = 0;
        for (var i = 0; i < number.Length; i++)
        {
            var digit = number[number.Length - 1 - i] - '0';
            if (i % 2 == 1)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }
        return sum % 10 == 0;
    }

    private static bool ValidRnc(string number)
    {
        var sum = 0;
        for (var i = 0; i < 8; i++)
            sum += (number[i] - '0') * RncWeights[i];
        var remainder = sum % 11;
        var expected = remainder switch { 0 => 2, 1 => 1, _ => 11 - remainder };
        return number[8] - '0' == expected;
    }

    private static TaxIdValidation Invalid(string message, string normalized = "", string? type = null) =>
        new(false, normalized, type, message);
}

public sealed record TaxIdValidation(bool IsValid, string Normalized, string? Type, string Message);
