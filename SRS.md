# HƯỚNG DẪN NGHIỆP VỤ HỆ THỐNG - SRS (DÀNH CHO AI & DEVELOPER)

## HỆ THỐNG QUẢN LÝ QUY TRÌNH SÁNG TÁC & XUẤT BẢN TRUYỆN TRANH (MCWPMS)

> [!IMPORTANT]
> **TÀI LIỆU QUY ĐỊNH TOÀN BỘ NGHIỆP VỤ HỆ THỐNG.**
> Tài liệu này tổng hợp toàn bộ các Tác nhân, Tính năng, Tiêu chí nghiệm thu (Acceptance Criteria), các luồng nghiệp vụ chi tiết và Quy tắc nghiệp vụ từ tài liệu SRS gốc. AI và Lập trình viên bắt buộc phải đọc và tuân thủ nghiêm ngặt để đảm bảo code logic chính xác.

---

## 1. CÁC TÁC NHÂN TRONG HỆ THỐNG (ACTORS)

Hệ thống phân chia người dùng thành 2 nhóm với 5 vai trò phân quyền (RBAC) nghiêm ngặt:

1. **System Admin**: Quản trị viên hệ thống. Quản lý cấu hình, phân quyền, thiết lập hợp đồng gốc, đối soát VNPay.
2. **Tantou Editor**: Biên tập viên phụ trách tác giả. Đánh giá bản nháp, QC bản thảo, giải quyết tranh chấp.
3. **Editorial Board**: Hội đồng biên tập. Phê duyệt/từ chối truyện mới, cấp ngân sách, đánh giá hủy truyện (axing).
4. **Mangaka**: Tác giả chính. Tạo truyện, giao task cho trợ lý, duyệt tranh, nhận nhuận bút.
5. **Assistant**: Trợ lý vẽ tranh tự do. Nhận việc, nộp tranh, nhận lương. (Vai trò duy nhất được tự đăng ký tài khoản).

---

## 2. DANH SÁCH TÍNH NĂNG VÀ TIÊU CHÍ NGHIỆM THU (FEATURES & ACCEPTANCE CRITERIA)

### 2.1. Nhóm Tính năng của Mangaka (Tác giả)

| ID              | Tên tính năng              | Phân hệ    | Mô tả nghiệp vụ                                                                                                                           | Tiêu chí nghiệm thu (Acceptance Criteria)                                                                                                    |
| --------------- | ----------------------------- | ------------ | --------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| **F1.1**  | Create Series Profiles        | Series       | Nhập tên truyện, thể loại, tóm tắt, ảnh minh họa và đề xuất ngân sách sản xuất chương 1 (`Chapter 1 Production Budget`). | Truyện được lưu thành công với trạng thái ban đầu là`Draft`.                                                                     |
| **F1.2**  | Submit Draft Manuscript       | Series       | Tải lên các trang vẽ bản thảo nháp/PDF của chương mẫu để kiểm tra.                                                              | Tệp bản thảo nháp được lưu trữ an toàn trên Cloud Storage.                                                                           |
| **F1.3**  | Submit for Review             | Series       | Chuyển trạng thái bộ truyện từ`Draft` sang `Pending_Approval`.                                                                      | Biên tập viên phụ trách nhận được thông báo hệ thống để tiến hành đánh giá.                                                 |
| **F1.4**  | View Series Status            | Series       | Theo dõi tiến độ duyệt bộ truyện (`Pending` / `Approved` / `Rejected`).                                                          | Trạng thái hiển thị chính xác theo thời gian thực (Realtime).                                                                           |
| **F2.1**  | Upload Manga Pages            | Task Mgmt    | Tải lên các trang vẽ riêng lẻ cho một chương mới cần sản xuất.                                                                   | Các file ảnh hiển thị mượt mà trên Canvas Viewer của hệ thống.                                                                       |
| **F2.2**  | Canvas Region Selection       | Task Mgmt    | Sử dụng công cụ (Fabric.js) vẽ vùng khoanh trên trang bản thảo nháp để phân chia.                                                | Tọa độ vùng được chọn lưu lại thành công dưới định dạng JSON.                                                                  |
| **F2.3**  | Assign Tasks to Assistants    | Task Mgmt    | Giao việc vẽ vùng đã khoanh cho Assistant, thiết lập đơn giá lương, hạn chót (Deadline), và mức độ Z-Index.                 | Hệ thống tự động khóa tiền tương ứng của Task trong ví Mangaka (ký quỹ - Escrow) và thông báo tới Assistant.                  |
| **F2.4**  | View Composited Canvas        | Task Mgmt    | Xem trước trang truyện hoàn chỉnh sau khi ghép tất cả các layer PNG trong suốt của các Assistant nộp.                            | Ảnh gộp (Composite Image) hiển thị đúng thứ tự phân lớp Z-Index và căn chỉnh thẳng hàng.                                         |
| **F2.5**  | Approve/Reject Assistant Work | Task Mgmt    | Duyệt bài nộp của Assistant hoặc Từ chối kèm theo ghim nhận xét lỗi trực quan trên Canvas (Canvas Annotation).                   | Khi duyệt: chuyển tiền ký quỹ từ ví Mangaka sang ví Assistant. Khi từ chối: tiền vẫn bị khóa để sửa đổi.                     |
| **F1.5**  | Track Series Ranking          | Ranking      | Xem thứ hạng và vị trí của bộ truyện trên bảng xếp hạng định kỳ.                                                               | Hiển thị chính xác các biểu đồ thống kê và bảng số liệu cập nhật.                                                               |
| **F1.6**  | Receive Alert Notifications   | Notification | Nhận cảnh báo khi bộ truyện bị rơi xuống nhóm thứ hạng thấp, có nguy cơ bị hủy xuất bản (axing).                            | Đảm bảo thông báo được đẩy đến Mangaka theo thời gian thực.                                                                       |
| **F1.7**  | View Wallet Dashboard         | Wallet       | Xem lịch sử giao dịch và số dư ví tại 2 tài khoản:`SetupFundBalance` & `WithdrawableBalance`.                                   | Hiển thị chính xác, realtime dữ liệu số dư và lịch sử giao dịch.                                                                    |
| **F1.8**  | Withdraw Funds                | Wallet       | Yêu cầu rút tiền từ ví được rút (`WithdrawableBalance`) về tài khoản ngân hàng thông qua cổng mô phỏng VNPay Sandbox.    | Sau khi VNPay phản hồi thành công, số dư ví giảm ngay lập tức.                                                                        |
| **F1.9**  | Deposit Funds                 | Wallet       | Nạp tiền cá nhân vào ví thông qua VNPay Sandbox để bổ sung ngân sách thuê Assistant.                                             | Sau khi thanh toán thành công, số dư ví được cộng tiền ngay lập tức.                                                               |
| **F1.10** | Approve Deadline Extension    | Task Mgmt    | Phê duyệt hoặc Từ chối yêu cầu xin gia hạn deadline nộp bài từ Assistant.                                                          | Nếu duyệt: cập nhật hạn chót mới = Hạn chót cũ + Số ngày xin thêm.                                                                 |
| **F2.13** | Manual Composite Tuning       | Task Mgmt    | Cho phép Mangaka kéo thả các layer ảnh, tinh chỉnh lại thứ tự đè Z-Index thủ công trên Canvas trước khi đóng gói trang.    | Giao diện kéo thả mượt mà, phân tách rõ ràng các layer và lưu lại đúng cấu trúc.                                              |
| **F2.14** | Set Absence Status            | Task Mgmt    | Bật chế độ nghỉ phép hoặc nghỉ khẩn cấp (`On_Leave`).                                                                             | Hệ thống tạm dừng đếm ngược thời gian tự động duyệt (Auto-Approve 3 ngày) của các task đang chờ để bảo vệ tiền ký quỹ. |

