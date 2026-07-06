# AiVisionService (Manga AI)

This is the Python Microservice dedicated to AI capabilities (Computer Vision) for the Manga Publishing System.
It uses **FastAPI**, **YOLOv8** (for panel segmentation), and **U-Net** (for manga page colorization).

## 🛠️ Yêu cầu môi trường (Prerequisites)
1. **Python 3.10+** (Khuyên dùng Python 3.11 hoặc 3.12).
2. Công cụ quản lý package: Sử dụng **`uv`** (nhanh hơn pip rất nhiều) hoặc `pip` truyền thống.

## 🚀 Hướng dẫn Cài đặt (Installation)

### Bước 1: Di chuyển vào thư mục Service
Mở Terminal và trỏ vào thư mục của service:
```bash
cd Services/AiVisionService
```

### Bước 2: Tạo môi trường ảo (Virtual Environment)
Tạo một môi trường ảo có tên là `venv` trong thư mục này:
```bash
# Sử dụng uv (Khuyên dùng)
uv venv

# Hoặc sử dụng Python tiêu chuẩn
python -m venv venv
```

### Bước 3: Kích hoạt môi trường ảo
```bash
# Trên Windows
.\venv\Scripts\activate

# Trên Linux/macOS
source venv/bin/activate
```

### Bước 4: Cài đặt thư viện (Dependencies)
```bash
# Sử dụng uv
uv pip install -r requirements.txt

# Hoặc sử dụng pip
pip install -r requirements.txt
```

---

## 🏃 Hướng dẫn Chạy (Running the Service)

### Cách 1: Chạy tự động thông qua Script của dự án (Khuyên dùng)
Ở thư mục gốc của dự án (`d:\SWP391\Project\BE-SWP391`), bạn chỉ cần chạy file:
```bash
run-be.bat
```
Script này sẽ tự động khởi động 3 terminal: Gateway, Manga Web API, và tự động gọi `uvicorn` trong môi trường `venv` của AiVisionService.

### Cách 2: Chạy thủ công
Nếu bạn chỉ muốn chạy riêng lẻ API của AI để test:
```bash
cd Services/AiVisionService
.\venv\Scripts\activate
python -m uvicorn main:app --port 8000 --reload
```

Sau khi chạy thành công, bạn có thể truy cập tài liệu API tại:
👉 **Swagger UI:** `http://localhost:8000/docs`

---

## 🧩 Các Endpoints chính
1. `POST /api/v1/vision/segment`: Nhận diện tọa độ các khung truyện (Bounding Boxes).
2. `POST /api/v1/vision/segment/draw`: Vẽ trực tiếp các viền đỏ xung quanh khung truyện được nhận diện (Dùng để test).
3. `POST /api/v1/vision/colorize`: Tự động tô màu cho bản thảo truyện tranh đen trắng.

## ⚠️ Lưu ý kỹ thuật
- **Download Model**: Lần đầu tiên chạy, thư viện `ultralytics` (YOLO) và logic của U-Net có thể mất vài phút để tự động tải pre-trained models (file `.pt` hoặc `.pth`) về máy. Hãy kiên nhẫn.
- **Port xung đột**: Mặc định chạy ở port `8000`. Nếu port này bị chiếm, hãy dùng lệnh tắt process hoặc restart lại bằng `run-be.bat` (script đã tích hợp tự động kill port cũ).