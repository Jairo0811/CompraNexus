import { NavLink, Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { useState } from 'react';
import {
  LayoutDashboard, Building2, Boxes, Ruler, Truck, ClipboardList, UsersRound,
  ChartColumnIncreasing, Link2, Menu, X, Network,
} from 'lucide-react';
import { catalogs } from './catalogs';
import { CatalogPage } from './pages/CatalogPage';
import { Dashboard } from './pages/Dashboard';
import { PurchaseOrders } from './pages/PurchaseOrders';
import { Reports } from './pages/Reports';
import { Accounting } from './pages/Accounting';

const nav = [
  { path: '/', text: 'Dashboard', Icon: LayoutDashboard },
  { path: '/orders', text: 'Órdenes de compra', Icon: ClipboardList },
  { path: '/reports', text: 'Consultas', Icon: ChartColumnIncreasing },
  { path: '/accounting', text: 'Contabilidad', Icon: Link2 },
];
const catalogsNav = [
  { path: '/catalogs/departments', text: 'Departamentos', Icon: Building2 },
  { path: '/catalogs/units', text: 'Unidades de medida', Icon: Ruler },
  { path: '/catalogs/suppliers', text: 'Proveedores', Icon: Truck },
  { path: '/catalogs/articles', text: 'Artículos', Icon: Boxes },
  { path: '/catalogs/employees', text: 'Empleados', Icon: UsersRound },
];

export default function App() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  const title = [...nav, ...catalogsNav].find(n => n.path === location.pathname)?.text ?? 'CompraNexus';
  const drawNav = (items: typeof nav) => items.map(({ path, text, Icon }) => (
    <NavLink key={path} end to={path} onClick={() => setMobileOpen(false)} className={({ isActive }) => 'nav-item ' + (isActive ? 'selected' : '')}>
      <Icon size={18} strokeWidth={1.8} />{text}
    </NavLink>
  ));
  return <div className="app">
    <aside className={'sidebar ' + (mobileOpen ? 'open' : '')} aria-label="Navegación principal">
      <NavLink to="/" className="brand" onClick={() => setMobileOpen(false)}>
        <div className="brand-emblem"><Network size={25}/></div>
        <div><strong>Compra<span>Nexus</span></strong><small>COMPRAS E INTEGRACIÓN</small></div>
      </NavLink>
      <div className="side-label">GENERAL</div><nav>{drawNav(nav)}</nav>
      <div className="side-label">MANTENIMIENTOS</div><nav>{drawNav(catalogsNav)}</nav>
      <div className="side-footer"><span className="status-dot"/> Sistema académico
        <small>UNAPEC · ISO-810 / ISO-815 · 2026</small></div>
    </aside>
    {mobileOpen && <button className="scrim" aria-label="Cerrar menú" onClick={() => setMobileOpen(false)}/>}
    <main className="main">
      <header className="top">
        <button className="mobile-toggle" aria-label={mobileOpen ? 'Cerrar menú' : 'Abrir menú'} onClick={() => setMobileOpen(v => !v)}>{mobileOpen ? <X/> : <Menu/>}</button>
        <div><div className="eyebrow">SISTEMA DE COMPRAS</div><h1>{title}</h1></div>
        <span className="top-tag"><span className="status-dot"/> CompraNexus</span>
      </header>
      <Routes>
        <Route path="/" element={<Dashboard/>}/>
        <Route path="/orders" element={<PurchaseOrders/>}/>
        <Route path="/reports" element={<Reports/>}/>
        <Route path="/accounting" element={<Accounting/>}/>
        <Route path="/catalogs/:kind" element={<CatalogPage key={location.pathname} catalogs={catalogs}/>}/>
        <Route path="*" element={<Navigate to="/" replace/>}/>
      </Routes>
      <footer>CompraNexus · Sistema de Compras e Interfaz con Contabilidad · UNAPEC 2026</footer>
    </main>
  </div>;
}