---

### 2.2. Nhóm Tính năng của Assistant (Trợ lý vẽ tranh)

| ID              | Tên tính năng           | Phân hệ | Mô tả nghiệp vụ                                                                                         | Tiêu chí nghiệm thu (Acceptance Criteria)                                                                              |
| --------------- | -------------------------- | --------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **F2.6**  | View Task Queue            | Task Mgmt | Xem danh sách các task được giao. Lọc theo trạng thái và sắp xếp theo hạn chót/độ ưu tiên. | Giao diện danh sách hiển thị chính xác các task của Assistant.                                                    |
| **F2.7**  | Download Files & Resources | Task Mgmt | Tải ảnh gốc phân vùng (`BaseLayer`) và thư mục tài liệu đi kèm (cọ vẽ, Style Guide).        | File tải về đầy đủ, không bị lỗi nén hoặc hỏng dữ liệu.                                                     |
| **F2.8**  | Upload Drawing Submission  | Task Mgmt | Tải lên file ảnh kết quả vẽ cho phân vùng được giao.                                             | File bắt buộc ở định dạng**PNG nền trong suốt**; gửi thông báo tự động cho Mangaka sau khi tải lên. |
| **F2.9**  | Track Completed Tasks      | Income    | Xem danh sách và tổng số lượng các task đã được duyệt theo từng tháng.                       | Thống kê số lượng khớp chính xác với lịch sử duyệt task.                                                      |
| **F2.10** | Track Income Statistics    | Income    | Xem tổng thu nhập kiếm được (tổng`Payment_Amount`) hiển thị theo biểu đồ hàng tháng.        | Biểu đồ thu nhập trực quan và số liệu khớp chính xác.                                                          |
| **F2.11** | Manage Wallet              | Wallet    | Xem số dư ví và lịch sử nhận tiền thanh toán nhiệm vụ.                                           | Số dư hiển thị khớp hoàn toàn với bảng lịch sử giao dịch.                                                     |

---

### 2.3. Nhóm Tính năng của Tantou Editor (Biên tập viên phụ trách)

| ID             | Tên tính năng      | Phân hệ | Mô tả nghiệp vụ                                                                                                                      | Tiêu chí nghiệm thu (Acceptance Criteria)                                                                                       |
| -------------- | --------------------- | --------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| **F3.1** | Manga Viewer          | Review    | Xem toàn bộ chương truyện đã hoàn thành, lật trang giống trải nghiệm đọc thực tế.                                       | Hiển thị mượt mà, hỗ trợ phóng to/thu nhỏ và kéo xem (Pan/Zoom).                                                        |
| **F3.2** | Annotation QC Tool    | Review    | Ghim nhận xét bắt lỗi trực tiếp trên các trang vẽ. Phân loại lỗi theo QC: Kỹ thuật (Technical), Mỹ thuật (Art, Content). | Lưu lại tọa độ điểm ghim lỗi, nội dung lỗi. Trang có lỗi tự động bị đánh dấu là Không hợp lệ (`Invalid`). |
| **F3.3** | Manage Series Profile | Series    | Tổng hợp chỉ số và thông tin đánh giá bộ truyện để chuẩn bị báo cáo trình lên Hội đồng duyệt.                     | Tạo ra báo cáo đánh giá chứa thông tin thứ hạng và mức độ tương tác của độc giả.                              |
| **F3.4** | Progress Dashboard    | Review    | Theo dõi tiến độ sản xuất chương truyện: số trang Done, In-progress, và Pending.                                              | Các chỉ số tiến độ tự động cập nhật theo thời gian thực.                                                              |
| **F3.5** | Deadline Tracking     | Review    | Giám sát hạn chót nộp chương của Mangaka và gửi cảnh báo khi trễ hạn.                                                      | Tự động đẩy cảnh báo (Alert) khi thời gian còn lại của chương từ 2 ngày trở xuống.                                |
| **F3.6** | Approve Chapter       | Review    | Kiểm tra QC Checklist của chương và xác nhận tổng số trang hợp lệ (`PageValidCount`) để duyệt chương.                  | Đổi trạng thái chương thành`Approved`, tự động kích hoạt tính toán và thanh toán nhuận bút (`Genkouryou`).   |
| **F3.7** | Dispute Resolution    | Task Mgmt | Đọc lịch sử các phiên bản bài nộp, comment để giải quyết tranh chấp tiền giữa Mangaka và Assistant.                     | Hệ thống tự động chia tiền ký quỹ (Unlock/Transfer) theo tỷ lệ phần trăm phân chia của Editor.                       |

