# Restaurant_v3 Setup Guide

## Prerequisites
Install the following before running the project:

- .NET 8 SDK
- SQL Server (or SQL Server Express)
- Node.js (optional)
  - Not strictly required for current setup
  - Useful if future frontend tooling/build steps are added

## 1) Clone the Repository
```bash
git clone <your-repo-url>
cd Restaurant_v3
```

## 2) Restore Dependencies
```bash
dotnet restore Restaurant_v3.sln
```

## 3) Configure Connection String
The web project uses `RestaurantManagement.Web/appsettings.json` (and optionally `appsettings.Development.json`).

Default key:
- `ConnectionStrings:DefaultConnection`

Example SQL Server connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=Restaurant_v3;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

If using SQL authentication:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=Restaurant_v3;User Id=sa;Password=<PASSWORD>;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

## 4) Apply Migrations
From repository root:
```bash
dotnet dotnet-ef database update \
  --project RestaurantManagement.Infrastructure/RestaurantManagement.Infrastructure.csproj \
  --startup-project RestaurantManagement.Web/RestaurantManagement.Web.csproj
```

If `dotnet-ef` is not available:
```bash
dotnet tool restore
```
Then rerun migration command.

## 5) Build the Solution
```bash
dotnet build Restaurant_v3.sln
```

## 6) Run the Application
```bash
dotnet run --project RestaurantManagement.Web
```

The app starts using configured ASP.NET Core launch settings or default Kestrel endpoints.

## Development Mode
To run in development mode:

Linux/macOS:
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project RestaurantManagement.Web
```

Windows PowerShell:
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project RestaurantManagement.Web
```

## Environment Configuration Notes
- `appsettings.json`: shared/default configuration.
- `appsettings.Development.json`: local development overrides.
- Keep secrets out of source control.
- Prefer user secrets or environment variables for sensitive settings.

## Seeded Identity Notes
The foundation seeds basic roles and a bootstrap admin account.

Default bootstrap account:
- Email: `superadmin@restaurant-v3.local`
- Password: `SuperAdmin#2026`

Important:
- Rotate/change this credential immediately outside local development.

## Common Issues and Fixes

### 1) `dotnet-ef` command not found
Run:
```bash
dotnet tool restore
```

### 2) SQL connection errors
- Verify SQL Server is running.
- Verify server name, auth mode, and credentials.
- Confirm firewall/network policies if using remote SQL Server.

### 3) Migration update fails due to schema mismatch
Try:
- Confirm correct connection string and target database.
- Ensure no conflicting manual schema edits.
- Regenerate migrations only when coordinated with team workflow.

### 4) HTTPS/certificate warnings in development
Use local development certificates if needed:
```bash
dotnet dev-certs https --trust
```

### 5) Build succeeds but app fails at startup
- Check startup logs for DI/configuration errors.
- Validate connection string and database availability.
- Ensure required `appsettings` keys are present.

## Recommended Developer Workflow
1. Pull latest branch updates.
2. Restore tools and NuGet packages.
3. Apply migrations.
4. Build solution.
5. Run web project.
6. Validate module pages and POS/reporting flows.
