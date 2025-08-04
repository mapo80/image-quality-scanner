from __future__ import annotations
import math
from tools.compute_metrics_py import compute_metrics

def check_quality(path: str) -> dict:
    """Compute quality metrics for the given image.

    Parameters
    ----------
    path: str
        Path to the image file.

    Returns
    -------
    dict
        Dictionary of quality metrics and flags.
    """
    res = compute_metrics(path)
    # Ensure HasBanding flag exists using same threshold as .NET (0.5)
    banding = res.get("BandingScore")
    if banding is not None and not math.isnan(banding):
        res["HasBanding"] = bool(banding > 0.5)
    else:
        res["HasBanding"] = False
    res.pop("path", None)
    return res