---

### 2.4. Nhóm Tính năng của Editorial Board (Hội đồng biên tập)

| ID             | Tên tính năng          | Phân hệ  | Mô tả nghiệp vụ                                                                                                                                                         | Tiêu chí nghiệm thu (Acceptance Criteria)                                                                                                                                                                                                                                                   |
| -------------- | ------------------------- | ---------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **F4.1** | View Pending Series       | Publishing | Xem danh sách các bộ truyện mới ở trạng thái`Pending_Board_Vote` kèm theo báo cáo đánh giá của Editor. HĐ chỉ thấy truyện khi Editor đã trình lên. | Hỗ trợ tìm kiếm, lọc danh sách dễ dàng.                                                                                                                                                                                                                                                |
| **F4.2** | Cast Vote                 | Publishing | Bỏ phiếu Đồng ý (cần nhập ngân sách đề xuất) / Từ chối / Bỏ qua. Mỗi thành viên hội đồng chỉ bỏ phiếu 1 lần cho mỗi bộ truyện.                  | Hệ thống tự động đánh giá sau mỗi phiếu. Truyện chỉ chuyển sang`Fund_Pending` hoặc `Rejected` khi đạt đủ ngưỡng % cấu hình. Có cơ chế xử lý hòa phiếu (Tie Policy) hoặc tự chốt sau 48h. Ngân sách duyệt là trung bình cộng các đề xuất Approve. |
| **F4.3** | Release Schedule Decision | Publishing | Thiết lập lịch đăng truyện định kỳ (Hàng tuần/Hàng tháng) cho các bộ truyện đã được duyệt.                                                            | Lịch đăng được lưu cố định vào cơ sở dữ liệu.                                                                                                                                                                                                                                   |
| **F4.4** | Input Vote Data           | Ranking    | Nhập thủ công hoặc import dữ liệu bình chọn bằng phiếu giấy của độc giả vào hệ thống.                                                                     | Ràng buộc kiểm tra dữ liệu đầu vào chặt chẽ, lưu dữ liệu thành công.                                                                                                                                                                                                            |
| **F4.5** | View Ranking Board        | Ranking    | Xem bảng xếp hạng tổng hợp của các bộ truyện sau mỗi kỳ bình chọn.                                                                                             | Bảng xếp hạng tự động tải lại và hiển thị chính xác khi có dữ liệu mới.                                                                                                                                                                                                       |
| **F4.6** | Cancel Series (Axing)     | Publishing | Bỏ phiếu quyết định dừng xuất bản (hủy truyện) đối với các bộ truyện có thứ hạng thấp kéo dài.                                                        | Trạng thái truyện đổi sang`Cancelled`; kích hoạt tiến trình xử lý hủy task và hoàn tiền ký quỹ.                                                                                                                                                                             |
| **F4.7** | Modify Publication Status | Publishing | Thay đổi tần suất lịch phát hành của bộ truyện (Weekly <=> Monthly).                                                                                              | Cập nhật có hiệu lực ngay lập tức trong toàn hệ thống.                                                                                                                                                                                                                               |

---

### 2.5. Nhóm Tính năng của System Admin (Quản trị viên)

| ID             | Tên tính năng           | Phân hệ | Mô tả nghiệp vụ                                                                                                                                              | Tiêu chí nghiệm thu (Acceptance Criteria)                                                                                            |
| -------------- | -------------------------- | --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| **F5.1** | Approve Accounts           | User Mgmt | Duyệt hồ sơ đăng ký (Portfolio) của các Assistant để kích hoạt tài khoản.                                                                          | Tài khoản chuyển sang trạng thái`Active`, cho phép Assistant đăng nhập.                                                      |
| **F5.2** | Manage Permissions (RBAC)  | User Mgmt | Cấu hình quyền hạn truy cập API cho các vai trò khác nhau (RBAC).                                                                                        | Hệ thống chặn quyền chặt chẽ dựa trên JWT Claim Role của người dùng.                                                        |
| **F5.3** | Input Base Page Rate       | User Mgmt | Nhập đơn giá trang vẽ cơ bản của Mangaka (`BaseGenkouryoPrice`) theo hợp đồng MOU ngoại tuyến.                                                    | Đơn giá được lưu và dùng làm căn cứ tự động tính toán nhuận bút sau này.                                            |
| **F5.4** | Disburse Production Budget | Wallet    | Hệ thống tự động chuyển khoản tiền ngân sách được duyệt vào ví của Mangaka (`SetupFundBalance`) ngay khi Mangaka bấm xác nhận hợp đồng. | Ghi nhận chi tiết lịch sử giao dịch loại`Funding` của hệ thống.                                                              |
| **F5.5** | Update Contract Addendum   | User Mgmt | Tạo phụ lục hợp đồng (`ContractAddendum`) để cập nhật đơn giá trang vẽ mới cho tác giả.                                                       | Đơn giá phụ lục chỉ áp dụng cho các chương tạo sau mốc thời gian ký phụ lục (không tính ngược lại).               |
| **F5.6** | VNPay Reconciliation       | Wallet    | Đối soát: Đối chiếu dữ liệu giao dịch nạp/rút tiền trên hệ thống với file CSV kết quả đối soát xuất từ VNPay.                             | Tự động quét và ghim cảnh báo các giao dịch lệch trạng thái (Ví dụ: hệ thống báo lỗi nhưng VNPay báo thành công). |
| **F5.7** | View System Dashboard      | Dashboard | Xem bảng điều khiển tổng quan hệ thống: Tổng số truyện, tổng người dùng theo role, và tổng doanh thu nạp ví.                                   | Hiển thị chính xác các con số thống kê theo thời gian thực.                                                                   |

---

## 3. CHI TIẾT 7 LUỒNG NGHIỆP VỤ CHÍNH (DETAILED MAIN FLOWS)

### Luồng 1 — Thiết lập tài khoản & Onboarding (Onboarding & Account Creation)

