import { test, expect } from '@playwright/test';

test('CompraNexus opens dashboard and navigates to purchasing modules', async ({ page }) => {
  await page.route('**/api/dashboard', route => route.fulfill({ json: { departments: 2, articles: 4, suppliers: 3, orders: 1 } }));
  await page.route('**/api/departments', route => route.fulfill({ json: [{ id: 1, name: 'Tecnología', status: 'Activo' }] }));
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  await expect(page.getByText('Compras conectadas.')).toBeVisible();
  await page.getByRole('link', { name: 'Departamentos' }).first().click();
  await expect(page.getByRole('heading', { name: 'Departamentos', exact: true })).toBeVisible();
  await expect(page.getByText('Tecnología')).toBeVisible();
  await page.getByRole('button', { name: 'Nuevo registro' }).click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Guardar registro' })).toBeVisible();
});
