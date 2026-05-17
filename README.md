# Backoffice Service Portal

A production-style full-stack backoffice system focused on internal
service request management, built with **ASP.NET Core Web API (.NET 8)**
and **Angular**. The current implementation combines an authenticated
SPA, a REST API, an operational dashboard, and a PostgreSQL-backed data
model:

-   **PostgreSQL** for primary business data
-   **PostgreSQL JSONB** for service request audit history

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
-   PostgreSQL (EF Core) for core application data and audit log
    persistence
-   Audit logging for create, update, and delete events using
    PostgreSQL JSONB
-   Audit history preserved after service request deletion
-   Backend audit history endpoint for service requests
-   Swagger UI available in local development and in the containerized
    backend runtime
-   Fail-safe audit logging (audit write failures do not block the API)

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
-   Audit log data is currently exposed by the backend API only; the
    frontend does not yet render audit history views

------------------------------------------------------------------------

## 🧱 Architecture

-   **Backend**: ASP.NET Core Web API (.NET 8), PostgreSQL for
    operational data and audit history
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

## 🚚 Deployment Overview

-   **OpenTofu** provisions the AWS deployment foundation, including EC2,
    a security group, and optional Elastic IP support
-   The application is deployed to a single **AWS EC2** instance
-   Public traffic is exposed on **HTTPS 443 only**
-   **Kestrel** terminates HTTPS using a mounted certificate
-   The **Angular frontend is served as static files by the ASP.NET Core
    app**, so the UI and API share the same host
-   **PostgreSQL** runs alongside the application in Docker Compose with
    persistent volume storage
-   The EC2 host is **deploy-only**: it pulls a prebuilt container image
    from **GitHub Container Registry (GHCR)** and restarts services

### Runtime Architecture

-   Browser requests `https://<host>/`
-   ASP.NET Core serves the compiled Angular SPA from `wwwroot`
-   Frontend API calls target the same host under `/api/...`
-   API requests are handled by ASP.NET Core controllers
-   PostgreSQL stores application data, identity data, and audit history
-   Health checks are exposed at `/health`, `/health/live`, and
    `/health/ready`

### Key Design Decisions

-   **Single-host deployment** keeps infrastructure simple while still
    demonstrating production-oriented delivery practices
-   **Same-origin frontend and API hosting** avoids an additional reverse
    proxy layer and simplifies browser integration
-   **Prebuilt image promotion** keeps deployment fast and predictable by
    separating CI build concerns from EC2 runtime concerns
-   **Containerized Postgres with persistent volume storage** keeps the
    stack self-contained for a portfolio environment

------------------------------------------------------------------------

## Deployment & Operations

The deployment path is intentionally lightweight and production-style
without claiming enterprise scale:

``` text
OpenTofu -> AWS EC2/Security Group -> GitHub Actions -> GHCR -> Docker Compose -> ASP.NET Core + PostgreSQL
```

Repository deployment capabilities are split into three layers:

-   **Infrastructure provisioning**: OpenTofu defines the AWS foundation
    for the EC2 host, security group, configurable SSH access, HTTPS
    access, and optional Elastic IP
-   **Deployment automation**: GitHub Actions publishes the application
    image to GHCR, and EC2 deployment scripts sync Compose artifacts,
    validate runtime configuration, pull images, and restart services
-   **Operational tooling**: verification, runtime inspection,
    conservative Docker cleanup, runbook troubleshooting, and Compose
    override guidance support day-to-day operation

Implemented capabilities include:

-   GHCR image publishing with both `latest` and commit SHA tags
-   EC2 Docker deployment using a prebuilt application image
-   HTTPS/TLS runtime configuration through mounted certificates
-   Runtime `.env` validation before Compose startup
-   Deployment verification for SSH, Docker, Compose status, and health
    endpoints
-   Optional self-signed certificate verification support for
    development checks
-   Runtime inspection helper for Compose status, backend logs, image
    summary, and disk usage
-   Conservative Docker maintenance helper for unused images, stopped
    containers, and builder cache
-   Deployment runbook with troubleshooting for GHCR auth, missing
    runtime config, certificate mounts, and restarting services
-   Compose override guidance for environment-specific changes without
    editing the tracked base Compose file

Operational debugging is centered on health checks, certificate mount
preflight validation, runtime inspection, and the deployment runbook.
Secrets, certificates, and host-specific values are kept out of tracked
files.

------------------------------------------------------------------------

## 🗄️ Data Architecture

-   **PostgreSQL** is the system of record for structured transactional
    data, including service requests, users, roles, related application
    records, and service request audit history.
