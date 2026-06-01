# Backend Rules — Manga Publishing System

> ASP.NET Core 8+ · Clean Architecture · EF Core · SignalR · JWT

## 1. Kiến trúc: Clean Architecture (4 Layers)

### 1.1 Cấu trúc Solution
```
MangaPublishing.sln
├── src/
│   ├── MangaPublishing.Domain/          # Core Domain Layer
│   ├── MangaPublishing.Application/     # Application/Use Case Layer
│   ├── MangaPublishing.Infrastructure/  # Infrastructure Layer
│   └── MangaPublishing.API/             # Presentation Layer
├── tests/
│   ├── MangaPublishing.Domain.Tests/
│   ├── MangaPublishing.Application.Tests/
│   ├── MangaPublishing.Infrastructure.Tests/
│   └── MangaPublishing.API.Tests/
```

### 1.2 Dependency Rules (Nghiêm ngặt)
- **Domain**: KHÔNG phụ thuộc vào bất kỳ layer nào khác. Không reference external packages.
- **Application**: Chỉ phụ thuộc vào Domain. Định nghĩa interfaces cho Infrastructure.
- **Infrastructure**: Phụ thuộc vào Application + Domain. Implement interfaces.
- **API**: Phụ thuộc vào Application. KHÔNG trực tiếp gọi Infrastructure.

## 2. Domain Layer

### 2.1 Entities — `Entities/`
- Kế thừa từ `BaseEntity` với `Id` (Guid), `CreatedAt`, `UpdatedAt`.
- Encapsulate business logic bên trong Entity methods.
- **KHÔNG** dùng public setters cho critical fields (balance, status).

```csharp
public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal SetupFundBalance { get; private set; }
    public decimal WithdrawableBalance { get; private set; }
    public decimal LockedFund { get; private set; }
    public decimal LockedWithdrawable { get; private set; }

    // F03: Ưu tiên SetupFundBalance trước → thiếu ghép WithdrawableBalance
    public void LockFund(decimal amount, out decimal lockedFromSetup, out decimal lockedFromWithdrawable)
    {
        // Business logic...
    }
}
```

### 2.2 Enums — `Enums/`

```csharp
public enum UserStatus { Pending, Active, Inactive, Banned }
public enum SeriesStatus { Draft, Pending_Approval, Board_Approved, In_Production, Published, Cancelled }
public enum TaskStatus { Pending, In_Progress, Revision, Approved, Cancelled, Disputed, Closed }
public enum TransactionType { Lock, Unlock, Transfer, Funding, Genkouryo, Deposit, Withdraw }
public enum AnnotationType { Technical_Error, Art_Error, Content_Error }
public enum AnnotationTargetType { Page, TaskVersion }
public enum DisputeResolution { Refund_100, Pay_100, Partial }
public enum VoteType { Approve, Reject }
public enum PublicationSchedule { Weekly, Monthly }
public enum TaskVersionStatus { Pending_Review, Approved, Rejected }
public enum NotificationType { System, Task_Update, Transaction, Ranking_Alert }
```

### 2.3 Repository Interfaces — `Interfaces/`
- Naming: `I{EntityName}Repository`.
- Generic base: `IRepository<T>` cho CRUD cơ bản.

### 2.4 Exceptions — `Exceptions/`
- Custom exceptions cụ thể cho nghiệp vụ:
  - `InsufficientBalanceException` — Không đủ số dư
  - `TaskOverdueException` — Task quá hạn
  - `UnauthorizedAccessException` — Truy cập không phép
  - `DuplicateExtensionRequestException` — T08: Đã xin gia hạn rồi
  - `InvalidTaskStateException` — Chuyển trạng thái không hợp lệ

## 3. Application Layer

### 3.1 CQRS Pattern (khuyến nghị dùng MediatR)

```
Application/
├── Commands/
│   ├── Tasks/
│   │   ├── CreateTaskCommand.cs
│   │   └── CreateTaskCommandHandler.cs
│   ├── Wallet/
│   │   ├── LockFundCommand.cs
│   │   └── LockFundCommandHandler.cs
│   └── ...
├── Queries/
│   ├── Tasks/
│   │   ├── GetMyTasksQuery.cs
│   │   └── GetMyTasksQueryHandler.cs
│   └── ...
```

### 3.2 DTOs

