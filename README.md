# Backoffice Service Portal API

A production-style full-stack project built with **ASP.NET Core Web API
(.NET 8)** and **Angular**, demonstrating a scalable architecture with a
dual-database design:

-   **PostgreSQL** for primary business data
-   **MongoDB (Docker-based)** for audit logging

> It is **not an open-source project**, and all rights are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Full CRUD operations for Service Requests (Create, Read, Update,
    Delete)
-   Angular frontend integrated with ASP.NET Core Web API
-   Reactive state management using **RxJS + AsyncPipe (zoneless
    Angular)**
-   DTO-based API design (no direct entity exposure)
-   Service layer architecture (Controller → Service → Data)
-   JWT authentication with role claims
-   Role-based authorization enforced on protected service request
    endpoints
-   Centralized validation handling
-   PostgreSQL (EF Core) for core data persistence
-   MongoDB for audit logging (Created, Updated, Deleted)
-   Docker-based MongoDB setup with container lifecycle management
-   Swagger API documentation
-   Fail-safe logging (MongoDB failures do not break API operations)

------------------------------------------------------------------------

## 🧱 Architecture

-   **Frontend**: Angular (SPA)
-   **Backend**: ASP.NET Core Web API (.NET 8)
-   **Pattern**: Controller → Service → DbContext
-   **Primary Database**: PostgreSQL
-   **Audit Logging**: MongoDB
-   **API Contract**: DTO-based separation
-   **Authentication**: JWT bearer tokens
-   **Authorization**: Role-based access control (`Admin`,
    `Operator`, `Viewer`)
-   **Validation**: DataAnnotations + centralized error handling
-   **CORS**: Configuration-driven (no hardcoding)

------------------------------------------------------------------------

## 📦 Tech Stack

-   ASP.NET Core Web API (.NET 8)
-   Angular (zoneless, AsyncPipe)
-   Entity Framework Core
-   ASP.NET Core Authentication / Authorization
-   PostgreSQL
-   MongoDB
-   Docker

------------------------------------------------------------------------

## 📡 API Endpoints

### Authentication

-   `POST /api/Auth/login`

### Service Requests

-   `GET /api/ServiceRequests`
-   `GET /api/ServiceRequests/{id}`
-   `POST /api/ServiceRequests`
-   `PUT /api/ServiceRequests/{id}`
-   `DELETE /api/ServiceRequests/{id}`

### Audit Logs

-   `GET /api/ServiceRequests/{id}/audit-logs`

Service request read endpoints require authentication. Create and update
operations require `Admin` or `Operator`, and delete requires `Admin`.

------------------------------------------------------------------------

## 🌐 Frontend Configuration

File:

    frontend/src/environments/environment.ts

Example:

``` ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:<your-backend-port>'
};
```

------------------------------------------------------------------------

## 🔒 Backend CORS Configuration

File:

    BackofficeServicePortal.Api/appsettings.json

Example:

``` json
"AllowedOrigins": [
  "http://localhost:<your-frontend-port>"
]
```

------------------------------------------------------------------------

## 🐳 Docker (MongoDB Setup)

MongoDB is used for audit logging and runs locally via Docker.

### First-time setup

``` bash
docker run -d -p <host-port>:<container-port> --name mongodb mongo
```

### Start container

``` bash
docker start mongodb
```

### Stop container

``` bash
docker stop mongodb
```

### Check status

``` bash
docker ps
```

### Notes

-   `docker run` should be used **only once** (initial container
    creation)
-   Use `docker start` and `docker stop` for subsequent runs
-   Re-running `docker run` with the same container name will cause a
    conflict

------------------------------------------------------------------------

## 🧪 Sample Request

``` json
{
  "title": "Printer issue",
  "description": "The office printer is not working.",
  "requesterName": "Jason"
}
```

------------------------------------------------------------------------

## 🧪 Sample Validation Error

``` json
{
  "message": "Validation failed",
  "errors": [
    {
      "field": "Title",
      "errors": ["The Title field is required."]
    }
  ]
}
```

------------------------------------------------------------------------

## ▶️ Run the Application

### Backend

``` bash
cd BackofficeServicePortal.Api
dotnet run
```

### Frontend

``` bash
cd frontend
ng serve
```

Open:

    http://localhost:<your-frontend-port>

------------------------------------------------------------------------

## 💡 Key Design Highlights

-   Clean separation of concerns (Controller / Service / Data)
-   DTO layer prevents over-posting and entity exposure
-   Reactive frontend using AsyncPipe (zoneless Angular)
-   Dual-database architecture (SQL + NoSQL)
-   Fail-safe logging design
-   Environment-based configuration
-   Full-stack integration with clear boundaries

------------------------------------------------------------------------

## 📌 Notes

-   MongoDB is used only for audit logs
-   PostgreSQL is the source of truth
-   Update environment ports before running locally
-   Backend must run on HTTPS for frontend integration

------------------------------------------------------------------------

## 📈 Future Improvements

-   Frontend authentication workflow
-   Expanded role-based access control coverage
-   FluentValidation integration
-   Unit & integration testing
-   Cloud deployment (Azure / AWS)
-   CI/CD pipeline

------------------------------------------------------------------------

## 👤 Author

Jason
