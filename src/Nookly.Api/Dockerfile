FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Nookly.sln ./
COPY src/Nookly.Api/Nookly.Api.csproj src/Nookly.Api/
COPY src/Nookly.Application/Nookly.Application.csproj src/Nookly.Application/
COPY src/Nookly.Contracts/Nookly.Contracts.csproj src/Nookly.Contracts/
COPY src/Nookly.Domain/Nookly.Domain.csproj src/Nookly.Domain/
COPY src/Nookly.Infrastructure/Nookly.Infrastructure.csproj src/Nookly.Infrastructure/
RUN dotnet restore src/Nookly.Api/Nookly.Api.csproj

COPY src/ src/
RUN dotnet publish src/Nookly.Api/Nookly.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Nookly.Api.dll"]
