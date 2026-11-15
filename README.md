<div align="center">

<h1 align="center">HelpDesk.WEB</h1>

<p align="center">
  <strong>A server-rendered help desk for employees, technicians, and managers — built with ASP.NET Core MVC.</strong>
</p>

<p align="center">
  <a href="https://helpdeskmvc.runasp.net"><strong>🌐 View Live Demo</strong></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/Armaghan-04/HelpDesk.WEB/issues">Report a Bug</a>
  &nbsp;·&nbsp;
  <a href="https://github.com/Armaghan-04/HelpDesk.WEB/issues">Request a Feature</a>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10"></a>
  <a href="https://learn.microsoft.com/aspnet/core/mvc/overview"><img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white" alt="ASP.NET Core MVC"></a>
  <a href="https://learn.microsoft.com/ef/core/"><img src="https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet&logoColor=white" alt="EF Core 10"></a>
  <a href="https://www.microsoft.com/sql-server"><img src="https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white" alt="SQL Server"></a>
  <a href="https://getbootstrap.com/"><img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white" alt="Bootstrap 5.3"></a>
</p>

</div>

---

## Overview

HelpDesk.WEB is a full-stack help-desk application built with **ASP.NET Core MVC** and **SQL Server**. Employees raise tickets, technicians work them through a defined workflow, and managers handle assignments, departments, and user roles from one place.

The project is designed as a realistic portfolio application: server-rendered Razor views, ASP.NET Core Identity with role-based authorization, EF Core migrations, FluentValidation, and a lightweight `Result` pattern for expected business failures.

## Live demo

**https://helpdeskmvc.runasp.net**

The live demo is hosted on MonsterASP.NET and comes with three pre-seeded accounts — one for each role.

### Demo accounts

| Role | Email | Password |
|---|---|---|
| Employee | `demoemployee@helpdesk.com` | `Pass@123` |
| Technician | `demotechnician@helpdesk.com` | `Pass@123` |
| Manager | `demomanager@helpdesk.com` | `Pass@123` |

Demo credentials are defined in [`Data/DemoAccountSeeder.cs`](HelpDesk.WEB/Data/DemoAccountSeeder.cs). The seeder is idempotent: it creates only missing accounts and never resets an existing account's password, profile, or roles. Because it runs before the bootstrap-manager check, the demo Manager counts as the first manager.

## Features

### Role-based access

| Role | Responsibilities |
|---|---|
| **Employee** | Create tickets, view their own tickets, comment, close/reopen their tickets |
| **Technician** | View tickets assigned to them, start and resolve work |
| **Manager** | See all tickets, assign/unassign work, manage departments and user accounts |

New registrations always start as **Employee**. A manager can promote them from the Team page.

### Ticket workflow

```text
Open ──assign──▶ Assigned ──start──▶ InProgress ──resolve──▶ Resolved ──close──▶ Closed
                    ▲                    │                       │                 │
                    └────────────── reopen ◀──────────────────────┴─────────────────┘
```

- Tickets are created by employees only.
- Assignment requires an active Technician or Manager.
- Internal comments are hidden from employees.
- Every status change is written to ticket history.
- Comments are rejected on closed tickets.

### Departments

Seeded with **IT Support**, **Facilities**, and **Human Resources**. Managers can create, edit, activate, and deactivate departments. Department names are unique (case-insensitive), and the last active department cannot be deactivated.

### User management

Managers get a paginated Team page with search, role/status filters, and sorting. They can change roles and activate/deactivate accounts. Deactivation preserves all tickets, comments, assignments, and history. The last active manager cannot be deactivated, and a manager cannot change their own role or deactivate themselves.

Security stamps are validated every minute, so role changes and deactivations take effect quickly.

### Identity and security

- Email confirmation is not required.
- Phone numbers and 2FA are intentionally removed; built-in 2FA pages return 404.
- Email changes are read-only in the UI.
- Self-registration is controlled by `Authentication:Registration:Enabled`. It is enabled in Development and disabled by default everywhere else; when disabled, the Register links are hidden and direct GET/POST requests return 404.
- Sign-in POSTs are limited to 10 attempts per minute per client IP; excess requests receive `429 Too Many Requests`. Loading the login page does not count toward the limit. Counters are in-memory per app instance and reset on restart.
- Password recovery is controlled by `Authentication:PasswordRecovery:Enabled` and is disabled by default. The "Forgot password?" link is hidden while it is disabled, and direct GET/POST requests to the Forgot Password, Reset Password, and confirmation pages return 404.
- Password reset emails are not sent because no email provider is configured.
- Three demo accounts (Employee, Technician, Manager) are seeded automatically on startup. They are created through Identity with the normal password policy and role assignment; public registration stays disabled.

