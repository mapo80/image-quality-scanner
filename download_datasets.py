import os
from roboflow import download_dataset

# The API key is read from the ROBOFLOW_API_KEY environment variable.
# This avoids storing credentials in the repository.
api_key = os.environ.get("ROBOFLOW_API_KEY")
if not api_key:
    raise EnvironmentError(
        "Please set the ROBOFLOW_API_KEY environment variable before running this script."
    )
os.environ["ROBOFLOW_API_KEY"] = api_key

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
