# Backoffice Service Portal API

A production-style full-stack project built with **ASP.NET Core Web API (.NET 8)** and **Angular**, demonstrating a dual-database architecture with:

- **PostgreSQL** for primary business data
- **MongoDB** for audit logging

> ⚠️ This repository is provided for **demonstration and portfolio purposes only**. It is **not an open-source project**, and all rights are reserved.

---

## 🚀 Features

- Full CRUD operations for Service Requests
- Angular frontend consuming ASP.NET Core Web API
- DTO-based API design (no direct entity exposure)
- Service layer architecture (Controller → Service → Data)
- Centralized validation handling
- PostgreSQL (EF Core) for primary business data
- MongoDB for audit logging (Created, Updated, Deleted)
- Docker-based MongoDB setup
- Swagger API documentation
- Fail-safe logging (MongoDB failures do not break API)

---

## 🧱 Architecture

- **Frontend**: Angular SPA
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Architecture Pattern**: Controller → Service → DbContext
- **Primary Data Store**: PostgreSQL
- **Audit Logging**: MongoDB
- **API Design**: DTO-based contract separation
- **Validation**: DataAnnotations + centralized error handling
- **Cross-Origin Access**: CORS configured through application settings

---

## 📦 Tech Stack

- ASP.NET Core Web API (.NET 8)
- Angular
- Entity Framework Core
- PostgreSQL
- MongoDB
- Docker

---

## 📡 API Endpoints

### Service Requests

- `GET /api/ServiceRequests`
- `GET /api/ServiceRequests/{id}`
- `POST /api/ServiceRequests`
- `PUT /api/ServiceRequests/{id}`
- `DELETE /api/ServiceRequests/{id}`

### Audit Logs

- `GET /api/ServiceRequests/{id}/audit-logs`

---

## 🌐 Frontend Configuration

The Angular frontend uses environment-based configuration for the backend API URL.

File:

```text
frontend/src/environments/environment.ts
```

Example:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:<your-port>'
};
```

### Important

Replace `<your-port>` with the actual port of your ASP.NET Core Web API after running:

```bash
dotnet run
```

Example output:

```text
Now listening on: https://localhost:<your-port>
```

Then update the value in `environment.ts` accordingly.

---

## 🔒 Backend CORS Configuration

The backend reads allowed frontend origins from configuration instead of hardcoding them in `Program.cs`.

File:

```text
BackofficeServicePortal.Api/appsettings.json
```

Example:

```json
"AllowedOrigins": [
  "http://localhost:<your-frontend-port>"
]
```

### Important

Replace `<your-frontend-port>` with the actual frontend origin used during local development.

For example, if Angular runs on its default local port, update the value accordingly before testing frontend-to-backend integration.

---

## 🧪 Sample Request

```json
{
  "title": "Printer issue",
  "description": "The office printer is not working.",
  "requesterName": "Jason"
}
```

---

## 🧪 Sample Validation Error Response

```json
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

---

## 🐳 Running MongoDB (Docker)

```bash
docker run -d -p <host-port>:<container-port> --name mongodb mongo
```

---

## 🔐 Configuration

Sensitive configuration values are managed using **.NET User Secrets**.

Required keys:

- `ConnectionStrings:DefaultConnection`
- `MongoDbSettings:ConnectionString`
- `MongoDbSettings:DatabaseName`
- `MongoDbSettings:AuditLogsCollectionName`

---

## ▶️ Run the Application

### Backend

```bash
cd BackofficeServicePortal.Api
dotnet run
```

### Frontend

```bash
cd frontend
ng serve
```

Open the frontend in your browser:

```text
http://localhost:<your-frontend-port>
```

---

## 💡 Key Design Highlights

- Clean separation of concerns (Controller / Service / Data)
- DTO layer prevents over-posting and entity exposure
- Centralized validation improves API consistency
- Dual-database architecture (SQL + NoSQL)
- Resilient logging with fail-safe MongoDB integration
- Configuration-driven CORS policy
- Full-stack integration with Angular and ASP.NET Core Web API

---

## 📌 Notes

- MongoDB is used for audit logs only
- PostgreSQL remains the source of truth
- Frontend API base URL is configured through Angular environment files
- Backend allowed origins are configured through `appsettings.json`
- Local development requires updating placeholder ports before running frontend-backend integration

---

## 📈 Future Improvements

- Authentication & Authorization (JWT)
- Role-based access control
- FluentValidation integration
- Unit and integration testing
- Cloud deployment (Azure / AWS)
- CI/CD pipeline

---

## 👤 Author

Jason
