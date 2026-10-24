import { FormEvent, useEffect, useState } from 'react';
import { Search, RotateCcw } from 'lucide-react';
import { api, moneyRD } from '../api';
import type { Lookup, Order } from '../types';

export function Reports() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [departments, setDepartments] = useState<Lookup[]>([]);
  const [employees, setEmployees] = useState<Lookup[]>([]);
  const [dep, setDep] = useState('');
  const [emp, setEmp] = useState('');
  const [status, setStatus] = useState('');
  const [error, setError] = useState('');
  useEffect(() => {
    Promise.all([api<Lookup[]>('/departments'), api<Lookup[]>('/employees')])
      .then(([d,e]) => { setDepartments(d); setEmployees(e); }).catch(e => setError(String(e)));
  }, []);
  const search = async (department = dep, employee = emp, selectedStatus = status) => {
    const p = new URLSearchParams();
    if (department) p.set('departmentId', department);
    if (employee) p.set('employeeId', employee);
    if (selectedStatus) p.set('status', selectedStatus);
    try { setOrders(await api<Order[]>('/purchase-orders?' + p)); setError(''); }
    catch (e) { setError(String(e)); }
  };
  useEffect(() => { void search('', '', ''); }, []);
  const clear = () => { setDep(''); setEmp(''); setStatus(''); void search('', '', ''); };
  return <>
    {error && <div className="alert error">{error}</div>}
    <section className="panel">
      <div className="panel-title"><div><h2>Consulta por criterios</h2><p>Órdenes filtradas por departamento, empleado y estado</p></div></div>
      <form className="filters" onSubmit={(e: FormEvent) => { e.preventDefault(); void search(); }}>
        <label className="field"><span>Departamento</span><select value={dep} onChange={e => { setDep(e.target.value); setEmp(''); }}><option value="">Todos</option>{departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select></label>
        <label className="field"><span>Empleado</span><select value={emp} onChange={e => setEmp(e.target.value)}><option value="">Todos</option>{employees.filter(e => !dep || e.departmentId === Number(dep)).map(e => <option key={e.id} value={e.id}>{e.name}</option>)}</select></label>
        <label className="field"><span>Estado</span><select value={status} onChange={e => setStatus(e.target.value)}><option value="">Todos</option><option>Borrador</option><option>Confirmada</option><option>Anulada</option></select></label>
        <button className="button" type="submit"><Search size={16}/> Consultar</button>
        <button className="button secondary" type="button" onClick={clear}><RotateCcw size={16}/> Limpiar</button>
      </form>
      <div className="table-scroll"><table><thead><tr><th>Número</th><th>Fecha</th><th>Departamento</th><th>Empleado</th><th>Proveedor</th><th>Estado</th><th className="align-right">Total</th></tr></thead>
        <tbody>{orders.map(o => <tr key={o.id}><td><strong>{o.number}</strong></td><td>{o.orderDate}</td><td>{o.department}</td><td>{o.employee}</td><td>{o.supplier}</td><td><span className="pill">{o.status}</span></td><td className="align-right">{moneyRD(o.total)}</td></tr>)}
        {orders.length === 0 && <tr><td colSpan={7} className="empty">No se encontraron órdenes para los criterios seleccionados.</td></tr>}
        </tbody></table></div>
      <div className="panel-bottom">{orders.length} resultado(s) · Total: <strong>{moneyRD(orders.reduce((sum,o) => sum + o.total, 0))}</strong></div>
    </section>
  </>;
}
