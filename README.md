# Nookly

Nookly is a personal desktop hub for tracking movies, series, anime, manga, notes, and future personal modules.

## Architecture

- `src/Nookly.Desktop`: WPF desktop client
- `src/Nookly.Api`: ASP.NET Core HTTP API
- `src/Nookly.Contracts`: HTTP contracts shared by the API and desktop client
- `src/Nookly.Application`: use cases and persistence abstractions
- `src/Nookly.Domain`: entities and business rules
- `src/Nookly.Infrastructure`: Entity Framework Core and PostgreSQL
- `tests`: automated tests

## Local development

Requirements: .NET 10 SDK and Docker Desktop.

```powershell
docker compose up -d
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Nookly.Infrastructure --startup-project src/Nookly.Api
dotnet run --project src/Nookly.Api
```

In a second terminal, start the desktop client:

```powershell
dotnet run --project src/Nookly.Desktop
```

The desktop client uses `http://localhost:5186/` by default. Override it with the
`NOOKLY_API_URL` environment variable when needed.

Build and test the complete solution:

```powershell
dotnet build Nookly.sln
dotnet test Nookly.sln --no-build
```

The PostgreSQL credentials in `compose.yaml` and `appsettings.Development.json` are for local development only.

### Administration and email

The reserved administrator account is `Emerick.Roeting1@gmail.com`. Configure its password with .NET user secrets locally:

```powershell
dotnet user-secrets set "NOOKLY_ADMIN_PASSWORD" "a-strong-local-password" --project src/Nookly.Api
```

Transactional email uses SMTP when `NOOKLY_SMTP_HOST`, `NOOKLY_SMTP_PORT`, `NOOKLY_SMTP_USER`,
`NOOKLY_SMTP_PASSWORD`, and `NOOKLY_SMTP_FROM` are configured. Without SMTP configuration, development
messages are written to `src/Nookly.Api/.email-outbox` so confirmation and password reset flows remain testable.

## Continuous integration

GitHub Actions validates every push and pull request targeting `main` on a Windows runner. The workflow checks formatting, builds with warnings treated as errors, runs the complete test suite with coverage, and verifies that the Entity Framework model has no missing migration.

## Releases

Create and push a semantic version tag to publish a release:

```powershell
git tag v0.1.0
git push origin v0.1.0
```

The release workflow produces:

- `Nookly-Desktop-win-x64.zip`: self-contained Windows desktop application
- `Nookly-Api-linux-x64.zip`: framework-dependent Linux API
- `ghcr.io/kaiza42/nookly-api`: versioned API container image

The deployed API requires these environment variables:

```text
ConnectionStrings__Database=Host=postgres;Port=5432;Database=nookly;Username=nookly;Password=change-me
Tmdb__ReadAccessToken=your-token
NOOKLY_ADMIN_PASSWORD=a-strong-password
NOOKLY_PUBLIC_API_URL=https://api.example.com
NOOKLY_SMTP_HOST=smtp.example.com
NOOKLY_SMTP_PORT=587
NOOKLY_SMTP_USER=your-user
NOOKLY_SMTP_PASSWORD=your-password
NOOKLY_SMTP_FROM=noreply@example.com
```