1. **Đối với Mangaka (Quy trình tuyển chọn offline)**:
   * Tác giả liên hệ với nhà xuất bản qua Landing Page. Nhà xuất bản cử một nhân viên tìm kiếm tài năng (Scout) gặp mặt trực tiếp để đánh giá năng lực tác giả (Khả năng đào tạo, Tốc độ vẽ, Khả năng làm việc nhóm, Độ phù hợp văn hóa, Tiềm năng thương mại).
   * Hai bên ký kết bản ghi nhớ hợp đồng giấy (MOU offline), chốt đơn giá trang vẽ gốc (`BaseGenkouryoPrice` VND/trang).
   * Scout gửi hồ sơ Mangaka cho Admin.
   * Admin nhập thông tin lên hệ thống: Tạo tài khoản Mangaka (F5.1, F5.2), phân bổ một Biên tập viên phụ trách (Tantou Editor), nhập đơn giá trang hợp đồng.
   * Tác giả nhận được email kích hoạt có kèm mật khẩu tạm thời, đăng nhập, đổi mật khẩu và bắt đầu sử dụng. Tác giả có thể tự cập nhật thông tin cá nhân bổ sung (Số điện thoại, Ảnh đại diện, Bút danh) thông qua API Profile (`PUT /api/profile`).
2. **Đối với Assistant (Đăng ký tự do)**:
   * Assistant truy cập Landing Page -> nhấn "Register".
   * Điền thông tin cá nhân: Họ tên, email, link Portfolio chứa các tác phẩm đã vẽ, và khai báo thẻ kỹ năng chuyên môn (`SpecialtyTags`).
   * Hệ thống lưu tài khoản ở trạng thái `Pending`.
   * Admin duyệt Portfolio ngoại tuyến -> Nếu đạt yêu cầu, Admin bấm Phê duyệt (F5.1) -> Tài khoản chuyển sang `Active`.
   * Assistant nhận email kích hoạt thành công, đăng nhập và sẵn sàng nhận Task. Assistant cũng có quyền cập nhật Hồ sơ (Số điện thoại, Avatar, Link Portfolio mới, Kỹ năng bổ sung) qua API Profile để làm phong phú CV.

### Luồng 2 — Phê duyệt truyện & Cấp ngân sách (Series Review & Production Funding)

1. **Mangaka tạo hồ sơ bộ truyện mới (F1.1)**:
   * Nhập tên truyện, thể loại, tóm tắt, tải lên ảnh minh họa.
   * Đề xuất số tiền ngân sách sản xuất chương 1 (`Chapter 1 Production Budget` bằng VND) để làm vốn thuê trợ lý.
   * Tải lên các file ảnh/PDF bản vẽ nháp của chương mẫu (F1.2).
   * Nhấn "Submit for review" (F1.3) -> Trạng thái truyện chuyển từ `Draft` sang `Pending_Approval`.
2. **Editor đánh giá**:
   * Editor mở bản vẽ nháp, sử dụng công cụ ghim nhận xét (Annotation Tool) để chỉ ra các điểm lỗi (F3.1, F3.2).
   * *Nếu không đạt*: Editor từ chối, gửi phản hồi để Mangaka sửa đổi và nộp lại.
   * *Nếu đạt*: Editor tổng hợp hồ sơ đánh giá và đề xuất mức ngân quỹ gửi lên Hội đồng biên tập (F3.3).
3. **Hội đồng biên tập (Board) biểu quyết (F4.1, F4.2)**:
   * (Cổng 2) Hội đồng xem xét hồ sơ truyện và số ngân sách được đề xuất ở trạng thái `Pending_Board_Vote`.
   * Các thành viên bỏ phiếu (Đồng ý / Từ chối / Bỏ phiếu trắng).
   * Hệ thống tự động tổng hợp phiếu:
     * *Đạt ngưỡng duyệt*: Đổi trạng thái truyện thành `Fund_Pending`. Ngân sách duyệt là trung bình các phiếu Đồng ý.
     * *Đạt ngưỡng từ chối*: Đổi trạng thái truyện thành `Rejected`.
     * *Hòa phiếu hoặc bế tắc*: Hệ thống tự động giải quyết theo cấu hình (Tie Policy: Leo thang cho Admin, Tự động từ chối, hoặc Theo phiếu Chủ tịch). Nếu leo thang, trạng thái là `Vote_Escalated`, Admin sẽ quyết định thủ công.
   * *Quá hạn 48h (AutoResolveHours)*: Hệ thống tự động đánh giá dựa trên các phiếu đã bỏ để chốt duyệt/từ chối/leo thang nhằm tránh việc treo vô hạn.
4. **Ký hợp đồng và Cấp quỹ**:
   * Admin tạo Hợp đồng lao động trên hệ thống (F5.3) dựa trên các thông số đã duyệt.
   * Mangaka nhận được thông báo, kiểm tra điều khoản và nhấn "Accept Fund".
   * Hệ thống đổi trạng thái truyện sang `In Production` và **tự động cấp số tiền ngân sách được duyệt vào ví tài trợ `SetupFundBalance` của Mangaka** (F5.4).

### Luồng 3 — Sản xuất & Thuê Trợ lý (Production & Hiring Assistants)

1. **Giao việc**:
   * Mangaka tải lên các trang bản nháp của chương mới cần vẽ (F2.1).
   * Sử dụng công cụ vẽ vùng khoanh Canvas (Fabric.js) để đánh dấu các khu vực cần hỗ trợ (vẽ nền, hiệu ứng, tô màu...) (F2.2).
   * Đăng Task tuyển Assistant kèm theo mô tả, số tiền thù lao, hạn chót (Deadline) và số thứ tự tầng ảnh (Z-Index) (F2.3).
2. **Ký quỹ bảo đảm (Escrow)**:
   * Hệ thống tự động kiểm tra số dư ví Mangaka:
     * *Đủ tiền*: Hệ thống thực hiện trừ và **Khóa (Lock) số tiền thù lao của Task** vào tài khoản ký quỹ. Trạng thái task chuyển thành `Pending` và gửi thông báo tới các Assistant.
     * *Thiếu tiền*: Hệ thống báo lỗi và yêu cầu Mangaka nạp thêm tiền qua cổng VNPay Sandbox (F1.9).
