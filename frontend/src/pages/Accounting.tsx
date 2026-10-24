import { useEffect, useState } from 'react';
import { Link2, RefreshCw } from 'lucide-react';
import { api, moneyRD } from '../api';
import type { AccountingEntry } from '../types';

export function Accounting() {
  const [entries, setEntries] = useState<AccountingEntry[]>([]);
  const [configured, setConfigured] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const load = async () => {
    const result = await api<{configured: boolean; entries: AccountingEntry[]}>('/accounting');
    setEntries(result.entries); setConfigured(result.configured);
  };
  useEffect(() => { void load().catch(e => setError(String(e))); }, []);
  const retry = async () => {
    try {
      const result = await api<{ sent: number }>('/accounting/retry', { method: 'POST' });
      setNotice('Asientos enviados: ' + result.sent); setError(''); await load();
    } catch (e) { setError(String(e)); }
  };
  return <>
    {notice && <div className="alert success">{notice}</div>}
    {error && <div className="alert error">{error}</div>}
    <div className={'info-box ' + (configured ? 'ready' : '')}>
      <Link2 size={18}/> {configured
        ? 'El servicio web contable está configurado. Verifica los estados de cada asiento.'
        : 'No se ha configurado el servicio web externo. Los asientos se conservan como pendientes y no se envían.'}
    </div>
    <section className="panel"><div className="panel-title"><div><h2>Asientos contables</h2><p>Registros DB/CR con trazabilidad e idempotencia</p></div>
      <button disabled={!configured} className="button" onClick={() => void retry()}><RefreshCw size={16}/> Reintentar pendientes</button></div>
      <div className="table-scroll"><table><thead><tr><th>Asiento</th><th>Orden</th><th>Movimiento</th><th>Cuenta</th><th>Tipo inv.</th><th>Fecha</th><th className="align-right">Monto</th><th>Estado</th><th>Intentos</th></tr></thead>
        <tbody>{entries.map(e => <tr key={e.id}><td className="mono">{e.journalId.slice(0, 8)}…</td><td>#{e.purchaseOrderId}</td><td><span className="pill">{e.movement}</span></td><td>{e.account}</td><td>{e.inventoryTypeId}</td><td>{e.entryDate}</td><td className="align-right">{moneyRD(e.amount)}</td><td><span className={'pill ' + (e.status === 'Enviado' ? 'ok' : 'off')}>{e.status}</span></td><td>{e.attempts}</td></tr>)}
        {entries.length === 0 && <tr><td colSpan={9} className="empty">No hay asientos. Confirma y contabiliza una orden de compra.</td></tr>}
        </tbody></table></div>
    </section>
  </>;
}
