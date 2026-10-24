export type FieldType = 'text' | 'number' | 'status' | 'units' | 'departments';
export type Field = { key: string; label: string; type: FieldType; required?: boolean };
export type CatalogConfig = { title: string; plural: string; fields: Field[] };

export const catalogs: Record<string, CatalogConfig> = {
  departments: { title: 'Departamentos', plural: 'departamentos', fields: [
    { key: 'name', label: 'Nombre', type: 'text', required: true },
    { key: 'status', label: 'Estado', type: 'status', required: true },
  ]},
  units: { title: 'Unidades de medida', plural: 'unidades', fields: [
    { key: 'description', label: 'Descripción', type: 'text', required: true },
    { key: 'status', label: 'Estado', type: 'status', required: true },
  ]},
  suppliers: { title: 'Proveedores', plural: 'proveedores', fields: [
    { key: 'taxId', label: 'Cédula / RNC', type: 'text', required: true },
    { key: 'commercialName', label: 'Nombre comercial', type: 'text', required: true },
    { key: 'status', label: 'Estado', type: 'status', required: true },
  ]},
  articles: { title: 'Artículos', plural: 'artículos', fields: [
    { key: 'description', label: 'Descripción', type: 'text', required: true },
    { key: 'brand', label: 'Marca', type: 'text', required: true },
    { key: 'unitMeasureId', label: 'Unidad de medida', type: 'units', required: true },
    { key: 'stock', label: 'Existencia', type: 'number', required: true },
    { key: 'status', label: 'Estado', type: 'status', required: true },
  ]},
  employees: { title: 'Empleados', plural: 'empleados', fields: [
    { key: 'name', label: 'Nombre', type: 'text', required: true },
    { key: 'departmentId', label: 'Departamento', type: 'departments', required: true },
    { key: 'status', label: 'Estado', type: 'status', required: true },
  ]},
};
export function draft(fields: Field[]) {
  return Object.fromEntries(fields.map(f => [f.key, f.type === 'status' ? 'Activo' : f.type === 'number' ? 0 : '']));
}
