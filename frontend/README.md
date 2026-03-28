# Frontend - Backoffice Service Portal

An Angular frontend for the **Backoffice Service Portal**, built to consume the **ASP.NET Core Web API** backend and provide a simple full-stack interface for managing service requests.

> ⚠️ This frontend is provided for **demonstration and portfolio purposes only**. It is **not an open-source project**, and all rights are reserved.

---

## 🚀 Features

- Angular frontend for the Backoffice Service Portal
- Retrieves service requests from the backend API
- Environment-based API configuration
- Structured foundation for future CRUD UI expansion
- Designed to integrate with an ASP.NET Core Web API backend

---

## 🧱 Architecture

- **Framework**: Angular
- **Application Type**: Single Page Application (SPA)
- **API Communication**: Angular `HttpClient`
- **Configuration**: Angular environment files
- **Backend Integration**: ASP.NET Core Web API

---

## 📦 Tech Stack

- Angular
- TypeScript
- HTML
- CSS
- RxJS

---

## 📁 Current Structure

```text
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

---

## 🌐 Environment Configuration

The frontend uses Angular environment files for backend API configuration.

File:

```text
src/environments/environment.ts
```

Example:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:<your-backend-port>'
};
```

### Important

Replace `<your-port>` with the actual port of your ASP.NET Core Web API before running the frontend.

The service layer uses this value to call the backend API.

---

## 🔌 Backend Dependency

This frontend depends on the backend API being available.

The backend project is expected to expose endpoints such as:

- `GET /api/ServiceRequests`
- `GET /api/ServiceRequests/{id}`
- `POST /api/ServiceRequests`
- `PUT /api/ServiceRequests/{id}`
- `DELETE /api/ServiceRequests/{id}`

---

## ▶️ Run the Frontend

Install dependencies:

```bash
npm install
```

Run the development server:

```bash
ng serve
```

Open in the browser:

```text
http://localhost:<your-frontend-port>
```

---

## ▶️ Run with Backend

Before starting the frontend:

1. Run the ASP.NET Core Web API backend
2. Update `src/environments/environment.ts` with the correct backend port
3. Ensure the backend CORS configuration allows the frontend origin

Then start the Angular frontend:

```bash
ng serve
```

---

## 💡 Current Implementation Highlights

- Type-safe frontend model for service requests
- Dedicated Angular service for API communication
- Environment-based API base URL configuration
- Prepared foundation for list, create, edit, and delete UI flows

---

## 📌 Notes

- This frontend currently focuses on backend integration and project structure
- API endpoint configuration is intentionally externalized through environment files
- Local development may require updating placeholder ports before running

---

## 📈 Future Improvements

- Service request list page UI refinement
- Create service request form
- Edit service request form
- Delete workflow
- Frontend validation and error handling improvements
- Routing for multiple pages
- Authentication and authorization integration

---

## 👤 Author

Jason
