import { describe, expect, it } from 'vitest';
import { catalogs, draft } from './catalogs';
import { moneyRD } from './api';
describe('CompraNexus catalog configuration', () => {
  it('has the required four CRUD modules and employees support', () => {
    for (const path of ['departments', 'units', 'suppliers', 'articles', 'employees'])
      expect(catalogs[path].fields.length).toBeGreaterThan(0);
  });
  it('builds a valid draft with active status', () => {
    expect(draft(catalogs.articles.fields)).toEqual({
      description: '', brand: '', unitMeasureId: '', stock: 0, status: 'Activo',
    });
  });
  it('uses Dominican peso formatting', () => {
    expect(moneyRD(25)).toContain('25.00');
  });
});
