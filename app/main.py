import base64
import os
import secrets
import uuid
from contextlib import asynccontextmanager
from datetime import date
from decimal import Decimal, InvalidOperation
from pathlib import Path
from urllib.parse import quote

import httpx
from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import JSONResponse, RedirectResponse
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from sqlalchemy import select, func, or_
from sqlalchemy.exc import IntegrityError
from starlette.middleware.sessions import SessionMiddleware

from app.db import SessionLocal, init_db
from app.models import (
    AccountingDispatch, Article, Department, Employee, OrderLine,
    PurchaseOrder, Supplier, Unit,
)

PRODUCTION = os.getenv("APP_ENV", "development").lower() == "production"
if PRODUCTION and not all(os.getenv(x) for x in ("APP_USERNAME", "APP_PASSWORD", "SESSION_SECRET")):
    raise RuntimeError("APP_USERNAME, APP_PASSWORD and SESSION_SECRET are required in production")

@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    yield

app = FastAPI(title="CompraNexus", version="0.2.0", lifespan=lifespan)
app.add_middleware(
    SessionMiddleware,
    secret_key=os.getenv("SESSION_SECRET") or "local-only-change-before-production",
    https_only=PRODUCTION,
    same_site="lax",
)
ROOT = Path(__file__).resolve().parent
templates = Jinja2Templates(directory=str(ROOT / "templates"))
app.mount("/static", StaticFiles(directory=str(ROOT / "static")), name="static")

CATALOGS = {
    "departments": {"title": "Departamentos", "model": Department, "fields": [
        ("name", "Nombre", "text", True), ("status", "Estado", "status", True)]},
    "units": {"title": "Unidades de medida", "model": Unit, "fields": [
        ("description", "Descripción", "text", True), ("status", "Estado", "status", True)]},
    "suppliers": {"title": "Proveedores", "model": Supplier, "fields": [
        ("tax_id", "Cédula / RNC", "text", True), ("commercial_name", "Nombre comercial", "text", True),
        ("status", "Estado", "status", True)]},
    "articles": {"title": "Artículos", "model": Article, "fields": [
        ("description", "Descripción", "text", True), ("brand", "Marca", "text", True),
        ("unit_id", "Unidad de medida", "unit", True), ("stock", "Existencia", "number", True),
        ("status", "Estado", "status", True)]},
    "employees": {"title": "Empleados", "model": Employee, "fields": [
        ("name", "Nombre", "text", True), ("department_id", "Departamento", "department", True),
        ("status", "Estado", "status", True)]},
}

@app.middleware("http")
async def basic_auth(request: Request, call_next):
    username, password = os.getenv("APP_USERNAME"), os.getenv("APP_PASSWORD")
    if username and password and request.url.path != "/health":
        value = request.headers.get("authorization", "")
        try:
            scheme, credentials = value.split(" ", 1)
            decoded = base64.b64decode(credentials, validate=True).decode("utf-8")
            candidate_user, candidate_password = decoded.split(":", 1)
        except (ValueError, UnicodeError):
            scheme, candidate_user, candidate_password = "", "", ""
        if scheme.lower() != "basic" or not (
            secrets.compare_digest(candidate_user, username) and
            secrets.compare_digest(candidate_password, password)
        ):
            return JSONResponse({"detail": "Autenticación requerida"}, status_code=401,
                                headers={"WWW-Authenticate": 'Basic realm="CompraNexus"'})
    return await call_next(request)

def view(request, page, **context):
    request.session.setdefault("csrf", secrets.token_urlsafe(32))
    context.update({
        "request": request,
        "csrf": request.session["csrf"],
        "notice": request.query_params.get("notice"),
        "error": request.query_params.get("error"),
    })
    return templates.TemplateResponse(request, page, context)

def verify_csrf(request, form):
    expected = request.session.get("csrf", "")
    supplied = str(form.get("_csrf", ""))
    if not expected or not secrets.compare_digest(expected, supplied):
        raise HTTPException(403, "Token CSRF inválido. Recargue la página e inténtelo de nuevo.")

