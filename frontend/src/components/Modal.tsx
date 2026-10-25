import { ReactNode, useEffect, useId, useRef } from 'react';
import { X } from 'lucide-react';

type Props = { title: string; subtitle?: string; wide?: boolean; error?: string; onClose: () => void; children: ReactNode };
const FOCUSABLE = 'button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled])';

/** Diálogo accesible: foco inicial, atrapa Tab, cierra con Escape y devuelve el foco al abrir. */
export function Modal({ title, subtitle, wide, error, onClose, children }: Props) {
  const ref = useRef<HTMLElement>(null);
  const closeRef = useRef(onClose);
  closeRef.current = onClose;
  const titleId = useId();
  useEffect(() => {
    const previous = document.activeElement as HTMLElement | null;
    const root = ref.current;
    root?.querySelector<HTMLElement>('form input, form select')?.focus();
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') { e.preventDefault(); closeRef.current(); return; }
      if (e.key !== 'Tab' || !root) return;
      const items = Array.from(root.querySelectorAll<HTMLElement>(FOCUSABLE));
      if (items.length === 0) return;
      const first = items[0], last = items[items.length - 1];
      if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
      else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
    }
    document.addEventListener('keydown', onKey);
    return () => { document.removeEventListener('keydown', onKey); previous?.focus(); };
  }, []);
  return <div className="modal-backdrop" role="presentation">
    <section ref={ref} className={'modal' + (wide ? ' wide' : '')} role="dialog" aria-modal="true" aria-labelledby={titleId}>
      <div className="modal-head">
        <div><h2 id={titleId}>{title}</h2>{subtitle && <p>{subtitle}</p>}</div>
        <button type="button" className="icon-button" onClick={onClose} aria-label="Cerrar"><X/></button>
      </div>
      {error && <div className="alert error modal-error" role="alert">{error}</div>}
      {children}
    </section>
  </div>;
}
