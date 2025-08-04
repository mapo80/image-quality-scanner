import os
import time
import math
import argparse
from typing import List

import cv2
import numpy as np
import pandas as pd
from tqdm import tqdm

try:
    from brisque import BRISQUE
except Exception:  # pragma: no cover - library may be missing
    BRISQUE = None


BOOL_METRICS = [
    "IsBlurry",
    "HasGlare",
    "HasNoise",
    "HasLowContrast",
    "HasColorDominance",
    "IsWellExposed",
]

NUM_METRICS = [
    "BlurScore",
    "MotionBlurScore",
    "GlareArea",
    "Exposure",
    "Contrast",
    "Noise",
    "ColorDominance",
    "BandingScore",
    "BrisqueScore",
    "ElapsedMs",
]


def compute_metrics(image_path: str) -> dict:
    start = time.time()
    img = cv2.imread(image_path)
    if img is None:
        raise ValueError(f"Unable to read image: {image_path}")

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    blur_score = cv2.Laplacian(gray, cv2.CV_64F).var()
    is_blurry = blur_score < 100

    grad_x = cv2.Sobel(gray, cv2.CV_64F, 1, 0, ksize=3)
    grad_y = cv2.Sobel(gray, cv2.CV_64F, 0, 1, ksize=3)
    grad_h = np.mean(np.abs(grad_x))
    grad_v = np.mean(np.abs(grad_y))
    motion_blur_score = max(grad_h, 1.0) / max(grad_v, 1.0)

    glare_area = int(np.sum(img > 240))
    has_glare = glare_area > 500

    exposure = float(np.mean(gray))
    is_well_exposed = 80 <= exposure <= 180

    contrast = float(np.std(gray))
    has_low_contrast = contrast < 30

    blurred = cv2.GaussianBlur(gray, (3, 3), 0)
    noise = float(np.mean(cv2.absdiff(gray, blurred)))
    has_noise = noise > 20

    mean_b = np.mean(img[:, :, 0])
    mean_g = np.mean(img[:, :, 1])
    mean_r = np.mean(img[:, :, 2])
    mean_rgb = (mean_r + mean_g + mean_b) / 3.0
    color_dominance = float(max(mean_r, mean_g, mean_b) / (mean_rgb + 1e-6))
    has_color_dominance = color_dominance > 1.5

    mean_rows = np.mean(gray, axis=1)
    mean_cols = np.mean(gray, axis=0)
    banding_score = float(np.var(mean_rows) + np.var(mean_cols))

    if BRISQUE is not None:
        try:
            rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            brisque_score = float(BRISQUE().score(rgb))
        except Exception:
            brisque_score = math.nan
    else:
        brisque_score = math.nan

    elapsed_ms = (time.time() - start) * 1000.0

    return {
        "path": image_path,
        "BlurScore": float(blur_score),
        "IsBlurry": bool(is_blurry),
        "MotionBlurScore": float(motion_blur_score),
        "GlareArea": int(glare_area),
        "HasGlare": bool(has_glare),
        "Exposure": float(exposure),
        "IsWellExposed": bool(is_well_exposed),
        "Contrast": float(contrast),
        "HasLowContrast": bool(has_low_contrast),
        "Noise": float(noise),
        "HasNoise": bool(has_noise),
        "ColorDominance": float(color_dominance),
        "HasColorDominance": bool(has_color_dominance),
        "BandingScore": float(banding_score),
        "BrisqueScore": brisque_score,
        "ElapsedMs": float(elapsed_ms),
    }


def read_paths(file_path: str) -> List[str]:
    with open(file_path, "r", encoding="utf-8") as f:
        return [line.strip() for line in f if line.strip()]


def main():
    parser = argparse.ArgumentParser(description="Compute quality metrics using OpenCV/NumPy")
    parser.add_argument(
        "--sample",
        default=os.path.join("data", "sample_50.txt"),
        help="File with list of image paths",
    )
    parser.add_argument(
        "--output",
        default=os.path.join("reports", "metrics_per_image_py.csv"),
        help="Output CSV path",
    )
    args = parser.parse_args()

    paths = read_paths(args.sample)
    results = []
    for p in tqdm(paths, desc="Processing"):
        try:
            results.append(compute_metrics(p))
        except Exception as exc:  # pragma: no cover
            print(f"Error processing {p}: {exc}")

    df = pd.DataFrame(results)
    os.makedirs(os.path.dirname(args.output), exist_ok=True)
    df.to_csv(args.output, index=False)

    if BRISQUE is None:
        print("Warning: BRISQUE not available, values set to NaN")


if __name__ == "__main__":
    main()
