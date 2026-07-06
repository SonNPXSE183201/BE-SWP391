from huggingface_hub import hf_hub_download
import os
import shutil

print("Downloading YOLOv8 Manga Frame Seg model...")
model_path = hf_hub_download(repo_id="TheBlindMaster/yolov8n-manga-frame-seg", filename="best.pt")

os.makedirs("./models/manga_model", exist_ok=True)
dest_path = "./models/manga_model/best.pt"
shutil.copyfile(model_path, dest_path)

print(f"Model downloaded and copied to {dest_path}")
