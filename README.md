# Nookly

Nookly is a personal desktop hub for tracking movies, series, anime, manga, notes, and future personal modules.

## Architecture

- `src/Nookly.Desktop`: WPF desktop client
- `src/Nookly.Api`: ASP.NET Core HTTP API
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

Build and test the complete solution:

```powershell
dotnet build Nookly.sln
dotnet test Nookly.sln --no-build
```

The PostgreSQL credentials in `compose.yaml` and `appsettings.Development.json` are for local development only.
