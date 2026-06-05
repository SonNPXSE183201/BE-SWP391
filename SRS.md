# HƯỚNG DẪN NGHIỆP VỤ HỆ THỐNG - SRS (DÀNH CHO AI)
## HỆ THỐNG QUẢN LÝ QUY TRÌNH SÁNG TÁC & XUẤT BẢN TRUYỆN TRANH (MCWPMS)

> [!IMPORTANT]
> **TÀI LIỆU QUY ĐỊNH NGHIỆP VỤ BẮT BUỘC.**
> Đây là bản tóm tắt các quy tắc nghiệp vụ cốt lõi từ tệp tài liệu SRS gốc. AI bắt buộc phải đọc và tham chiếu tệp này để tránh làm sai logic, sai quyền hạn (Authorization) hoặc sai quy trình thanh toán của hệ thống.

---

## 1. CÁC TÁC NHÂN TRONG HỆ THỐNG (ACTORS)

Hệ thống phân chia người dùng thành 2 nhóm chính với 5 vai trò (Roles) phân quyền (RBAC) nghiêm ngặt:

### A. Nhóm nội bộ (Internal Group - Tài khoản do Admin cấp)
1. **System Admin (Quản trị viên)**: Quản lý tham số hệ thống, tạo tài khoản người dùng, phân bổ biên tập viên cho tác giả, cấu hình giá sàn trang viết (Base Genkouryou Rate), xử lý đối soát VNPay.
2. **Tantou Editor (Biên tập viên phụ trách)**: Nhân viên của nhà xuất bản. Giám sát tác giả (Mangaka), thiết lập hạn chót (Deadline) của chương truyện, kiểm tra chất lượng bản thảo (QC Checklist), giải quyết tranh chấp (Dispute Resolution).
3. **Editorial Board (Hội đồng biên tập)**: Lãnh đạo cấp cao. Đánh giá và bỏ phiếu phê duyệt dự án mới, quyết định ngân sách sản xuất, quyết định lịch phát hành và hủy các bộ truyện có thứ hạng thấp.

### B. Nhóm bên ngoài (External Group - Tương tác qua hệ thống)
4. **Mangaka (Tác giả chính)**: Đối tác ký hợp đồng với nhà xuất bản. Tạo hồ sơ truyện, nộp bản thảo, chia nhỏ khung tranh trên canvas, giao nhiệm vụ vẽ cho các Trợ lý, thanh toán tiền cho trợ lý, nhận nhuận bút (Genkouryou).
5. **Assistant (Trợ lý vẽ tranh)**: Họa sĩ tự do. **Đây là vai trò duy nhất được phép tự đăng ký (Self-register)** trên hệ thống để ứng tuyển công việc và nhận tiền lương vẽ từ tác giả chính.

---

## 2. 7 LUỒNG NGHIỆP VỤ CHÍNH (MAIN FLOWS)

### Luồng 1: Thiết lập tài khoản & Onboarding (Onboarding & Account Creation)
* **Mangaka**: Ký hợp đồng giấy (MOU) ngoại tuyến thiết lập đơn giá trang (`BaseGenkouryoPrice`). Admin tạo tài khoản trên hệ thống, phân bổ Biên tập viên phụ trách, nhập đơn giá trang hợp đồng. Mangaka nhận email kích hoạt, đổi mật khẩu và sử dụng hệ thống.
* **Assistant**: Tự đăng ký qua Landing Page, tải lên danh mục tác phẩm (Portfolio) và chọn thẻ kỹ năng chuyên môn (`SpecialtyTags`). Tài khoản ở trạng thái `Pending` và chỉ hoạt động (`Active`) sau khi được Admin phê duyệt.

### Luồng 2: Duyệt truyện mới & Cấp ngân sách (Series Review & Production Funding)
1. Mangaka tạo hồ sơ bộ truyện mới (nhập tên, thể loại, tóm tắt, ảnh minh họa) + Đề xuất Ngân sách sản xuất chương 1 + Tải lên bản thảo chương 1 (bản nháp). Trạng thái truyện: `Draft` -> `Pending_Approval`.
2. Editor sử dụng công cụ Canvas Annotation để ghim nhận xét lỗi. Nếu không duyệt: chuyển về cho Mangaka sửa. Nếu duyệt: Editor lập báo cáo đánh giá gửi lên Hội đồng (Board).
3. Hội đồng biên tập duyệt hồ sơ và bỏ phiếu:
   * **Từ chối**: Trả về yêu cầu Mangaka chỉnh sửa.
   * **Phê duyệt**: Đổi trạng thái truyện thành `Board_Approved`, Hội đồng quyết định số ngân sách cấp thực tế và quyết định lịch xuất bản (Hàng tuần/Hàng tháng).
