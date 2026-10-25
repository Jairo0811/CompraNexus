import { FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Plus, Pencil, Search, Trash2 } from 'lucide-react';
import { api, errorMessage } from '../api';
import { draft, type CatalogConfig } from '../catalogs';
import { Modal } from '../components/Modal';
import type { CatalogRecord, Lookup } from '../types';

type FormValue = string | number;
type FormState = Record<string, FormValue>;
const display = (row: CatalogRecord, field: string, lookup: Record<string, Lookup[]>) => {
  if (field === 'unitMeasureId') return lookup.units?.find(x => x.id === Number(row[field]))?.description ?? String(row[field] ?? '');
  if (field === 'departmentId') return lookup.departments?.find(x => x.id === Number(row[field]))?.name ?? String(row[field] ?? '');
  return String(row[field] ?? '');
};
export function CatalogPage({ catalogs }: { catalogs: Record<string, CatalogConfig> }) {
  const { kind = 'departments' } = useParams();
  const config = catalogs[kind];
  const [rows, setRows] = useState<CatalogRecord[]>([]);
  const [lookups, setLookups] = useState<Record<string, Lookup[]>>({});
  const [search, setSearch] = useState('');
  const [form, setForm] = useState<FormState | null>(null);
  const [editing, setEditing] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [modalError, setModalError] = useState('');
  const [notice, setNotice] = useState('');
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);
  const load = useCallback(async () => {
    if (!config) return;
    try { setRows(await api<CatalogRecord[]>('/' + kind)); setError(''); }
    catch (e) { setError(errorMessage(e)); }
    finally { setLoading(false); }
  }, [kind, config]);
  useEffect(() => { void load(); }, [load]);
  useEffect(() => {
    if (!config) return;
    const needs = config.fields.filter(f => f.type === 'departments' || f.type === 'units').map(f => f.type);
    Promise.all([...new Set(needs)].map(async type => [type, await api<Lookup[]>('/' + type)] as const))
      .then(result => setLookups(Object.fromEntries(result)))
      .catch(e => setError(errorMessage(e)));
  }, [kind, config]);
  const filtered = useMemo(() => rows.filter(row => !search || config?.fields.some(f =>
    display(row, f.key, lookups).toLowerCase().includes(search.toLowerCase()))
    || String(row.id).includes(search)), [rows, search, config, lookups]);
  if (!config) return <div className="alert error">Módulo no encontrado.</div>;
  // Solo se ofrecen registros activos; se conserva el valor ya guardado al editar.
  const options = (type: string, current: FormValue | undefined) =>
    (lookups[type] ?? []).filter(x => x.status !== 'Inactivo' || x.id === Number(current));
  const rowLabel = (row: CatalogRecord) => String(row[config.fields[0].key] ?? row.id);
  const openNew = () => { setEditing(null); setForm(draft(config.fields)); setError(''); setModalError(''); setNotice(''); };
  const openEdit = (row: CatalogRecord) => {
    setEditing(row.id); setForm(Object.fromEntries(config.fields.map(f => [f.key, row[f.key] ?? '']))); setError(''); setModalError(''); setNotice('');
  };
  const closeForm = () => { if (!busy) { setForm(null); setModalError(''); } };
  async function submit(e: FormEvent) {
    e.preventDefault();
    if (!form || busy) return;
    setBusy(true); setModalError('');
    try {
      await api('/' + kind + (editing ? '/' + editing : ''), {
        method: editing ? 'PUT' : 'POST',
        body: JSON.stringify(form),
      });
      setForm(null); setNotice(editing ? 'Registro actualizado correctamente.' : 'Registro creado correctamente.');
      await load();
    } catch (err) { setModalError(errorMessage(err)); }
    finally { setBusy(false); }
  }
  async function deactivate(row: CatalogRecord) {
    if (!confirm('¿Desactivar este registro? Se conservará el historial relacionado.')) return;
    setError(''); setNotice('');
    try { await api('/' + kind + '/' + row.id, { method: 'DELETE' }); setNotice('Registro desactivado.'); await load(); }
    catch (e) { setError(errorMessage(e)); }
  }
  return <>
    {notice && <div className="alert success" role="status">{notice}</div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <section className="panel">
      <div className="panel-title"><div><h2>Gestión de {config.title.toLowerCase()}</h2><p>Crear, consultar, actualizar y desactivar</p></div>
        <button className="button" onClick={openNew}><Plus size={16}/> Nuevo registro</button>
      </div>
      <div className="toolbar"><label className="searchbox"><Search size={18}/><input aria-label="Buscar registros" placeholder={'Buscar ' + config.plural + '...'} value={search} onChange={e => setSearch(e.target.value)}/></label><small>{filtered.length} registro(s)</small></div>
      <div className="table-scroll"><table aria-busy={loading}><thead><tr><th>ID</th>{config.fields.map(f => <th key={f.key}>{f.label}</th>)}<th className="align-right">Acciones</th></tr></thead>
        <tbody>{filtered.map(row => <tr key={row.id}><td className="muted">#{row.id}</td>{config.fields.map(f =>
          <td key={f.key}>{f.key === 'status' ? <span className={'pill ' + (row.status === 'Activo' ? 'ok' : 'off')}>{row.status}</span> : display(row, f.key, lookups)}</td>)}
          <td className="align-right"><div className="table-actions"><button className="icon-button" aria-label={'Editar ' + rowLabel(row)} onClick={() => openEdit(row)}><Pencil size={17}/></button><button className="icon-button red" aria-label={'Desactivar ' + rowLabel(row)} disabled={row.status === 'Inactivo'} onClick={() => void deactivate(row)}><Trash2 size={17}/></button></div></td>
        </tr>)}
        {loading && <tr><td colSpan={config.fields.length + 2} className="loading">Cargando {config.plural}...</td></tr>}
        {!loading && filtered.length === 0 && <tr><td colSpan={config.fields.length + 2} className="empty">{rows.length === 0 && !error
          ? 'No hay registros para mostrar. Utiliza «Nuevo registro» para comenzar.'
          : error ? 'No se pudieron cargar los registros.' : 'Ningún registro coincide con la búsqueda.'}</td></tr>}
        </tbody></table></div>
    </section>
    {form && <Modal title={(editing ? 'Editar' : 'Nuevo') + ' · ' + config.title} subtitle="Completa los campos de este mantenimiento." error={modalError} onClose={closeForm}>
      <form onSubmit={e => void submit(e)} className="form-grid">
        {config.fields.map(field => <label className="field" key={field.key}><span>{field.label}{field.required ? ' *' : ''}</span>
          {field.type === 'status' ? <select required value={form[field.key] ?? 'Activo'} onChange={e => setForm(v => v && ({ ...v, [field.key]: e.target.value }))}><option>Activo</option><option>Inactivo</option></select>
          : field.type === 'units' || field.type === 'departments' ?
          <select required value={form[field.key] ?? ''} onChange={e => setForm(v => v && ({ ...v, [field.key]: Number(e.target.value) }))}>
            <option value="">Seleccionar...</option>{options(field.type, form[field.key]).map(x => <option key={x.id} value={x.id}>{x.description ?? x.name}{x.status === 'Inactivo' ? ' (inactivo)' : ''}</option>)}
          </select> : <input required={field.required} type={field.type === 'number' ? 'number' : 'text'}
            min={field.type === 'number' ? 0 : undefined}
            value={form[field.key] ?? ''} onChange={e => setForm(v => v && ({ ...v, [field.key]: field.type === 'number' ? Number(e.target.value) : e.target.value }))}/>}
        </label>)}
        <div className="form-actions"><button type="button" className="button secondary" disabled={busy} onClick={closeForm}>Cancelar</button><button disabled={busy} className="button" type="submit">{busy ? 'Guardando...' : 'Guardar registro'}</button></div>
      </form>
    </Modal>}
  </>;
}