-   Audit logs are stored in PostgreSQL using a **JSONB** column for the
    audit details payload while preserving the service request id,
    action, and timestamp as first-class fields.
-   Audit history is retained after a service request is deleted by
    preserving the original `ServiceRequestId` in the audit record
    without requiring the live service request row to remain present.

------------------------------------------------------------------------

## 📦 Tech Stack

-   ASP.NET Core Web API (.NET 8)
-   Angular
-   RxJS
-   Entity Framework Core
-   ASP.NET Core Authentication / Authorization
-   PostgreSQL
-   xUnit
-   Testcontainers for PostgreSQL
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

Audit log responses currently include:

-   `Id`
-   `ServiceRequestId`
-   `Action`
-   `TimestampUtc`
-   `Details`

Audit history is stored in PostgreSQL JSONB and is retained after a
service request is deleted. The backend can still return deleted-request
history when the original service request id is known.

### Health

-   `GET /health` - overall service health
-   `GET /health/live` - liveness check (process is running)
-   `GET /health/ready` - readiness check (includes database
    connectivity)

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
-   `ServiceRequestAuditLogService` integration tests covering
    PostgreSQL audit log persistence and read-back behavior
-   Auth/login API integration tests covering successful login, invalid
    credentials, unauthenticated access rejection, and authenticated
    access with a JWT obtained from the login endpoint
-   Authorization integration tests for protected service request
    endpoints through authenticated and unauthenticated API flows
-   Validation integration tests for service request create/update
    endpoints, including the centralized validation response shape

### Testing Approach

-   Service-level integration tests run against ephemeral
    **PostgreSQL Testcontainers**
-   API integration tests use **`WebApplicationFactory`** to boot the
    ASP.NET Core application in a test host and exercise real HTTP
    endpoints
-   Authentication-focused API tests log in through
    `POST /api/Auth/login` and use the returned bearer token for
    protected endpoint coverage
-   Audit retention coverage verifies that delete operations preserve
    historical audit entries in PostgreSQL

------------------------------------------------------------------------

## CI/CD Pipeline

This project uses **GitHub Actions** for both validation and deployment.

### CI Flow

-   Build and test the backend with **.NET 8**
-   Build and test the frontend with **Angular + Vitest**
-   Run the backend and frontend jobs independently in CI
-   Publish the production container image to **GHCR** only after the
    required jobs succeed on `push` to `main`
-   Publish both `latest` and the commit SHA tag for the container image

### Container Image Tags

-   `latest` is convenient for simple deployments that should always
    pull the newest successful `main` image
-   Commit SHA tags are better for immutable, traceable deployments
-   `docker-compose.yml` supports `DEPLOY_IMAGE_REPOSITORY` and
    `DEPLOY_IMAGE_TAG` so the deployed image can be selected through
    environment configuration
-   Do not put secrets, credentials, or private values in image
    configuration

### CD Flow

-   A separate **CD** workflow is triggered after the `CI` workflow
    completes successfully for a `push` to `main`
-   Deployment runs on a dedicated **self-hosted GitHub Actions runner**
    on the EC2 instance
-   The runner pulls the latest GHCR image, restarts the Docker Compose
    services, and verifies readiness through the backend health endpoint

This setup demonstrates a clean separation between **build**, **image
publication**, and **deployment execution**.

------------------------------------------------------------------------

## 🎭 Playwright E2E Coverage

Playwright coverage currently focuses on authenticated, role-aware
frontend workflows:

-   role-based visibility for service request actions
-   operator create service request flow
-   admin delete service request flow

E2E runs are designed to target an isolated database environment rather
than local development data.

------------------------------------------------------------------------

## 🌐 Frontend Configuration

The Angular application is configured to talk to the backend API
through the environment files and a runtime override, and currently
provides:

-   a login screen
-   an authenticated dashboard with summary counts
-   route protection via an auth guard
-   automatic bearer token attachment through an HTTP interceptor
-   a service request workspace for viewing and maintaining records

Files:

    frontend/src/environments/environment.ts
    frontend/public/runtime-config.js

Default fallback:

``` ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7179'
};
```

Runtime override:

-   the frontend reads `window.BACKOFFICE_API_BASE_URL` from
    `/runtime-config.js`
-   Playwright startup generates `frontend/public/runtime-config.js`
    before `ng serve`
-   `frontend/public/runtime-config.js` is a generated artifact and
    should not be committed

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

## 🐳 Isolated E2E Databases

Playwright E2E uses `docker-compose.e2e.yml` for isolated database
services only:

-   PostgreSQL on `localhost:55432`

