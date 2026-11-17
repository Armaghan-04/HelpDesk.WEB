# HelpDesk.WEB

A server-rendered help desk for employees, technicians, and managers, built with ASP.NET Core MVC and SQL Server.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)
![EF Core 10](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white)
![Bootstrap 5.3](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)

**Live demo:** https://helpdeskmvc.runasp.net

| Role | Email | Password |
|---|---|---|
| Employee | `demoemployee@helpdesk.com` | `Pass@123` |
| Technician | `demotechnician@helpdesk.com` | `Pass@123` |
| Manager | `demomanager@helpdesk.com` | `Pass@123` |

## Features

- **Employee** — create tickets, comment, close/reopen their own tickets.
- **Technician** — view assigned tickets, start and resolve work.
- **Manager** — see all tickets, assign work, manage departments, roles, and accounts.
- Ticket workflow: `Open → Assigned → InProgress → Resolved → Closed` (with reopen).
- Internal comments hidden from employees; every status change is logged to ticket history.
- Seeded departments: IT Support, Facilities, HR. New registrations start as Employee.

## Tech stack

ASP.NET Core MVC (`net10.0`) with Razor views, EF Core 10 + SQL Server, ASP.NET Core Identity with role-based authorization, FluentValidation, Mapperly, and Bootstrap 5.3 (via CDN).

## Architecture

Server-rendered MVC with a service layer. Controllers stay thin; business rules live in services and return a lightweight `Result` type. Identity is isolated in an Area.

```text
HelpDesk.WEB/
├── Areas/          # Identity Razor Pages (login, register, account management)
├── Controllers/    # MVC controllers (tickets, departments, team, home)
├── Services/       # Business logic and interfaces
├── Data/           # DbContext and seeders (roles, demo accounts, departments)
├── Models/         # Entities and view models
├── Mappings/       # Mapperly source-generated mappings
├── Validators/     # FluentValidation validators
├── Migrations/     # EF Core migrations
├── Views/          # Razor views and Shared/_Layout.cshtml
├── wwwroot/        # Static assets
├── Common/         # Result / Result<T> types
├── Configuration/  # Options classes
├── Extensions/     # DI and helper extensions
├── TagHelpers/     # Custom tag helpers
├── Validation/     # Validation infrastructure
└── Program.cs      # Composition root and startup seeding
```

## Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), SQL Server (LocalDB is fine), and the EF CLI tool.

```bash
dotnet tool install --global dotnet-ef
git clone https://github.com/Armaghan-04/HelpDesk.WEB.git
cd HelpDesk.WEB
```

Set `ConnectionStrings:DefaultConnection` in `appsettings.json` (or User Secrets):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Apply migrations and run:

```bash
dotnet restore
dotnet ef database update --project HelpDesk.WEB --startup-project HelpDesk.WEB
dotnet run --project HelpDesk.WEB
```

Dev URLs: `https://localhost:7190` / `http://localhost:5141`. Startup seeds the roles, departments, and the three demo accounts above.

## Configuration

| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Authentication:Registration:Enabled` | Public sign-up (enabled in Development only by default) |
| `Authentication:PasswordRecovery:Enabled` | Forgot-password pages (off by default; no email sender configured) |

Use `__` instead of `:` in environment variables (e.g. `ConnectionStrings__DefaultConnection`). User Secrets are for local development only.

## Known limitations

No automated tests or email provider; no attachments, notifications, or dashboards; fixed 8-row pagination on list pages.

## License

No license file is included yet.

---

Built and maintained by [@Armaghan-04](https://github.com/Armaghan-04).