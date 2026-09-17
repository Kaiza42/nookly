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