## Tech stack

- **Framework:** ASP.NET Core MVC (`net10.0`), Razor views + Razor Pages for Identity
- **Database:** SQL Server via EF Core 10 (migrations committed)
- **Authentication:** ASP.NET Core Identity with `Guid` keys and role-based authorization
- **Validation:** DataAnnotations + FluentValidation (auto-wired through SharpGrip)
- **Mapping:** Riok.Mapperly (source-generated)
- **UI:** Bootstrap 5.3 served locally, hand-written `wwwroot/css/site.css`, vanilla JS
- **Patterns:** lightweight in-repo `Result` / `Result<T>` for expected business failures

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB is fine for local development)
- `dotnet-ef` CLI tool (for applying migrations)

```bash
dotnet tool install --global dotnet-ef
```

### Clone the repository

```bash
git clone https://github.com/Armaghan-04/HelpDesk.WEB.git
cd HelpDesk.WEB
```

### Configure the database

Set `ConnectionStrings:DefaultConnection` to a SQL Server connection string. For LocalDB, use:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

You can place it in `appsettings.json` for local development, or keep it out of source control and supply it through User Secrets (recommended for anything beyond a throwaway LocalDB instance):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_CONNECTION_STRING>" --project HelpDesk.WEB
```

In production, set the `ConnectionStrings__DefaultConnection` environment variable to override the committed value.

### User secrets

Local secrets are stored in the .NET User Secrets store and are never committed to the repository. The project is already initialized (`UserSecretsId` is set in `HelpDesk.WEB/HelpDesk.WEB.csproj`), so you can set values directly:

```bash
dotnet user-secrets list --project HelpDesk.WEB
dotnet user-secrets set "<Key>" "<Value>" --project HelpDesk.WEB
dotnet user-secrets remove "<Key>" --project HelpDesk.WEB
```

User Secrets are loaded automatically when `ASPNETCORE_ENVIRONMENT=Development`. The included launch profiles (`Properties/launchSettings.json`) set this for `dotnet run` and the EF Core tools.

### Optional: bootstrap manager

On a fresh database, the demo Manager is seeded first, so a separate bootstrap manager is only needed if you remove `DemoAccountSeeder.SeedAsync(services)` from `Program.cs`. If you use it, set these through user secrets locally:

```bash
dotnet user-secrets set "BootstrapManager:FullName" "Help Desk Manager" --project HelpDesk.WEB
dotnet user-secrets set "BootstrapManager:Email" "manager@example.com" --project HelpDesk.WEB
dotnet user-secrets set "BootstrapManager:Password" "<YOUR_STRONG_PASSWORD>" --project HelpDesk.WEB
```

If no manager exists and no credentials are configured, the seeder logs a warning and skips account creation.

### Apply migrations and run

Migrations are applied manually, not at startup. Update the database first, then run the app:

```bash
dotnet restore
dotnet ef database update --project HelpDesk.WEB --startup-project HelpDesk.WEB
dotnet run --project HelpDesk.WEB
```

The development profile listens on:

- `https://localhost:7190`
- `http://localhost:5141`

Startup is idempotent: it ensures the three roles, the demo accounts, the bootstrap manager, and the default departments exist. Run `dotnet ef database update` again after pulling new migrations.

### Build

```bash
dotnet build
```

The solution (`HelpDesk.WEB.slnx`) contains the web project only.

## Configuration

| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Authentication:Registration:Enabled` | Allows anonymous self-registration and shows Register links |
| `Authentication:PasswordRecovery:Enabled` | Shows the Forgot Password link; enable only when email delivery works |
| `BootstrapManager:FullName` | Display name for the first manager |
| `BootstrapManager:Email` | Email/login for the first manager |
| `BootstrapManager:Password` | Password for the first manager |
| `Logging:LogLevel:*` | Standard ASP.NET Core logging levels |

Use environment variables in production, for example:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
Authentication__Registration__Enabled=false
Authentication__PasswordRecovery__Enabled=false
BootstrapManager__FullName=...
BootstrapManager__Email=...
BootstrapManager__Password=...
```

