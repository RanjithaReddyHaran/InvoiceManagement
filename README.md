# IT Staffing Invoice Management System

A portfolio-grade starter for an IT staffing company to manage clients, consultants, contracts, approved timesheets, invoices, PDF output and payments.

## Solution contents
- `src/StaffingInvoice.Api`: ASP.NET Core 8 Web API, EF Core SQL Server, invoice business logic, PDF generation and global exception handling.
- `tests/StaffingInvoice.Tests`: xUnit tests for core financial calculations.
- `database`: database creation and sample seed scripts.
- `frontend`: Angular 14 setup and API integration starter.

## Prerequisites
- Visual Studio 2022 with ASP.NET and web development workload
- .NET 8 SDK
- SQL Server Express, Developer edition or LocalDB
- Node.js compatible with Angular 14 (Node 16.14+ or 18.10+ recommended)
- Angular CLI 14

## Run backend
1. Open `StaffingInvoiceManagement.sln` in Visual Studio 2022.
2. Set `StaffingInvoice.Api` as startup project.
3. Verify `ConnectionStrings:InvoiceDb` in `src/StaffingInvoice.Api/appsettings.json`.
4. Open Package Manager Console, set Default project to `StaffingInvoice.Api`, then run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
   Alternatively:
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.11
   dotnet ef migrations add InitialCreate --project src/StaffingInvoice.Api
   dotnet ef database update --project src/StaffingInvoice.Api
   ```
5. Run the API. Swagger opens at `https://localhost:7181/swagger`.

## Seed sample data
Run `database/02-sample-data.sql` after migrations. This creates a sample client, consultant, contract and approved timesheet for October 1-15, 2026. If testing at a later date, adjust the sample period as needed.

## Typical API workflow
1. `POST /api/clients`
2. `POST /api/resources`
3. `POST /api/contracts`
4. `POST /api/timesheets`
5. `POST /api/timesheets/{id}/approve`
6. `POST /api/invoices` with `{ "clientId": 1, "periodStart": "2026-10-01", "periodEnd": "2026-10-31", "taxRate": 0, "notes": "October staffing services" }`
7. `GET /api/invoices/{id}` to review
8. `POST /api/invoices/{id}/approve`
9. `GET /api/invoices/{id}/pdf`
10. `POST /api/invoices/{id}/payments` with `{ "amount": 5000, "paymentDate": "2026-10-30", "method": "ACH", "reference": "ACH-123" }`

## Run Angular 14 frontend
1. Open a terminal in `frontend/staffing-invoice-ui`.
2. Install dependencies: `npm install`.
3. Start the UI: `npm start`.
4. Browse to `http://localhost:4200`.
5. Ensure the API is running at `https://localhost:7181` and trust its local HTTPS development certificate (`dotnet dev-certs https --trust`).

The UI includes invoice KPIs, invoice listing, invoice generation, approval and PDF download. Client/resource/contract/timesheet setup is available through the documented API endpoints and Swagger.

## Run tests
```bash
dotnet test StaffingInvoiceManagement.sln
```

## Engineering practices included
- Separation of API, DTOs, data access, services, models and middleware.
- Async I/O and cancellation token propagation.
- EF Core precision configuration for money and hours.
- Transactional invoice creation and timesheet-to-invoice association.
- Invoice line snapshots preserve billed resource/rate/hours.
- Business-rule validation and consistent Problem Details errors.
- Structured logging and unique invoice numbering.
- xUnit tests for financial calculations.

## Important production hardening before real customer deployment
This is an interview/demo-ready foundation, not a certified production billing platform. Before handling real customer financial data, add:
- Identity provider integration (Entra ID/OIDC), role-based authorization and tenant isolation.
- Email provider integration, delivery retries and immutable send history.
- Concurrency tokens and idempotency keys for invoice creation/payment endpoints.
- Tax jurisdiction rules, configurable overtime multipliers, multi-currency exchange handling and credit notes.
- Audit log persistence, secrets management, data retention and backups.
- More integration tests against SQL Server and PDF visual regression tests.
- Invoice lifecycle controls for sending, voiding and credit/reissue workflows.

No project can be guaranteed bug-free across every machine and SQL Server configuration. Restore NuGet packages and run the included tests in your environment before presenting or deploying it.
