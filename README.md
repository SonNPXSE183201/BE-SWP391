# HƯỚNG DẪN CODEBASE & CẨM NANG PHÁT TRIỂN (DEVELOPER GUIDE)
## HỆ THỐNG QUẢN LÝ QUY TRÌNH SÁNG TÁC & XUẤT BẢN TRUYỆN TRANH (MANGA PUBLISHING SYSTEM)

Chào mừng bạn đến với mã nguồn Backend của dự án **Manga Creation Workflow & Publishing Management System (MCWPMS)**. Tài liệu này cung cấp cái nhìn chi tiết về công nghệ, kiến trúc Clean Architecture được áp dụng và cẩm nang hướng dẫn lập trình viên phát triển tính năng mới một cách chuẩn hóa.

---

## 1. CÔNG NGHỆ & KIẾN TRÚC SỬ DỤNG
* **Kiến trúc chính**: Modular Monolith định hướng Microservices.
* **Cổng vào tập trung**: API Gateway chạy **Ocelot** (cổng **5000**).
* **Dịch vụ chính**: ASP.NET Core Web API (.NET 8/10) chạy Clean Architecture 4 tầng (cổng **5010**).
* **Công nghệ tích hợp**:
  * **Entity Framework Core (SQL Server)**: Quản lý truy xuất dữ liệu quan hệ.
  * **Redis**: Bộ nhớ đệm và lưu trữ session/bảng xếp hạng.
  * **FluentValidation**: Tự động xác thực dữ liệu đầu vào.
  * **SignalR**: Truyền tải thông báo thời gian thực.
  * **BuildingBlocks**: Thư viện dùng chung quản lý xác thực JWT Bearer, CORS và xử lý lỗi tập trung.

---

## 2. QUY ĐỊNH CẤU TRÚC LAYER TRONG CLEAN ARCHITECTURE

Dự án nghiệp vụ chính `Services/MangaPublishingSystem` được chia nhỏ thành 4 Layer độc lập:

### 2.1. Domain Layer (`MangaPublishingSystem.Domain`)
* **Trách nhiệm**: Chứa các thực thể nghiệp vụ cốt lõi, giá trị và logic miền nghiệp vụ.
* **Thư mục cho phép**:
  * `Entities/`: Các lớp thực thể kế thừa từ `BaseEntity` (ví dụ: `User.cs`, `Task.cs`, `Chapter.cs`).
  * `Enums/`: Các kiểu liệt kê của hệ thống (ví dụ: `Role.cs`, `TaskStatus.cs`).
  * `ValueObjects/`: Các đối tượng giá trị không có định danh độc lập.
  * `Exceptions/`: Các Exception nghiệp vụ thuần của miền Domain.
* **Quy tắc**: **Tuyệt đối không tham chiếu thư viện ngoài hoặc các tầng khác.**

### 2.2. Application Layer (`MangaPublishingSystem.Application`)
* **Trách nhiệm**: Định nghĩa các Use Case, DTOs truyền nhận dữ liệu, quy tắc kiểm tra đầu vào và điều phối luồng nghiệp vụ.
* **Thư mục cho phép**:
  * `DTOs/`: Chứa các lớp Data Transfer Object (Ví dụ: `CreateTaskDto.cs`, `TaskDto.cs`).
  * `Validations/`: Chứa các bộ Validator kế thừa từ `AbstractValidator<T>` của FluentValidation để xác thực DTO.
  * `IRepositories/`: Định nghĩa Interface truy xuất dữ liệu (ví dụ: `ITaskRepository.cs`) và `IUnitOfWork.cs`.
  * `IServices/`: Định nghĩa Interface của Service nghiệp vụ điều khiển Use Case (ví dụ: `ITaskService.cs`).
  * `Services/`: Hiện thực hóa của Service Interface (chứa logic nghiệp vụ chính của hệ thống).
* **Quy tắc**: Không truy cập trực tiếp DB, mọi tương tác dữ liệu phải thông qua các interface trong `IRepositories`.

