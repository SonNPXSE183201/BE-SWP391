FROM tiangolo/uvicorn-gunicorn:python3.10

RUN apt update && \
    apt install -y htop libgl1 libglib2.0-0 && \
    apt clean && rm -rf /var/lib/apt/lists/*

COPY requirements.txt /tmp/requirements.txt
# Install CPU-only PyTorch first to avoid downloading 4GB of CUDA libraries
RUN pip install --no-cache-dir torch torchvision --index-url https://download.pytorch.org/whl/cpu
RUN pip install --no-cache-dir -r /tmp/requirements.txt

WORKDIR /app
COPY . /app

# Tự động tạo thư mục và tải model weights cho Colorizer
RUN mkdir -p manga_colorization/denoising/models && \
    python download_unet.py

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]