3. **Thực hiện Task**:
   * Assistant xem danh sách task (F2.6), bấm nhận task -> Tải ảnh nền gốc phân vùng (`BaseLayer`) và tài nguyên mẫu về vẽ (F2.7).
   * Sau khi vẽ xong, Assistant tải lên kết quả dưới định dạng ảnh **PNG nền trong suốt** (transparent PNG) (F2.8). Gửi thông báo tự động cho Mangaka.
   * *Gia hạn deadline*: Nếu gặp khó khăn, Assistant có thể xin gia hạn trước khi quá hạn (F2.12). Nếu Mangaka duyệt (F1.10), deadline mới = Deadline cũ + Số ngày gia hạn. Mỗi task chỉ được xin gia hạn tối đa 1 lần.
4. **Nghiệm thu**:
   * Sau khi tất cả các phân vùng của một trang hoàn thành, hệ thống tự động gộp (composite) các ảnh PNG trong suốt của trợ lý đè lên ảnh nền gốc theo đúng Z-Index (F2.4).
   * Mangaka kiểm tra trang đã gộp:
     * *Duyệt (Approve)*: Hệ thống đổi trạng thái task thành `Approved`, **tự động chuyển tiền ký quỹ đang khóa sang ví của Assistant**.
     * *Yêu cầu sửa (Revision)*: Tiền ký quỹ **vẫn tiếp tục bị khóa**. Assistant sửa tranh và nộp lại phiên bản mới (lưu lịch sử dạng `TaskVersion`).
     * *Hủy task (Cancel)*: Hệ thống mở khóa tiền ký quỹ hoàn lại vào ví Mangaka.

### Luồng 4 — Biên tập, Thanh toán nhuận bút & Xuất bản (Editing, Genkouryou & Publishing)

1. **Mangaka nộp chương**: Sau khi tất cả trang vẽ được nghiệm thu xong, hệ thống tự động xuất bản vẽ gộp hoàn chỉnh của cả chương. Mangaka kiểm tra và bấm "Submit Chapter". Trạng thái chương truyện chuyển thành `Pending_Review` (gửi tới Editor).
2. **Editor duyệt chương**:
   * Editor mở Manga Viewer để duyệt QC cả chương (F3.1).
   * Sử dụng công cụ Annotation để chỉ điểm và bắt lỗi (F3.2) theo QC Checklist: Kỹ thuật (Technical), Mỹ thuật (Art), Nội dung (Content).
   * *Nếu phát hiện lỗi*: Trả về yêu cầu Mangaka sửa đổi.
   * *Nếu đạt*: Editor xác nhận số trang hợp lệ cuối cùng (`ValidPageCount`) trên QC Checklist và nhấn duyệt chương (F3.6). Trạng thái chương chuyển sang `Approved`.
3. **Trả nhuận bút tự động (Genkouryou Payout)**:
   * Hệ thống tự động kiểm tra phụ lục hợp đồng (`ContractAddendum`) xem có đơn giá trang mới không. Nếu không, sử dụng đơn giá gốc (`BaseGenkouryoPrice`).
   * Tính toán nhuận bút: **Nhuận bút = Đơn giá trang × ValidPageCount**.
   * Hệ thống **tự động giải ngân số tiền nhuận bút từ ngân quỹ nhà xuất bản vào ví rút tiền `WithdrawableBalance` của Mangaka**.
4. **Xuất bản**: Chương truyện được hệ thống tự động đăng tải và hiển thị công khai trên ứng dụng đọc truyện theo đúng lịch phát hành định sẵn.

### Luồng 5 — Xếp hạng & Duy trì phát hành (Ranking & Axing)

1. **Nhập dữ liệu bình chọn**: Định kỳ kết thúc kỳ phát hành, dữ liệu phiếu bầu từ độc giả được nhập thủ công hoặc import hàng loạt vào hệ thống (do Editorial Board nhập F4.4).
2. **Xếp hạng**: Hệ thống tổng hợp thuật toán bình chọn và tự động cập nhật bảng xếp hạng trong Dashboard của Hội đồng biên tập (F4.5) và Mangaka (F1.5).
3. **Đánh giá duy trì / Hủy xuất bản**:
   * Bộ truyện có xếp hạng cao/ổn định: Tiếp tục sản xuất các chương tiếp theo.
   * Bộ truyện xếp hạng thấp kéo dài: Hội đồng bỏ phiếu quyết định dừng sản xuất (Hủy bộ truyện / Axing) (F4.6). Trạng thái truyện đổi sang `Cancelled`. Mangaka nhận thông báo cảnh báo (F1.6).
   * *Tiến trình dọn dẹp khi Hủy truyện (T05)*:
     * Hủy ngay các task đang ở trạng thái `Pending` và hoàn trả tiền ký quỹ về ví Mangaka.
     * Các task đang làm dở (`In-Progress`) được kích hoạt **24 giờ ân hạn (Grace Period)** để Assistant nộp phần dở dang. Sau 24h, Mangaka kiểm tra và nghiệm thu thanh toán theo tỷ lệ phần việc đã làm, số tiền thừa còn lại được mở khóa trả về ví Mangaka.

### Luồng 6 — Quản lý Ví & Giao dịch thanh toán (Wallet & Payment Management)

Hệ thống quản lý dòng tiền nội bộ qua hệ thống Ví điện tử với 5 loại hành động chính:

| Loại giao dịch (Action)    | Actor thực hiện   | Mô tả hoạt động                                                                                   | Cơ chế hệ thống                                                                                                                                                                          |
| ---------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Production Funding** | Hệ thống (System) | Cấp ngân sách sản xuất chương 1 từ Nhà xuất bản khi bộ truyện được phê duyệt.        | **Tự động (Auto)**: Cộng tiền vào ví tài trợ `SetupFundBalance` của Mangaka. Không cho phép rút.                                                                        |
| **Genkouryou Payout**  | Hệ thống (System) | Thanh toán nhuận bút cho Mangaka sau khi chương truyện được duyệt.                           | **Tự động (Auto)**: Cộng tiền nhuận bút vào ví được rút `WithdrawableBalance` của Mangaka.                                                                             |
| **Task Payment**       | Hệ thống (System) | Ký quỹ thù lao thuê Assistant khi tạo task, giải ngân khi duyệt hoặc trả lại khi hủy task. | **Tự động (Auto)**: Ưu tiên khóa tiền từ ví `SetupFundBalance`, nếu thiếu trừ tiếp sang ví `WithdrawableBalance`. Giải ngân sang ví Assistant khi được duyệt. |
| **Deposit**            | Mangaka             | Nạp thêm tiền từ ngân hàng vào ví khi thiếu ngân sách thuê trợ lý.                       | **Thủ công (Manual)**: Kết nối thanh toán qua cổng mô phỏng VNPay Sandbox. Thành công thì cộng tiền vào ví `WithdrawableBalance`.                                     |
| **Withdrawal**         | Mangaka / Assistant | Rút tiền khả dụng từ ví về tài khoản ngân hàng cá nhân.                                   | **Thủ công (Manual)**: Khởi tạo yêu cầu rút từ số dư `WithdrawableBalance`. Giao dịch chờ Admin duyệt. Tiền được khóa lại và chỉ trừ khi Admin phê duyệt.    |

* **Đối soát (Reconciliation) (F5.6)**: Định kỳ, Admin import tệp CSV danh sách đối soát từ VNPay để hệ thống tự động so khớp, phát hiện các giao dịch sai lệch trạng thái nạp/rút tiền giữa ví hệ thống và cổng thanh toán để xử lý thủ công.

### Luồng 7 — Tích hợp trợ lý AI (AI Integration)

Dịch vụ trí tuệ nhân tạo (FastAPI + YOLOv8/U-Net/Gemini) chạy nền hỗ trợ Mangaka sáng tác:

1. **Phân vùng khung tranh tự động (Panel Segmentation)**: Khi Mangaka tải trang phác thảo nháp lên, mô hình YOLOv8 tự động nhận diện các khung tranh (Manga Panels) và tạo ra các vùng Canvas có tọa độ tự động (kèm theo ảnh xem trước) để Mangaka chỉ cần nhấn và giao việc nhanh cho trợ lý.
2. **Hỗ trợ tô màu nền (Colorization Assistance)**: Tích hợp mô hình U-Net hỗ trợ xử lý nhiễu (Denoising) và tự động tô màu cơ bản cho các trang truyện/khung tranh đen trắng phác thảo, giúp Assistant có bản nháp màu cơ sở và đẩy nhanh tốc độ vẽ.
3. **Gợi ý Thẻ thể loại tự động (Gemini Tag Suggestion)**: Tích hợp LLM Google Gemini hỗ trợ tự động đọc và phân tích nội dung tóm tắt (Synopsis) của truyện khi Mangaka tạo truyện mới, từ đó gợi ý danh sách các Thẻ thể loại (Tags/Genres) phù hợp nhất.

---

## 4. CHI TIẾT CHUYÊN ĐỀ: BIỂU QUYẾT HỘI ĐỒNG (BOARD VOTING)

Cơ chế biểu quyết cấp vốn Series hoạt động theo nguyên tắc Hai cổng (Two-gate):

1. **Cổng 1 (Editor):** `Pending_Approval` - Editor xem hồ sơ. Khi đạt yêu cầu, gọi SubmitSeriesToBoard để trình HĐ.
2. **Cổng 2 (Hội đồng):** `Pending_Board_Vote` - Các thành viên HĐ xem và bỏ phiếu.

**Cấu hình quản lý tập trung (BoardVotingConfig):**

* `ApprovalThresholdPercent`: % số phiếu Đồng ý cần thiết trên tổng số thành viên HĐ (N) để duyệt.
* `RejectionThresholdPercent`: % số phiếu Từ chối cần thiết để đánh rớt.
* Công thức tính ngưỡng: `ceil(N × %Threshold / 100)`. Mẫu số (N) luôn là tổng số thành viên HĐ đang Active.
* Các loại phiếu bầu (Vote Type):
  * **Approve (Đồng ý)**: Bắt buộc kèm theo đề xuất ngân sách. Tính vào phe duyệt.
  * **Reject (Từ chối)**: Tính vào phe từ chối.
  * **Abstain (Bỏ qua/Phiếu trắng)**: Ghi nhận đã vote nhưng không cộng điểm vào phe nào.
* **Tie Policy (Chính sách khi hòa phiếu):** Áp dụng khi N thành viên đã vote và số Approve = Reject.
  * `Escalate`: Leo thang chuyển trạng thái `Vote_Escalated` chờ Admin quyết định thủ công.
  * `Reject`: Tự động đánh rớt Series (`Rejected`).
  * `ChairDecides`: Giải quyết theo phiếu đã bỏ của Chủ tịch HĐ (cấu hình qua `ChairUserId`).
* **Auto Resolve (Tự động chốt):** Một background job định kỳ kiểm tra các Series ở trạng thái `Pending_Board_Vote` quá thời gian `AutoResolveHours` (mặc định 48h). Hệ thống sẽ tự tính toán kết quả dựa trên các phiếu ĐÃ bỏ thực tế, ưu tiên phe nào nhiều phiếu hơn hoặc áp dụng Tie Policy.
* **Xóa phiếu khi nộp lại (`ClearVotesOnResubmit`):** Nếu Editor rút lại hoặc trình lại hồ sơ lên Hội đồng, mọi phiếu bầu cũ trước đó sẽ bị xóa trắng để bầu lại từ đầu.

---

## 5. BẢN QUY TẮC NGHIỆP VỤ CHI TIẾT (BUSINESS RULES) - BẮT BUỘC TUÂN THỦ 100%

### 5.1. Quy tắc Tài khoản & Quyền hạn (Account & Permission Rules)

