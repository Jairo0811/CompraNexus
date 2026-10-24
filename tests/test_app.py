from datetime import date
from fastapi.testclient import TestClient
from sqlalchemy import select, func
import pytest

from app.main import app
from app.db import Base, engine, SessionLocal
from app.models import AccountingDispatch, PurchaseOrder, Article, Department, Unit, Employee, Supplier

@pytest.fixture()
def client(monkeypatch):
    monkeypatch.delenv("ACCOUNTING_WS_URL", raising=False)
    Base.metadata.drop_all(engine)
    Base.metadata.create_all(engine)
    with TestClient(app) as test_client:
        yield test_client

def token(client):
    page = client.get("/")
    assert page.status_code == 200
    import re
    page = client.get("/catalogs/departments/new")
    return re.search(r'name="_csrf" value="([^"]+)"', page.text).group(1)

def create_catalog(client, csrf, kind, data):
    response = client.post("/catalogs/" + kind + "/save", data={**data, "_csrf": csrf})
    assert response.status_code == 200, response.text
    assert "guardado" in response.text
    return response

def seed(client, csrf):
    create_catalog(client, csrf, "departments", {"name": "Tecnología", "status": "Activo"})
    create_catalog(client, csrf, "units", {"description": "Unidad", "status": "Activo"})
    create_catalog(client, csrf, "suppliers", {"tax_id": "101000001", "commercial_name": "Proveedor Uno", "status": "Activo"})
    create_catalog(client, csrf, "employees", {"name": "Empleado Prueba", "department_id": "1", "status": "Activo"})
    create_catalog(client, csrf, "articles", {"description": "Laptop", "brand": "Demo", "unit_id": "1", "stock": "5", "status": "Activo"})

def test_health_and_dashboard(client):
    assert client.get("/health").json()["status"] == "ok"
    assert "CompraNexus" in client.get("/").text
    assert client.get("/catalogs/departments").status_code == 200

def test_cruds_and_constraints(client):
    csrf = token(client)
    seed(client, csrf)
    with SessionLocal() as db:
        assert db.scalar(select(func.count()).select_from(Article)) == 1
    response = client.post("/catalogs/articles/save", data={"_csrf": csrf, "description": "Inválido", "brand": "X", "unit_id": "1", "stock": "-4", "status": "Activo"})
    assert "Valor inv%C3%A1lido" not in response.url.path
    with SessionLocal() as db:
        assert db.scalar(select(func.count()).select_from(Article)) == 1
    response = client.post("/catalogs/departments/1/delete", data={"_csrf": csrf})
    with SessionLocal() as db:
        assert db.get(Department, 1).status == "Inactivo"

def test_order_report_accounting_idempotence(client):
    csrf = token(client)
    seed(client, csrf)
    r = client.post("/orders/save", data={
        "_csrf": csrf, "number": "OC-1001", "order_date": "2026-10-24", "status": "Confirmada",
        "department_id": "1", "employee_id": "1", "supplier_id": "1",
        "article_id": "1", "quantity": "3", "cost": "650.50",
    })
    assert "guardada" in r.text
    assert "OC-1001" in client.get("/reports?department_id=1&employee_id=1").text
    r = client.post("/orders/1/accounting", data={"_csrf": csrf})
    assert "Pendiente" in r.text
    client.post("/orders/1/accounting", data={"_csrf": csrf})
    with SessionLocal() as db:
        entries = db.scalars(select(AccountingDispatch)).all()
        assert len(entries) == 2
        assert {e.movement for e in entries} == {"DB", "CR"}
        assert all(str(e.amount) == "1951.50" for e in entries)
        assert all(e.status == "Pendiente" and e.attempts == 0 for e in entries)

def test_csrf_is_required(client):
    response = client.post("/catalogs/departments/save", data={"name": "Fake", "status": "Activo"}, follow_redirects=False)
    assert response.status_code == 403
