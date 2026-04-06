# Frontend - Backoffice Service Portal

Angular frontend for the **Backoffice Service Portal**. This module
provides the authenticated client application for logging in and working
with service requests through the backend API.

> It is **not an open-source project**, and all rights are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Login page backed by the API authentication endpoint
-   Protected routing via an authentication guard
-   HTTP interceptor that attaches bearer tokens and handles `401`
    responses
-   Service requests page for listing, creating, editing, and deleting
    records
-   Search, status filtering, sorting, and client-side pagination in the
    request workspace
-   Environment-based API configuration
-   Separation between pages, services, guards, interceptors, and models
-   API error handling for login and service request operations

------------------------------------------------------------------------

## 🧱 Architecture

-   **Framework**: Angular
-   **Application Type**: Single Page Application (SPA)
-   **Routing**: Login route and guarded service request route
-   **API Communication**: Angular `HttpClient`
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
│   │   │   └── service-request.model.ts
│   │   ├── pages/
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

## 🔌 Backend Dependency

This frontend depends on the backend API being available.

Expected endpoints:

-   `POST /api/Auth/login`
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
    page loads
-   Authentication state is maintained with token expiry checks in local
    storage
-   API requests are centralized in dedicated services
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
-   Audit log views for individual service requests
-   Additional test coverage

------------------------------------------------------------------------

## 👤 Author

Jason
