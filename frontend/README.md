# Frontend - Backoffice Service Portal

Angular-based UI for the **Backoffice Service Portal**. This frontend
provides the authenticated user experience for the backoffice system and
consumes the ASP.NET Core Web API.

> It is **not an open-source project**, and all rights are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Login page backed by the API authentication endpoint
-   Service request management UI for listing, creating, editing, and
    deleting records
-   Dashboard view with summary reporting and drill-down into the
    service request workspace
-   API-driven querying for service requests:
    filtering, keyword search, sorting (`createdAt`, `title`), and
    pagination
-   Multi-page navigation through an authenticated app shell
-   Protected routing via an authentication guard
-   HTTP interceptor that attaches bearer tokens and handles `401`
    responses
-   Environment-based API configuration with runtime API base URL
    override support
-   Separation between pages, services, guards, interceptors, and models
-   API error handling for login and service request operations

------------------------------------------------------------------------

## 🧱 Architecture

-   **Framework**: Angular standalone components
-   **Application Type**: Single Page Application (SPA)
-   **Routing**: Login route plus authenticated shell routes for
    dashboard and service requests
-   **Views**: Login page, guarded dashboard page, guarded service
    requests page
-   **State / Rendering**: RxJS streams with `AsyncPipe`
-   **API Communication**: Angular `HttpClient` through a dedicated
    service layer
-   **Authentication Flow**: Login → token storage → guarded requests
-   **Configuration**: Angular environment files plus runtime config
    script
-   **Backend Integration**: ASP.NET Core Web API

------------------------------------------------------------------------

## 📦 Tech Stack

-   Angular
-   TypeScript
-   HTML
-   CSS
-   RxJS

------------------------------------------------------------------------

## 📁 Project Structure

``` text
frontend/
├── src/
│   ├── app/
│   │   ├── guards/
│   │   │   └── auth.guard.ts
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts
│   │   ├── models/
│   │   │   ├── service-request-dashboard.model.ts
│   │   │   └── service-request.model.ts
│   │   ├── pages/
│   │   │   ├── dashboard/
│   │   │   ├── login/
│   │   │   └── service-requests/
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── service-request.service.ts
│   │   ├── app.ts
│   │   ├── app.html
│   │   ├── app.css
│   │   ├── app.routes.ts
│   │   └── app.config.ts
│   └── environments/
│       ├── environment.ts
│       └── environment.prod.ts
```

------------------------------------------------------------------------

## 🌐 Environment Configuration

The frontend uses Angular environment files with a runtime override for
backend API configuration.

File:

    src/environments/environment.ts

Runtime script:

    public/runtime-config.js

Example:

``` ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7179'
};
```

### Important

-   `environment.ts` / `environment.prod.ts` keep
    `https://localhost:7179` as the fallback API URL
-   `src/index.html` loads `/runtime-config.js` before Angular bootstraps
-   Playwright startup generates `public/runtime-config.js` so the
    frontend can target the isolated E2E backend without modifying
    tracked source files
-   `public/runtime-config.js` is a generated artifact and should not be
    committed

------------------------------------------------------------------------

## 🔌 API Integration

This frontend depends on the backend API being available.

All service request querying is handled by the backend. The frontend
sends query parameters for:

-   `status`
-   `search`
-   `sort`
-   `page`
-   `pageSize`

Expected endpoints:

-   `POST /api/Auth/login`
-   `GET /api/ServiceRequests/dashboard`
-   `GET /api/ServiceRequests`
-   `POST /api/ServiceRequests`
-   `PUT /api/ServiceRequests/{id}`
-   `DELETE /api/ServiceRequests/{id}`

------------------------------------------------------------------------

## ▶️ Run the Frontend

Install dependencies:

``` bash
npm install
```

Start development server:

``` bash
ng serve
```

Open:

    http://localhost:<your-frontend-port>

For Playwright E2E, the frontend is started by Playwright and the
runtime config file is generated before `ng serve` starts.

------------------------------------------------------------------------

## 🎭 Playwright E2E

Current Playwright E2E coverage includes:

-   role-based visibility
-   operator create service request flow
-   admin delete service request flow

E2E runs are intended to use isolated PostgreSQL and MongoDB services
from the repository root `docker-compose.e2e.yml`. The backend applies
EF Core migrations on startup for fresh E2E databases and supports
bootstrap users for `Admin`, `Operator`, and `Viewer`.

Minimal setup:

``` bash
export POSTGRES_PASSWORD=<your-e2e-postgres-password>
docker compose -f ../docker-compose.e2e.yml up -d
```

------------------------------------------------------------------------

## 💡 Implementation Highlights

-   Route-level access control is enforced before the service request
    and dashboard pages load
-   Authentication state is maintained with token expiry checks in local
    storage
-   API requests are centralized in dedicated services
-   Query-driven list behavior is delegated to the backend API rather
    than handled in-memory in the client
-   The dashboard reuses the service request domain to provide a compact
    reporting view
-   The service request page combines list management and record
    maintenance in a single workspace

------------------------------------------------------------------------

## 📌 Notes

-   Requires backend to run on HTTPS for proper integration
-   Uses environment fallback plus runtime-config override for API base
    URL selection
-   Depends on the backend for authentication and request data

------------------------------------------------------------------------

## 📈 Future Improvements

-   Expanded dashboard reporting (charts, trends, recent activity)
-   Audit log views for individual service requests
-   Additional test coverage

------------------------------------------------------------------------

## 👤 Author

Jason

## Usage and Licensing Clarification

This project is developed as a **portfolio and demonstration project** to showcase software engineering practices, including full-stack development, system design, and deployment.

- It is **not used for any commercial purpose**
- It is **not deployed for real customers or business operations**
- It is **not provided as a service to third parties**

All components, including MongoDB used for audit logging, are used **strictly as internal implementation details** within the application.

This project is intended solely for:
- Learning
- Demonstration
- Technical evaluation

No commercial usage is intended or planned.
