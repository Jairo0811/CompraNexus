import { describe, expect, it } from 'vitest';
import { allowedStatuses, isEditable } from './orderRules';

describe('order status rules (mirror of OrderRules.cs)', () => {
  it('new orders can only start as Borrador or Confirmada', () => {
    expect(allowedStatuses(null)).toEqual(['Borrador', 'Confirmada']);
  });
  it('a Confirmada order cannot go back to Borrador', () => {
    expect(allowedStatuses('Confirmada')).toEqual(['Confirmada', 'Anulada']);
  });
  it('an Anulada order is final and not editable', () => {
    expect(allowedStatuses('Anulada')).toEqual([]);
    expect(isEditable('Anulada')).toBe(false);
    expect(isEditable('Borrador')).toBe(true);
  });
});
