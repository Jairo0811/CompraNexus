export type Status = 'Activo' | 'Inactivo';
export type CatalogRecord = Record<string, string | number> & { id: number; status: Status };
export interface Lookup { id: number; name?: string; description?: string; commercialName?: string; departmentId?: number; status?: string; }
export interface OrderLine { id?: number; articleId: number; article?: string; unitMeasureId?: number; unitMeasure?: string; quantity: number; unitCost: number; }
export interface Order {
  id: number; number: string; orderDate: string; status: string;
  departmentId: number; department: string; employeeId: number; employee: string;
  supplierId: number; supplier: string; total: number; lines: OrderLine[];
}
export interface OrderInput {
  number: string; orderDate: string; status: string; departmentId: number; employeeId: number; supplierId: number; lines: OrderLine[];
}
export interface AccountingEntry {
  id: number; journalId: string; purchaseOrderId: number; description: string;
  inventoryTypeId: string; account: string; movement: string;
  entryDate: string; amount: number; status: string; attempts: number;
}
