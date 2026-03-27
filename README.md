# Backoffice Service Portal API

A full-stack backend service built with **.NET 8**, demonstrating a
dual-database architecture using **PostgreSQL** for primary data and
**MongoDB** for audit logging.

> ⚠️ This repository is provided for **demonstration and portfolio
> purposes only**. It is **not an open-source project**, and all rights
> are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Full CRUD operations for Service Requests
-   PostgreSQL (EF Core) for primary business data
-   MongoDB for audit logging (Created, Updated, Deleted)
-   Docker-based MongoDB setup
-   Swagger API documentation
-   Fail-safe logging (MongoDB failures do not break API)

------------------------------------------------------------------------

## 🧱 Architecture

-   **Primary Data Store**: PostgreSQL\
-   **Audit Logging**: MongoDB\
-   **Backend Framework**: ASP.NET Core Web API\
-   **ORM**: Entity Framework Core\
-   **Configuration**: User Secrets\
-   **Containerization**: Docker

------------------------------------------------------------------------

## 📦 Tech Stack

-   .NET 8
-   ASP.NET Core Web API
-   Entity Framework Core
-   PostgreSQL
-   MongoDB
-   Docker

------------------------------------------------------------------------

## 📡 API Endpoints

### Service Requests

-   `GET /api/ServiceRequests`
-   `GET /api/ServiceRequests/{id}`
-   `POST /api/ServiceRequests`
-   `PUT /api/ServiceRequests/{id}`
-   `DELETE /api/ServiceRequests/{id}`

### Audit Logs

-   `GET /api/ServiceRequests/{id}/audit-logs`

------------------------------------------------------------------------

## 🧪 Sample Request

``` json
{
  "title": "Printer issue",
  "description": "The office printer is showing a paper jam error and cannot print.",
  "requesterName": "Jason",
  "status": "Open"
}
```

------------------------------------------------------------------------

## 🐳 Running MongoDB (Docker)

``` bash
docker run -d -p <host-port>:<container-port> --name mongodb mongo
```

> Example: map your local port to MongoDB's default container port.

------------------------------------------------------------------------

## 🔐 Configuration

Sensitive configuration values are managed using **.NET User Secrets**
and are not stored in the repository.

Required configuration keys:

-   `MongoDbSettings:ConnectionString`
-   `MongoDbSettings:DatabaseName`
-   `MongoDbSettings:AuditLogsCollectionName`

Example (for local development):

``` bash
dotnet user-secrets set "MongoDbSettings:ConnectionString" "<your-mongodb-connection-string>"
dotnet user-secrets set "MongoDbSettings:DatabaseName" "<your-database-name>"
dotnet user-secrets set "MongoDbSettings:AuditLogsCollectionName" "<your-collection-name>"
```

> ⚠️ Do not commit sensitive information such as connection strings,
> credentials, or API keys to source control.

------------------------------------------------------------------------

## ▶️ Run the Application

``` bash
dotnet run
```

Swagger UI:

    http://localhost:<your-port>/swagger

------------------------------------------------------------------------

## 💡 Key Design Highlights

-   Separation of concerns (Controller / Service)
-   Dual-database architecture (SQL + NoSQL)
-   Resilient logging (fail-safe MongoDB integration)
-   Clean configuration management

------------------------------------------------------------------------

## 📌 Notes

-   MongoDB is used for audit logs only
-   PostgreSQL remains the source of truth
-   Audit logging does not impact primary operations

------------------------------------------------------------------------

## 📈 Future Improvements

-   DTO layer for API contracts
-   Authentication & Authorization
-   Frontend integration
-   Cloud deployment (Azure)

------------------------------------------------------------------------

## 👤 Author

Jason
