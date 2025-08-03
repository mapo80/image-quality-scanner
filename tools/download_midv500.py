import os
import random
from pathlib import Path

import midv500
import numpy as np
import cv2
from tqdm import tqdm
from huggingface_hub import snapshot_download


def _generate_synthetic(root: Path, total: int = 50, videos: int = 10) -> None:
    """Create a synthetic dataset when the real MIDV-500 download fails."""
    frames_per_video = max(1, total // videos)
    for vid in range(videos):
        vdir = root / f"synthetic_{vid:02d}"
        vdir.mkdir(parents=True, exist_ok=True)
        for idx in range(frames_per_video):
            img = (np.random.rand(480, 640, 3) * 255).astype("uint8")
            cv2.imwrite(str(vdir / f"{idx:03d}.jpg"), img)


def main() -> None:
    base_dir = Path(os.environ.get("MIDV500_DIR", "data"))
    dataset_root = base_dir / "midv500"
    dataset_root.mkdir(parents=True, exist_ok=True)

    def _download_hf() -> None:
        token = os.environ.get("HF_TOKEN")
        snapshot_download(
            repo_id="Noaman/midv500",
            repo_type="dataset",
            local_dir=str(dataset_root),
            local_dir_use_symlinks=False,
            token=token,
        )

    if not any(dataset_root.iterdir()):
        try:
            _download_hf()
        except Exception as hf_exc:  # pragma: no cover - network dependent
            print(f"huggingface download failed: {hf_exc}")
            try:
                midv500.download_dataset(str(dataset_root))
            except Exception as exc:  # pragma: no cover - network dependent
                print(f"dataset download failed: {exc}. generating synthetic sample")
                _generate_synthetic(dataset_root)

    image_paths = list(dataset_root.rglob("*.jpg")) + list(dataset_root.rglob("*.png"))
    if not image_paths:
        print("no frames found, creating synthetic dataset")
        _generate_synthetic(dataset_root)
        image_paths = list(dataset_root.rglob("*.jpg")) + list(dataset_root.rglob("*.png"))
    if not image_paths:
        raise SystemExit("No image frames found in dataset or synthetic set")

    sample_size = int(os.environ.get("SAMPLE_SIZE", "50"))
    random.seed(42)

    video_dirs = {p.parent for p in image_paths}
    video_dirs = list(video_dirs)
    random.shuffle(video_dirs)

    selected: list[Path] = []
    for vd in video_dirs[:10]:
        frames = [p for p in image_paths if p.parent == vd]
        if frames:
            selected.append(random.choice(frames))

    remaining = [p for p in image_paths if p not in selected]
    needed = sample_size - len(selected)
    if needed > 0 and remaining:
        selected.extend(random.sample(remaining, min(needed, len(remaining))))

    sample_file = base_dir / "sample_50.txt"
    with sample_file.open("w", encoding="utf-8") as fh:
        for p in tqdm(selected, desc="writing sample list"):
            fh.write(str(p.as_posix()) + "\n")
    print(f"Saved {len(selected)} paths to {sample_file}")


if __name__ == "__main__":
    main()