### 2.3. Infrastructure Layer (`MangaPublishingSystem.Infrastructure`)
* **Trách nhiệm**: Hiện thực hóa các giao tiếp ngoại vi (Cơ sở dữ liệu, bên thứ ba VNPay Sandbox, File Storage).
* **Thư mục cho phép**:
  * `Data/`: `MangaPublishingDbContext` cấu hình DB, các lớp `IEntityTypeConfiguration<T>` cấu hình Fluent API.
  * `Repositories/`: Hiện thực cụ thể của các Repository Interface (ví dụ: `TaskRepository.cs`) và `UnitOfWork.cs`.
  * `Services/`: Hiện thực các dịch vụ ngoại vi (ví dụ: `FirebaseStorageService`, `VnPayPaymentService`).

### 2.4. Presentation Layer (`MangaPublishingSystem.Presentation`)
* **Trách nhiệm**: Expose API ra bên ngoài, xử lý yêu cầu HTTP và đăng ký DI.
* **Thư mục cho phép**:
  * `Controllers/`: Chứa các Controller tiếp nhận HTTP request.
  * `Extensions/`: Các phương thức đăng ký dịch vụ (`DependencyInjectionExtensions.cs` để đăng ký DI cho Repo/Service, `InfrastructureExtensions.cs` để đăng ký DbContext).
  * `Program.cs`, `appsettings.json`, `launchSettings.json`.

---

## 3. DEVELOPER GUIDE: QUY TRÌNH THÊM MỘT TÍNH NĂNG / API MỚI

Khi bạn (hoặc AI) cần tạo một API chức năng mới (Ví dụ: Tạo nhiệm vụ cho Trợ lý - `Create Task`), bắt buộc phải làm theo thứ tự 6 bước nghiêm ngặt dưới đây:

### Bước 1: Khai báo Entity ở tầng Domain
* Tạo tệp `Task.cs` trong `MangaPublishingSystem.Domain/Entities/`.
* Lớp này kế thừa từ `BaseEntity` và chứa các thuộc tính nghiệp vụ.
* Tạo các Enum cần thiết trong `Domain/Enums/`.

### Bước 2: Tạo DTOs và Validator ở tầng Application
* Tạo tệp `CreateTaskDto.cs` và `TaskDto.cs` trong `MangaPublishingSystem.Application/DTOs/Task/`.
* Tạo trình xác thực dữ liệu đầu vào `CreateTaskDtoValidator.cs` kế thừa `AbstractValidator<CreateTaskDto>` trong `MangaPublishingSystem.Application/Validations/Task/`. Định nghĩa các ràng buộc nghiệp vụ (không trống, độ dài tối đa,...).

### Bước 3: Định nghĩa Repository và Service Interface ở tầng Application
* Tạo `ITaskRepository.cs` trong `Application/IRepositories/` khai báo các hàm truy xuất dữ liệu đặc thù.
* Cập nhật `IUnitOfWork.cs` để bổ sung thuộc tính đăng ký repository: `ITaskRepository Task { get; }`.
* Tạo `ITaskService.cs` trong `Application/IServices/` và triển khai `TaskService.cs` trong `Application/Services/`. 
* *Lưu ý*: Lớp `TaskService` gọi các Repository thông qua `_unitOfWork.Task` và thực hiện lưu trữ qua `_unitOfWork.SaveChangesAsync()`. Sử dụng các ngoại lệ chuẩn của `BuildingBlocks.Exceptions` (như `NotFoundException`, `ConflictException`) để ném ra khi có lỗi.

### Bước 4: Triển khai Cực thể DB và Repository ở tầng Infrastructure
* Khai báo `public virtual DbSet<Task> Tasks { get; set; }` vào trong lớp `MangaPublishingDbContext` ở dự án `MangaPublishingSystem.Infrastructure`.
* Thêm tệp cấu hình quan hệ bảng `TaskConfiguration.cs` (nếu cần) vào `Infrastructure/Data/Configurations/`.
* Tạo tệp `TaskRepository.cs` kế thừa `ITaskRepository` trong `Infrastructure/Repositories/` để triển khai mã EF Core cụ thể.
* Cập nhật lớp `UnitOfWork.cs` ở tầng Infrastructure để tiêm và khởi tạo `TaskRepository`.

