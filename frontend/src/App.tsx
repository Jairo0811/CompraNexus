import { NavLink, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import {
  LayoutDashboard, Building2, Boxes, Ruler, Truck, ClipboardList, UsersRound,
  ChartColumnIncreasing, Link2, Menu, X, Network, LogOut, UserCircle,
} from 'lucide-react';
import { api, getSession, resetCsrfToken, SESSION_EXPIRED, errorMessage, type SessionUser } from './api';
import { catalogs } from './catalogs';
import { CatalogPage } from './pages/CatalogPage';
import { Dashboard } from './pages/Dashboard';
import { PurchaseOrders } from './pages/PurchaseOrders';
import { Reports } from './pages/Reports';
import { Accounting } from './pages/Accounting';
import { Login } from './pages/Login';

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
  const [session, setSession] = useState<SessionUser | null | undefined>(undefined);
  const [startupError, setStartupError] = useState('');
  const [mobileOpen, setMobileOpen] = useState(false);
  const [signingOut, setSigningOut] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  useEffect(() => {
    let active = true;
    void getSession().then(user => { if (active) setSession(user); })
      .catch(e => { if (active) { setStartupError(errorMessage(e)); setSession(null); } });
    return () => { active = false; };
  }, []);
  useEffect(() => {
    const expired = () => { resetCsrfToken(); setSession(null); navigate('/login', { replace: true }); };
    window.addEventListener(SESSION_EXPIRED, expired);
    return () => window.removeEventListener(SESSION_EXPIRED, expired);
  }, [navigate]);

  async function logout() {
    if (signingOut) return;
    setSigningOut(true);
    try {
      await api('/auth/logout', { method: 'POST' });
      resetCsrfToken();
      setSession(null);
      navigate('/login', { replace: true });
    } catch (e) {
      setStartupError(errorMessage(e));
    } finally {
      setSigningOut(false);
    }
  }

  if (session === undefined) return <div className="auth-loading" role="status">Comprobando sesión...</div>;
  if (!session) return <Routes>
    <Route path="/login" element={<Login onLogin={user => { setSession(user); setStartupError(''); }} startupError={startupError}/>}/>
    <Route path="*" element={<Navigate to="/login" state={{ from: location.pathname }} replace/>}/>
  </Routes>;

  const admin = session.role === 'Administrador';
  const canPurchase = admin || session.role === 'Compras';
  const canAccount = admin || session.role === 'Contabilidad';
  const title = [...nav, ...catalogsNav].find(n => n.path === location.pathname)?.text ?? 'CompraNexus';
  const drawNav = (items: typeof nav) => items.map(({ path, text, Icon }) => (
    <NavLink key={path} end to={path} onClick={() => setMobileOpen(false)}
      className={({ isActive }) => 'nav-item ' + (isActive ? 'selected' : '')}>
      <Icon size={18} strokeWidth={1.8} />{text}
    </NavLink>
  ));

  return <div className="app">
    <aside className={'sidebar ' + (mobileOpen ? 'open' : '')} aria-label="Navegación principal">
      <NavLink to="/" className="brand" onClick={() => setMobileOpen(false)}>
        <div className="brand-emblem"><Network size={25}/></div>
        <div><strong>Compra<span>Nexus</span></strong><small>COMPRAS E INTEGRACIÓN</small></div>
      </NavLink>
      <div className="side-label">GENERAL</div>
      <nav>{drawNav(nav.filter(item => item.path !== '/accounting' || canAccount))}</nav>
      {canPurchase && <>
        <div className="side-label">MANTENIMIENTOS</div>
        <nav>{drawNav(catalogsNav)}</nav>
      </>}
      <div className="side-footer"><span className="status-dot"/> Sesión activa
        <small>{session.email} · {session.role}</small>
        <small>UNAPEC · ISO-810 / ISO-815 · 2026</small>
      </div>
    </aside>
    {mobileOpen && <button className="scrim" aria-label="Cerrar menú" onClick={() => setMobileOpen(false)}/>}
    <main className="main">
      <header className="top">
        <button className="mobile-toggle" aria-label={mobileOpen ? 'Cerrar menú' : 'Abrir menú'}
          onClick={() => setMobileOpen(v => !v)}>{mobileOpen ? <X/> : <Menu/>}</button>
        <div><div className="eyebrow">SISTEMA DE COMPRAS</div><h1>{title}</h1></div>
        <div className="top-session">
          <span className="session-email"><UserCircle size={18}/> {session.email}</span>
          <button className="button secondary" disabled={signingOut}
            onClick={() => void logout()} aria-label="Cerrar sesión">
            <LogOut size={16}/> <span>{signingOut ? 'Cerrando...' : 'Salir'}</span>
          </button>
        </div>
      </header>
      <Routes>
        <Route path="/" element={<Dashboard/>}/>
        <Route path="/orders" element={<PurchaseOrders canPurchase={canPurchase} canAccount={canAccount}/>}/>
        <Route path="/reports" element={<Reports/>}/>
        <Route path="/accounting" element={canAccount ? <Accounting/> : <Navigate to="/" replace/>}/>
        <Route path="/catalogs/:kind" element={canPurchase
          ? <CatalogPage key={location.pathname} catalogs={catalogs}/>
          : <Navigate to="/" replace/>}/>
        <Route path="*" element={<Navigate to="/" replace/>}/>
      </Routes>
      <footer>CompraNexus · Sistema de Compras e Interfaz con Contabilidad · UNAPEC 2026</footer>
    </main>
  </div>;
}