The backend now applies EF Core migrations on startup, and startup
bootstrap supports `Admin`, `Operator`, and `Viewer` users for isolated
environments.

Minimal local E2E setup:

``` bash
export POSTGRES_PASSWORD=<your-e2e-postgres-password>
docker compose -f docker-compose.e2e.yml up -d
```

The API should then be started with E2E-specific PostgreSQL connection
settings, JWT key, and bootstrap credentials for the three test roles.

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

Deployment uses the repository root `docker-compose.yml` on a single
EC2 instance.

-   Docker Compose manages the ASP.NET Core application container and
    the PostgreSQL container
-   The application container is pulled from **GHCR** as a prebuilt
    image
-   The container already includes the compiled Angular frontend, which
    ASP.NET Core serves as static files
-   HTTPS is handled by Kestrel with a mounted certificate
-   Only port `443` is exposed publicly
-   Backend HTTP is bound to `127.0.0.1:8080`
-   PostgreSQL is bound to `127.0.0.1:5432`
-   PostgreSQL data is persisted in the `postgres_data` Docker volume
-   Health checks are configured for both services
-   Backend startup depends on PostgreSQL health through `depends_on`
    with `condition: service_healthy`

Required environment variables include:

``` bash
POSTGRES_DB=backoffice_service_portal
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me
JWT_KEY=replace_with_a_secure_random_secret
```

The backend container also requires HTTPS certificate configuration:

``` bash
HTTPS_CERT_PATH=/https/<your-certificate>.pfx
HTTPS_CERT_PASSWORD=<your-certificate-password>
```

Deployment startup:

``` bash
docker compose pull
docker compose up -d
```

Public access:

-   `https://<host-or-domain>`
-   Swagger UI: `https://<host-or-domain>/swagger`
-   Frontend SPA: `https://<host-or-domain>/`

Internal bindings:

-   Backend HTTP: `127.0.0.1:8080`
-   PostgreSQL: `127.0.0.1:5432`

On container startup, the API applies EF Core migrations
automatically. Startup migration includes retry logic for PostgreSQL
readiness.

The backend container health check calls `/health/ready`. PostgreSQL
health is checked with `pg_isready`.

JWT signing configuration for the containerized backend is provided
through environment variables, including `JWT_KEY`.

Local non-container development:

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

For Playwright E2E, the frontend is started by Playwright and receives
its API target through the generated runtime config file rather than by
editing tracked source files.

Open:

    http://localhost:<your-frontend-port>

------------------------------------------------------------------------

## 💡 Key Design Highlights

-   The frontend and backend are separated cleanly in the codebase, but
    are deployed as a single web application
-   The dashboard provides a lightweight reporting surface using the
    existing service request domain
-   DTOs prevent direct entity exposure and keep the API contract
    explicit
-   Authentication is enforced in both routing and HTTP request flow
-   The frontend applies role-aware UI behavior through `AuthService`
    permission helpers and conditional rendering
-   Role-based access rules are applied where records are created,
    changed, and deleted
-   PostgreSQL is the single system of record for both transactional
    data and audit history
-   Audit history is preserved after service request deletion while
    remaining queryable by original service request id
-   Configuration is environment-driven for API base URL, CORS, JWT, and
    database settings
-   CI/CD promotes a prebuilt image to EC2 rather than rebuilding on the
    deployment host

------------------------------------------------------------------------

## 📌 Notes

-   PostgreSQL is the source of truth
-   Audit logs are stored in PostgreSQL JSONB and exposed through
    `GET /api/ServiceRequests/{id}/audit-logs`
-   Audit log views are not yet implemented in the frontend
-   The frontend currently consumes the API over HTTPS
-   A bootstrap admin account can be created from configuration when the
    application starts with an empty user store

------------------------------------------------------------------------

## 📈 Future Improvements

-   Audit log views in the frontend
-   Expanded dashboard reporting (trend views, charts, recent activity)
-   Broader UI coverage for role-specific workflows
-   Blue/green or rolling deployment strategy
-   Environment-specific image tagging and release promotion

------------------------------------------------------------------------

## 👤 Author

Jason

## Usage and Licensing Clarification

This project is developed as a **portfolio and demonstration project** to showcase software engineering practices, including full-stack development, system design, and deployment.

- It is **not an open-source project**
- It is **not used for any commercial purpose**
- It is **not deployed for real customers or business operations**
- It is **not provided as a service to third parties**

All components are used **strictly as internal implementation details** within the application.

This project is intended solely for:
- Learning
- Demonstration
- Technical evaluation

All rights reserved.

No commercial usage is intended or planned.
