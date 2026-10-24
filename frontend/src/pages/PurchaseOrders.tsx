import { FormEvent, useEffect, useState } from 'react';
import { ClipboardList, Plus, Pencil, Trash2, X, Link2, Minus } from 'lucide-react';
import { api, moneyRD, isoDate } from '../api';
import type { Lookup, Order, OrderInput, OrderLine } from '../types';

type Lists = { departments: Lookup[]; employees: Lookup[]; suppliers: Lookup[]; articles: Lookup[] };
const emptyOrder = (): OrderInput => ({
  number: 'OC-' + Date.now().toString().slice(-7), orderDate: isoDate(),
  status: 'Borrador', departmentId: 0, employeeId: 0, supplierId: 0,
  lines: [{ articleId: 0, quantity: 1, unitCost: 0 }],
});
const blankLists: Lists = { departments: [], employees: [], suppliers: [], articles: [] };
export function PurchaseOrders() {
  const [rows, setRows] = useState<Order[]>([]);
  const [lists, setLists] = useState<Lists>(blankLists);
  const [editing, setEditing] = useState<number | null>(null);
  const [form, setForm] = useState<OrderInput | null>(null);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [busy, setBusy] = useState(false);
  const load = async () => {
    const [orders, departments, employees, suppliers, articles] = await Promise.all([
      api<Order[]>('/purchase-orders'), api<Lookup[]>('/departments'), api<Lookup[]>('/employees'),
      api<Lookup[]>('/suppliers'), api<Lookup[]>('/articles'),
    ]);
    setRows(orders); setLists({ departments, employees, suppliers, articles });
  };
  useEffect(() => { void load().catch(e => setError(String(e))); }, []);
  function openNew() { setError(''); setEditing(null); setForm(emptyOrder()); }
  function openEdit(order: Order) {
    setEditing(order.id); setError('');
    setForm({ number: order.number, orderDate: order.orderDate, status: order.status,
      departmentId: order.departmentId, employeeId: order.employeeId, supplierId: order.supplierId,
      lines: order.lines.map(l => ({ articleId: l.articleId, quantity: l.quantity, unitCost: l.unitCost })) });
  }
  function change<K extends keyof OrderInput>(key: K, value: OrderInput[K]) {
    setForm(f => f ? { ...f, [key]: value, ...(key === 'departmentId' ? { employeeId: 0 } : {}) } : null);
  }
  function changeLine(index: number, key: keyof OrderLine, value: number) {
    setForm(f => f && { ...f, lines: f.lines.map((l, i) => i === index ? { ...l, [key]: value } : l) });
  }
  async function submit(e: FormEvent) {
    e.preventDefault(); if (!form) return;
    setBusy(true); setError('');
    try {
      await api('/purchase-orders' + (editing ? '/' + editing : ''), { method: editing ? 'PUT' : 'POST', body: JSON.stringify(form) });
      setNotice('Orden guardada correctamente.'); setForm(null);
      await load();
    } catch (e) { setError(e instanceof Error ? e.message : String(e)); }
    finally { setBusy(false); }
  }
  async function remove(order: Order) {
    if (!confirm('¿Eliminar esta orden en borrador?')) return;
    try { await api('/purchase-orders/' + order.id, { method: 'DELETE' }); await load(); setNotice('Orden eliminada.'); }
    catch (e) { setError(String(e)); }
  }
  async function accounting(order: Order) {
    if (!confirm('¿Generar y transmitir (si hay servicio configurado) los asientos DB/CR?')) return;
    try {
      const result = await api<{ sent: number; configured: boolean }>('/purchase-orders/' + order.id + '/accounting', { method: 'POST' });
      setNotice(result.configured ? 'Envío procesado. Asientos enviados: ' + result.sent : 'Asientos generados como pendientes; configure el servicio contable.');
      await load();
    } catch (e) { setError(String(e)); }
  }
  return <>
    {notice && <div className="alert success" role="status">{notice}</div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <section className="panel">
      <div className="panel-title"><div><h2>Órdenes registradas</h2><p>Gestión de encabezados y detalles de compra</p></div><button className="button" onClick={openNew}><Plus size={17}/> Nueva orden</button></div>
      <div className="table-scroll"><table><thead><tr><th>Orden</th><th>Fecha</th><th>Departamento</th><th>Empleado</th><th>Proveedor</th><th>Estado</th><th className="align-right">Total</th><th className="align-right">Acciones</th></tr></thead>
        <tbody>{rows.map(row => <tr key={row.id}><td><strong>{row.number}</strong><small className="subcell">{row.lines.length} artículos</small></td><td>{row.orderDate}</td><td>{row.department}</td><td>{row.employee}</td><td>{row.supplier}</td><td><span className={'pill ' + (row.status === 'Confirmada' ? 'ok' : '')}>{row.status}</span></td><td className="align-right"><strong>{moneyRD(row.total)}</strong></td>
          <td><div className="table-actions">
            <button className="icon-button" aria-label={'Editar ' + row.number} onClick={() => openEdit(row)}><Pencil size={16}/></button>
            {row.status === 'Confirmada' && <button className="icon-button" aria-label={'Contabilizar ' + row.number} onClick={() => void accounting(row)}><Link2 size={16}/></button>}
            {row.status === 'Borrador' && <button className="icon-button red" aria-label={'Eliminar ' + row.number} onClick={() => void remove(row)}><Trash2 size={16}/></button>}
          </div></td></tr>)}
        {rows.length === 0 && <tr><td colSpan={8} className="empty"><ClipboardList size={32}/><p>No hay órdenes registradas.</p></td></tr>}
        </tbody></table></div>
    </section>
    <div className="info-box">Solo pueden contabilizarse órdenes confirmadas con monto mayor que cero. El sistema conserva las cuentas DB y CR y evita duplicar asientos.</div>
    {form && <div className="modal-backdrop"><section className="modal wide" role="dialog" aria-modal="true" aria-label="Formulario de orden">
      <div className="modal-head"><div><h2>{editing ? 'Editar orden' : 'Nueva orden de compra'}</h2><p>Registra encabezado, proveedor, departamento y artículos.</p></div><button className="icon-button" aria-label="Cerrar" onClick={() => setForm(null)}><X/></button></div>
      <form onSubmit={e => void submit(e)} className="form-grid cols-2">
        <label className="field"><span>Número *</span><input required maxLength={40} value={form.number} onChange={e => change('number', e.target.value)}/></label>
        <label className="field"><span>Fecha *</span><input required type="date" value={form.orderDate} onChange={e => change('orderDate', e.target.value)}/></label>
        <label className="field"><span>Departamento *</span><select required value={form.departmentId || ''} onChange={e => change('departmentId', Number(e.target.value))}><option value="">Seleccione...</option>{lists.departments.filter(x => (x as {status?:string}).status !== 'Inactivo').map(x => <option key={x.id} value={x.id}>{x.name}</option>)}</select></label>
        <label className="field"><span>Empleado *</span><select required value={form.employeeId || ''} onChange={e => change('employeeId', Number(e.target.value))}><option value="">Seleccione...</option>{lists.employees.filter(x => x.departmentId === form.departmentId).map(x => <option key={x.id} value={x.id}>{x.name}</option>)}</select></label>
        <label className="field"><span>Proveedor *</span><select required value={form.supplierId || ''} onChange={e => change('supplierId', Number(e.target.value))}><option value="">Seleccione...</option>{lists.suppliers.map(x => <option key={x.id} value={x.id}>{x.commercialName}</option>)}</select></label>
        <label className="field"><span>Estado *</span><select value={form.status} onChange={e => change('status', e.target.value)}><option>Borrador</option><option>Confirmada</option><option>Anulada</option></select></label>
        <div className="line-header"><strong>Detalle de artículos</strong><button type="button" className="button secondary" onClick={() => setForm(f => f && { ...f, lines: [...f.lines, { articleId: 0, quantity: 1, unitCost: 0 }] })}><Plus size={15}/> Añadir</button></div>
        {form.lines.map((line, index) => <div className="line-entry" key={index}>
          <label className="field"><span>Artículo *</span><select required value={line.articleId || ''} onChange={e => changeLine(index, 'articleId', Number(e.target.value))}><option value="">Seleccione...</option>{lists.articles.map(x => <option key={x.id} value={x.id}>{x.description}</option>)}</select></label>
          <label className="field"><span>Cantidad *</span><input min={1} type="number" required value={line.quantity} onChange={e => changeLine(index, 'quantity', Number(e.target.value))}/></label>
          <label className="field"><span>Costo RD$ *</span><input min={0} step=".01" type="number" required value={line.unitCost} onChange={e => changeLine(index, 'unitCost', Number(e.target.value))}/></label>
          <button type="button" aria-label="Quitar detalle" className="icon-button red" disabled={form.lines.length === 1} onClick={() => setForm(f => f && { ...f, lines: f.lines.filter((_, i) => i !== index) })}><Minus size={17}/></button>
        </div>)}
        <div className="order-total"><span>Total estimado</span><strong>{moneyRD(form.lines.reduce((sum, l) => sum + l.quantity * l.unitCost, 0))}</strong></div>
        <div className="form-actions"><button type="button" className="button secondary" onClick={() => setForm(null)}>Cancelar</button><button disabled={busy} type="submit" className="button">{busy ? 'Guardando...' : 'Guardar orden'}</button></div>
      </form>
    </section></div>}
  </>;
}