def go(path, notice=None, error=None):
    if notice:
        path += ("&" if "?" in path else "?") + "notice=" + quote(notice)
    if error:
        path += ("&" if "?" in path else "?") + "error=" + quote(error)
    return RedirectResponse(path, status_code=303)

def catalog(kind):
    if kind not in CATALOGS:
        raise HTTPException(404, "Módulo inexistente")
    return CATALOGS[kind]

def field_options(db, kind):
    return {
        "unit": [(u.id, u.description) for u in db.scalars(select(Unit).order_by(Unit.description))],
        "department": [(d.id, d.name) for d in db.scalars(select(Department).order_by(Department.name))],
        "status": [("Activo", "Activo"), ("Inactivo", "Inactivo")],
    }

def fill(model, form, fields):
    for name, label, field_type, required in fields:
        raw = str(form.get(name, "")).strip()
        if required and not raw:
            raise ValueError("El campo '" + label + "' es obligatorio.")
        if field_type in ("number", "unit", "department"):
            try:
                value = int(raw)
            except ValueError as exc:
                raise ValueError("Valor inválido en " + label) from exc
            if field_type == "number" and value < 0 or field_type != "number" and value <= 0:
                raise ValueError("Valor inválido en " + label)
        elif field_type == "status":
            if raw not in ("Activo", "Inactivo"):
                raise ValueError("Estado inválido")
            value = raw
        else:
            value = raw
        setattr(model, name, value)

@app.get("/health")
def health():
    with SessionLocal() as db:
        db.execute(select(1)).scalar_one()
    return {"status": "ok", "service": "CompraNexus", "accounting_configured": bool(os.getenv("ACCOUNTING_WS_URL"))}

@app.get("/")
def dashboard(request: Request):
    with SessionLocal() as db:
        metrics = {
            "departments": db.scalar(select(func.count()).select_from(Department)),
            "articles": db.scalar(select(func.count()).select_from(Article)),
            "suppliers": db.scalar(select(func.count()).select_from(Supplier)),
            "orders": db.scalar(select(func.count()).select_from(PurchaseOrder)),
        }
        orders = db.scalars(select(PurchaseOrder).order_by(PurchaseOrder.id.desc()).limit(6)).all()
        return view(request, "dashboard.html", title="Dashboard", metrics=metrics, orders=orders)

@app.get("/catalogs/{kind}", name="catalog_list")
def catalog_list(request: Request, kind: str, q: str = ""):
    cfg = catalog(kind)
    with SessionLocal() as db:
        records = db.scalars(select(cfg["model"]).order_by(cfg["model"].id.desc())).all()
        cols = [(name, label) for name, label, _, _ in cfg["fields"]]
        def label_value(record, field):
            if field == "unit_id":
                return record.unit.description if record.unit else ""
            if field == "department_id":
                return record.department.name if record.department else ""
            return getattr(record, field, "")
        rows = [{"id": r.id, "values": [label_value(r, key) for key, _ in cols]} for r in records]
        if q.strip():
            rows = [r for r in rows if q.casefold() in (" ".join(map(str, r["values"]))).casefold()]
        return view(request, "catalog.html", title=cfg["title"], kind=kind,
                    columns=cols, rows=rows, q=q, count=len(rows))

@app.get("/catalogs/{kind}/new")
def catalog_new(request: Request, kind: str):
    cfg = catalog(kind)
    with SessionLocal() as db:
        return view(request, "catalog_form.html", title="Nuevo · " + cfg["title"],
                    kind=kind, fields=cfg["fields"], options=field_options(db, kind), record=None)

@app.get("/catalogs/{kind}/{record_id}/edit")
def catalog_edit(request: Request, kind: str, record_id: int):
    cfg = catalog(kind)
    with SessionLocal() as db:
        record = db.get(cfg["model"], record_id)
        if record is None:
            raise HTTPException(404, "Registro no encontrado")
        return view(request, "catalog_form.html", title="Editar · " + cfg["title"],
                    kind=kind, fields=cfg["fields"], options=field_options(db, kind), record=record)

