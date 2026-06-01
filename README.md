<p align="center">
  <img src="../diagrams/Context Diagram - English.png" alt="Manga Publishing System" width="600"/>
</p>

<h1 align="center">Manga Publishing System — Backend API</h1>

<p align="center">
  <strong>ASP.NET Core 8 · Clean Architecture · EF Core · SignalR · JWT</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/EF_Core-8.0-512BD4?logo=dotnet&logoColor=white" alt="EF Core"/>
  <img src="https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white" alt="SQL Server"/>
  <img src="https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis&logoColor=white" alt="Redis"/>
  <img src="https://img.shields.io/badge/SignalR-Real--time-512BD4?logo=dotnet&logoColor=white" alt="SignalR"/>
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white" alt="Docker"/>
</p>

---

## 📋 Giới thiệu

Backend API cho **Manga Creation Workflows & Publishing Management System** — Nền tảng B2B Digital Workspace giúp Nhà Xuất Bản manga quản lý quy trình từ **sáng tác → phân công → xét duyệt → xuất bản**.

> ⚠️ Đây **KHÔNG PHẢI** trang web đọc truyện (B2C). Đây là hệ thống quản lý nội bộ NXB.

## 🏗️ Kiến trúc

Hệ thống tuân thủ **Clean Architecture** với 4 layers:

```
┌──────────────────────────────────────────────────┐
│                  API Layer                        │  ← Controllers, Middleware, Swagger
│  (MangaPublishing.API)                           │
├──────────────────────────────────────────────────┤
│              Application Layer                    │  ← CQRS, DTOs, Validators, Services
│  (MangaPublishing.Application)                   │
├──────────────────────────────────────────────────┤
│              Domain Layer (Core)                  │  ← Entities, Enums, Interfaces
│  (MangaPublishing.Domain)                        │
├──────────────────────────────────────────────────┤
│            Infrastructure Layer                   │  ← EF Core, SignalR, VNPay, Storage
│  (MangaPublishing.Infrastructure)                │
└──────────────────────────────────────────────────┘

Dependency: API → Application → Domain ← Infrastructure
```

## 📁 Cấu trúc Solution

```
backend/
├── src/
│   ├── MangaPublishing.Domain/              # Core Domain
│   │   ├── Entities/                        # User, Series, Task, Wallet, ...
│   │   ├── Enums/                           # UserStatus, TaskStatus, TransactionType, ...
│   │   ├── Interfaces/                      # IRepository<T>, IUnitOfWork
│   │   └── Exceptions/                      # InsufficientBalanceException, ...
│   │
│   ├── MangaPublishing.Application/         # Use Cases
│   │   ├── Commands/                        # CQRS Commands (MediatR)
│   │   ├── Queries/                         # CQRS Queries
│   │   ├── DTOs/                            # Request/Response DTOs
│   │   ├── Validators/                      # FluentValidation
│   │   ├── Interfaces/                      # IWalletService, ITaskService, ...
│   │   └── Mappings/                        # AutoMapper Profiles
│   │
│   ├── MangaPublishing.Infrastructure/      # Implementations
│   │   ├── Data/                            # DbContext, Configurations, Migrations
│   │   ├── Repositories/                    # Repository implementations
│   │   ├── Services/                        # VNPay, Firebase, Redis
│   │   ├── Hubs/                            # SignalR NotificationHub
│   │   └── BackgroundJobs/                  # Auto-Approve, Auto-Cleanup, Composite
│   │
│   └── MangaPublishing.API/                 # Presentation
│       ├── Controllers/                     # REST API Controllers
│       ├── Middleware/                       # Exception, Logging, Auth
│       └── Program.cs                       # Entry point + DI
│
└── tests/
    ├── MangaPublishing.Domain.Tests/
    ├── MangaPublishing.Application.Tests/
    └── MangaPublishing.API.Tests/
```

## 🔑 Modules & API Endpoints

| Module | Endpoints | Roles |
|--------|-----------|-------|
| **Auth** | `POST /api/auth/login` · `POST /api/auth/register` · `POST /api/auth/refresh` | All |
| **Users** | `GET/POST/PUT /api/users` · `PUT /api/users/{id}/approve` | Admin |
| **Series** | `GET/POST/PUT /api/series` · `POST /api/series/{id}/submit` | Mangaka, Editor |
| **Chapters** | `GET/POST /api/chapters` · `PUT /api/chapters/{id}/approve` | Mangaka, Editor |
| **Pages** | `POST /api/pages/upload` | Mangaka |
| **Regions** | `GET/POST/PUT/DELETE /api/regions` | Mangaka |
| **Tasks** | `GET/POST /api/tasks` · `PUT /api/tasks/{id}/approve` · `PUT /api/tasks/{id}/revision` | Mangaka, Assistant |
| **TaskVersions** | `POST /api/taskversions` | Assistant |
| **Annotations** | `GET/POST/DELETE /api/annotations` | Editor, Mangaka |
| **Wallets** | `GET /api/wallets/me` · `GET /api/wallets/transactions` | Mangaka, Assistant |
| **Payments** | `POST /api/payments/deposit` · `POST /api/payments/withdraw` | Mangaka, Assistant |
| **Votes** | `POST /api/votes` | Board |
| **Rankings** | `GET/POST /api/rankings` | Board, Mangaka |
| **Contracts** | `GET/POST/PUT /api/contracts` · `POST /api/contracts/{id}/addendum` | Admin |
| **Disputes** | `POST /api/disputes/{taskId}/resolve` | Editor |
| **Notifications** | `GET /api/notifications` · `PUT /api/notifications/{id}/read` | All |
| **Reports** | `GET/POST /api/reports` | All |

