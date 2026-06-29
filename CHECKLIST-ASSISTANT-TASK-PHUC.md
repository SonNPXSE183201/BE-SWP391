# Checklist nghiệp vụ Task — Assistant (Phúc)

> Nguồn: `SRS.md` — F2.3, F2.6–F2.8, Luồng 3, quy tắc T01–T09.  
> Mục tiêu: triển khai/QA luồng Assistant từ lúc nhận task đến khi task kết thúc.

---

## A. Phạm vi Assistant (F2.6 – F2.8)

| ID | Chức năng | Tiêu chí hoàn thành (Done khi…) |
|----|-----------|----------------------------------|
| **F2.6** | Hàng đợi task | Danh sách chỉ task của Assistant đăng nhập; lọc theo trạng thái; sắp xếp theo deadline / ưu tiên đúng SRS. |
| **F2.7** | Tải tài nguyên | Tải được `BaseLayer` + tài liệu đính kèm (style guide, brush…); file không hỏng/nén lỗi. |
| **F2.8** | Nộp bài vẽ | Upload **PNG nền trong suốt**; sau nộp có thông báo tới Mangaka. |

**Checklist QA nhanh (F2.6–F2.8)**

- [ ] Assistant `Active` thấy task `Pending` / đã nhận; Assistant khác không thấy task của người khác.
- [ ] Nhận task → trạng thái chuyển sang đang làm (theo API hiện tại).
- [ ] Download base layer + resource folder thành công.
- [ ] Upload file không phải PNG trong suốt → bị chặn / báo lỗi rõ.
- [ ] Upload PNG hợp lệ → Mangaka nhận notification.

---

## B. Luồng 3 — Sản xuất & thuê trợ lý (góc nhìn Assistant)

1. **Mangaka giao task (F2.3)** — Mangaka khoanh vùng, set thù lao, deadline, Z-Index → hệ thống **khóa escrow** trên ví Mangaka, task `Pending`, notify Assistant.
2. **Assistant thực hiện** — F2.6 → nhận task → F2.7 tải mẫu → F2.8 nộp PNG.
3. **Gia hạn (F2.12 / F1.10 / T08)** — Xin gia hạn **trước** deadline; tối đa **1 lần/task**; nếu Mangaka duyệt: deadline mới = cũ + số ngày duyệt.
4. **Nghiệm thu (phía Mangaka, ảnh hưởng Assistant)**
   - Approve → task `Approved`, tiền escrow → ví Assistant.
   - Revision → escrow **vẫn khóa**; nộp lại tạo `TaskVersion` (T07).
   - Cancel → hoàn escrow về Mangaka.

**Checklist luồng end-to-end**

- [ ] Task mới xuất hiện trên queue Assistant sau khi Mangaka tạo (escrow đã lock).
- [ ] Assistant nộp bài → Mangaka thấy submission / composite pipeline (F2.4) không chặn luồng Assistant.
- [ ] Sau Approve → số dư ví Assistant tăng đúng `Payment_Amount`.
- [ ] Sau Revision → Assistant nộp version mới, không ghi đè file cũ (T07).

---

## C. Quy tắc tự động & trạng thái (T01–T09)

| Rule | Ý nghĩa | Assistant / BE cần đảm bảo |
|------|---------|----------------------------|
| **T01** | Tạo task | Chỉ khi Mangaka đủ số dư; lock ngay khi tạo. |
| **T02** | Revision | Escrow không unlock khi yêu cầu sửa; deadline revision +24h/+48h (Mangaka set). |
| **T03a** | Trễ >3 ngày | Auto-cancel + refund Mangaka nếu đã nhận mà không nộp (hoặc không nộp bản sửa). |
| **T03b** | Không tải mẫu 24h | Mangaka được hủy khẩn cấp, thu hồi escrow. |
| **T04** | Auto-approve 3 ngày | Mangaka im lặng 3 ngày sau nộp → `Approved` + chuyển tiền; nhắc ngày 2; **pause** nếu Mangaka `On_Leave`. |
| **T05** | Hủy series | Hủy task `Pending` + refund; task đang làm: grace 24h rồi thanh toán partial / hoàn phần còn lại. |
| **T06** | Dispute | Editor chia % escrow (Assistant / Mangaka). |
| **T07** | TaskVersion | Mỗi lần nộp lưu version mới, không overwrite. |
| **T08** | Gia hạn | Tối đa 1 request gia hạn / task. |
| **T09** | AssistantProfile | Job cập nhật KPI khi task terminal: `Approved`, `Closed`, `Cancelled`. |

**Checklist automation / job**

- [ ] Background job T03a/T04 chạy đúng mốc thời gian (mock clock hoặc seed task quá hạn).
- [ ] T04 tôn trọng `On_Leave` của Mangaka (timer dừng).
- [ ] T09 chạy sau terminal state; `AssistantProfile` đổi metric.

---

## D. Gợi ý phân công cho Phúc (BE + FE)

### Backend (ưu tiên)

- [ ] API queue Assistant: filter/sort (F2.6) — `TasksController` / `TasksService`.
- [ ] Download resource + base layer (F2.7) — storage URL, quyền Assistant được assign.
- [ ] Upload submission: validate PNG alpha / MIME (F2.8).
- [ ] `TaskVersion` on re-upload (T07).
- [ ] Extension request: 1 lần/task (T08) + duyệt Mangaka (F1.10).
- [ ] Hook notification sau accept / submit.

### Frontend

- [ ] `TaskQueueFeature` + `AssistantTaskDetailModal`: list, filter, deadline sort (F2.6).
- [ ] Nút tải tài nguyên + trạng thái loading/lỗi (F2.7).
- [ ] Form upload PNG + preview (F2.8).
- [ ] UI xin gia hạn + trạng thái đã dùng 1 lần (T08).

---

## E. Dữ liệu test gợi ý

| Vai trò | Tài khoản seed (mật khẩu `12345`) |
|---------|-----------------------------------|
| Mangaka | `mangaka1` |
| Assistant Active | `assistant1` |
| Assistant Pending | `assistant_pending` (không được nhận task) |

---

*Tài liệu checklist — cập nhật khi SRS thay đổi.*
