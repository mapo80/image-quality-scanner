from __future__ import annotations
import csv
import json
import random
import subprocess
from pathlib import Path
from statistics import fmean

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

def run_dotnet(image: Path) -> dict:
    proc = subprocess.run(
        ["dotnet", "./bin/DocQualityChecker/DocQualityChecker.dll", "--json", str(image)],
        check=True,
        capture_output=True,
        text=True,
    )
    return json.loads(proc.stdout)


def main() -> None:
    dataset = Path("./midv500")
    images = list(dataset.rglob("*.png")) + list(dataset.rglob("*.jpg"))
    if len(images) < 100:
        raise SystemExit("Dataset must contain at least 100 images")
    random.seed(0)
    sample = random.sample(images, 100)

    records: list[tuple[dict, dict]] = []
    for img in sample:
        py_metrics = check_quality(str(img))
        net_metrics = run_dotnet(img)
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

    with open("midv500_comparison.csv", "w", newline="", encoding="utf-8") as fh:
        writer = csv.writer(fh)
        writer.writerow(["Metric / Flag", "Python (mean)", ".NET (mean)", "DeltaPercent"])
        writer.writerows(rows)


if __name__ == "__main__":
    main()
