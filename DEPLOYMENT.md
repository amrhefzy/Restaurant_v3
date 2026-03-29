# Restaurant_v3 Deployment Guide

## Goal
Provide a repeatable Linux/server deployment path for `Restaurant_v3` with explicit runtime validation.

## Baseline
- Runtime environment: `Production`
- Web project: `RestaurantManagement.Web`
- Migration project: `RestaurantManagement.Infrastructure`
- Health endpoints:
  - `/health/live`
  - `/health/ready`

## Recommended Order
1. Pull the latest branch state
2. Restore tools and packages
3. Apply database migrations
4. Build in Release
5. Run in Production mode
6. Execute smoke checks

## Commands
### 1) Restore and build
```bash
dotnet tool restore
dotnet restore Restaurant_v3.sln
dotnet build Restaurant_v3.sln --configuration Release
```

### 2) Apply migrations
```bash
./scripts/update-database.sh
```

### 3) Generate reviewed SQL when needed
```bash
./scripts/generate-idempotent-migration-sql.sh
```
Output:
- `artifacts/sql/migrations-idempotent.sql`

### 4) Run in production mode
```bash
./scripts/run-production.sh
```

### 5) Verify runtime health
```bash
./scripts/smoke-check.sh http://localhost:5056
```

## Deployment Notes
- Avoid LocalDB and integrated Windows auth on Linux/server targets.
- Keep secrets outside source control.
- Prefer environment-level overrides for machine-specific connection strings.
- Use `/health/ready` as the readiness signal after startup.
- If startup fails during database initialization, inspect application logs first.

## Minimum Post-Deploy Check
- App starts without startup exceptions
- `/health/live` returns success
- `/health/ready` returns success
- Login page loads
- Dashboard or POS shell opens for authorized users