* **A01 (Quyền tự đăng ký)**: Chỉ duy nhất vai trò `Assistant` được tự đăng ký tài khoản qua trang Landing Page. Tài khoản mới sẽ ở trạng thái `Pending` và chỉ hoạt động (`Active`) sau khi được Admin phê duyệt. Các vai trò khác phải do Admin khởi tạo thủ công.
* **A02 (Mô hình quan hệ nhóm)**: **Tuyệt đối không thiết kế thực thể Nhóm/Team/Group trong Database.** Mangaka quản lý và làm việc trực tiếp với từng Assistant độc lập theo mô hình quan hệ `1 Mangaka - N Assistants`.
* **A03 (Trạng thái tài khoản & Đăng nhập)**: Hệ thống quản lý tài khoản qua 4 trạng thái nghiêm ngặt:
  * `Pending`: Trạng thái mặc định khi Assistant đăng ký. Không được phép đăng nhập (báo lỗi: *"Tài khoản của bạn đang chờ duyệt. Vui lòng quay lại sau."*).
  * `Active`: Tài khoản hợp lệ đã duyệt hoặc tạo bởi Admin. Được phép đăng nhập hệ thống bình thường.
  * `Rejected`: Tài khoản đăng ký bị từ chối phê duyệt. Không được phép đăng nhập (báo lỗi: *"Tài khoản của bạn đã bị từ chối phê duyệt."*).
  * `Locked`: Tài khoản bị khóa tạm thời hoặc vĩnh viễn do vi phạm. Không được phép đăng nhập (báo lỗi: *"Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với quản trị viên."*).
* **A04 (Gán Biên tập viên phụ trách - Tantou Editor)**: Biên tập viên phụ trách được System Admin gán cho Mangaka khi tạo mới hoặc chỉnh sửa tài khoản (thông qua API `PUT /api/admin/users/{id}`). Hệ thống chỉ cho phép gán tài khoản có vai trò là *Tantou Editor* ở trạng thái hoạt động (*Active*) cho tài khoản *Mangaka*. Khi Mangaka gửi duyệt truyện mới, hệ thống sẽ tự động gán `Series.EditorId` từ thông tin `AssignedEditorId` của Mangaka đó.
* **A05 (Cập nhật hồ sơ cá nhân - Profile Update)**: Hệ thống cung cấp cơ chế cập nhật hồ sơ cá nhân độc lập dựa trên vai trò của người dùng (API `/api/v1/profile`).
  * Mangaka và Admin/Board/Editor: Chỉ được phép cập nhật Tên (`FullName`) và Bút danh (`PenName`).
  * Assistant: Ngoài Tên, còn được phép cập nhật liên kết năng lực (`PortfolioUrl`), Kỹ năng (`Skills`) và Nhãn chuyên môn (`SpecialtyTags`). Việc cập nhật phải được xử lý tự động trong cùng một API thay vì gọi các API chuyên trách của Admin.

### 5.2. Quy tắc Duyệt truyện & Ví sản xuất (Review & Funding Rules)

* **F01 (Duyệt ngân sách)**: Ngân sách sản xuất chương 1 không tự động áp dụng theo đề xuất của Mangaka mà phải do Hội đồng biên tập (Editorial Board) quyết định hạn mức tối đa.
* **F02 (Cấp ngân quỹ)**: Hệ thống chỉ tự động chuyển tiền ngân sách sản xuất vào ví của Mangaka (`SetupFundBalance`) sau khi Hội đồng đã bỏ phiếu duyệt và Mangaka bấm nút "Accept Fund" trên hệ thống.
* **F03 (Quy tắc tiêu tiền ví - Rất Quan Trọng)**: Khi tạo Task thuê vẽ, hệ thống bắt buộc phải ưu tiên trừ và khóa tiền từ ví tài trợ trước (`SetupFundBalance`). Nếu ví tài trợ này hết tiền, hệ thống mới được phép khấu trừ tiếp vào ví được rút (`WithdrawableBalance`) của Mangaka. **Nghiêm cấm cho phép chi tiêu thấu chi (Overdraft) âm số dư ví.**
* **F04 (Giao dịch VNPay)**: Mọi giao dịch nạp tiền (Deposit) và rút tiền (Withdraw) thật qua VNPay Sandbox phải có mã tham chiếu `ReferenceCode` hợp lệ để đối soát.

### 5.3. Quy tắc Quản lý Nhiệm vụ & Thuê vẽ (Task Management & Sub-Contract Rules)

* **T01 (Điều kiện tạo Task)**: Mangaka chỉ được phép tạo Task nếu tổng số dư khả dụng trong ví lớn hơn hoặc bằng đơn giá thuê vẽ của Task đó. Tiền thuê sẽ bị khóa (Lock) ngay lập tức vào sổ quỹ ký quỹ của hệ thống.
* **T02 (Khóa tiền khi yêu cầu sửa)**: Khi Mangaka từ chối bài nộp và yêu cầu Assistant sửa đổi (`Revision`), **tiền ký quỹ vẫn phải tiếp tục khóa**, không được mở khóa. Mangaka phải thiết lập thời gian gia hạn cho deadline (+24 giờ hoặc +48 giờ).
* **T03a (Tự động hủy trễ hạn - Auto-Refund)**: Nếu Assistant đã nhận Task nhưng quá hạn chót **quá 3 ngày** mà không tải lên bài vẽ (hoặc không tải lên bản sửa đổi mới), hệ thống phải tự động Hủy task, giải phóng trạng thái và trả lại toàn bộ số tiền đã ký quỹ về ví của Mangaka.
* **T03b (Hủy khẩn cấp do không hoạt động)**: Nếu Assistant nhận Task nhưng **không thực hiện tải các tệp mẫu hoặc tài nguyên vẽ về trong vòng 24 giờ đầu tiên**, Mangaka có quyền bấm hủy nhiệm vụ khẩn cấp để thu hồi ngay tiền ký quỹ.
* **T04 (Tự động duyệt bài - Auto-Approve)**: Khi Assistant tải bài nộp lên, nếu Mangaka **không phản hồi (chấp nhận hoặc yêu cầu sửa) trong vòng 3 ngày**, hệ thống phải tự động Duyệt bài, chuyển trạng thái thành `Approved` và chuyển tiền ký quỹ sang ví Assistant. Gửi cảnh báo nhắc nhở cho Mangaka vào ngày thứ 2. *Ngoại lệ: Nếu Mangaka đang bật trạng thái nghỉ phép `On_Leave`, bộ đếm thời gian này sẽ tạm dừng.*
* **T05 (Quy tắc Hủy truyện - Series Cancellation)**: Khi bộ truyện bị Hội đồng biểu quyết hủy (`Cancelled`):
  * Tự động hủy các Task đang ở trạng thái chờ nhận (`Pending`) và trả lại tiền ký quỹ về ví Mangaka.
  * Với các Task đang làm dở (`In-Progress`), hệ thống kích hoạt **24 giờ ân hạn (Grace Period)** để Assistant nộp phần việc hiện tại. Sau 24 giờ, Mangaka kiểm tra, nghiệm thu thanh toán theo tỷ lệ phần việc đã làm, số tiền dư còn lại được mở khóa trả về ví Mangaka.