```
DTOs/
├── Requests/
│   ├── CreateSeriesRequest.cs
│   ├── CreateTaskRequest.cs
│   ├── SubmitTaskVersionRequest.cs
│   └── ...
├── Responses/
│   ├── SeriesResponse.cs
│   ├── TaskResponse.cs
│   ├── WalletDashboardResponse.cs
│   └── ...
```

- Naming: `{Action}{Entity}Request`, `{Entity}Response`.

```csharp
public class CreateTaskRequest
{
    public Guid RegionId { get; set; }
    public Guid AssistantId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public DateTime Deadline { get; set; }
    public int ZIndexOrder { get; set; }
}
```

### 3.3 Validators — FluentValidation
- Mỗi Request DTO có 1 Validator class.
- Naming: `{RequestName}Validator`.
- Đặt trong `Validators/`.

```csharp
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.PaymentAmount).GreaterThan(0).WithMessage("Tiền công phải lớn hơn 0.");
        RuleFor(x => x.Deadline).GreaterThan(DateTime.UtcNow).WithMessage("Deadline phải là tương lai.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ZIndexOrder).GreaterThanOrEqualTo(0);
    }
}
```

### 3.4 Service Interfaces — `Interfaces/`
- `IWalletService` — Lock / Transfer / Unlock / Funding / Genkouryo
- `ITaskService` — Quản lý vòng đời Task (create, assign, approve, revision, cancel, dispute)
- `ICompositeService` — Gộp ảnh (ImageSharp, Alpha Blending, Z-Index)
- `INotificationService` — Gửi thông báo realtime (SignalR)
- `IVNPayService` — Tích hợp VNPay Sandbox (Deposit/Withdraw)
- `IFileStorageService` — Upload/Download files (Firebase/MinIO)
- `IChapterService` — Quản lý chapters, QC, Genkoūryō calculation

### 3.5 AutoMapper Profiles — `Mappings/`
- 1 Profile per feature area: `TaskMappingProfile`, `WalletMappingProfile`, etc.

## 4. Infrastructure Layer

### 4.1 EF Core
- DbContext: `MangaPublishingDbContext`
- Entity configs: `Configurations/{EntityName}Configuration.cs` (IEntityTypeConfiguration)
- Migrations folder: `Migrations/`
- Connection: SQL Server

### 4.2 Repositories — `Repositories/`
- Implement từ Domain interfaces.
- Generic Repository pattern cho CRUD cơ bản.
- Specific repositories cho complex queries.

### 4.3 External Services — `Services/`
- `VNPayService.cs` — VNPay Sandbox integration + Checksum Signature validation.
- `FirebaseStorageService.cs` — Upload/download files lên Firebase Storage.
- `MinIOStorageService.cs` — Local dev alternative.
- `RedisService.cs` — Caching (Ranking, Dashboard data).

### 4.4 SignalR Hubs — `Hubs/`
- `NotificationHub.cs` — Broadcast theo UserId hoặc Role.
- Events: `TaskStatusChanged`, `NewNotification`, `WalletUpdated`, `ChapterApproved`.
- Auto-fallback: WebSocket → SSE → Long Polling.

### 4.5 Background Jobs — `BackgroundJobs/`
- `AutoCompositeJob` — G01: Gộp ảnh khi tất cả Region trên trang hoàn thành.
- `AutoApproveJob` — T04: Auto-approve sau 3 ngày Mangaka không phản hồi.
- `AutoCleanupJob` — T03a: Hủy Task overdue 3 ngày, Unlock tiền.
- `ProfileUpdateJob` — T09: Cập nhật AssistantProfile stats.
- Sử dụng Hangfire hoặc BackgroundService tùy scale.

## 5. API / Presentation Layer

