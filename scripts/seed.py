"""Carga de datos de demostración, idempotente para catálogos básicos."""
from app.db import SessionLocal, init_db
from app.models import Article, Department, Employee, Supplier, Unit
from sqlalchemy import select

init_db()
with SessionLocal() as db:
    if db.scalar(select(Department.id).limit(1)) is None:
        db.add_all([Department(name="Tecnología", status="Activo"),
                    Department(name="Administración", status="Activo")])
        db.flush()
    if db.scalar(select(Unit.id).limit(1)) is None:
        db.add_all([Unit(description="Unidad", status="Activo"),
                    Unit(description="Caja", status="Activo")])
        db.flush()
    if db.scalar(select(Supplier.id).limit(1)) is None:
        db.add(Supplier(tax_id="101000001", commercial_name="Proveedor de demostración", status="Activo"))
        db.flush()
    if db.scalar(select(Employee.id).limit(1)) is None:
        dep = db.scalar(select(Department).order_by(Department.id).limit(1))
        db.add(Employee(name="Empleado de demostración", department_id=dep.id, status="Activo"))
    if db.scalar(select(Article.id).limit(1)) is None:
        unit = db.scalar(select(Unit).order_by(Unit.id).limit(1))
        db.add(Article(description="Artículo de demostración", brand="Genérica",
                       unit_id=unit.id, stock=10, status="Activo"))
    db.commit()
print("Datos de demostración preparados.")
