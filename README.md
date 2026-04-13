# Backoffice Service Portal

A production-style full-stack backoffice system focused on internal
service request management, built with **ASP.NET Core Web API (.NET 8)**
and **Angular**. The current implementation combines an authenticated
SPA, a REST API, an operational dashboard, and a dual-database design:

-   **PostgreSQL** for primary business data
-   **MongoDB (Docker-based)** for audit logging

> It is **not an open-source project**, and all rights are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Login-based access to the backoffice UI using JWT authentication
-   Protected Angular routes for authenticated users
-   Authenticated dashboard with summary reporting and drill-down into
    the service request workspace
-   Service request management workflow covering create, list, update,
    and delete operations
-   Backend-driven querying for the service request workspace:
    filtering (`status`), keyword search (`title`, `description`,
    `requesterName`), sorting (`createdAt`, `title`), and pagination
-   DTO-based API design (no direct entity exposure)
-   Service layer architecture (Controller → Service → DbContext)
-   Role-based authorization on service request write operations
-   Centralized validation responses for invalid API payloads
-   PostgreSQL (EF Core) for core application data
-   MongoDB for audit logging of create, update, and delete events
-   Swagger available in development
-   Fail-safe audit logging (MongoDB write failures do not block the API)

### Role-Aware UI

-   **Admin** users can create, edit, and delete service requests
-   **Operator** users can create and edit service requests
-   **Viewer** users have read-only access to dashboard and request data
-   Frontend-driven access control is enforced from JWT role claims, so
    the UI aligns with the authenticated user’s effective permissions
-   Permission checks are centralized in `AuthService`, providing a
    single role-aware permission layer for feature access decisions
-   Read-only users are not shown irrelevant write actions, keeping the
    interface clean and reducing accidental or unauthorized workflows

------------------------------------------------------------------------

## 🧱 Architecture

-   **Backend**: ASP.NET Core Web API (.NET 8), PostgreSQL for
    operational data, MongoDB for audit logs
-   **Frontend**: Angular standalone SPA using RxJS and `AsyncPipe`
-   **Application Flow**: Angular client → authenticated API endpoints
-   **Client Views**: Login, dashboard, and service request workspace
-   **Frontend Access Control**: Role-aware UI logic using JWT-derived
    permission helpers
-   **Backend Pattern**: Controller → Service → DbContext
-   **API Contract**: DTO-based separation
-   **Authentication**: JWT bearer tokens
-   **Authorization**: Role-based access control (`Admin`,
    `Operator`, `Viewer`)
-   **Validation**: DataAnnotations + centralized model validation
-   **CORS**: Configuration-driven allowed origins

------------------------------------------------------------------------

## 🗄️ Data Architecture

-   **PostgreSQL** is the primary database for structured transactional
    data, including service requests, users, roles, and related
    application records.
-   **MongoDB** is used for flexible, append-only audit logs that track
    service request changes without affecting the main transactional
    workflow.

------------------------------------------------------------------------

## 📦 Tech Stack

-   ASP.NET Core Web API (.NET 8)
-   Angular
-   RxJS
-   Entity Framework Core
-   ASP.NET Core Authentication / Authorization
-   PostgreSQL
-   MongoDB
-   xUnit
-   Testcontainers for PostgreSQL and MongoDB
-   Docker

------------------------------------------------------------------------

## 📡 API Capabilities

### Service Request Querying

`GET /api/ServiceRequests` supports database-level querying through
optional query parameters:

-   `status`
-   `search`
-   `sort`
-   `page`
-   `pageSize`

Filtering, search, sorting, and pagination are executed in PostgreSQL
through the API query pipeline rather than in-memory in the client.

### Dashboard Aggregation

`GET /api/ServiceRequests/dashboard` uses database-level aggregation for
summary counts and reporting fields rather than loading all service
requests into memory. Dashboard status grouping is normalized
case-insensitively so values such as `Open`, `open`, and `OPEN` are
treated consistently, while the API contract remains stable.

### Endpoints

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

## 📁 Project Structure

``` text
backend/
  BackofficeServicePortal.Api/
  BackofficeServicePortal.Api.Tests/

frontend/
  src/
```

------------------------------------------------------------------------

## 🧪 Backend Testing

Backend automated testing is implemented with **xUnit** and is focused
on integration-heavy coverage around the current service request and
authentication flows.

### Current Coverage

-   `ServiceRequestService` integration tests covering query filtering,
    search, sorting, pagination, dashboard aggregation, and
    create/update/delete behavior against PostgreSQL
-   `ServiceRequestAuditLogService` integration tests covering MongoDB
    audit log persistence
-   Auth/login API integration tests covering successful login, invalid
    credentials, unauthenticated access rejection, and authenticated
    access with a JWT obtained from the login endpoint
-   Authorization integration tests for protected service request
    endpoints through authenticated and unauthenticated API flows
-   Validation integration tests for service request create/update
    endpoints, including the centralized validation response shape

### Testing Approach

-   Service-level integration tests run against ephemeral
    **PostgreSQL Testcontainers** and **MongoDB Testcontainers**
-   API integration tests use **`WebApplicationFactory`** to boot the
    ASP.NET Core application in a test host and exercise real HTTP
    endpoints
-   Authentication-focused API tests log in through
    `POST /api/Auth/login` and use the returned bearer token for
    protected endpoint coverage

------------------------------------------------------------------------

## 🌐 Frontend Configuration

The Angular application is configured to talk to the backend API
through the environment files and currently provides:

-   a login screen
-   an authenticated dashboard with summary counts
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
-   The dashboard provides a lightweight reporting surface using the
    existing service request domain
-   DTOs prevent direct entity exposure and keep the API contract
    explicit
-   Authentication is enforced in both routing and HTTP request flow
-   The frontend applies role-aware UI behavior through `AuthService`
    permission helpers and conditional rendering
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
-   Expanded dashboard reporting (trend views, charts, recent activity)
-   Broader UI coverage for role-specific workflows
-   Cloud deployment (Azure / AWS)
-   CI/CD pipeline

------------------------------------------------------------------------

## 👤 Author

Jason
