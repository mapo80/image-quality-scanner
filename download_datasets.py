import os
from roboflow import download_dataset

API_KEY = "tcaZqJkWcEENQPa2p2H1"
os.environ["ROBOFLOW_API_KEY"] = API_KEY

def main():
    datasets = [
        ("https://universe.roboflow.com/pradeep-singh/glare-xw4ce/1", "glare_dataset"),
        ("https://universe.roboflow.com/yolov7-lwj30/blur-nv01n/1", "blur_dataset"),
    ]
    for url, folder in datasets:
        print(f"Downloading {url} to {folder}...")
        download_dataset(url, "yolov8", location=folder)

if __name__ == "__main__":
    main()
