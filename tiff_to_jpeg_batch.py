#!/usr/bin/env python3
"""
Batch convert all TIFF files in a directory (and subdirectories) to JPEG format, with detailed logging.
Usage:
    python tiff_to_jpeg_batch.py /path/to/input_dir /path/to/output_dir --quality 90 --verbose

Dependencies:
    pip install pillow
"""
import os
import sys
import logging
from PIL import Image

def convert_tiff_to_jpeg(input_dir, output_dir, quality=85):
    """
    Walk through input_dir, convert each .tif/.tiff file to .jpg,
    preserving subdirectory structure, with detailed logging.

    Returns:
        tuple: (converted_count, failed_count, total_tiffs)
    """
    logging.info(f"Starting conversion: '{input_dir}' -> '{output_dir}', quality={quality}")

    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
        logging.info(f"Created output directory: {output_dir}")

    total = 0
    converted = 0
    failed = 0

    for root, _, files in os.walk(input_dir):
        logging.debug(f"Scanning directory: {root}")
        for filename in files:
            full_path = os.path.join(root, filename)
            total += 1
            if not filename.lower().endswith(('.tif', '.tiff')):
                logging.debug(f"Skipping non-TIFF file: {full_path}")
                continue

            logging.info(f"Converting TIFF: {full_path}")
            try:
                with Image.open(full_path) as img:
                    rgb_im = img.convert('RGB')

                    rel_path = os.path.relpath(root, input_dir)
                    dest_dir = os.path.join(output_dir, rel_path)
                    os.makedirs(dest_dir, exist_ok=True)

                    base_name = os.path.splitext(filename)[0]
                    jpeg_path = os.path.join(dest_dir, base_name + '.jpg')
                    rgb_im.save(jpeg_path, 'JPEG', quality=quality)

                    converted += 1
                    logging.info(f"Saved JPEG: {jpeg_path}")
            except Exception as e:
                failed += 1
                logging.error(f"Failed to convert {full_path}: {e}", exc_info=True)

    logging.info(f"Conversion complete. Total TIFF files: {total}, Converted: {converted}, Failed: {failed}")
    return converted, failed, total


def main():
    import argparse
    parser = argparse.ArgumentParser(
        description='Batch convert TIFF images to JPEG format with logging'
    )
    parser.add_argument(
        'input_dir',
        help='Directory containing TIFF images'
    )
    parser.add_argument(
        'output_dir',
        help='Directory to save JPEG images'
    )
    parser.add_argument(
        '--quality',
        type=int,
        default=85,
        help='JPEG quality (1-100), higher means better quality and larger file size'
    )
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Enable debug-level logging'
    )
    args = parser.parse_args()

    log_level = logging.DEBUG if args.verbose else logging.INFO
    logging.basicConfig(
        level=log_level,
        format='%(asctime)s [%(levelname)s] %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )

    if not os.path.isdir(args.input_dir):
        logging.error(f"Input directory does not exist or is not a directory: {args.input_dir}")
        sys.exit(1)

    converted, failed, total = convert_tiff_to_jpeg(args.input_dir, args.output_dir, args.quality)

    if total == 0:
        logging.warning(f"No TIFF files found in '{args.input_dir}'")
    if failed > 0:
        logging.warning(f"{failed} file(s) failed to convert. Check above logs for details.")
    else:
        logging.info("All TIFF files converted successfully.")

if __name__ == '__main__':
    main()
