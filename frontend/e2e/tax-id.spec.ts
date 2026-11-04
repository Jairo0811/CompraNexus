import { test, expect } from '@playwright/test';

test('El formulario de proveedores comprueba cédula y RNC sin consultar servicios externos', async ({ page }) => {
  await page.route('**/api/auth/me', route => route.fulfill({ status: 401, json: {} }));
  await page.route('**/api/auth/csrf', route => route.fulfill({ json: { token: 'csrf-test' } }));
  await page.route('**/api/auth/login', route => route.fulfill({
    json: { id: 1, email: 'qa@example.test', role: 'Administrador' },
  }));
  await page.route('**/api/suppliers', route => route.fulfill({ json: [] }));

  let verificationCalls = 0;
  await page.route('**/api/identifications/validate', async route => {
    verificationCalls++;
    expect(route.request().method()).toBe('POST');
    expect(route.request().headers()['x-csrf-token']).toBe('csrf-test');
    const body = route.request().postDataJSON() as { number: string };
    const isValid = body.number === '101000007';
    await route.fulfill({ json: {
      isValid, normalized: body.number, type: 'rnc',
      message: isValid
        ? 'RNC con formato y dígito verificador correctos. No confirma inscripción ni estado ante DGII.'
        : 'Dígito verificador de RNC incorrecto.',
    } });
  });
  await page.goto('/login');
  await page.getByRole('textbox', { name: 'Correo electrónico' }).fill('qa@example.test');
  await page.getByLabel('Contraseña', { exact: true }).fill('ClaveSoloParaMock123!');
  await page.getByRole('button', { name: 'Iniciar sesión' }).click();
  await page.getByRole('link', { name: 'Proveedores' }).click();
  await page.getByRole('button', { name: 'Nuevo registro' }).click();

  const id = page.locator('#supplier-tax-id');
  await id.fill('101000001');
  await page.getByRole('button', { name: 'Validar', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Dígito verificador de RNC incorrecto' })).toBeVisible();

  await id.fill('101000007');
  await expect(page.getByRole('status').filter({ hasText: 'Dígito verificador de RNC incorrecto' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Validar', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'RNC con formato' })).toBeVisible();
  expect(verificationCalls).toBe(2);
  await expect(page.getByText('No certifica existencia ni estado en DGII o JCE.')).toBeVisible();
});
