#!/usr/bin/env python3
"""Generate PDF files from the sample dataset images.

This script collects all JPEG and PNG images under ``docs/dataset_samples``
and creates one PDF per subdirectory. The resulting files are saved in the
``generated_pdfs`` folder or in a custom directory passed via command line.

Usage::

    python generate_dataset_pdfs.py [output_dir]

Dependencies::

    pip install pillow
"""
from __future__ import annotations

import os
import sys
from typing import Iterable

from PIL import Image

ROOT_DIR = os.path.dirname(os.path.abspath(__file__))
DATASET_DIR = os.path.join(ROOT_DIR, "docs", "dataset_samples")


def list_images(folder: str) -> Iterable[str]:
    """Yield absolute paths of JPEG/PNG images in ``folder`` sorted by name."""
    for name in sorted(os.listdir(folder)):
        if name.lower().endswith((".jpg", ".jpeg", ".png")):
            yield os.path.join(folder, name)


def images_to_pdf(images: Iterable[str], output: str) -> bool:
    """Save ``images`` into a multipage PDF located at ``output``."""
    pages = [Image.open(path).convert("RGB") for path in images]
    if not pages:
        return False
    first, rest = pages[0], pages[1:]
    first.save(output, save_all=True, append_images=rest)
    for img in pages:
        img.close()
    return True


def convert_dataset(out_dir: str) -> None:
    os.makedirs(out_dir, exist_ok=True)
    for item in os.listdir(DATASET_DIR):
        folder = os.path.join(DATASET_DIR, item)
        if not os.path.isdir(folder):
            continue
        images = list(list_images(folder))
        if not images:
            continue
        out_pdf = os.path.join(out_dir, f"{item}.pdf")
        if images_to_pdf(images, out_pdf):
            print(f"Created {out_pdf}")


def main() -> None:
    out_dir = sys.argv[1] if len(sys.argv) > 1 else "generated_pdfs"
    convert_dataset(out_dir)
    print("Done")


if __name__ == "__main__":
    main()
