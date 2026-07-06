import gdown
import os
import zipfile

# Download generator
print("Downloading generator...")
gdown.download(id="1qmxUEKADkEM4iYLp1fpPLLKnfZ6tcF-t", output="manga_colorization/networks/generator.zip", quiet=False)

# Download denoiser
print("Downloading denoiser...")
gdown.download(id="161oyQcYpdkVdw8gKz_MA8RD-Wtg9XDp3", output="manga_colorization/denoising/models/denoiser.pth", quiet=False)

print("Extracting generator.zip...")
with zipfile.ZipFile("manga_colorization/networks/generator.zip", 'r') as zip_ref:
    zip_ref.extractall("manga_colorization/networks/")

print("Done downloading colorization weights!")
