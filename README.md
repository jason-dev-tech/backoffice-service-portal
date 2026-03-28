# Backoffice Service Portal API

A production-style backend service built with **.NET 8**, demonstrating a clean architecture approach with a dual-database design using **PostgreSQL** for primary data and **MongoDB** for audit logging.

> ⚠️ This repository is provided for **demonstration and portfolio purposes only**. It is **not an open-source project**, and all rights are reserved.

---

## 🚀 Features

- Full CRUD operations for Service Requests
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

- **Primary Data Store**: PostgreSQL  
- **Audit Logging**: MongoDB  
- **Backend Framework**: ASP.NET Core Web API  
- **ORM**: Entity Framework Core  
- **Architecture Pattern**: Controller → Service → DbContext  
- **API Design**: DTO-based contract separation  
- **Validation**: DataAnnotations + centralized error handling  
- **Containerization**: Docker  

---

## 📦 Tech Stack

- .NET 8
- ASP.NET Core Web API
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

## 🧪 Sample Request

```json
{
  "title": "Printer issue",
  "description": "The office printer is showing a paper jam error and cannot print.",
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

Sensitive configuration values are managed using **.NET User Secrets** and are not stored in the repository.

Required configuration keys:

- `MongoDbSettings:ConnectionString`
- `MongoDbSettings:DatabaseName`
- `MongoDbSettings:AuditLogsCollectionName`

Example:

```bash
dotnet user-secrets set "MongoDbSettings:ConnectionString" "<your-mongodb-connection-string>"
dotnet user-secrets set "MongoDbSettings:DatabaseName" "<your-database-name>"
dotnet user-secrets set "MongoDbSettings:AuditLogsCollectionName" "<your-collection-name>"
```

---

## ▶️ Run the Application

```bash
dotnet build
dotnet run
```

Swagger UI:

```
http://localhost:<your-port>/swagger
```

---

## 💡 Key Design Highlights

- Clean separation of concerns (Controller / Service / Data)
- DTO layer prevents over-posting and entity exposure
- Centralized validation improves API consistency
- Dual-database architecture (SQL + NoSQL)
- Resilient logging (fail-safe MongoDB integration)
- Dependency Injection with interface-based services

---

## 📌 Notes

- MongoDB is used for audit logs only
- PostgreSQL remains the source of truth
- Audit logging does not impact primary operations

---

## 📈 Future Improvements

- Authentication & Authorization (JWT / Identity)
- Role-based access control (RBAC)
- FluentValidation integration
- Unit & integration testing
- Cloud deployment (Azure / AWS)
- CI/CD pipeline

---

## 👤 Author

Jason