@app.post("/catalogs/{kind}/save")
async def catalog_save(request: Request, kind: str):
    cfg = catalog(kind)
    form = await request.form()
    verify_csrf(request, form)
    back = "/catalogs/" + kind
    with SessionLocal() as db:
        try:
            record_id = int(form.get("id") or 0)
            record = db.get(cfg["model"], record_id) if record_id else cfg["model"]()
            if record is None:
                raise ValueError("Registro no encontrado")
            fill(record, form, cfg["fields"])
            if isinstance(record, Employee) and not db.get(Department, record.department_id):
                raise ValueError("Departamento inválido")
            if isinstance(record, Article) and not db.get(Unit, record.unit_id):
                raise ValueError("Unidad de medida inválida")
            db.add(record)
            db.commit()
            return go(back, notice="Registro guardado correctamente")
        except (ValueError, IntegrityError) as exc:
            db.rollback()
            message = "Datos duplicados o registro relacionado inválido" if isinstance(exc, IntegrityError) else str(exc)
            return go(back, error=message)

@app.post("/catalogs/{kind}/{record_id}/delete")
async def catalog_delete(request: Request, kind: str, record_id: int):
    cfg = catalog(kind)
    form = await request.form()
    verify_csrf(request, form)
    back = "/catalogs/" + kind
    with SessionLocal() as db:
        record = db.get(cfg["model"], record_id)
        if not record:
            return go(back, error="Registro no encontrado")
        try:
            db.delete(record)
            db.commit()
            return go(back, notice="Registro eliminado")
        except IntegrityError:
            db.rollback()
            record = db.get(cfg["model"], record_id)
            record.status = "Inactivo"
            db.commit()
            return go(back, notice="Registro relacionado: se desactivó para preservar el historial")

def order_form_data(db):
    return {
        "departments": db.scalars(select(Department).filter_by(status="Activo").order_by(Department.name)).all(),
        "employees": db.scalars(select(Employee).filter_by(status="Activo").order_by(Employee.name)).all(),
        "suppliers": db.scalars(select(Supplier).filter_by(status="Activo").order_by(Supplier.commercial_name)).all(),
        "articles": db.scalars(select(Article).filter_by(status="Activo").order_by(Article.description)).all(),
    }

@app.get("/orders")
def orders_list(request: Request):
    with SessionLocal() as db:
        orders = db.scalars(select(PurchaseOrder).order_by(PurchaseOrder.id.desc())).all()
        return view(request, "orders.html", title="Órdenes de compra", orders=orders)

@app.get("/orders/new")
def orders_new(request: Request):
    with SessionLocal() as db:
        return view(request, "order_form.html", title="Nueva orden de compra",
                    order=None, today=date.today().isoformat(), **order_form_data(db))

@app.get("/orders/{order_id}/edit")
def orders_edit(request: Request, order_id: int):
    with SessionLocal() as db:
        order = db.get(PurchaseOrder, order_id)
        if not order:
            raise HTTPException(404, "Orden no encontrada")
        if db.scalar(select(func.count()).select_from(AccountingDispatch).where(AccountingDispatch.order_id == order_id)):
            return go("/orders", error="No se puede modificar una orden que tenga asientos contables generados")
        return view(request, "order_form.html", title="Editar orden de compra",
                    order=order, today=date.today().isoformat(), **order_form_data(db))