* **T06 (Phán quyết của Biên tập viên)**: Khi có tranh chấp (`Dispute`), Tantou Editor là người phán quyết cuối cùng. Hệ thống phải cho phép Editor thiết lập tỷ lệ chia tiền ký quỹ (ví dụ: chuyển 60% cho Assistant và hoàn lại 40% cho Mangaka).
* **T07 (Lịch sử các phiên bản nộp bài)**: Bài vẽ do Assistant tải lên lần sau không được ghi đè lên file cũ mà phải được lưu trữ dưới dạng bản ghi lịch sử `TaskVersion` để Mangaka có thể so sánh và duyệt bất kỳ phiên bản nào.
* **T08 (Giới hạn xin gia hạn)**: Assistant có quyền gửi yêu cầu xin gia hạn trước khi task trễ hạn. Nếu Mangaka duyệt, hạn chót mới sẽ được cập nhật. Để tránh spam, mỗi task **chỉ được phép yêu cầu gia hạn tối đa 1 lần**.
* **T09 (Cập nhật chỉ số Assistant)**: Khi một Task chuyển sang trạng thái cuối cùng (`Approved`, `Closed`, hoặc `Cancelled`), hệ thống sẽ chạy một background job để tính toán lại các chỉ số hiệu suất của Assistant (tỷ lệ đúng hạn, tỷ lệ bị sửa...) và cập nhật vào `AssistantProfile`.
* **T10 (Hoàn tiền ký quỹ chính xác ngăn ví)**: Khi thực hiện hủy task và hoàn tiền ký quỹ, hệ thống phải truy vấn giao dịch khóa tiền gốc của task đó để hoàn trả chính xác số tiền vào đúng các tài khoản số dư nguồn (`SetupFundBalance` và `WithdrawableBalance`) theo tỷ lệ đã trừ lúc đầu.

### 5.4. Quy tắc Tính nhuận bút Tác giả (Review Genkōūryō Rules)

* **G01 (Tự động ghép trang - Auto-Compositing)**: Ngay sau khi tất cả các phân vùng trên một trang truyện được Assistant hoàn thành (`Approved`), hệ thống phải tự động chạy background job để ghép các layer ảnh PNG trong suốt đè lên ảnh nền gốc theo đúng Z-Index.
* **G02 (Tính nhuận bút - Genkoūryō Payout)**: Nhuận bút được tính dựa trên số trang hợp lệ của chương truyện: **Nhuận bút = Đơn giá trang × ValidPageCount**.
  * `ValidPageCount` ban đầu được đếm từ số trang có `IsApproved = True` sau khi biên tập viên duyệt.
  * Tuy nhiên, Editor có quyền điều chỉnh (override) tổng số trang thực tế trên biểu mẫu QC Checklist (gồm đánh giá Kỹ thuật, Mỹ thuật, Nội dung). Số trang xác nhận cuối cùng trên biểu mẫu QC này là căn cứ pháp lý duy nhất để thanh toán.
* **G03 (Tự động giải ngân nhuận bút)**: Hệ thống phải tự động chuyển tiền nhuận bút từ quỹ của nhà xuất bản vào tài khoản được rút (`WithdrawableBalance`) của ví Mangaka ngay khi Editor bấm duyệt chương thành công.

---

## 6. LƯU Ý KỸ THUẬT KHI TRIỂN KHAI (TECHNICAL IMPLEMENTATION NOTES)

* **Cơ chế Realtime (SignalR & WebSockets)**: Các tính năng thông báo và cập nhật tiến trình realtime sử dụng SignalR qua giao thức WebSockets. API Gateway (Ocelot) proxy các kết nối này bằng cách chia thành 2 route (route negotiate HTTP và route WebSockets chính). Token xác thực (JWT) được truyền qua URL query string dưới dạng `access_token` và Backend tự động phân tích qua sự kiện `OnMessageReceived`.
* **Cấu hình CORS & Credentials**: CORS của hệ thống bắt buộc phải được cấu hình động sử dụng `SetIsOriginAllowed(_ => true)` kết hợp với `AllowCredentials()` thay vì sử dụng wildcard `*` để đảm bảo hỗ trợ cơ chế bảo mật xác thực (credentials) của SignalR.
* **Quy chuẩn Định dạng dữ liệu API**: Mọi phản hồi API từ Backend đều sử dụng lớp chung `ApiResponse<T>` với thuộc tính trạng thái thành công mang tên `Success` (kiểu `bool`). Thuộc tính này sẽ được chuyển thành `success` trong JSON (camelCase) trả về cho Client, đảm bảo Frontend không bị lỗi `success is undefined`.
* **Quản lý Giao dịch (ACID Transactions)**: Tất cả các nghiệp vụ có tính chất tài chính hoặc liên quan đến dữ liệu liên kết quan trọng (ví dụ: Tạo Task, Duyệt Task, Xử lý Tranh chấp, Hủy Truyện) bắt buộc phải được bọc trong các Database Transaction (`BeginTransactionAsync`) để đảm bảo không bị sai lệch số dư ví và trạng thái nhiệm vụ khi xảy ra lỗi đột xuất.
* **Tối ưu hóa Truy vấn (Eager Loading)**: Để đảm bảo hiệu suất khi hiển thị Manga Viewer cho Editor, hệ thống áp dụng cơ chế Eager Loading (như `Include`, `ThenInclude`) để nạp toàn bộ cấu trúc Cây (Series -> Chapters -> Pages -> Annotations/Layers) trong 1 lần truy vấn SQL duy nhất thay vì sinh ra N+1 query.
