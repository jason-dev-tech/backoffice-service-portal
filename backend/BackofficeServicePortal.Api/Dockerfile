FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backoffice-service-portal.sln ./
COPY backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj backend/BackofficeServicePortal.Api/
RUN dotnet restore backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj

COPY . .
RUN dotnet publish backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM node:22.12.0 AS frontend-build
WORKDIR /src/frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN printf 'window.BACKOFFICE_API_BASE_URL = window.location.origin;\n' > public/runtime-config.js \
    && npm run build \
    && FRONTEND_INDEX_PATH="$(find /src/frontend/dist -type f -name index.html | head -n 1)" \
    && test -n "$FRONTEND_INDEX_PATH" \
    && mkdir -p /app/wwwroot \
    && cp -R "$(dirname "$FRONTEND_INDEX_PATH")"/. /app/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY --from=frontend-build /app/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "BackofficeServicePortal.Api.dll"]
