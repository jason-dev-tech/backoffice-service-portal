# Frontend - Backoffice Service Portal

Angular-based UI for the **Backoffice Service Portal**. This frontend
provides the authenticated user experience for the backoffice system and
consumes the ASP.NET Core Web API.

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
-   No frontend audit log view yet, even though the backend exposes
    audit history

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

### Local Development vs Deployed Runtime

-   In local development, the Angular app runs independently with
    `npm start` and uses the configured backend API base URL
-   In deployed environments, the Angular app is built into the backend
    Docker image and served by **ASP.NET Core** from `wwwroot`
-   In production, the frontend and API share the same host, with the
    SPA served at `/` and the API exposed under `/api/...`

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
-   The frontend reads `window.BACKOFFICE_API_BASE_URL` from
    `/runtime-config.js` when that value is present
-   The production Docker build and Playwright config generate
    `public/runtime-config.js` for their respective runtime targets
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

Backend capability not currently used by this frontend:

-   `GET /api/ServiceRequests/{id}/audit-logs`

Audit history is available from the backend API, including preserved
history for deleted service requests, but the current frontend does not
request or render that data.

------------------------------------------------------------------------

## ▶️ Run the Frontend

Install dependencies:

``` bash
npm install
```

Start development server:

``` bash
npm start
```

Open:

    http://localhost:<your-frontend-port>

This standalone Angular dev server flow is for local development only.
In deployment, the compiled frontend is not hosted by `ng serve`; it is
served by the ASP.NET Core application from the backend container.

For Playwright E2E, start the frontend separately. The Playwright config
generates the runtime config file from `BACKOFFICE_API_BASE_URL` when
the test suite starts.

------------------------------------------------------------------------

## 🎭 Playwright E2E

Current Playwright E2E coverage includes:

-   viewer, operator, and admin role-based action visibility
-   authenticated post-login readiness before protected-page assertions
-   operator-authorized service request creation through the API with
    frontend list verification
-   admin delete service request flow against a test-created record

E2E runs are intended to use an isolated PostgreSQL service
from the repository root `docker-compose.e2e.yml`. The backend applies
EF Core migrations on startup for fresh E2E databases and supports
bootstrap users for `Admin`, `Operator`, and `Viewer`. The suite runs
serially with one Playwright worker.

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

-   Local development defaults to `https://localhost:7179`, while E2E
    and deployment can override the API base URL at runtime
-   Uses environment fallback plus runtime-config override for API base
    URL selection
-   Depends on the backend for authentication and request data
-   In production, the frontend is delivered by ASP.NET Core from
    `wwwroot` on the same host as the API

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

- It is **not an open-source project**
- It is **not used for any commercial purpose**
- It is **not deployed for real customers or business operations**
- It is **not provided as a service to third parties**

All components are used **strictly as internal implementation details** within the application.

This project is intended for demonstration purposes only.

All rights reserved.

No commercial usage is intended or planned.
