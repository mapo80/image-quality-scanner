from __future__ import annotations
import csv
import json
import os
import random
import subprocess
import sys
from pathlib import Path
from statistics import fmean

import zipfile
from huggingface_hub import hf_hub_download

from python_quality import check_quality

NUMERIC_METRICS = [
    "BlurScore",
    "MotionBlurScore",
    "GlareArea",
    "Exposure",
    "Contrast",
    "Noise",
    "ColorDominance",
    "BandingScore",
    "BrisqueScore",
]

BOOL_METRICS = [
    "IsBlurry",
    "HasGlare",
    "IsWellExposed",
    "HasLowContrast",
    "HasNoise",
    "HasColorDominance",
    "HasBanding",
]


def download_dataset() -> Path:
    token = os.environ.get("HF_TOKEN")
    if not token:
        raise RuntimeError("HF_TOKEN environment variable is required")
    cache_dir = Path("data/gopro_large")
    zip_path = hf_hub_download(
        repo_id="snah/GOPRO_Large",
        filename="GOPRO_Large.zip",
        repo_type="dataset",
        token=token,
        cache_dir=cache_dir,
    )
    extract_dir = cache_dir / "GoPro_Large"
    test_dir = extract_dir / "test"
    if not test_dir.exists():
        with zipfile.ZipFile(zip_path) as zf:
            members = [
                m
                for m in zf.namelist()
                if m.startswith("test/") and ("/blur/" in m or "/sharp/" in m)
            ]
            for m in members:
                zf.extract(m, extract_dir)
    return test_dir


def run_dotnet(image: Path) -> dict:
    env = os.environ.copy()
    env["PYTHON_EXECUTABLE"] = sys.executable
    proc = subprocess.run(
        ["dotnet", "./bin/DocQualityChecker/DocQualityChecker.dll", "--json", str(image)],
        check=True,
        capture_output=True,
        text=True,
        env=env,
    )
    return json.loads(proc.stdout)


def main() -> None:
    test_dir = download_dataset()
    blur_images = list(test_dir.rglob("blur/*.png")) + list(test_dir.rglob("blur/*.jpg"))
    pairs = []
    for blur_path in blur_images:
        sharp_path = blur_path.parent.parent / "sharp" / blur_path.name
        if sharp_path.exists():
            pairs.append((blur_path, sharp_path))
    if len(pairs) < 50:
        raise SystemExit("Dataset must contain at least 50 image pairs")

    random.seed(42)
    sample = random.sample(pairs, 50)

    records: list[tuple[dict, dict]] = []
    for blur_path, _ in sample:
        py_metrics = check_quality(str(blur_path))
        net_metrics = run_dotnet(blur_path)
        records.append((py_metrics, net_metrics))

    rows: list[list[str | float]] = []
    for metric in NUMERIC_METRICS:
        py_vals = [r[0][metric] for r in records]
        net_vals = [r[1][metric] for r in records]
        py_mean = fmean(py_vals)
        net_mean = fmean(net_vals)
        diffs = [abs(p - n) / max(abs(p), abs(n), 1e-6) * 100 for p, n in zip(py_vals, net_vals)]
        delta = fmean(diffs)
        rows.append([metric, py_mean, net_mean, delta])

    for metric in BOOL_METRICS:
        py_vals = [bool(r[0][metric]) for r in records]
        net_vals = [bool(r[1][metric]) for r in records]
        py_mean = fmean(py_vals)
        net_mean = fmean(net_vals)
        disagreement = [p ^ n for p, n in zip(py_vals, net_vals)]
        delta = fmean(disagreement) * 100
        rows.append([metric, py_mean, net_mean, delta])

    print("| Metric / Flag | Python (μ) | .NET (μ) | Δ% (medio) |")
    print("| --- | --- | --- | --- |")
    for metric, py_mean, net_mean, delta in rows:
        print(f"| {metric} | {py_mean:.3f} | {net_mean:.3f} | {delta:.2f}% |")

    with open("gopro_test_comparison.csv", "w", newline="", encoding="utf-8") as fh:
        writer = csv.writer(fh)
        writer.writerow(["Metric / Flag", "Python (mean)", ".NET (mean)", "DeltaPercent"])
        writer.writerows(rows)


if __name__ == "__main__":
    main()