4. Admin tạo hợp đồng chính thức trên hệ thống. Mangaka nhấn "Accept Fund" -> Hệ thống đổi trạng thái truyện thành `In Production` và **tự động chuyển số tiền ngân sách được duyệt vào ví của Mangaka** (Ví nguồn sản xuất).

### Luồng 3: Phân chia nhiệm vụ & Giao việc cho Trợ lý (Task Assignment & Escrow)
1. Mangaka tải lên các trang bản nháp của chương mới.
2. Mangaka sử dụng công cụ Canvas Region Selection (dựa trên Fabric.js) để vẽ/khoanh vùng các vị trí cần trợ lý hỗ trợ (vẽ nền, tô bóng, hiệu ứng...).
3. Mangaka đăng nhiệm vụ (Task) kèm: mô tả nghiệp vụ, đơn giá thuê, hạn chót (Deadline) và số thứ tự tầng ảnh (Z-Index).
   * **Quy tắc ký quỹ (Escrow)**: Hệ thống tự động kiểm tra số dư ví Mangaka. Nếu đủ tiền, hệ thống **Khóa (Lock) số tiền tương ứng của Task** trong quỹ ký quỹ của ví, tạo Task ở trạng thái `Pending` và gửi thông báo tới các Assistant.
4. Assistant nhấn nhận Task -> Tải về ảnh gốc phân vùng (`BaseLayer`) và thư mục tài nguyên vẽ -> Tiến hành vẽ -> Tải lên kết quả vẽ dưới định dạng ảnh **PNG nền trong suốt (transparent PNG)**.

### Luồng 4: Biên tập, Tính nhuận bút & Xuất bản (Editing, Genkouryou & Publishing)
1. **Gộp ảnh tự động (Auto-Compositing)**: Sau khi tất cả các phân vùng của một trang truyện được trợ lý hoàn thành, hệ thống tự động gộp các ảnh PNG của trợ lý đè lên ảnh gốc dựa trên thông số Z-Index tạo thành trang bản thảo hoàn chỉnh.
2. **Mangaka duyệt**: Duyệt kết quả của Assistant. 
   * **Nếu Duyệt (Approve)**: Tiền ký quỹ tự động chuyển từ ví Mangaka sang ví của Assistant.
   * **Nếu Yêu cầu sửa (Revision)**: Tiền cọc vẫn bị Khóa. Assistant tiến hành sửa và nộp phiên bản mới (lưu lịch sử dạng `TaskVersion`).
   * **Nếu Hủy nhiệm vụ**: Tiền được mở khóa trả lại ví Mangaka.
3. **Editor duyệt chương**: Mangaka nộp toàn bộ chương truyện. Editor thực hiện đánh giá chất lượng (QC Checklist) theo 3 tiêu chí: Kỹ thuật (Technical), Mỹ thuật (Art) và Nội dung (Content), sau đó xác nhận tổng số trang hợp lệ (`ValidPageCount`) và bấm Duyệt chương.
4. **Tính Nhuận bút (Genkouryou)**:
   * Hệ thống tự động quét xem bộ truyện có phụ lục hợp đồng (`ContractAddendum`) nào đang hoạt động hay không. Nếu có: dùng giá phụ lục. Nếu không: dùng giá hợp đồng gốc (`BaseGenkouryoPrice`).
   * **Nhuận bút = Đơn giá trang × ValidPageCount**.
   * Hệ thống tự động chuyển số tiền này từ ngân quỹ của nhà xuất bản vào **Ví rút tiền (WithdrawableBalance)** của Mangaka.
5. **Xuất bản**: Chương truyện được đăng tải tự động theo lịch phát hành đã định cấu hình.

### Luồng 5: Đánh giá xếp hạng & Hủy truyện (Ranking & Axing)
1. Cuối mỗi chu kỳ xuất bản, dữ liệu bình chọn từ độc giả được nhập thủ công/import vào hệ thống (do Editorial Board nhập).
2. Hệ thống cập nhật bảng xếp hạng thực tế trong Dashboard.
3. **Hội đồng đánh giá**:
   * Thứ hạng cao: Giữ nguyên lịch xuất bản.
   * Thứ hạng thấp kéo dài: Hội đồng bỏ phiếu quyết định **Hủy bộ truyện (Cancel Series/Axing)**.
   * **Quy tắc Hủy truyện (T05)**: Hệ thống tự động hủy các Task ở trạng thái `Pending` và hoàn trả tiền ký quỹ về ví Mangaka. Các Task đang thực hiện (`In-Progress`) sẽ kích hoạt **24 giờ ân hạn (Grace Period)** để trợ lý nộp phần dở dang. Sau 24h, Mangaka nghiệm thu trả tiền theo tỷ lệ và phần tiền thừa còn lại được mở khóa trả về ví Mangaka.