## 💰 Wallet System (Critical)

Hệ thống Ví nội bộ với **4 ngăn quỹ**:

```
Wallet
├── SetupFundBalance       ← Vốn sản xuất (Board cấp)
├── WithdrawableBalance    ← Quỹ khả dụng (Nhuận bút + tự nạp)
├── LockedFund             ← Tiền khóa từ Quỹ sản xuất
└── LockedWithdrawable     ← Tiền khóa từ Quỹ khả dụng
```

**7 loại Transaction:**

| Type | Mô tả | Trigger |
|------|--------|---------|
| `Lock` | Khóa tiền khi tạo Task | Mangaka tạo Task |
| `Unlock` | Mở khóa hoàn tiền | Cancel Task |
| `Transfer` | Chuyển tiền cho Assistant | Approve Task |
| `Funding` | Cấp vốn sản xuất | Board Approve + Accept Fund |
| `Genkouryo` | Trả nhuận bút | Editor Approve Chapter |
| `Deposit` | Nạp tiền (VNPay) | Mangaka nạp |
| `Withdraw` | Rút tiền (VNPay) | Mangaka/Assistant rút |

> ⚠️ Tất cả wallet operations **BẮT BUỘC** bọc trong DB Transaction (ACID).

## 🔐 Phân quyền (RBAC)

| Role | Mô tả | Truy cập |
|------|--------|----------|
| **System Admin** | Quản trị IT | Admin cấp |
| **Tantou Editor** | Biên tập viên | Admin cấp |
| **Editorial Board** | Hội đồng | Admin cấp |
| **Mangaka** | Tác giả manga | Admin cấp (sau MOU) |
| **Assistant** | Trợ lý vẽ | **Tự đăng ký** (cần Admin approve) |

JWT Payload:
```json
{
  "sub": "user-guid-id",
  "role": "Mangaka",
  "exp": "..."
}
```

## ⚙️ Tech Stack

| Công nghệ | Mục đích |
|-----------|----------|
| ASP.NET Core 8 | Web API Framework |
| EF Core 8 | ORM + Migrations |
| SQL Server | Database chính |
| Redis | Cache (Ranking, Dashboard) |
| SignalR | Real-time Notifications |
| JWT Bearer | Authentication |
| AutoMapper | DTO Mapping |
| FluentValidation | Input Validation |
| ImageSharp | Image Compositing (Alpha Blending) |
| Serilog | Structured Logging |
| VNPay SDK | Payment Gateway (Sandbox) |
| xUnit + Moq | Unit Testing |
| Swagger/OpenAPI | API Documentation |

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) hoặc Docker
- [Redis](https://redis.io/) (optional, cho caching)

### Chạy với Docker

```bash
# Start SQL Server + Redis + MinIO
docker-compose up -d

# Apply migrations
cd src/MangaPublishing.API
dotnet ef database update --project ../MangaPublishing.Infrastructure

# Run API
dotnet run
```

### Chạy local

```bash
# 1. Clone repo
git clone <repo-url>
cd backend

# 2. Restore packages
dotnet restore

# 3. Update connection string trong appsettings.Development.json

# 4. Apply migrations
dotnet ef database update --startup-project src/MangaPublishing.API --project src/MangaPublishing.Infrastructure

# 5. Run
cd src/MangaPublishing.API
dotnet run

# API: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### Chạy Tests

```bash
dotnet test
```

## 📖 Tài liệu tham khảo

- [Manga.md](../Manga.md) — Tài liệu tổng quan dự án
- [GEMINI.md](./GEMINI.md) — Backend coding rules & conventions
- [design.md](../design.md) — Design System
- [Diagrams](../diagrams/) — ERD, Context Diagram, Swimlane Flows

## 👥 Team

| Thành viên | MSSV | Phụ trách |
|-----------|------|-----------|
| Nguyễn Phạm Xuân Sơn | SE183201 | Infra, Auth, User, Contract |
| Phạm Lê Hoàng Phúc | SE183189 | Series, Chapter, Voting, Ranking |
| Nguyễn Phạm Thiên Bảo | SE183336 | Task, Canvas, Region, TaskVersion |
| Trần Duy Anh | SE190675 | Wallet, Transaction, VNPay, Dispute |
| Lê Trung Kiên | SE193179 | Notification, SignalR, Background Jobs |