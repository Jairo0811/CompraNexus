export async function api<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch('/api' + path, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...(options?.headers ?? {}) },
  });
  if (!res.ok) {
    const raw: unknown = await res.json().catch(() => ({}));
    const message = typeof raw === 'object' && raw !== null && 'message' in raw
      ? String((raw as { message: unknown }).message)
      : 'Error HTTP ' + res.status;
    throw new Error(message);
  }
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
export const money = (value: number) => new Intl.NumberFormat('es-DO', {
  minimumFractionDigits: 2, maximumFractionDigits: 2,
}).format(value);
export const moneyRD = (v: number) => 'RD$ ' + money(v);
export const isoDate = () => new Date().toISOString().slice(0, 10);
