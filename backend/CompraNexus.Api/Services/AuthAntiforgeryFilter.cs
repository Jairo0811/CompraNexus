using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompraNexus.Api.Services;

/// <summary>
/// Antiforgery para API REST: no requiere MVC Views / ViewFeatures.
/// Protege toda operación no segura, incluida la creación y destrucción de sesión.
/// </summary>
public sealed class AuthAntiforgeryFilter(IAntiforgery antiforgery) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method)) return;

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Token de seguridad ausente o inválido. Actualice la sesión y vuelva a intentarlo."
            });
        }
    }
}
