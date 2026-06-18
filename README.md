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

## 2. QUY ĐỊNH CẤU TRÚC LAYER & NGUYÊN TẮC TỔ CHỨC CODE (ĐẶC BIỆT QUAN TRỌNG)

Dự án áp dụng kiến trúc **Clean Architecture** kết hợp quy chuẩn **Feature Folders**. Lập trình viên **bắt buộc** phải tuân thủ việc viết đúng mã nguồn tại đúng nơi quy định, không viết lộn xộn.

### 2.1. Phân chia Layer & Trách nhiệm (Dev viết gì ở đâu?)

* **Tầng Domain (`MangaPublishingSystem.Domain`)**:
  * *Viết gì ở đây:* Thực thể nghiệp vụ (`Entities`), các kiểu liệt kê (`Enums`), đối tượng giá trị (`ValueObjects`), hoặc ngoại lệ nghiệp vụ lõi (`Exceptions`).
  * *Quy tắc:* **CẤM TUYỆT ĐỐI** tham chiếu Entity Framework, thư viện ngoài, hoặc các tầng khác.
* **Tầng Application (`MangaPublishingSystem.Application`)**:
  * *Viết gì ở đây:* Interfaces nghiệp vụ (`IServices`, `IRepositories`, `IUnitOfWork`), các lớp DTOs truyền nhận, các bộ kiểm tra đầu vào (FluentValidation) và các Service nghiệp vụ điều khiển Use Case.
  * *Quy tắc:* **Sạch hoàn toàn.** Cấm truy cập trực tiếp DB hoặc sử dụng các thư viện cụ thể của EF Core (`Microsoft.EntityFrameworkCore.Relational`). 
* **Tầng Infrastructure (`MangaPublishingSystem.Infrastructure`)**:
  * *Viết gì ở đây:* Cấu hình kết nối DB (`MangaPublishingDbContext`), cấu hình ánh xạ bảng Fluent API (`Configurations/`), hiện thực cụ thể của các Repository, và các dịch vụ bên thứ ba (VNPay Sandbox, Storage).
  * *Quy tắc:* Mọi helper truy vấn CSDL liên quan đến EF Core (ví dụ: `WhereContainsUnsigned`) phải nằm ở đây.
* **Tầng Presentation (`MangaPublishingSystem.Presentation`)**:
  * *Viết gì ở đây:* Controllers tiếp nhận HTTP request, các cấu hình khởi chạy (`Program.cs`, `appsettings.json`), và các hàm đăng ký DI (`Extensions/DependencyInjectionExtensions.cs`).
  * *Quy tắc:* Chỉ gọi xuống tầng Application, giữ Controller mỏng nhất có thể.

---

### 2.2. Quy tắc tổ chức Code theo thư mục tính năng (Feature Folders)

Để tránh mã nguồn bị lộn xộn và chồng chéo, lập trình viên **bắt buộc phải phân cụm mã nguồn theo thư mục tính năng** bên trong các thư mục lớn của `Application` và `Presentation`:

* **Cách cấu trúc đúng:**
  ```text
  MangaPublishingSystem.Application/
  ├── DTOs/
  │   ├── Chapter/         <-- Thư mục tính năng
  │   │   ├── ChapterDto.cs
  │   │   └── CreateChapterDto.cs
  │   └── Task/
  │       └── TaskDto.cs
  ├── Validations/
  │   └── Chapter/         <-- Thư mục tính năng
  │       └── CreateChapterDtoValidator.cs
  ```
* **CẤM TUYỆT ĐỐI:** Tạo các file DTO, Validator, Service, hay Controller trực tiếp dưới thư mục cha (`DTOs/`, `Validations/`, `Services/`, `Controllers/`) mà không nằm trong thư mục tính năng cụ thể.

---

### 2.3. Các cấu hình và Tiện ích dùng chung (Không tạo thêm, không viết trùng)

