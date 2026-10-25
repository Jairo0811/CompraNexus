import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, errorMessage, messageFrom } from './api';

const respond = (status: number, body?: unknown) =>
  vi.stubGlobal('fetch', vi.fn(async () => new Response(body === undefined ? null : JSON.stringify(body), { status })));

afterEach(() => vi.unstubAllGlobals());

describe('api error handling', () => {
  it('uses the {message} shape returned by the controllers', () => {
    expect(messageFrom({ message: 'El número de orden ya existe.' }, 409)).toBe('El número de orden ya existe.');
  });
  it('flattens ASP.NET ProblemDetails validation errors', () => {
    const raw = { title: 'One or more validation errors occurred.', errors: { Name: ['El nombre es obligatorio.'], Status: ['Estado inválido.'] } };
    expect(messageFrom(raw, 400)).toBe('El nombre es obligatorio. Estado inválido.');
  });
  it('falls back to title and then to the HTTP status', () => {
    expect(messageFrom({ title: 'Not Found' }, 404)).toBe('Not Found');
    expect(messageFrom({}, 500)).toBe('Error HTTP 500');
  });
  it('explains that the API is unavailable on gateway errors', async () => {
    respond(502);
    await expect(api('/departments')).rejects.toThrow(/No se pudo conectar con la API/);
  });
  it('explains that the API is unavailable when fetch itself fails', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => { throw new TypeError('Failed to fetch'); }));
    await expect(api('/departments')).rejects.toThrow(/SQL Server/);
  });
  it('surfaces validation messages from a 400 response', async () => {
    respond(400, { errors: { Description: ['La descripción es obligatoria.'] } });
    await expect(api('/units', { method: 'POST', body: '{}' })).rejects.toThrow('La descripción es obligatoria.');
  });
  it('returns undefined for 204 and parsed JSON otherwise', async () => {
    respond(204);
    await expect(api('/units/1', { method: 'DELETE' })).resolves.toBeUndefined();
    respond(200, [{ id: 1 }]);
    await expect(api('/units')).resolves.toEqual([{ id: 1 }]);
  });
  it('errorMessage handles Error and non-Error values', () => {
    expect(errorMessage(new Error('x'))).toBe('x');
    expect(errorMessage('y')).toBe('y');
  });
});
