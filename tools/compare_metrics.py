import argparse
import os
import warnings

import numpy as np
import pandas as pd
from tabulate import tabulate

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


def load_csv(path: str) -> pd.DataFrame:
    return pd.read_csv(path)


def compare(dotnet_df: pd.DataFrame, py_df: pd.DataFrame) -> pd.DataFrame:
    records = []
    for col in NUM_METRICS:
        if col not in dotnet_df.columns or col not in py_df.columns:
            continue
        a = dotnet_df[col]
        b = py_df[col]
        mask = (~a.isna()) & (~b.isna())
        count = int(mask.sum())
        if count == 0:
            warnings.warn(f"No valid values for {col}, skipping")
            mean_err = np.nan
        else:
            rel_err = (a[mask] - b[mask]).abs() / np.maximum(np.maximum(a[mask].abs(), b[mask].abs()), 1e-6)
            mean_err = rel_err.mean()
        status = "OK" if np.isnan(mean_err) or mean_err <= 0.10 else "FAIL"
        records.append(
            {
                "Metric": col,
                "Type": "numeric",
                "MeanRelError/DisagreeRate": mean_err,
                "Threshold": 0.10,
                "Status": status,
                "CountCompared": count,
            }
        )

    for col in BOOL_METRICS:
        if col not in dotnet_df.columns or col not in py_df.columns:
            continue
        a = dotnet_df[col].astype(bool)
        b = py_df[col].astype(bool)
        mask = (~dotnet_df[col].isna()) & (~py_df[col].isna())
        count = int(mask.sum())
        if count == 0:
            warnings.warn(f"No valid values for {col}, skipping")
            rate = np.nan
        else:
            rate = (a[mask] ^ b[mask]).mean()
        status = "OK" if np.isnan(rate) or rate <= 0.10 else "FAIL"
        records.append(
            {
                "Metric": col,
                "Type": "bool",
                "MeanRelError/DisagreeRate": rate,
                "Threshold": 0.10,
                "Status": status,
                "CountCompared": count,
            }
        )
    return pd.DataFrame(records)


def evaluate_against_gt(dotnet_df: pd.DataFrame, py_df: pd.DataFrame, gt_df: pd.DataFrame) -> pd.DataFrame:
    records = []
    for col in gt_df.columns:
        if col == "path":
            continue
        gt_series = gt_df[col]
        if col in BOOL_METRICS:
            gt_bool = gt_series.astype(bool)
            net = dotnet_df[col].astype(bool)
            py = py_df[col].astype(bool)
            mask = gt_bool.notna() & net.notna() & py.notna()
            count = int(mask.sum())
            if count == 0:
                acc_net = acc_py = np.nan
            else:
                acc_net = (net[mask] == gt_bool[mask]).mean()
                acc_py = (py[mask] == gt_bool[mask]).mean()
            records.append(
                {
                    "Metric": col,
                    "Type": "bool",
                    "NetAccuracy": acc_net,
                    "PyAccuracy": acc_py,
                    "CountCompared": count,
                }
            )
        else:
            gt_num = gt_series.astype(float)
            net = dotnet_df[col].astype(float)
            py = py_df[col].astype(float)
            mask = gt_num.notna() & net.notna() & py.notna()
            count = int(mask.sum())
            if count == 0:
                mae_net = mae_py = np.nan
            else:
                mae_net = (net[mask] - gt_num[mask]).abs().mean()
                mae_py = (py[mask] - gt_num[mask]).abs().mean()
            records.append(
                {
                    "Metric": col,
                    "Type": "numeric",
                    "NetMAE": mae_net,
                    "PyMAE": mae_py,
                    "CountCompared": count,
                }
            )
    return pd.DataFrame(records)


def main():
    parser = argparse.ArgumentParser(description="Compare .NET and Python metrics")
    parser.add_argument("--dotnet", default=os.path.join("reports", "metrics_per_image_dotnet.csv"))
    parser.add_argument("--python", default=os.path.join("reports", "metrics_per_image_py.csv"))
    parser.add_argument("--gt")
    args = parser.parse_args()

    dotnet_df = load_csv(args.dotnet)
    py_df = load_csv(args.python)

    diff_df = compare(dotnet_df, py_df)
    os.makedirs("reports", exist_ok=True)
    diff_df.to_csv(os.path.join("reports", "metrics_diff.csv"), index=False)
    print(tabulate(diff_df, headers="keys", tablefmt="github", floatfmt=".4f"))

    if args.gt:
        gt_df = load_csv(args.gt)
        gt_metrics = evaluate_against_gt(dotnet_df, py_df, gt_df)
        gt_metrics.to_csv(os.path.join("reports", "metrics_against_gt.csv"), index=False)
    else:
        print("No ground-truth provided.")


if __name__ == "__main__":
    main()
