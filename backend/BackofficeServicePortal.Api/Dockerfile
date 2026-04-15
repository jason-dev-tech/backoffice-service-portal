FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backoffice-service-portal.sln ./
COPY backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj backend/BackofficeServicePortal.Api/
RUN dotnet restore backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj

COPY . .
RUN dotnet publish backend/BackofficeServicePortal.Api/BackofficeServicePortal.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BackofficeServicePortal.Api.dll"]
