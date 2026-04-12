# Backoffice Service Portal

A full-stack backoffice application for managing internal service
requests, built with **ASP.NET Core Web API (.NET 8)** and **Angular**.
The current implementation combines an authenticated SPA with a REST API
and a dual-database design:

-   **PostgreSQL** for primary business data
-   **MongoDB (Docker-based)** for audit logging

> It is **not an open-source project**, and all rights are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Login-based access to the backoffice UI using JWT authentication
-   Protected Angular routes for authenticated users
-   Service request management workflow covering create, list, update,
    and delete operations
-   Request workspace with search, status filtering, sorting, and
    client-side pagination
-   DTO-based API design (no direct entity exposure)
-   Service layer architecture (Controller → Service → Data)
-   Role-based authorization on service request write operations
-   Centralized validation responses for invalid API payloads
-   PostgreSQL (EF Core) for core application data
-   MongoDB for audit logging of create, update, and delete events
-   Swagger available in development
-   Fail-safe audit logging (MongoDB write failures do not block the API)

------------------------------------------------------------------------

## 🧱 Architecture

-   **Frontend**: Angular standalone SPA
-   **Backend**: ASP.NET Core Web API (.NET 8)
-   **Application Flow**: Angular client → authenticated API endpoints
-   **Backend Pattern**: Controller → Service → DbContext
-   **Primary Database**: PostgreSQL
-   **Audit Logging**: MongoDB
-   **API Contract**: DTO-based separation
-   **Authentication**: JWT bearer tokens
-   **Authorization**: Role-based access control (`Admin`,
    `Operator`, `Viewer`)
-   **Validation**: DataAnnotations + centralized model validation
-   **CORS**: Configuration-driven allowed origins

------------------------------------------------------------------------

## 📦 Tech Stack

-   ASP.NET Core Web API (.NET 8)
-   Angular
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
-   `POST /api/ServiceRequests/batch`
-   `PUT /api/ServiceRequests/{id}`
-   `DELETE /api/ServiceRequests/{id}`

### Audit Logs

-   `GET /api/ServiceRequests/{id}/audit-logs`

All service request endpoints require authentication. Create and update
operations require `Admin` or `Operator`, and delete requires `Admin`.

------------------------------------------------------------------------

## 🌐 Frontend Configuration

The Angular application is configured to talk to the backend API
through the environment files and currently provides:

-   a login screen
-   route protection via an auth guard
-   automatic bearer token attachment through an HTTP interceptor
-   a service request workspace for viewing and maintaining records

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

    backend/BackofficeServicePortal.Api/appsettings.json

Example:

``` json
"AllowedOrigins": [
  "http://localhost:<your-frontend-port>"
]
```

------------------------------------------------------------------------

## 🐳 Docker (MongoDB Setup)

MongoDB is used only for audit logging and is expected to run locally
via Docker.

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
cd backend/BackofficeServicePortal.Api
dotnet run --launch-profile https
```

HTTPS is required for frontend integration during local development.
If the local development certificate is not trusted, run:

``` bash
dotnet dev-certs https --trust
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

-   The frontend and backend are separated cleanly, but operate as a
    single backoffice application
-   DTOs prevent direct entity exposure and keep the API contract
    explicit
-   Authentication is enforced in both routing and HTTP request flow
-   Role-based access rules are applied where records are created,
    changed, and deleted
-   PostgreSQL remains the source of truth while MongoDB is limited to
    audit history
-   Configuration is environment-driven for API base URL, CORS, JWT, and
    database settings

------------------------------------------------------------------------

## 📌 Notes

-   MongoDB is used only for audit logs
-   PostgreSQL is the source of truth
-   The frontend currently consumes the API over HTTPS
-   A bootstrap admin account can be created from configuration when the
    application starts with an empty user store

------------------------------------------------------------------------

## 📈 Future Improvements

-   Audit log views in the frontend
-   Broader UI coverage for role-specific workflows
-   Unit & integration testing
-   Cloud deployment (Azure / AWS)
-   CI/CD pipeline

------------------------------------------------------------------------

## 👤 Author

Jason