@app.post("/orders/save")
async def orders_save(request: Request):
    form = await request.form()
    verify_csrf(request, form)
    with SessionLocal() as db:
        try:
            order_id = int(form.get("id") or 0)
            if order_id and db.scalar(select(func.count()).select_from(AccountingDispatch).where(AccountingDispatch.order_id == order_id)):
                raise ValueError("La orden ya tiene asientos contables y no puede modificarse")
            order = db.get(PurchaseOrder, order_id) if order_id else PurchaseOrder()
            if order is None:
                raise ValueError("Orden inexistente")
            order.number = str(form.get("number", "")).strip()
            order.order_date = date.fromisoformat(str(form.get("order_date", "")))
            order.status = str(form.get("status", "Borrador"))
            if not order.number or order.status not in ("Borrador", "Confirmada", "Anulada"):
                raise ValueError("Número o estado de orden inválidos")
            order.department_id = int(form.get("department_id"))
            order.employee_id = int(form.get("employee_id"))
            order.supplier_id = int(form.get("supplier_id"))
            employee = db.get(Employee, order.employee_id)
            if not employee or employee.department_id != order.department_id:
                raise ValueError("El empleado debe pertenecer al departamento seleccionado")
            if not db.get(Supplier, order.supplier_id) or not db.get(Department, order.department_id):
                raise ValueError("Departamento o proveedor inválidos")
            articles = form.getlist("article_id")
            quantities = form.getlist("quantity")
            costs = form.getlist("cost")
            if not articles or not (len(articles) == len(quantities) == len(costs)):
                raise ValueError("Debe incluir al menos un detalle válido")
            order.lines.clear()
            for article_text, qty_text, cost_text in zip(articles, quantities, costs):
                article = db.get(Article, int(article_text))
                qty = int(qty_text)
                amount = Decimal(str(cost_text))
                if not article or qty <= 0 or not amount.is_finite() or amount < 0:
                    raise ValueError("Artículo, cantidad o costo inválidos")
                order.lines.append(OrderLine(article_id=article.id, unit_id=article.unit_id,
                                             quantity=qty, cost=amount.quantize(Decimal("0.01"))))
            db.add(order)
            db.commit()
            return go("/orders", notice="Orden guardada correctamente")
        except (ValueError, IntegrityError, InvalidOperation, TypeError, OverflowError) as exc:
            db.rollback()
            msg = "Número de orden duplicado o referencia inválida" if isinstance(exc, IntegrityError) else str(exc)
            return go("/orders", error=msg)

@app.post("/orders/{order_id}/delete")
async def orders_delete(request: Request, order_id: int):
    form = await request.form()
    verify_csrf(request, form)
    with SessionLocal() as db:
        order = db.get(PurchaseOrder, order_id)
        if not order:
            return go("/orders", error="Orden no encontrada")
        if order.status != "Borrador" or db.scalar(select(func.count()).select_from(AccountingDispatch).where(AccountingDispatch.order_id == order_id)):
            return go("/orders", error="Solo se pueden eliminar órdenes en borrador sin asientos")
        db.delete(order)
        db.commit()
    return go("/orders", notice="Orden eliminada")

@app.get("/reports")
def reports(request: Request, department_id: int = 0, employee_id: int = 0, status: str = ""):
    with SessionLocal() as db:
        stmt = select(PurchaseOrder).order_by(PurchaseOrder.order_date.desc(), PurchaseOrder.id.desc())
        if department_id:
            stmt = stmt.where(PurchaseOrder.department_id == department_id)
        if employee_id:
            stmt = stmt.where(PurchaseOrder.employee_id == employee_id)
        if status:
            stmt = stmt.where(PurchaseOrder.status == status)
        orders = db.scalars(stmt).all()
        departments = db.scalars(select(Department).order_by(Department.name)).all()
        employees = db.scalars(select(Employee).order_by(Employee.name)).all()
        return view(request, "reports.html", title="Consultas por criterios", orders=orders,
                    departments=departments, employees=employees,
                    filters={"department_id": department_id, "employee_id": employee_id, "status": status})

