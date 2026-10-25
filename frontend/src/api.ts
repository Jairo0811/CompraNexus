const NETWORK_MESSAGE = 'No se pudo conectar con la API. Verifique que el servicio y SQL Server estén disponibles.';

/** Extrae un mensaje legible de {message}, ProblemDetails (errors/title) o del código HTTP. */
export function messageFrom(raw: unknown, status: number): string {
  if (typeof raw === 'object' && raw !== null) {
    const r = raw as Record<string, unknown>;
    if (typeof r.message === 'string' && r.message) return r.message;
    if (typeof r.errors === 'object' && r.errors !== null) {
      const msgs = Object.values(r.errors as Record<string, unknown>)
        .flatMap(v => (Array.isArray(v) ? v.map(String) : [String(v)]));
      if (msgs.length) return msgs.join(' ');
    }
    if (typeof r.title === 'string' && r.title) return r.title;
  }
  if (status === 502 || status === 503 || status === 504) return NETWORK_MESSAGE;
  return 'Error HTTP ' + status;
}

export const errorMessage = (e: unknown): string => (e instanceof Error ? e.message : String(e));

export async function api<T>(path: string, options?: RequestInit): Promise<T> {
  let res: Response;
  try {
    res = await fetch('/api' + path, {
      ...options,
      headers: { 'Content-Type': 'application/json', ...(options?.headers ?? {}) },
    });
  } catch {
    throw new Error(NETWORK_MESSAGE);
  }
  if (!res.ok) {
    const raw: unknown = await res.json().catch(() => ({}));
    throw new Error(messageFrom(raw, res.status));
  }
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
export const money = (value: number) => new Intl.NumberFormat('es-DO', {
  minimumFractionDigits: 2, maximumFractionDigits: 2,
}).format(value);
export const moneyRD = (v: number) => 'RD$ ' + money(v);
export const isoDate = () => new Date().toISOString().slice(0, 10);