* **Tự động hóa Kiểm toán (Audit Fields - CreateAt/UpdateAt):**
  * Tất cả các thực thể kế thừa từ `BaseEntity` đều tự động có cột `CreateAt` và `UpdateAt`.
  * Lập trình viên **không tự gán thủ công** thời gian khi Insert/Update trong Service. Hệ thống sẽ tự động gán thông qua cơ chế ghi đè `SaveChanges` tại [MangaPublishingDbContext.cs](file:///d:/SWP391/Project/BE-SWP391/Services/MangaPublishingSystem/MangaPublishingSystem.Infrastructure/Data/MangaPublishingDbContext.cs).
* **Quy tắc ánh xạ Enum & Ràng buộc CSDL (Enum Mapping & DB Constraints):**
  * Để dữ liệu trực quan dễ đọc, mọi Enum sử dụng trong DB (ví dụ: `UserStatus` cho cột `Status`) **bắt buộc** phải được chuyển đổi thành chuỗi khi lưu trữ bằng cách sử dụng `.HasConversion<string>()` trong các file cấu hình Fluent API (trong `Configurations/`).
  * Đồng thời, **bắt buộc** phải thêm ràng buộc `CHECK CONSTRAINT` tương ứng trong file `schema.sql` (ví dụ: `CONSTRAINT CK_User_Status CHECK (Status IN (N'Pending', N'Active', N'Rejected', N'Locked'))`) để đảm bảo tính toàn vẹn dữ liệu chặt chẽ ở tầng CSDL vật lý.
* **Múi giờ & Định dạng JSON (UTC & Vietnam Time):**
  * Database và Backend C# lưu trữ toàn bộ thời gian theo **múi giờ chuẩn UTC** (`DateTime.UtcNow`).
  * Khi trả dữ liệu ra ngoài API cho FE, hệ thống sử dụng bộ lọc `DateTimeJsonConverter` tự động đổi sang múi giờ Việt Nam (**UTC+7**) và format dạng dễ đọc: **`yyyy-MM-dd HH:mm:ss`**.
* **Phân trang dùng chung (Pagination Capped at 50):**
  * Không viết lại logic phân trang. Sử dụng các lớp có sẵn trong `BuildingBlocks.Web.Responses`:
    * Đầu vào: **`PagedRequest`** (Tự động khống chế `PageSize` tối đa là **50**).
    * Đầu ra: **`PagedResult<T>`** đóng gói kèm metadata cho FE vẽ UI.
    * Lọc DB: Dùng phương thức mở rộng **`ToPagedListAsync(...)`** ở tầng Infrastructure.
    * Lọc bộ nhớ: Dùng **`ToPagedList(...)`** trong `BuildingBlocks.Extensions`.
* **Tìm kiếm không dấu & không phân biệt hoa thường (Search Extensions):**
  * Lọc cơ sở dữ liệu: Dùng **`WhereContainsUnsigned(...)`** ở tầng Infrastructure để thực thi collation `SQL_Latin1_General_CP1_CI_AI` trên SQL Server.
  * Lọc bộ nhớ RAM: Dùng **`ContainsUnsigned(...)`** trong `BuildingBlocks.Extensions`.

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
  dotnet build MangaPublishingSystem.sln
  ```
* **Khởi tạo và nạp dữ liệu Database (Hỗ trợ tiếng Việt UTF-8)**:
  * Do cơ sở dữ liệu chứa dữ liệu tiếng Việt Unicode, khi nạp schema và seed data bằng `sqlcmd` bắt buộc phải sử dụng tham số `-f 65001` để chỉ định encoding UTF-8, tránh lỗi hỏng font (mojibake) trong database:
  ```powershell
  sqlcmd -S localhost -f 65001 -i Database\schema.sql
  sqlcmd -S localhost -f 65001 -i Database\seed.sql
  ```
* **Chạy API dịch vụ chính (Manga Service)**:
  ```powershell
  dotnet run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
  ```
* **Chạy API Gateway**:
  ```powershell
  dotnet run --project GatewayAPI/GatewayAPI.csproj
  ```
* **Chạy Script Test API tự động**:
  * Để hiển thị tiếng Việt chính xác trên console Windows (cmd/powershell), bạn cần đổi code page của terminal sang UTF-8 trước khi chạy script test:
  ```powershell
  chcp 65001
  node test-api.js
  ```

---

## 5. HƯỚNG DẪN CẤU HÌNH & SỬ DỤNG CÁC DỊCH VỤ TÍCH HỢP

### 5.1. Cấu hình thông báo thời gian thực (SignalR)
* **Hub Endpoint**: `/hubs/notification` (Địa chỉ upstream qua Gateway: `/api/v1/hubs/notification`).
* **Cấu hình WebSockets & Gateway (Ocelot)**:
  * API Gateway và Manga Service (downstream) đều được cấu hình hỗ trợ WebSockets thông qua middleware `app.UseWebSockets()`.
  * Ocelot được cấu hình chia 2 route: Route bắt tay (HTTP Post/Options `/negotiate`) và Route kết nối chính (WebSockets `ws://`).
* **Xác thực JWT Token qua WebSockets**: Do trình duyệt không hỗ trợ gửi custom HTTP Header (`Authorization: Bearer...`) khi khởi tạo kết nối WebSocket, Frontend sẽ truyền token qua URL query string dưới dạng `?access_token=...`. JwtBearer ở Backend được cấu hình sự kiện `OnMessageReceived` để tự động trích xuất token này đối với các kết nối có đường dẫn chứa `/hubs`.
* **Lưu ý về CORS**: Trình duyệt sẽ chặn kết nối nếu CORS để wildcard `*` khi dùng Credentials. Để đảm bảo SignalR hoạt động ổn định khi cho phép credentials, cấu hình CORS ở Backend được cập nhật sử dụng `builder.SetIsOriginAllowed(_ => true).AllowCredentials()` (động hóa origin) khi bật chế độ AllowAnyOrigin, hoặc khai báo chính xác địa chỉ Client trong khóa `"Cors:AllowedOrigins"` tại tệp `appsettings.json`.

### 5.2. Công cụ tự động hóa Git Hooks (Husky.Net)
* **Mục tiêu**: Ngăn chặn commit/push mã nguồn gặp lỗi biên dịch.
* **Cấu hình Git Hooks**:
  * `pre-commit`: Tự động chạy lệnh `dotnet build MangaPublishingSystem.sln` trước khi commit cục bộ.
  * `pre-push`: Tự động chạy lệnh `dotnet build MangaPublishingSystem.sln` trước khi push lên GitHub.
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

### 5.4. Tích hợp thanh toán VNPay Sandbox
* **Cách hoạt động**: Tích hợp quy trình nạp tiền thông qua cổng thanh toán VNPay Sandbox.
* **Cấu hình trong `appsettings.json`**:
  ```json
  "VnPay": {
    "TmnCode": "0NLJV3OJ",
    "HashSecret": "4W2N65NBLLT0XAM4ULPMXF5MPY3JC18C",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "QueryUrl": "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction",
    "ReturnUrl": "http://localhost:5000/api/v1/wallets/deposit/return",
    "FrontendReturnUrl": "http://localhost:5173/mangaka/wallet",
    "IpnUrl": "http://localhost:5000/api/v1/wallets/deposit/ipn"
  }
  ```
* **Quy trình hoạt động**:
  * **Tạo URL thanh toán (Deposit)**: Client gọi API `POST /api/wallets/deposit`. Backend tạo một giao dịch `Pending` và trả về URL thanh toán của VNPay Sandbox.
  * **Frontend Redirect**: Client chuyển hướng trình duyệt tới URL thanh toán để người dùng nhập thẻ (Thẻ test: Ngân hàng NCB, Số thẻ 9704198526191432198, Tên NGUYEN VAN A, Ngày phát hành 07/15, OTP 123456).
  * **Nhận kết quả trực tiếp (Return URL)**: VNPay redirect về `ReturnUrl` kèm tham số. Backend xác thực chữ ký (HMAC-SHA512), cập nhật trạng thái giao dịch, sau đó chuyển hướng người dùng về `FrontendReturnUrl` để Frontend hiển thị kết quả UX liền mạch.
  * **Cập nhật số dư ngầm (IPN URL)**: VNPay gọi ngầm tới `IpnUrl` (server-to-server webhook). Đây là bước bảo mật bắt buộc để Backend cập nhật số dư `WithdrawableBalance` và đổi trạng thái giao dịch thành `Success`, ngăn chặn mất mát dữ liệu khi Frontend bị tắt đột ngột.
  * **Rút tiền (Withdraw)**: Hệ thống sử dụng quy trình Phê duyệt thủ công (Phương án 3). Tiền được khóa (chuyển sang `LockedWithdrawable`) khi user yêu cầu rút, và chỉ được giải ngân thực sự (hoặc hoàn tiền) khi System Admin duyệt qua API `POST /api/wallets/withdraw/{id}/approve`.

### 5.5. Định dạng phản hồi API thống nhất (JSON camelCase)
* **Quy chuẩn định dạng**: Mọi dữ liệu trả về client đều sử dụng lớp `ApiResponse<T>` với thuộc tính `Success` (kiểu `bool`). Khi serialize sang JSON camelCase, thuộc tính này sẽ được chuyển thành `success` để đồng bộ hoàn toàn với Frontend, tránh lỗi `success is undefined` (do sử dụng `IsSuccess` cũ bị serialize thành `isSuccess`).

