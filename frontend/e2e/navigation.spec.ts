import { test, expect } from '@playwright/test';

test('El sistema solicita credenciales cuando no hay sesión', async ({ page }) => {
  await page.route('**/api/auth/me', route => route.fulfill({ status: 401, json: { message: 'No autenticado' } }));
  await page.goto('/orders');
  await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
  await expect(page).toHaveURL(/\/login$/);
});

test('El login permite acceder al dashboard y a departamentos (API simulada)', async ({ page }) => {
  await page.route('**/api/auth/me', route => route.fulfill({ status: 401, json: {} }));
  await page.route('**/api/auth/csrf', route => route.fulfill({ json: { token: 'csrf-test' } }));
  await page.route('**/api/auth/login', async route => {
    const headers = route.request().headers();
    expect(headers['x-csrf-token']).toBe('csrf-test');
    await route.fulfill({ json: { id: 1, email: 'qa@example.test', role: 'Administrador' } });
  });
  await page.route('**/api/dashboard', route => route.fulfill({ json: {
    departments: 2, articles: 4, suppliers: 3, orders: 1
  } }));
  await page.route('**/api/departments', route => route.fulfill({ json: [
    { id: 1, name: 'Tecnología', status: 'Activo' }
  ] }));
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
  await page.getByRole('textbox', { name: 'Correo electrónico' }).fill('qa@example.test');
  await page.getByLabel('Contraseña', { exact: true }).fill('ClaveSoloParaMock123!');
  await page.getByRole('button', { name: 'Iniciar sesión' }).click();
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  await expect(page.getByText('Compras conectadas.')).toBeVisible();
  await page.getByRole('link', { name: 'Departamentos' }).first().click();
  await expect(page.getByRole('heading', { name: 'Departamentos', exact: true })).toBeVisible();
  await expect(page.getByText('Tecnología')).toBeVisible();
  await page.getByRole('button', { name: 'Nuevo registro' }).click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Guardar registro' })).toBeVisible();
});
