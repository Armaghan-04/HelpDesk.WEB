# HelpDesk.WEB

A server-rendered help-desk application built with ASP.NET Core MVC. Employees raise tickets, technicians work them, and managers run assignments, departments, and user roles from one place.

## Tech stack

- **Framework:** ASP.NET Core MVC (`net10.0`), Razor views + Razor Pages for Identity
- **Database:** SQL Server via EF Core 10 (migrations committed)
- **Authentication:** ASP.NET Core Identity with `Guid` keys and role-based authorization
- **Validation:** DataAnnotations + FluentValidation (auto-wired through SharpGrip)
- **Mapping:** Riok.Mapperly (source-generated)
- **UI:** Bootstrap 5.3 served locally, hand-written `wwwroot/css/site.css`, vanilla JS
- **Patterns:** lightweight in-repo `Result` / `Result<T>` for expected business failures

## Features

### Roles

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

### Identity notes

- Email confirmation is not required.
- Phone numbers and 2FA are intentionally removed; built-in 2FA pages return 404.
- Email changes are read-only in the UI.
- Self-registration is controlled by `Authentication:Registration:Enabled`. It is enabled in Development and disabled by default everywhere else; when disabled, the Register links are hidden and direct GET/POST requests return 404.
- Sign-in POSTs are limited to 10 attempts per minute per client IP; excess requests receive `429 Too Many Requests`. Loading the login page does not count toward the limit. Counters are in-memory per app instance and reset on restart.
- Password recovery is controlled by `Authentication:PasswordRecovery:Enabled` and is disabled by default. The "Forgot password?" link is hidden while it is disabled, and direct GET/POST requests to the Forgot Password, Reset Password, and confirmation pages return 404.
- Password reset emails are not sent because no email provider is configured.
- Demo accounts are controlled by `DemoAccounts:Enabled` and are only created when that role's email and password are also configured. They are created through Identity with the normal password policy and role assignment; public registration stays disabled.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB is fine for local development)
- `dotnet-ef` CLI tool (for applying migrations)

```bash
dotnet tool install --global dotnet-ef
```

### User Secrets

Local secrets are stored in the .NET User Secrets store and are never committed to
the repository. The project is already initialized (`UserSecretsId` is set in
`HelpDesk.WEB/HelpDesk.WEB.csproj`), so you can set values directly:

```bash
dotnet user-secrets list --project HelpDesk.WEB
dotnet user-secrets set "<Key>" "<Value>" --project HelpDesk.WEB
dotnet user-secrets remove "<Key>" --project HelpDesk.WEB
```

User Secrets are loaded automatically when `ASPNETCORE_ENVIRONMENT=Development`.
The included launch profiles (`Properties/launchSettings.json`) set this for
`dotnet run` and the EF Core tools.

### Configure the database

No connection string is committed to the repository. `appsettings.json` intentionally
leaves `ConnectionStrings:DefaultConnection` unset; local development supplies it
through User Secrets. For LocalDB, run:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskDb;Trusted_Connection=True;TrustServerCertificate=True" --project HelpDesk.WEB
```

Use your own SQL Server connection string instead if you are not using LocalDB.
In production, set the same key through the
`ConnectionStrings__DefaultConnection` environment variable.

### Configure the bootstrap manager

On a fresh database, the app creates its first manager from the `BootstrapManager` section. No credentials are committed to the repository; set them through user secrets locally:

```bash
dotnet user-secrets set "BootstrapManager:FullName" "Help Desk Manager" --project HelpDesk.WEB
dotnet user-secrets set "BootstrapManager:Email" "manager@example.com" --project HelpDesk.WEB
dotnet user-secrets set "BootstrapManager:Password" "<YOUR_STRONG_PASSWORD>"
```

If no manager exists and no credentials are configured, the seeder logs a warning and skips account creation.

### Configure demo accounts

For a portfolio deployment you can seed three predefined accounts: an Employee, a Technician, and a Manager. The feature is controlled by `DemoAccounts:Enabled`; an account is only created when that role's email and password are also configured. Enable it only for the initial setup.

Create the accounts on the first production start, then turn the feature off again:

1. Set `DemoAccounts__Enabled=true` and provide credentials for each account you want (see the environment variables below).
2. Start the app once. Missing accounts are created with their role; existing accounts are left completely unchanged.
3. Remove `DemoAccounts__Enabled` or set it to `false`, and remove the password variables from the host configuration. The accounts remain in the database and continue to work.

The seeder is idempotent. On later starts it never resets a password, overwrites a profile, or changes an account's roles. If an existing account is inactive or missing its expected role, the app logs a warning and leaves it alone. Invalid or weak passwords are rejected by the existing Identity password policy and logged; the incomplete account is removed so it can be retried.

Because the demo accounts run before the bootstrap-manager check, an enabled demo Manager counts as the first manager and prevents an extra bootstrap account from being created.

### Apply migrations and run

```bash
dotnet restore
dotnet ef database update --project HelpDesk.WEB --startup-project HelpDesk.WEB
dotnet run --project HelpDesk.WEB
```

The development profile listens on:

- `https://localhost:7190`
- `http://localhost:5141`

