const NETWORK_MESSAGE = 'No se pudo conectar con la API. Verifique que el servicio y SQL Server estén disponibles.';
export const SESSION_EXPIRED = 'compranexus:session-expired';

export type SessionUser = { id: number; email: string; role: string };

// Token CSRF solo en memoria; la cookie de sesión permanece HttpOnly.
let csrfTokenPromise: Promise<string> | null = null;
export function resetCsrfToken(): void { csrfTokenPromise = null; }

async function csrfToken(): Promise<string> {
  if (!csrfTokenPromise) {
    csrfTokenPromise = fetch('/api/auth/csrf', { credentials: 'same-origin' })
      .then(async res => {
        if (!res.ok) throw new Error('No se pudo iniciar la protección de sesión.');
        const data = await res.json() as { token: string };
        if (!data.token) throw new Error('El servidor no proporcionó el token de seguridad.');
        return data.token;
      }).catch(e => { csrfTokenPromise = null; throw e; });
  }
  return csrfTokenPromise;
}

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
  if (status === 401) return 'Sesión no válida o credenciales incorrectas.';
  if (status === 403) return 'No tienes permisos para realizar esta operación.';
  if (status === 502 || status === 503 || status === 504) return NETWORK_MESSAGE;
  return 'Error HTTP ' + status;
}
export const errorMessage = (e: unknown): string => (e instanceof Error ? e.message : String(e));

export async function getSession(): Promise<SessionUser | null> {
  let res: Response;
  try { res = await fetch('/api/auth/me', { credentials: 'same-origin', cache: 'no-store' }); }
  catch { throw new Error(NETWORK_MESSAGE); }
  if (res.status === 401) return null;
  if (!res.ok) throw new Error('No se pudo comprobar la sesión.');
  return res.json() as Promise<SessionUser>;
}

export async function api<T>(path: string, options?: RequestInit): Promise<T> {
  const method = (options?.method ?? 'GET').toUpperCase();
  const headers = new Headers(options?.headers);
  if (options?.body !== undefined && !headers.has('Content-Type'))
    headers.set('Content-Type', 'application/json');
  if (!['GET', 'HEAD', 'OPTIONS', 'TRACE'].includes(method))
    headers.set('X-CSRF-TOKEN', await csrfToken());

  let res: Response;
  try {
    res = await fetch('/api' + path, { ...options, headers, credentials: 'same-origin' });
  } catch {
    throw new Error(NETWORK_MESSAGE);
  }
  if (!res.ok) {
    if (res.status === 401 && path !== '/auth/login' && typeof window !== 'undefined')
      window.dispatchEvent(new Event(SESSION_EXPIRED));
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
