####################################### IMPORT #################################
import json
import pandas as pd
from PIL import Image
from loguru import logger
import sys

from fastapi import FastAPI, File, status
from fastapi.responses import RedirectResponse
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
from fastapi.exceptions import HTTPException

from io import BytesIO

from app import get_image_from_bytes
from app import detect_sample_model
from app import add_bboxs_on_img
from app import get_bytes_from_image

####################################### logger #################################

logger.remove()
logger.add(
    sys.stderr,
    colorize=True,
    format="<green>{time:HH:mm:ss}</green> | <level>{message}</level>",
    level=10,
)
logger.add("log.log", rotation="1 MB", level="DEBUG", compression="zip")

###################### FastAPI Setup #############################

# title
app = FastAPI(
    title="Object Detection FastAPI Template",
    description="""Obtain object value out of image
                    and return image and json result""",
    version="2023.1.31",
)

# This function is needed if you want to allow client requests 
# from specific domains (specified in the origins argument) 
# to access resources from the FastAPI server, 
# and the client and server are hosted on different domains.
origins = [
    "http://localhost",
    "http://localhost:8008",
    "*"
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.on_event("startup")
def save_openapi_json():
    '''This function is used to save the OpenAPI documentation 
    data of the FastAPI application to a JSON file. 
    The purpose of saving the OpenAPI documentation data is to have 
    a permanent and offline record of the API specification, 
    which can be used for documentation purposes or 
    to generate client libraries. It is not necessarily needed, 
    but can be helpful in certain scenarios.'''
    openapi_data = app.openapi()
    # Change "openapi.json" to desired filename
    with open("openapi.json", "w") as file:
        json.dump(openapi_data, file)

# redirect
@app.get("/", include_in_schema=False)
async def redirect():
    return RedirectResponse("/docs")


@app.get('/healthcheck', status_code=status.HTTP_200_OK)
def perform_healthcheck():
    '''
    It basically sends a GET request to the route & hopes to get a "200"
    response code. Failing to return a 200 response code just enables
    the GitHub Actions to rollback to the last version the project was
    found in a "working condition". It acts as a last line of defense in
    case something goes south.
    Additionally, it also returns a JSON response in the form of:
    {
        'healtcheck': 'Everything OK!'
    }
    '''
    return {'healthcheck': 'Everything OK!'}


######################### Support Func #################################

def crop_image_by_predict(image: Image, predict: pd.DataFrame(), crop_class_name: str,) -> Image:
    """Crop an image based on the detection of a certain object in the image.
    
    Args:
        image: Image to be cropped.
        predict (pd.DataFrame): Dataframe containing the prediction results of object detection model.
        crop_class_name (str, optional): The name of the object class to crop the image by. if not provided, function returns the first object found in the image.
    
    Returns:
        Image: Cropped image or None
    """
    crop_predicts = predict[(predict['name'] == crop_class_name)]

    if crop_predicts.empty:
        raise HTTPException(status_code=400, detail=f"{crop_class_name} not found in photo")

    # if there are several detections, choose the one with more confidence
    if len(crop_predicts) > 1:
        crop_predicts = crop_predicts.sort_values(by=['confidence'], ascending=False)

    crop_bbox = crop_predicts[['xmin', 'ymin', 'xmax','ymax']].iloc[0].values
    # crop
    img_crop = image.crop(crop_bbox)
    return(img_crop)


######################### MAIN Func #################################


######################### MAIN Func #################################
import requests
# pyrefly: ignore [missing-import]
from pydantic import BaseModel

class SegmentationRequest(BaseModel):
    imageUrl: str

@app.post("/api/v1/vision/segment")
def segment_image(request: SegmentationRequest):
    """
    Object Detection from an image URL.
    Returns:
        dict: JSON format matching AiSegmentationResultDto
    """
    result = {
        'success': False,
        'panels': []
    }

    try:
        # Download image
        response = requests.get(request.imageUrl, timeout=10)
        response.raise_for_status()
        file_bytes = response.content
        
        # Step 2: Convert the image file to an image object
        input_image = get_image_from_bytes(file_bytes)

        # Step 3: Predict from model
        predict = detect_sample_model(input_image)

        # Step 4: Map to C# DTO format
        for index, row in predict.iterrows():
            bbox = {
                "x": float(row['xmin']),
                "y": float(row['ymin']),
                "width": float(row['xmax'] - row['xmin']),
                "height": float(row['ymax'] - row['ymin']),
                "label": str(row['name'])
            }
            result['panels'].append(bbox)

        result['success'] = True
        logger.info(f"Segmented {len(result['panels'])} panels.")

    except Exception as e:
        logger.error(f"Segmentation failed: {str(e)}")
        result['success'] = False
        
    return result

@app.post("/api/v1/vision/segment/draw")
def segment_image_draw(request: SegmentationRequest):
    """
    Object Detection from an image URL and returns drawn image.
    Returns:
        StreamingResponse: JPEG image binary
    """
    try:
        response = requests.get(request.imageUrl, timeout=10)
        response.raise_for_status()
        file_bytes = response.content
        
        input_image = get_image_from_bytes(file_bytes)
        predict = detect_sample_model(input_image)
        
        # Draw bounding boxes
        result_image = add_bboxs_on_img(input_image, predict)
        result_bytes = get_bytes_from_image(result_image)
        
        return StreamingResponse(result_bytes, media_type="image/jpeg")
        
    except Exception as e:
        logger.error(f"Segmentation draw failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

import os
sys.path.append(os.path.join(os.path.dirname(__file__), "manga_colorization"))
from colorizator import MangaColorizator
import torch
import numpy as np
import cv2

# Initialize colorizer
device = 'cuda' if torch.cuda.is_available() else 'cpu'
generator_path = os.path.join(os.path.dirname(__file__), "manga_colorization/networks/generator.zip")
extractor_path = os.path.join(os.path.dirname(__file__), "manga_colorization/networks/extractor.pth")
try:
    colorizer = MangaColorizator(device, generator_path, extractor_path)
    logger.info("MangaColorizator initialized successfully.")
except Exception as e:
    logger.error(f"Failed to load MangaColorizator: {e}")
    colorizer = None

@app.post("/api/v1/vision/colorize")
def colorize_image(request: SegmentationRequest):
    """
    Object Colorization from an image URL using U-Net.
    Returns:
        StreamingResponse: PNG image binary
    """
    if colorizer is None:
        raise HTTPException(status_code=500, detail="Colorizer model not loaded")

    try:
        # Download image
        response = requests.get(request.imageUrl, timeout=15)
        response.raise_for_status()
        
        # Read image to numpy array (equivalent to plt.imread)
        file_bytes = np.frombuffer(response.content, np.uint8)
        image = cv2.imdecode(file_bytes, cv2.IMREAD_COLOR)
        if image is None:
            raise HTTPException(status_code=400, detail="Invalid image file")
            
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        # Colorize
        colorizer.set_image(image, size=576, apply_denoise=True, denoise_sigma=25)
        result = colorizer.colorize() # returns float numpy array [0, 1] RGB

        # Convert to PNG bytes
        result_img = (result * 255).astype(np.uint8)
        result_img = cv2.cvtColor(result_img, cv2.COLOR_RGB2BGR)
        is_success, buffer = cv2.imencode(".png", result_img)
        
        if not is_success:
            raise Exception("Failed to encode image")
            
        io_buf = BytesIO(buffer)
        
        return StreamingResponse(io_buf, media_type="image/png")
        
    except Exception as e:
        logger.error(f"Colorization failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
