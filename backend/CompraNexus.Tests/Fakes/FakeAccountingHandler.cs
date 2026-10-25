using System.Net;

namespace CompraNexus.Tests.Fakes;

/// <summary>
/// SERVICIO CONTABLE SIMULADO. Sustituye al servicio real mediante HttpMessageHandler: no hace red,
/// registra cada solicitud y devuelve respuestas programadas. Nada de esto prueba el contrato
/// ni la disponibilidad del servicio de Contabilidad de otros grupos.
/// </summary>
public sealed class FakeAccountingHandler(Func<HttpRequestMessage, int, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<string> Bodies { get; } = [];
    public List<string> IdempotencyKeys { get; } = [];
    public int Calls => Bodies.Count;

    public static FakeAccountingHandler Always(HttpStatusCode code, string body = "{}") =>
        new((_, _) => new HttpResponseMessage(code) { Content = new StringContent(body) });

    /// <summary>Simula el timeout del HttpClient (TaskCanceledException sin cancelación del llamador).</summary>
    public static FakeAccountingHandler Timeout() =>
        new((_, _) => throw new TaskCanceledException("Simulated timeout"));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IdempotencyKeys.Add(request.Headers.TryGetValues("Idempotency-Key", out var values) ? values.Single() : "");
        Bodies.Add(request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken));
        return respond(request, Bodies.Count);
    }
}
