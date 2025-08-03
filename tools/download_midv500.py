import os
import random
from pathlib import Path

from tqdm import tqdm
import midv500


def main():
    base_dir = Path(os.environ.get("MIDV500_DIR", "data"))
    dataset_root = base_dir / "midv500"
    dataset_root.mkdir(parents=True, exist_ok=True)

    if not any(dataset_root.iterdir()):
        midv500.download_dataset(str(dataset_root))

    # gather all image frames
    image_paths = list(dataset_root.rglob("*.jpg")) + list(dataset_root.rglob("*.png"))
    if not image_paths:
        raise SystemExit("No image frames found in dataset. Ensure frames are extracted.")

    sample_size = int(os.environ.get("SAMPLE_SIZE", "50"))
    random.seed(42)

    # ensure at least 10 distinct videos
    video_dirs = {p.parent for p in image_paths}
    video_dirs = list(video_dirs)
    random.shuffle(video_dirs)
    selected = []
    for vd in video_dirs[:10]:
        frames = [p for p in image_paths if p.parent == vd]
        if frames:
            selected.append(random.choice(frames))
    remaining = [p for p in image_paths if p not in selected]
    needed = sample_size - len(selected)
    if needed > 0:
        selected.extend(random.sample(remaining, needed))

    sample_file = base_dir / "sample_50.txt"
    with sample_file.open("w", encoding="utf-8") as fh:
        for p in tqdm(selected, desc="writing sample list"):
            fh.write(str(p.as_posix()) + "\n")
    print(f"Saved {len(selected)} paths to {sample_file}")


if __name__ == "__main__":
    main()
