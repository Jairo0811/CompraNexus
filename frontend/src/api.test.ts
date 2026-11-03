import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, errorMessage, getSession, messageFrom, resetCsrfToken } from './api';

const respond = (status: number, body?: unknown) =>
  vi.stubGlobal('fetch', vi.fn(async (input: string) => {
    if (input === '/api/auth/csrf') return new Response(JSON.stringify({ token: 'token-prueba' }), { status: 200 });
    return new Response(body === undefined ? null : JSON.stringify(body), { status });
  }));

afterEach(() => { vi.unstubAllGlobals(); resetCsrfToken(); });

describe('api y seguridad', () => {
  it('explica errores de validación y acceso', () => {
    expect(messageFrom({ message: 'Duplicado' }, 409)).toBe('Duplicado');
    expect(messageFrom({ errors: { Name: ['Requerido'] } }, 400)).toBe('Requerido');
    expect(messageFrom({ title: 'Not Found' }, 404)).toBe('Not Found');
    expect(messageFrom({}, 500)).toBe('Error HTTP 500');
    expect(messageFrom({}, 403)).toMatch(/permisos/);
  });
  it('explica API caída por gateway y error de red', async () => {
    respond(502);
    await expect(api('/departments')).rejects.toThrow(/No se pudo conectar/);
    vi.stubGlobal('fetch', vi.fn(async () => { throw new Error('offline'); }));
    await expect(api('/departments')).rejects.toThrow(/SQL Server/);
  });
  it('propaga validaciones del backend', async () => {
    respond(400, { errors: { Description: ['La descripción es obligatoria.'] } });
    await expect(api('/units', { method: 'POST', body: '{}' })).rejects.toThrow('La descripción es obligatoria.');
  });
  it('envía CSRF en POST y no en GET', async () => {
    const spy = vi.fn(async (path: string, init?: RequestInit) =>
      path === '/api/auth/csrf'
        ? new Response(JSON.stringify({ token: 'token-prueba' }))
        : new Response(JSON.stringify({ ok: true })));
    vi.stubGlobal('fetch', spy);
    await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'qa@example.test', password: 'x' }) });
    expect(spy).toHaveBeenCalledTimes(2);
    const headers = spy.mock.calls[1][1]?.headers as Headers;
    expect(headers.get('X-CSRF-TOKEN')).toBe('token-prueba');
    await api('/departments');
    expect(spy).toHaveBeenCalledTimes(3);
    const getHeaders = spy.mock.calls[2][1]?.headers as Headers;
    expect(getHeaders.has('X-CSRF-TOKEN')).toBe(false);
  });
  it('identifica ausencia de sesión y acepta una sesión activa', async () => {
    respond(401, { message: 'No autorizado' });
    await expect(getSession()).resolves.toBeNull();
    respond(200, { id: 3, email: 'qa@example.test', role: 'Administrador' });
    await expect(getSession()).resolves.toEqual({ id: 3, email: 'qa@example.test', role: 'Administrador' });
  });
  it('204 devuelve undefined; JSON válido devuelve datos', async () => {
    respond(204);
    await expect(api('/units/1', { method: 'DELETE' })).resolves.toBeUndefined();
    respond(200, [{ id: 1 }]);
    await expect(api('/units')).resolves.toEqual([{ id: 1 }]);
  });
  it('formatea errores', () => {
    expect(errorMessage(new Error('x'))).toBe('x');
    expect(errorMessage('y')).toBe('y');
  });
});
