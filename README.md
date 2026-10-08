# Survey_UP

A CRUD web application for managing **student evaluation surveys**, built as an engineering thesis project. It supports creating surveys, managing questions and answers, inviting voters, and reviewing aggregated results.

## Tech stack

- **.NET 10** / **ASP.NET Core 10** (Razor Pages)
- **Entity Framework Core 10** (Code-First + Migrations)
- **Microsoft SQL Server** (production) / **SQLite** (local development and demos)
- **ASP.NET Core Identity** — registration, login, roles
- **Bootstrap** + jQuery (UI layer, `wwwroot/`)

## Project structure

| Directory | Description |
|---|---|
| `Pages/` | Razor Pages views (Surveys, Questions, Answers, Voters, Users, Roles, Public, Shared) |
| `Models/Tables/` | EF Core entities (`VtsTb*`, `ApplicationUser`, `ApplicationRole`, …) |
| `Models/Context/` | Two `DbContext`s: `N3mikosContext` (domain data) and `SurveyUpIdDbContext2` (Identity) |
| `Migrations/` | EF Core migrations for both contexts |
| `Areas/Identity/` | Scaffolded login / registration pages |
| `Services/` | `EmailSender` (SMTP) |
| `Data/` | Helpers and enums (e.g. `Roles`) |
| `wwwroot/` | Static assets (CSS, JS, images, libraries) |

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft SQL Server (local, LocalDB, or Docker container) — or nothing at all when you use the SQLite quick start below
- (Optional) `dotnet-ef` global tool:
  ```bash
  dotnet tool install --global dotnet-ef --version 10.*
  ```

## Configuration

1. Copy `appsettings.example.json` to `appsettings.Development.json` (the file is git-ignored — it won't be committed).
   ```bash
   cp appsettings.example.json appsettings.Development.json
   ```
2. Fill in `ConnectionStrings:DefaultConnection` with your SQL Server connection string, e.g.:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SurveyUP;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```
3. Fill in the SMTP settings in the `EmailSender` section (host, port, username, password).

> **Security note:** Never commit real passwords or connection strings. Use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) instead:
> ```bash
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
> dotnet user-secrets set "EmailSender:Password" "..."
> ```

## Quick start without SQL Server (SQLite + demo data)

No database server is needed to try the application. This creates a local SQLite file, the schema, four demo accounts and a sample survey (all data is fictional):

```bash
export Database__Provider=Sqlite
export ConnectionStrings__DefaultConnection="Data Source=surveyup.db"
export Seed__DemoData=true
dotnet run
```

Demo accounts (password `Demo!2345`, override with `Seed__DemoPassword`): `admin@demo.local`, `tworca@demo.local`, `sekretariat@demo.local`, `student@demo.local`.

Other seeding options (work with SQL Server too): `Seed__Roles=true` creates the application roles; `Seed__AdminEmail` + `Seed__AdminPassword` create the first administrator.

## Getting started

```bash
# 1. Clone and enter the project
git clone https://github.com/merynos94/Survey_UP.git
cd Survey_UP

# 2. Restore dependencies
dotnet restore

# 3. Apply migrations — two separate contexts
dotnet ef database update --context SurveyUpIdDbContext2
dotnet ef database update --context N3mikosContext

# 4. Run the application
dotnet run
```

The application listens on:
- `https://localhost:5001`
- `http://localhost:5000`

### Alternative — Visual Studio / Rider

Open `SurveyUP.sln` and run the **SurveyUP** profile (or **IIS Express**). Profiles are defined in `Properties/launchSettings.json`.

## EF Core migrations

The project uses **two** `DbContext`s, so every EF command must specify one via `--context` (CLI) or `-Context` (Package Manager Console).

```bash
# Add a new migration
dotnet ef migrations add <Name> --context N3mikosContext
dotnet ef migrations add <Name> --context SurveyUpIdDbContext2

# Update the database
dotnet ef database update --context N3mikosContext
dotnet ef database update --context SurveyUpIdDbContext2
```

## Roles and first login

Roles are defined in `Data/Enums/Roles.cs`. User accounts are created through `/Identity/Account/Register` — accounts require email confirmation (`RequireConfirmedAccount = true` in `Startup.cs`). Use the seeding options above to create the first administrator.

## Upgrading an existing database

Versions before the .NET 10 upgrade stored the registration password in clear text in `AspNetUsers.Password`. The migration `RemovePlaintextPasswordColumns` drops those columns (`dotnet ef database update --context SurveyUpIdDbContext2`). Old database backups still contain the plaintext values, so ask users to reset their passwords.

## Build / Publish

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## License

Student project (engineering thesis). For educational purposes.
