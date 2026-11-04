using System.ComponentModel.DataAnnotations;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompraNexus.Api.Controllers;

public sealed record IdentificationInput([Required, StringLength(32)] string Number);

[ApiController]
[Authorize(Roles = "Administrador,Compras")]
[Route("api/identifications")]
public sealed class IdentificationController : ControllerBase
{
    // POST impide colocar cédulas en URL, historial y logs de solicitudes.
    // Está protegido por el filtro antiforgery global igual que los demás POST.
    [HttpPost("validate")]
    public ActionResult<TaxIdValidation> Validate([FromBody] IdentificationInput input) =>
        Ok(DominicanTaxId.Check(input.Number));
}