def create_entries(db, order):
    if order.status != "Confirmada" or order.total <= 0:
        raise ValueError("Solo se contabilizan órdenes confirmadas con monto mayor que cero")
    if db.scalar(select(func.count()).select_from(AccountingDispatch).where(AccountingDispatch.order_id == order.id)):
        return
    inventory = os.getenv("ACCOUNTING_INVENTORY_TYPE_ID", "1")
    for movement, account in (
        ("DB", os.getenv("ACCOUNTING_DEBIT_ACCOUNT", "1401")),
        ("CR", os.getenv("ACCOUNTING_CREDIT_ACCOUNT", "2101")),
    ):
        db.add(AccountingDispatch(
            journal_id=str(uuid.uuid4()), order_id=order.id,
            description="Orden de compra " + order.number, inventory_type_id=inventory,
            account=account, movement=movement, entry_date=order.order_date,
            amount=order.total, status="Pendiente", attempts=0,
        ))
    db.commit()

async def send_entries(db, entries):
    url = os.getenv("ACCOUNTING_WS_URL", "").strip()
    if not url:
        return 0
    headers = {"Content-Type": "application/json"}
    token = os.getenv("ACCOUNTING_WS_TOKEN", "").strip()
    if token:
        headers["Authorization"] = "Bearer " + token
    delivered = 0
    async with httpx.AsyncClient(timeout=12, follow_redirects=False) as client:
        for entry in entries:
            if entry.status == "Enviado":
                continue
            payload = {
                "identificadorAsiento": entry.journal_id,
                "descripcion": entry.description,
                "identificadorTipoInventario": entry.inventory_type_id,
                "cuentaContable": entry.account,
                "tipoMovimiento": entry.movement,
                "fechaAsiento": entry.entry_date.isoformat(),
                "montoAsiento": float(entry.amount),
                "estado": "Activo",
            }
            entry.attempts += 1
            try:
                response = await client.post(url, json=payload, headers={**headers, "Idempotency-Key": entry.journal_id})
                entry.response = response.text[:1800]
                if 200 <= response.status_code < 300:
                    entry.status = "Enviado"
                    delivered += 1
                else:
                    entry.status = "Error"
            except httpx.HTTPError as exc:
                entry.status = "Error"
                entry.response = str(exc)[:1800]
            db.commit()
    return delivered

@app.get("/accounting")
def accounting(request: Request):
    with SessionLocal() as db:
        records = db.scalars(select(AccountingDispatch).order_by(AccountingDispatch.id.desc())).all()
        return view(request, "accounting.html", title="Interfaz con contabilidad", entries=records,
                    configured=bool(os.getenv("ACCOUNTING_WS_URL")))

@app.post("/orders/{order_id}/accounting")
async def order_accounting(request: Request, order_id: int):
    form = await request.form()
    verify_csrf(request, form)
    with SessionLocal() as db:
        order = db.get(PurchaseOrder, order_id)
        if not order:
            return go("/orders", error="Orden no encontrada")
        try:
            create_entries(db, order)
        except ValueError as exc:
            return go("/orders", error=str(exc))
        entries = db.scalars(select(AccountingDispatch).where(
            AccountingDispatch.order_id == order_id, AccountingDispatch.status != "Enviado")).all()
        delivered = await send_entries(db, entries)
        return go("/accounting", notice=("Asientos creados; " + str(delivered) +
                   " enviados. Pendientes hasta configurar WS." if not os.getenv("ACCOUNTING_WS_URL")
                   else "Transmisión realizada; verifique los estados de cada asiento"))

@app.post("/accounting/retry")
async def retry_accounting(request: Request):
    form = await request.form()
    verify_csrf(request, form)
    with SessionLocal() as db:
        entries = db.scalars(select(AccountingDispatch).where(
            AccountingDispatch.status.in_(["Pendiente", "Error"]))).all()
        delivered = await send_entries(db, entries)
        if not os.getenv("ACCOUNTING_WS_URL"):
            return go("/accounting", error="Configure ACCOUNTING_WS_URL antes de enviar")
        return go("/accounting", notice=str(delivered) + " asiento(s) transmitido(s). Revise los estados.")
