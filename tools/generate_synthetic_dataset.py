import os
import argparse
import numpy as np
import cv2
import pandas as pd
import sys

# Allow importing compute_metrics from same directory
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.append(SCRIPT_DIR)
from compute_metrics_py import compute_metrics  # noqa: E402


def make_base(size=256):
    """Return a neutral gray image"""
    return np.full((size, size, 3), 128, dtype=np.uint8)


def add_glare(img):
    cv2.rectangle(img, (int(img.shape[1]*0.6), int(img.shape[0]*0.1)),
                  (int(img.shape[1]*0.9), int(img.shape[0]*0.4)), (255, 255, 255), -1)
    return img


def add_noise(img, amt=25):
    noise = np.random.normal(0, amt, img.shape).astype(np.int16)
    noisy = np.clip(img.astype(np.int16) + noise, 0, 255).astype(np.uint8)
    return noisy


def low_contrast(img):
    return cv2.convertScaleAbs(img, alpha=0.3, beta=128*(1-0.3))


def over_exposed(img):
    return cv2.convertScaleAbs(img, alpha=1.0, beta=100)


def under_exposed(img):
    return cv2.convertScaleAbs(img, alpha=1.0, beta=-80)


def color_dominant(img):
    img = img.copy()
    img[:, :, 2] = 255  # strong red channel
    return img


def banding(img):
    img = img.copy()
    for i in range(0, img.shape[0], 20):
        img[i:i+10] = img[i:i+10] + 40
    return np.clip(img, 0, 255).astype(np.uint8)


def motion_blur(img):
    ksize = 15
    kernel = np.zeros((ksize, ksize))
    kernel[int((ksize - 1)/2), :] = np.ones(ksize)
    kernel = kernel / ksize
    return cv2.filter2D(img, -1, kernel)


def blur(img):
    return cv2.GaussianBlur(img, (15, 15), 0)


def main():
    parser = argparse.ArgumentParser(description="Generate synthetic dataset with ground truth metrics")
    parser.add_argument('--out-dir', default=os.path.join('data', 'synthetic'))
    parser.add_argument('--sample', default=os.path.join('data', 'synthetic_sample.txt'))
    parser.add_argument('--gt', default=os.path.join('data', 'synthetic_gt.csv'))
    args = parser.parse_args()

    os.makedirs(args.out_dir, exist_ok=True)
    images = []
    # Create set of images with different issues
    base = make_base()
    images.append(('good', base))
    images.append(('blur', blur(base)))
    images.append(('motion', motion_blur(base)))
    images.append(('glare', add_glare(base.copy())))
    images.append(('noise', add_noise(base)))
    images.append(('low_contrast', low_contrast(base)))
    images.append(('over_exposed', over_exposed(base)))
    images.append(('under_exposed', under_exposed(base)))
    images.append(('color_dom', color_dominant(base)))
    images.append(('banding', banding(base)))

    paths = []
    records = []
    for name, img in images:
        path = os.path.join(args.out_dir, f'{name}.png')
        cv2.imwrite(path, img)
        paths.append(path)
        records.append(compute_metrics(path))

    # write sample list
    with open(args.sample, 'w', encoding='utf-8') as f:
        for p in paths:
            f.write(p + '\n')

    # ground truth metrics
    pd.DataFrame(records).to_csv(args.gt, index=False)
    print(f'Generated {len(paths)} images in {args.out_dir}')
    print(f'Sample list written to {args.sample}')
    print(f'Ground truth metrics written to {args.gt}')


if __name__ == '__main__':
    main()