### 5.1 Controllers
- Naming: `{Feature}Controller`.
- Attribute: `[ApiController]`.
- Route template: `api/[controller]`.
- Authorization per Role:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Mangaka")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> CreateTask(
        CreateTaskRequest request, CancellationToken ct) { }

    [HttpGet]
    [Authorize(Roles = "Assistant")]
    public async Task<ActionResult<ApiResponse<List<TaskResponse>>>> GetMyTasks(
        CancellationToken ct) { }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Mangaka")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> ApproveTask(
        Guid id, CancellationToken ct) { }
}
```

### 5.2 Key Controllers
| Controller | Role chính | Mô tả |
|-----------|-----------|-------|
| `AuthController` | All | Login, Register (chỉ Assistant), Refresh Token |
| `UsersController` | Admin | CRUD users, phê duyệt tài khoản |
| `SeriesController` | Mangaka, Editor | CRUD series, submit xét duyệt |
| `ChaptersController` | Mangaka, Editor | Upload, submit, approve chapters |
| `PagesController` | Mangaka | Upload trang truyện |
| `RegionsController` | Mangaka | Tạo/quản lý vùng khoanh |
| `TasksController` | Mangaka, Assistant | CRUD tasks, approve, revision, dispute |
| `TaskVersionsController` | Assistant | Upload kết quả nộp bài |
| `AnnotationsController` | Editor, Mangaka | Tạo/xem annotations bắt lỗi |
| `WalletsController` | Mangaka, Assistant | Xem số dư, lịch sử giao dịch |
| `PaymentsController` | Mangaka, Assistant | VNPay deposit/withdraw |
| `VotesController` | Board | Board voting (Approve/Reject series) |
| `RankingsController` | Board, Mangaka | Nhập/xem ranking data |
| `NotificationsController` | All | Xem thông báo |
| `ContractsController` | Admin | Quản lý hợp đồng, phụ lục |
| `ReportsController` | All | Báo cáo vi phạm |
| `DisputesController` | Editor | Phân xử tranh chấp |

### 5.3 Middleware
- **ExceptionHandlingMiddleware** — Trả về chuẩn ProblemDetails (RFC 7807).
- **RequestLoggingMiddleware** — Log request/response với Serilog.
- **JWT Authentication Middleware** — ASP.NET Core built-in.

### 5.4 API Response Format chuẩn
```json
{
  "success": true,
  "data": { },
  "message": "Operation completed successfully.",
  "errors": []
}
```

## 6. Security

- Mọi endpoint phải có `[Authorize]` attribute (trừ Login/Register).
- Password hash bằng BCrypt hoặc ASP.NET Core Identity.
- JWT payload: `{ "sub": "user-guid", "role": "Mangaka", "exp": "..." }`.
- VNPay giao tiếp phải check **Checksum Signature**.
- Chống IDOR: luôn filter theo `UserId` từ JWT claims, không tin client input.

## 7. Wallet & Transaction — CRITICAL RULES

> ⚠️ Mọi thao tác Wallet **BẮT BUỘC** bọc trong `DbTransaction` (ACID).

- **Lock**: Trừ `SetupFundBalance` trước → thiếu thì trừ `WithdrawableBalance`.
- **Unlock**: Đọc giao dịch Lock gốc → hoàn trả chính xác vào 2 ngăn.
- **Transfer**: Chuyển từ Locked funds → Ví Assistant.
- **Funding**: Hệ thống cấp vốn → `SetupFundBalance` Mangaka.
- **Genkouryo**: Hệ thống trả nhuận bút → `WithdrawableBalance` Mangaka.
- **Deposit/Withdraw**: VNPay Sandbox, phải có `ReferenceCode`.
- **KHÔNG BAO GIỜ** cho phép số dư âm.
- Mọi Transaction phải ghi log: `TransactionId`, `WalletId`, `Amount`, `Type`, `ReferenceId`, `ReferenceCode`, `Timestamp`.

## 8. Logging — Serilog

- **Information**: Business events (Task created, Chapter approved, Funding disbursed).
- **Warning**: Anomalies (Auto-approve triggered, Low ranking alert).
- **Error**: Exceptions, failed transactions.
- Financial transactions **PHẢI** log structured data đầy đủ để kiểm toán.

```csharp
_logger.LogInformation("Wallet Lock executed. TransactionId={TransactionId}, WalletId={WalletId}, Amount={Amount}, TaskId={TaskId}",
    transaction.Id, wallet.Id, amount, taskId);
```

## 9. Testing

- Framework: **xUnit** + Moq + FluentAssertions.
- Test naming: `{Method}_{Scenario}_{Expected}`.
- Unit test bắt buộc cho:
  - Domain logic (đặc biệt Wallet operations: Lock, Unlock, Transfer).
  - Validators.
  - Command/Query handlers.
- Integration test cho API endpoints.

## 10. Coding Conventions

- `async/await` cho tất cả I/O operations.
- Nullable reference types **enabled**.
- File-scoped namespaces.
- `CancellationToken` propagation qua tất cả async methods.
- Không dùng magic strings — sử dụng constants hoặc enums.
- Mỗi file chỉ chứa 1 class/interface/enum.
- Regions **KHÔNG** dùng trong code.
- XML doc comments cho public APIs.
