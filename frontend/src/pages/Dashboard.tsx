import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Building2, Boxes, Truck, ClipboardList, ArrowRight, Plus, Network } from 'lucide-react';
import { api } from '../api';

type Counts = { departments: number; articles: number; suppliers: number; orders: number };
export function Dashboard() {
  const [stats, setStats] = useState<Counts | null>(null);
  const [error, setError] = useState('');
  useEffect(() => {
    api<Counts>('/dashboard').then(setStats).catch(e => setError(String(e)));
  }, []);
  const items = [
    { label: 'Departamentos', value: stats?.departments, Icon: Building2, path: '/catalogs/departments', help: 'Áreas registradas' },
    { label: 'Artículos', value: stats?.articles, Icon: Boxes, path: '/catalogs/articles', help: 'Catálogo de productos' },
    { label: 'Proveedores', value: stats?.suppliers, Icon: Truck, path: '/catalogs/suppliers', help: 'Empresas proveedoras' },
    { label: 'Órdenes de compra', value: stats?.orders, Icon: ClipboardList, path: '/orders', help: 'Documentos registrados' },
  ];
  return <>
    <section className="hero">
      <div><span className="hero-eyebrow">CENTRO DE OPERACIONES</span>
        <h2>Compras conectadas.<br/>Procesos inteligentes.</h2>
        <p>Gestiona proveedores, artículos y órdenes con trazabilidad e integración contable.</p>
        <Link className="button white" to="/orders"><Plus size={16}/> Gestionar compras</Link>
      </div><div className="hero-shape"><Network size={110} strokeWidth={1.2}/></div>
    </section>
    {error && <div className="alert error">No se pudo conectar con la API: {error}</div>}
    <div className="stat-grid">
      {items.map(({label,value,Icon,path,help}) => <Link to={path} key={label} className="stat-card">
        <div className="stat-icon"><Icon size={21}/></div>
        <div className="stat-label">{label}</div><strong>{value ?? '—'}</strong><small>{help}</small>
      </Link>)}
    </div>
    <section className="panel">
      <div className="panel-title"><div><h2>Accesos rápidos</h2><p>Los módulos principales del sistema</p></div></div>
      <div className="shortcut-grid">
        <Link to="/catalogs/suppliers"><Truck/><strong>Proveedores</strong><span>Mantenimiento comercial</span><ArrowRight size={16}/></Link>
        <Link to="/catalogs/articles"><Boxes/><strong>Artículos</strong><span>Inventario y unidades</span><ArrowRight size={16}/></Link>
        <Link to="/orders"><ClipboardList/><strong>Órdenes</strong><span>Solicitudes de compra</span><ArrowRight size={16}/></Link>
      </div>
    </section>
  </>;
}
