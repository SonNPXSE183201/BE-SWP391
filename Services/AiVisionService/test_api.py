import requests
import json
import io

url_segment = "http://127.0.0.1:8000/api/v1/vision/segment"
url_colorize = "http://127.0.0.1:8000/api/v1/vision/colorize"
# A sample manga image from the internet
payload = {"imageUrl": "https://raw.githubusercontent.com/ultralytics/yolov5/master/data/images/zidane.jpg"}
headers = {"Content-Type": "application/json"}

print("Testing FastAPI Segmentation...")
try:
    response = requests.post(url_segment, json=payload, timeout=15)
    print(f"Status Code: {response.status_code}")
    print(json.dumps(response.json(), indent=2))
except Exception as e:
    print(f"Error: {e}")

print("\nTesting FastAPI Colorization...")
try:
    response_col = requests.post(url_colorize, json=payload, timeout=60)
    print(f"Status Code: {response_col.status_code}")
    if response_col.status_code == 200:
        with open("colorized_result.png", "wb") as f:
            f.write(response_col.content)
        print("Colorized image saved to colorized_result.png")
    else:
        print(response_col.text)
except Exception as e:
    print(f"Error: {e}")
