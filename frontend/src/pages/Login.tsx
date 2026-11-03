import { FormEvent, useState } from 'react';
import { Eye, EyeOff, LockKeyhole, LogIn, ShieldCheck } from 'lucide-react';
import { useLocation, useNavigate } from 'react-router-dom';
import { api, resetCsrfToken, errorMessage, type SessionUser } from '../api';

export function Login({ onLogin, startupError }: {
  onLogin: (user: SessionUser) => void;
  startupError?: string;
}) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from ?? '/';
  const destination = from.startsWith('/') && !from.startsWith('//') && from !== '/login' ? from : '/';

  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (busy) return;
    setError('');
    setBusy(true);
    try {
      const session = await api<SessionUser>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: email.trim(), password }),
      });
      setPassword('');
      resetCsrfToken(); // Token anterior ligado a identidad anónima; regenerar tras autenticar.
      onLogin(session);
      navigate(destination, { replace: true });
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return <main className="login-shell">
    <div className="login-frame">
      <section className="login-promo" aria-label="CompraNexus">
        <div className="login-brand">
          <img className="login-brand-logo" src="/compranexus-logo.png"
            alt="CompraNexus — Sistema de compras e integración contable"
            width="230" height="170" fetchPriority="high"/>
        </div>
        <div className="login-promo-copy">
          <span className="login-kicker">PLATAFORMA DE COMPRAS</span>
          <h1>Compras conectadas.<br/><span>Procesos seguros.</span></h1>
          <p>Gestiona tus órdenes, proveedores y operaciones contables desde un entorno integrado y protegido.</p>
          <div className="login-promo-check"><ShieldCheck size={19}/> Acceso exclusivo para usuarios autorizados</div>
        </div>
        <div className="login-academic">UNAPEC · ISO-810 / ISO-815</div>
      </section>
      <section className="login-form-side">
        <div className="login-form-wrap">
          <span className="login-lock"><LockKeyhole size={23}/></span>
          <span className="login-kicker">BIENVENIDO A COMPRANEXUS</span>
          <h2>Iniciar sesión</h2>
          <p className="login-intro">Introduce las credenciales de tu cuenta para continuar.</p>
          {(error || startupError) && <div className="alert error" role="alert">{error || startupError}</div>}
          <form onSubmit={e => void submit(e)} className="login-form">
            <label className="field">
              <span>Correo electrónico</span>
              <input type="email" name="email" autoComplete="username" required
                value={email} onChange={e => setEmail(e.target.value)}
                placeholder="usuario@empresa.com" maxLength={180}/>
            </label>
            <label className="field">
              <span>Contraseña</span>
              <div className="password-field">
                <input type={showPassword ? 'text' : 'password'} name="password"
                  autoComplete="current-password" required
                  value={password} onChange={e => setPassword(e.target.value)}
                  placeholder="Introduce tu contraseña"/>
                <button type="button" className="password-toggle"
                  aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                  onClick={() => setShowPassword(value => !value)}>
                  {showPassword ? <EyeOff size={19}/> : <Eye size={19}/>}
                </button>
              </div>
            </label>
            <button className="button login-submit" disabled={busy} type="submit">
              <LogIn size={18}/> {busy ? 'Verificando...' : 'Iniciar sesión'}
            </button>
          </form>
          <p className="login-note">¿No tienes acceso? Solicita una cuenta al administrador del sistema.</p>
        </div>
        <p className="login-footer">CompraNexus · Sistema académico de compras · 2026</p>
      </section>
    </div>
  </main>;
}
