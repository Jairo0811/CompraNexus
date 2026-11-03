import { test, expect } from '@playwright/test';

test('Favicon de CompraNexus es un archivo ICO real', async ({ request }) => {
  const response = await request.get('/favicon.ico');
  expect(response.ok()).toBeTruthy();
  expect(response.headers()['content-type']).toMatch(/image\//);
  const bytes = await response.body();
  expect(bytes.byteLength).toBeGreaterThan(100);
});