Startup is idempotent: it ensures the three roles, any enabled demo accounts, the bootstrap manager, and the default departments exist.

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
| `DemoAccounts:Enabled` | Creates the demo accounts when `true`; leave `false` after initial setup |
| `DemoAccounts:Employee:Email` / `Password` | Demo Employee credentials |
| `DemoAccounts:Technician:Email` / `Password` | Demo Technician credentials |
| `DemoAccounts:Manager:Email` / `Password` | Demo Manager credentials |
| `DemoAccounts:{Employee\|Technician\|Manager}:FullName` | Optional display name; defaults to "Demo Employee", etc. |
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
DemoAccounts__Enabled=false
DemoAccounts__Employee__Email=...
DemoAccounts__Employee__Password=...
DemoAccounts__Employee__FullName=...
DemoAccounts__Technician__Email=...
DemoAccounts__Technician__Password=...
DemoAccounts__Technician__FullName=...
DemoAccounts__Manager__Email=...
DemoAccounts__Manager__Password=...
DemoAccounts__Manager__FullName=...
```

Environment variable names use `__` (double underscore) in place of the `:` used in
configuration keys. For example, `ConnectionStrings:DefaultConnection` becomes
`ConnectionStrings__DefaultConnection`.

On MonsterASP.NET, add these values in the hosting panel's environment variables /
application settings instead of editing `appsettings.json`. Never commit a real
connection string or password.

### Registration and password recovery

Registration is enabled only by `appsettings.Development.json`. The base `appsettings.json` disables it, so Production and other environments reject both the registration page and direct POSTs unless explicitly enabled.

Password recovery is disabled by default because the app has no email sender. When disabled, the Forgot Password link is hidden and direct GET/POST requests to `ForgotPassword`, `ForgotPasswordConfirmation`, `ResetPassword`, and `ResetPasswordConfirmation` return 404. Setting `Authentication:PasswordRecovery:Enabled` re-enables those pages, but it does not make email delivery work. Before enabling it, register a real `IEmailSender` implementation and test the full reset flow.

## Project structure

```text
HelpDesk.WEB/
├── Areas/
├── Common/
├── Configuration/
├── Controllers/
├── Data/
├── Extensions/
├── Mappings/
├── Models/
├── Services/
├── TagHelpers/
├── Validation/
├── Validators/
├── Views/
├── wwwroot/
├── Program.cs
└── appsettings.json
```

## Deployment notes

Before deploying to production:

1. Set `ASPNETCORE_ENVIRONMENT=Production`.
2. Set `ConnectionStrings__DefaultConnection` to a real SQL Server connection string.
3. Apply migrations before the app starts.
4. Provide `BootstrapManager__FullName`, `BootstrapManager__Email`, and `BootstrapManager__Password` for the first deploy through environment variables. User Secrets are for local development only and are not read in Production.
5. Optionally enable `DemoAccounts` for the first start to create the demo Employee, Technician, and Manager accounts, then disable it and remove the password variables.
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
