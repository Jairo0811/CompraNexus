from datetime import date
from decimal import Decimal
from sqlalchemy import Column, Date, ForeignKey, Integer, Numeric, String, Text
from sqlalchemy.orm import relationship
from app.db import Base

class Department(Base):
    __tablename__ = "departments"
    id = Column(Integer, primary_key=True)
    name = Column(String(120), unique=True, nullable=False)
    status = Column(String(12), nullable=False, default="Activo")

class Unit(Base):
    __tablename__ = "units"
    id = Column(Integer, primary_key=True)
    description = Column(String(100), unique=True, nullable=False)
    status = Column(String(12), nullable=False, default="Activo")

class Supplier(Base):
    __tablename__ = "suppliers"
    id = Column(Integer, primary_key=True)
    tax_id = Column(String(20), unique=True, nullable=False)
    commercial_name = Column(String(150), nullable=False)
    status = Column(String(12), nullable=False, default="Activo")

class Employee(Base):
    __tablename__ = "employees"
    id = Column(Integer, primary_key=True)
    name = Column(String(140), nullable=False)
    department_id = Column(Integer, ForeignKey("departments.id"), nullable=False)
    status = Column(String(12), nullable=False, default="Activo")
    department = relationship("Department")

class Article(Base):
    __tablename__ = "articles"
    id = Column(Integer, primary_key=True)
    description = Column(String(180), nullable=False)
    brand = Column(String(100), nullable=False)
    unit_id = Column(Integer, ForeignKey("units.id"), nullable=False)
    stock = Column(Integer, nullable=False, default=0)
    status = Column(String(12), nullable=False, default="Activo")
    unit = relationship("Unit")

class PurchaseOrder(Base):
    __tablename__ = "purchase_orders"
    id = Column(Integer, primary_key=True)
    number = Column(String(40), nullable=False, unique=True)
    order_date = Column(Date, nullable=False, default=date.today)
    status = Column(String(16), nullable=False, default="Borrador")
    department_id = Column(Integer, ForeignKey("departments.id"), nullable=False)
    employee_id = Column(Integer, ForeignKey("employees.id"), nullable=False)
    supplier_id = Column(Integer, ForeignKey("suppliers.id"), nullable=False)
    department = relationship("Department")
    employee = relationship("Employee")
    supplier = relationship("Supplier")
    lines = relationship("OrderLine", cascade="all, delete-orphan", back_populates="order")

    @property
    def total(self):
        return sum((line.cost * line.quantity for line in self.lines), Decimal("0.00"))

class OrderLine(Base):
    __tablename__ = "order_lines"
    id = Column(Integer, primary_key=True)
    order_id = Column(Integer, ForeignKey("purchase_orders.id"), nullable=False)
    article_id = Column(Integer, ForeignKey("articles.id"), nullable=False)
    unit_id = Column(Integer, ForeignKey("units.id"), nullable=False)
    quantity = Column(Integer, nullable=False)
    cost = Column(Numeric(14, 2), nullable=False)
    order = relationship("PurchaseOrder", back_populates="lines")
    article = relationship("Article")
    unit = relationship("Unit")

class AccountingDispatch(Base):
    __tablename__ = "accounting_dispatches"
    id = Column(Integer, primary_key=True)
    journal_id = Column(String(36), nullable=False, unique=True)
    order_id = Column(Integer, ForeignKey("purchase_orders.id"), nullable=False)
    description = Column(String(255), nullable=False)
    inventory_type_id = Column(String(40), nullable=False)
    account = Column(String(40), nullable=False)
    movement = Column(String(2), nullable=False)
    entry_date = Column(Date, nullable=False, default=date.today)
    amount = Column(Numeric(14, 2), nullable=False)
    status = Column(String(16), nullable=False, default="Pendiente")
    attempts = Column(Integer, nullable=False, default=0)
    response = Column(Text, nullable=True)
    order = relationship("PurchaseOrder")
