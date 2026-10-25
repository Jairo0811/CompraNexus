export const ORDER_STATUSES = ['Borrador', 'Confirmada', 'Anulada'] as const;

/** Espejo de OrderRules.cs: estados que se pueden elegir al crear (current = null) o editar. */
export function allowedStatuses(current: string | null): string[] {
  if (current === null) return ['Borrador', 'Confirmada'];
  if (current === 'Borrador') return ['Borrador', 'Confirmada', 'Anulada'];
  if (current === 'Confirmada') return ['Confirmada', 'Anulada'];
  return [];
}
export const isEditable = (status: string) => allowedStatuses(status).length > 0;
