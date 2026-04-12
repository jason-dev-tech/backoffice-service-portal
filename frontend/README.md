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
-   Environment-based API configuration
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
-   **Configuration**: Angular environment files
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

The frontend uses Angular environment files for backend API
configuration.

File:

    src/environments/environment.ts

Example:

``` ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:<your-backend-port>'
};
```

### Important

-   Replace `<your-backend-port>` with the actual backend port
-   The frontend will not work unless this value matches the running
    backend

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
-   Uses environment-based configuration (no hardcoded URLs)
-   Depends on the backend for authentication and request data

------------------------------------------------------------------------

## 📈 Future Improvements

-   Frontend access control that reflects API roles in the UI
-   Expanded dashboard reporting (charts, trends, recent activity)
-   Audit log views for individual service requests
-   Additional test coverage

------------------------------------------------------------------------

## 👤 Author

Jason
