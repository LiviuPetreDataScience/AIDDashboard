# AID Dashboard — Running the App

> How to run the application in development and how the production/Azure build works.
> Last updated: 2026-06-29.

## Prerequisites
- .NET SDK 9
- Node.js 22+ / npm

## Solution layout
```
AID Dashboard/
├─ AidDashboard.sln
├─ src/
│  ├─ AidDashboard.Domain/          # entities, enums, audit base
│  ├─ AidDashboard.Application/     # DTOs, service interfaces (contracts)
│  ├─ AidDashboard.Infrastructure/  # EF Core, services, auth, seeding
│  ├─ AidDashboard.Api/             # ASP.NET Core Web API + serves the built SPA
│  └─ web/                          # React + TypeScript (Vite) frontend
└─ docs/
```

## Development (two processes)

1. **Backend** (from `src/AidDashboard.Api`):
   ```
   dotnet run
   ```
   Runs on its launch-profile URL. For a fixed port: `dotnet run --urls http://localhost:5080`.
   On first run it creates the SQLite database, applies migrations, and seeds the reference
   tables and the five admin accounts.

2. **Frontend** (from `src/web`):
   ```
   npm install      # first time only
   npm run dev
   ```
   Vite serves the SPA on http://localhost:5173 and proxies `/api` to the backend on port 5080
   (see `vite.config.ts`). Open http://localhost:5173.

### Default logins
Five placeholder admins are seeded: **admin1 … admin5**, each with a temporary password equal to
its username (e.g. `admin1` / `admin1`). All are administrators; change them via the admin Users
screen or the reset-password action.

## Production build (single deployable, Azure-ready)

The API serves the built SPA from its `wwwroot`. To produce the deployable:
1. Build the SPA: from `src/web`, `npm run build` → outputs `src/web/dist`.
2. Copy `src/web/dist/*` into `src/AidDashboard.Api/wwwroot/`.
3. Publish the API: `dotnet publish src/AidDashboard.Api -c Release`.

(An MSBuild target automates steps 1–2 during `dotnet publish` — see the API project file.)

### Required production configuration
- `Jwt__SigningKey` — a long secret signing key (the app refuses to start in Production without it).
  Set it as an Azure App Service application setting or via Key Vault.
- `ConnectionStrings__Default` — SQLite connection string for now (e.g. `Data Source=/home/data/aid-dashboard.db`
  on App Service so it persists). Designed to switch to Azure SQL later by changing the EF provider
  and this connection string.

## Import / Export
Each table tab has Export (XLSX / CSV / PDF), an import Template download (XLSX / CSV) and Import
(XLSX / CSV). Endpoints: `GET|POST /api/accounts/{accountId}/tabs/{tabKey}/export|template|import`.
Import always targets the open account and replaces that tab's data; reference columns are matched
by name. PDF export needs a system font on the server (the resolver searches common Windows/Linux/macOS
font paths).

## Notes
- Enums are exchanged as strings over the API (e.g. role `"Admin"`, coverage `"EightByFive"`).
- Read access is open to any authenticated user; all writes require an administrator (for now).
- Every record-level change is captured in the `ChangeLogs` table (who / when / what).
- `dotnet publish src/AidDashboard.Api -c Release` builds the SPA and bundles it automatically
  (needs Node/npm on the build machine).
