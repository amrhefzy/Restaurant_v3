# Restaurant_v3

## Overview
Restaurant_v3 is a production-oriented Restaurant Management System designed for real operations, not a demo app. The platform focuses on operational speed, inventory-aware transactions, and long-term scalability for single-branch and multi-branch restaurant businesses.

The system is built to support cashier workflows, management visibility, and operational control across sales, purchasing, reservations, inventory, and reporting.

## Purpose
The goal of Restaurant_v3 is to provide a strong enterprise-ready foundation for a modern restaurant platform with:
- POS-first operational workflows
- Premium internal admin experience
- Inventory movement-based stock tracking
- Role-based secure access
- Arabic/English bilingual readiness with RTL/LTR support
- Clean architecture for maintainability and future expansion

## Key Features
- POS-first workflow foundation
  - Fast product browsing and search
  - Cart management (add, quantity update, remove, line notes)
  - Dine-in / takeaway / delivery order type structure
  - Hold and resume order support
  - Checkout flow with order persistence
- Dashboard foundation
  - Sales, orders, reservations, low stock indicators
  - Top products and cashier placeholders for operational insights
- Inventory-aware design
  - Inventory movements (sales, purchases, returns, adjustments)
  - Low stock monitoring hooks
- Order lifecycle foundations
  - Sales orders and purchase orders
  - Sales returns and purchase returns
- Core operations modules
  - Categories, products, suppliers, customers, tables, reservations
- Reporting foundation
  - KPI overview, sales trends, category sales breakdown (AJAX-driven)
- Platform foundations
  - ASP.NET Core Identity with seeded roles
  - Localization infrastructure (EN/AR)
  - RTL/LTR-aware shared shell layout

## Architecture
Restaurant_v3 follows a clean modular architecture with clear separation of concerns:

### 1) RestaurantManagement.Domain
Contains core business model elements:
- Entities
- Enums
- Base abstractions (auditing/soft delete)
- Domain constants and primitives

### 2) RestaurantManagement.Application
Contains application contracts and orchestration boundaries:
- DTOs
- Service interfaces
- Validators (FluentValidation)
- Mapping profiles (AutoMapper)

### 3) RestaurantManagement.Infrastructure
Contains technical implementation details:
- EF Core DbContext and configurations
- Identity persistence and seeding
- Repository and unit of work implementation
- Service implementations
- Migrations and design-time DB tooling

### 4) RestaurantManagement.Web
Presentation and interaction layer:
- MVC controllers and Razor views
- Premium admin shell (sidebar/topbar/components)
- POS UI
- AJAX modules and frontend scripts
- Localization resources and culture switching

## Tech Stack
- ASP.NET Core 8 MVC
- C# (.NET 8)
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity
- AutoMapper
- FluentValidation
- Bootstrap 5
- jQuery
- JavaScript (AJAX-enhanced workflows)

## Module Breakdown (High Level)
- Dashboard
- POS
- Categories
- Products
- Suppliers
- Customers
- Restaurant Tables
- Reservations
- Sales Orders
- Purchase Orders
- Sales Returns
- Purchase Returns
- Inventory
- Reports
- Settings foundation

## Future Vision
Restaurant_v3 is designed to scale into a broader restaurant platform:
- Multi-branch operations and centralized reporting
- Stronger permission granularity and module-level authorization
- Kitchen display system integration
- Online ordering and delivery integrations
- Loyalty and customer engagement modules
- Advanced analytics and operational forecasting
- Mobile and external service integration readiness

## Screenshots
> Placeholder: add UI screenshots after deployment-ready UX pass.

Suggested sections:
- Dashboard
- POS screen
- Inventory view
- Reports view
- Reservation management

## Development Status
This repository currently contains a strong foundation with phased implementation across:
- Core architecture
- Admin shell
- Module foundations
- POS-first implementation
- Reporting and refinement
- Runtime settings governance

## Runtime / Deployment Notes
- Development config lives in `RestaurantManagement.Web/appsettings.Development.json`.
- Production-oriented baseline config lives in `RestaurantManagement.Web/appsettings.Production.json`.
- Linux/server deployments should avoid LocalDB and Windows-integrated auth.
- Prefer environment-level overrides for secrets and machine-specific connection strings.
- Health endpoints are available at:
  - `/health/live`
  - `/health/ready`
- Request diagnostics now include lightweight HTTP logging for method/path/status/duration.
- Migration discipline helpers are available at:
  - `./scripts/update-database.sh`
  - `./scripts/generate-idempotent-migration-sql.sh`

Refer to `SETUP.md` for full installation and local execution steps.