### Bước 5: Đăng ký Dependency Injection và Viết Controller ở Presentation
* Mở tệp [DependencyInjectionExtensions.cs](file:///d:/SWP391/Project/BE-SWP391/Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/Extensions/DependencyInjectionExtensions.cs) ở tầng Presentation và thêm các dòng đăng ký DI Scoped:
  ```csharp
  services.AddScoped<ITaskRepository, TaskRepository>();
  services.AddScoped<ITaskService, TaskService>();
  ```
* Tạo tệp `TasksController.cs` kế thừa `ControllerBase` trong `MangaPublishingSystem.Presentation/Controllers/`. Expose API bằng các Route thích hợp (ví dụ: `[Route("api/tasks")]`). Tiêm `ITaskService` vào constructor để xử lý yêu cầu.

### Bước 6: Định tuyến Gateway trong GatewayAPI
* Mở hai tệp cấu hình của GatewayAPI: [ocelot.json](file:///d:/SWP391/Project/BE-SWP391/GatewayAPI/ocelot.json) and [ocelot.Development.json](file:///d:/SWP391/Project/BE-SWP391/GatewayAPI/ocelot.Development.json).
* Bổ sung khối định tuyến định nghĩa upstream và downstream cho API mới của bạn để Client có thể gọi qua cổng **5000**.

---

## 4. CÁC LỆNH CHẠY DỰ ÁN THƯỜNG DÙNG
* **Biên dịch toàn bộ Solution**:
  ```powershell
  dotnet build MangaPublishingSystem.slnx
  ```
* **Chạy API dịch vụ chính (Manga Service)**:
  ```powershell
  dotnet run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
  ```
* **Chạy API Gateway**:
  ```powershell
  dotnet run --project GatewayAPI/GatewayAPI.csproj
  ```

---

## 5. HƯỚNG DẪN CẤU HÌNH & SỬ DỤNG CÁC DỊCH VỤ TÍCH HỢP

### 5.1. Cấu hình thông báo thời gian thực (SignalR)
* **Hub Endpoint**: `/hubs/notification` (Địa chỉ upstream qua Gateway: `/api/v1/hubs/notification`).
* **Định tuyến qua Gateway (Ocelot)**: 
  * Ocelot được cấu hình chia 2 route: Route bắt tay (HTTP Post/Options `/negotiate`) và Route kết nối chính (WebSockets `ws://`).
  * **Lưu ý về CORS**: Trình duyệt sẽ chặn kết nối nếu CORS để wildcard `*` khi dùng Credentials. Bắt buộc phải khai báo chính xác địa chỉ Client trong khóa `"Cors:AllowedOrigins"` tại tệp `appsettings.json`.

### 5.2. Công cụ tự động hóa Git Hooks (Husky.Net)
* **Mục tiêu**: Ngăn chặn commit/push mã nguồn gặp lỗi biên dịch.
* **Cấu hình Git Hooks**:
  * `pre-commit`: Tự động chạy lệnh `dotnet build MangaPublishingSystem.slnx` trước khi commit cục bộ.
  * `pre-push`: Tự động chạy lệnh `dotnet build MangaPublishingSystem.slnx` trước khi push lên GitHub.
* **Tự động hóa cho team**:
  * Lập trình viên mới chỉ cần clone mã nguồn về và **Build lần đầu tiên**, MSBuild Target được cài sẵn trong tệp [GatewayAPI.csproj](file:///d:/SWP391/Project/BE-SWP391/GatewayAPI/GatewayAPI.csproj) sẽ tự động khôi phục công cụ Husky và cài đặt Git Hooks ngầm xuống thư mục `.git` mà không cần bất kỳ thao tác thủ công nào.

### 5.3. Dịch vụ gửi Email (FluentEmail)
* **Cách hoạt động**: Tích hợp gửi email trực tiếp qua SMTP Server.
* **Cấu hình trong `appsettings.json`**:
  ```json
  "EmailSettings": {
    "DefaultFromEmail": "noreply@yourdomain.com",
    "DefaultFromName": "Manga Publishing System",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
  ```
* **Cách sử dụng**:
  * Tiêm `IFluentEmail` vào các Service cần gửi thông báo (ví dụ: ở lớp xử lý Onboarding) và gọi `SendAsync()` để thực hiện gửi mail.
