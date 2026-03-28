# Backoffice Service Portal API

A production-style backend service built with **ASP.NET Core Web API (.NET 8)** and an **Angular frontend**, demonstrating a full-stack architecture with a dual-database design:

- **PostgreSQL** for primary data
- **MongoDB** for audit logging

> ⚠️ This repository is provided for **demonstration and portfolio purposes only**. It is **not an open-source project**, and all rights are reserved.

---

## 🚀 Features

- Full CRUD operations for Service Requests
- Angular frontend consuming ASP.NET Core Web API
- DTO-based API design (no direct entity exposure)
- Service layer architecture (Controller → Service → Data)
- PostgreSQL (EF Core) for primary business data
- MongoDB for audit logging (Created, Updated, Deleted)
- Docker-based MongoDB setup
- Centralized validation handling
- Swagger API documentation
- Fail-safe logging (MongoDB failures do not break API)

---

## 🧱 Architecture

- **Frontend**: Angular (SPA)
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Architecture Pattern**: Controller → Service → DbContext
- **Primary Data Store**: PostgreSQL  
- **Audit Logging**: MongoDB  
- **API Design**: DTO-based contract separation  
- **Validation**: DataAnnotations + centralized error handling  

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

The Angular frontend uses environment-based configuration for API endpoints.

### Development Configuration

File:

```
frontend/src/environments/environment.ts
```

Example:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:<your-port>'
};
```

### ⚠️ Important

- Replace `<your-port>` with the actual port of your ASP.NET Core Web API.
- You can find the port after running:

```bash
dotnet run
```

Example output:

```
Now listening on: https://localhost:<your-port>
```

Then update:

```ts
apiBaseUrl: 'https://localhost:<your-port>'
```

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

- `MongoDbSettings:ConnectionString`
- `MongoDbSettings:DatabaseName`
- `MongoDbSettings:AuditLogsCollectionName`

---

## ▶️ Run the Application

### Backend

```bash
dotnet run
```

### Frontend

```bash
cd frontend
ng serve
```

Open:

```
http://localhost:<your-port>
```

---

## 💡 Key Design Highlights

- Clean separation of concerns (Controller / Service / Data)
- DTO layer prevents over-posting and entity exposure
- Centralized validation improves API consistency
- Dual-database architecture (SQL + NoSQL)
- Resilient logging (fail-safe MongoDB integration)
- Full-stack integration (Angular + ASP.NET Core)

---

## 📌 Notes

- MongoDB is used for audit logs only
- PostgreSQL remains the source of truth
- Frontend requires manual API port configuration for local development

---

## 📈 Future Improvements

- Authentication & Authorization (JWT)
- Role-based access control
- FluentValidation integration
- Unit & integration testing
- Cloud deployment (Azure / AWS)
- CI/CD pipeline

---

## 👤 Author

Jason
