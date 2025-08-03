import argparse
import os
import time
from pathlib import Path

import polars as pl
from tqdm import tqdm
import pythonnet


def load_checker():
    assembly_dir = Path('bin/DocQualityChecker').resolve()
    os.environ.setdefault('DOTNET_ROOT', str(Path.home() / 'dotnet'))
    os.environ.setdefault('PYTHONNET_RUNTIME', 'coreclr')
    runtime_config = str(assembly_dir / 'DocQualityChecker.runtimeconfig.json')
    deps = str(assembly_dir / 'DocQualityChecker.deps.json')
    pythonnet.load(runtime_config=runtime_config,
                   assembly_dir=[str(assembly_dir)],
                   deps_file=deps)
    import System
    System.Reflection.Assembly.LoadFile(str(assembly_dir / 'SkiaSharp.dll'))
    System.Reflection.Assembly.LoadFile(str(assembly_dir / 'PDFtoImage.dll'))
    asm = System.Reflection.Assembly.LoadFile(str(assembly_dir / 'DocQualityChecker.dll'))
    dq_type = asm.GetType('DocQualityChecker.DocumentQualityChecker')
    settings_type = asm.GetType('DocQualityChecker.QualitySettings')
    checker = System.Activator.CreateInstance(dq_type)
    settings = System.Activator.CreateInstance(settings_type)
    import SkiaSharp
    from System.IO import File
    return checker, settings, SkiaSharp, File


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--sample', default='data/sample_50.txt')
    parser.add_argument('--out', default='data/smoke_metrics.csv')
    args = parser.parse_args()

    checker, settings, SkiaSharp, File = load_checker()

    with open(args.sample, 'r', encoding='utf-8') as fh:
        paths = [l.strip() for l in fh if l.strip()]

    rows = []
    for p in tqdm(paths, desc='checking'):
        fs = File.OpenRead(p)
        bmp = SkiaSharp.SKBitmap.Decode(fs)
        start = time.time()
        res = checker.CheckQuality(bmp, settings)
        elapsed = (time.time() - start) * 1000
        rows.append({
            'path': p,
            'BlurScore': float(res.BlurScore),
            'IsBlurry': bool(res.IsBlurry),
            'GlareArea': float(res.GlareArea),
            'HasGlare': bool(res.HasGlare),
            'Exposure': float(res.Exposure),
            'IsWellExposed': bool(res.IsWellExposed),
            'ElapsedMs': elapsed,
        })
        fs.Close()

    df = pl.DataFrame(rows)
    print(df.select(['path','BlurScore','IsBlurry','GlareArea','HasGlare','Exposure','IsWellExposed']))

    # failure rates
    fail_rates = {
        'IsBlurry': df['IsBlurry'].mean(),
        'HasGlare': df['HasGlare'].mean(),
        '!IsWellExposed': (~df['IsWellExposed']).mean(),
    }
    pl.DataFrame([
        {'Metric': k, 'FailRate': v} for k, v in fail_rates.items()
    ]).write_csv(args.out)
    print('Aggregated metrics saved to', args.out)


if __name__ == '__main__':
    main()
