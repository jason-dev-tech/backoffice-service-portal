# Frontend - Backoffice Service Portal

An Angular frontend for the **Backoffice Service Portal**, designed to
work with an **ASP.NET Core Web API** backend and provide a complete
full-stack interface for managing service requests.

> ⚠️ This frontend is provided for **demonstration and portfolio
> purposes only**. It is **not an open-source project**, and all rights
> are reserved.

------------------------------------------------------------------------

## 🚀 Features

-   Full CRUD UI for Service Requests (Create, Read, Update, Delete)
-   Integrated with ASP.NET Core Web API backend
-   Reactive state management using **RxJS + AsyncPipe (zoneless
    Angular)**
-   Environment-based API configuration
-   Clean separation between UI, service layer, and models
-   Dynamic UI updates after create, update, and delete operations
-   Error handling for API operations

------------------------------------------------------------------------

## 🧱 Architecture

-   **Framework**: Angular
-   **Application Type**: Single Page Application (SPA)
-   **State Management**: RxJS + AsyncPipe (zoneless Angular pattern)
-   **API Communication**: Angular `HttpClient`
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
│   │   ├── models/
│   │   │   └── service-request.model.ts
│   │   ├── services/
│   │   │   └── service-request.service.ts
│   │   ├── app.ts
│   │   ├── app.html
│   │   ├── app.css
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

## ▶️ Run with Backend

1.  Start backend:

``` bash
cd BackofficeServicePortal.Api
dotnet run
```

2.  Update environment config:

```{=html}
<!-- -->
```
    src/environments/environment.ts

3.  Ensure backend CORS allows frontend origin

4.  Start frontend:

``` bash
ng serve
```

------------------------------------------------------------------------

## 💡 Implementation Highlights

-   Reactive UI using AsyncPipe (no manual subscriptions)
-   Zoneless Angular-compatible state updates
-   Centralized API service layer
-   Clean separation of concerns (UI / Service / Model)
-   Automatic list refresh after CRUD operations
-   Form reuse for Create and Edit workflows

------------------------------------------------------------------------

## 📌 Notes

-   Requires backend to run on HTTPS for proper integration
-   Uses environment-based configuration (no hardcoded URLs)
-   Designed for extensibility (routing, forms, validation)

------------------------------------------------------------------------

## 📈 Future Improvements

-   Form validation improvements (Reactive Forms / Validators)
-   User feedback (success/error notifications)
-   Routing for multi-page structure
-   Pagination and filtering
-   Authentication & authorization (JWT)
-   UI/UX improvements

------------------------------------------------------------------------

## 👤 Author

Jason