### Luồng 6: Ví điện tử & Tích hợp VNPay Sandbox
Hệ thống quản lý dòng tiền nội bộ qua ví điện tử:
* **Ví Mangaka** gồm 2 tài khoản số dư:
  1. `SetupFundBalance` (Số dư nguồn cấp): Tiền được tài trợ từ nhà xuất bản cấp cho việc thuê trợ lý. Số tiền này **không được rút ra**, chỉ dùng để thanh toán nhiệm vụ thuê vẽ.
  2. `WithdrawableBalance` (Số dư được rút): Tiền nhận từ nhuận bút truyện vẽ (`Genkouryou`) hoặc tiền do Mangaka tự nạp vào. Tiền này **được phép rút** về tài khoản ngân hàng.
* **Nạp tiền (Deposit)**: Mangaka nạp thêm tiền từ tài khoản ngân hàng vào ví qua **cổng VNPay Sandbox** (để bổ sung vốn thuê trợ lý).
* **Rút tiền (Withdraw)**: Mangaka/Assistant thực hiện rút tiền từ ví về tài khoản ngân hàng thông qua kết nối giả lập với **VNPay Sandbox API**.
* **Đối soát (Reconciliation)**: Admin định kỳ đối soát các giao dịch trên hệ thống với tệp CSV đối soát xuất từ VNPay để phát hiện chênh lệch.

### Luồng 7: Tích hợp trợ lý AI (AI Integration)
* Trợ lý AI chạy như một dịch vụ nền bổ trợ quá trình sáng tác:
  1. **Tự động phân vùng khung tranh (Panel Segmentation)**: Khi Mangaka tải trang nháp lên, AI (YOLOv8/U-Net/SAM) tự động phân tích và chia trang truyện thành các khung tranh nhỏ và Spech bubble để tạo vùng giao việc tự động.
  2. **Hỗ trợ tô màu (Colorization Assistance)**: AI hỗ trợ gợi ý hoặc tự động tô màu nền cơ bản dựa trên các tham số khu vực.

---

## 3. CÁC QUY TẮC NGHIỆP VỤ CỐT LÕI (BUSINESS RULES) - BẮT BUỘC TUÂN THỦ

* **A02 (Mô hình quan hệ nhóm)**: **Không thiết kế thực thể Team/Group trong Database.** Mỗi Mangaka làm việc độc lập trực tiếp với các Trợ lý theo mô hình quan hệ `1 Mangaka - N Assistants`.
* **F03 (Quy tắc tiêu tiền ví)**: Khi tạo Task, hệ thống ưu tiên trừ và khóa tiền từ ví tài trợ `SetupFundBalance`. Nếu ví này hết tiền mới trừ tiếp sang ví được rút `WithdrawableBalance`. **Nghiêm cấm cho phép chi tiêu thấu chi (Overdraft) âm tiền ví.**
* **T03a (Quy tắc tự động hoàn tiền - Auto-Refund)**: Nếu Assistant nhận Task nhưng quá hạn chót **quá 3 ngày** mà không nộp bài vẽ (hoặc không nộp bản sửa đổi), hệ thống phải **tự động Hủy task** và trả lại tiền ký quỹ cho Mangaka.
* **T03b (Quy tắc hủy khẩn cấp - Emergency Cancel)**: Nếu Assistant nhận Task nhưng **không tải các tệp mẫu/tài nguyên về trong vòng 24 giờ**, Mangaka có quyền hủy nhiệm vụ khẩn cấp để thu hồi tiền ký quỹ thuê người khác.
* **T04 (Quy tắc tự động duyệt - Auto-Approve)**: Khi Assistant nộp bài vẽ, nếu Mangaka **không phản hồi (Duyệt hoặc Yêu cầu sửa) trong vòng 3 ngày**, hệ thống phải **tự động Duyệt bài** và chuyển tiền từ ký quỹ sang ví Assistant.
* **T06 (Quy tắc phân chia tranh chấp)**: Khi xảy ra tranh chấp giữa tác giả và trợ lý, Biên tập viên đứng ra phán quyết và có quyền thiết lập tỷ lệ chia tiền (ví dụ: giải ngân 70% cho Trợ lý, trả lại 30% cho tác giả).
* **T07 (Lịch sử phiên bản bài vẽ)**: Bài vẽ do trợ lý tải lên nhiều lần không được ghi đè trực tiếp mà phải được lưu dưới dạng bản ghi lịch sử `TaskVersion` để Mangaka có thể so sánh và duyệt bất kỳ phiên bản nào.