Environment variable names use `__` (double underscore) in place of the `:` used in configuration keys. For example, `ConnectionStrings:DefaultConnection` becomes `ConnectionStrings__DefaultConnection`.

On MonsterASP.NET, add these values in the hosting panel's environment variables / application settings instead of editing `appsettings.json`. Never commit a real connection string or password.

### Registration and password recovery

Registration is enabled only by `appsettings.Development.json`. `appsettings.json` disables it, so Production rejects the registration page and direct POSTs unless it is explicitly enabled in host configuration.

Password recovery is disabled by default because the app has no email sender. When disabled, the Forgot Password link is hidden and direct GET/POST requests to `ForgotPassword`, `ForgotPasswordConfirmation`, `ResetPassword`, and `ResetPasswordConfirmation` return 404. Setting `Authentication:PasswordRecovery:Enabled` re-enables those pages, but it does not make email delivery work. Before enabling it, register a real `IEmailSender` implementation and test the full reset flow.

## Project structure

```text
HelpDesk.WEB/
├── Areas/              # Razor Pages for Identity and account management
├── Common/             # Result / Result<T> types
├── Configuration/      # Options classes
├── Controllers/        # MVC controllers
├── Data/               # DbContext and seeders
├── Extensions/         # Service and helper extensions
├── Mappings/           # Mapperly mapping definitions
├── Migrations/         # EF Core migrations
├── Models/             # Entities and view models
├── Services/           # Business logic and interfaces
├── TagHelpers/         # Custom tag helpers
├── Validation/         # Validation infrastructure
├── Validators/         # FluentValidation validators
├── Views/              # Razor views
├── wwwroot/            # Static assets (Bootstrap, CSS, JS)
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## Deployment

Before deploying to production:

1. Set `ASPNETCORE_ENVIRONMENT=Production`.
2. Set `ConnectionStrings__DefaultConnection` to a real SQL Server connection string.
3. Apply migrations manually before starting the app with `dotnet ef database update --project HelpDesk.WEB --startup-project HelpDesk.WEB`. The SQL login must be allowed to create and alter schema.
4. Provide `BootstrapManager__FullName`, `BootstrapManager__Email`, and `BootstrapManager__Password` for the first deploy through environment variables if you are not relying on the demo accounts. User Secrets are for local development only and are not read in Production.
5. No demo-account configuration is needed: startup creates the three public accounts with the credentials in [Demo accounts](#demo-accounts). If you do not want them in production, remove the `DemoAccountSeeder.SeedAsync(services)` call from `Program.cs`.
6. Keep `Authentication__Registration__Enabled=false` unless public sign-up is intended.
7. Keep `Authentication__PasswordRecovery__Enabled=false` until a real email sender is configured and tested.
8. Serve over HTTPS.
9. If hosting behind a reverse proxy or load balancer, add forwarded-headers support so HTTPS redirection and per-IP login rate limiting work correctly.
10. Persist ASP.NET Core DataProtection keys if running multiple instances.
11. Do not deploy `appsettings.Development.json` or a Development environment. Registration is enabled there by design.

Publish with:

```bash
dotnet publish HelpDesk.WEB/HelpDesk.WEB.csproj -c Release -o ./publish
```

The output is framework-dependent by default, so the host needs the ASP.NET Core 10 runtime. Add `--self-contained` with a runtime identifier if you need a standalone build.

## Known limitations

- No automated test project is currently included.
- No email provider, so password reset and email confirmation messages are not delivered.
- No attachments, notifications, or inline comment editing.
- Public ticket lists, team lists, and department lists use fixed 8-row pagination.
- Technicians can open any ticket by URL, not only tickets assigned to them.
- History tracks status changes only, not reassignment or comment activity.
- No dark mode, dashboard, or role-aware home page.

## Contributing

Contributions, issues, and feature requests are welcome. For substantial changes, open an issue first so the approach can be discussed before you invest time in a pull request. Please keep the existing code style and update the README when behavior changes.

## License

No license file is currently included in this repository. Add a `LICENSE` file if you intend others to reuse or redistribute the project.

---

<p align="center">
  Built and maintained by <a href="https://github.com/Armaghan-04">@Armaghan-04</a>
</p>
