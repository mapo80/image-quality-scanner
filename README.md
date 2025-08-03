# Image Quality Scanner

Questa libreria fornisce semplici controlli di qualità su immagini di documenti tramite API gestite.
L'implementazione utilizza [SkiaSharp](https://github.com/mono/SkiaSharp) per la manipolazione delle immagini.
È inoltre possibile analizzare documenti **PDF** grazie alla libreria [PDFtoImage](https://www.nuget.org/packages/PDFtoImage). Il metodo `CheckQuality(Stream, QualitySettings, int? pageIndex)` elabora la singola pagina indicata o, in assenza di parametro, tutte le pagine del file.

## Requisiti

 - .NET 9 SDK

Le dipendenze NuGet vengono ripristinate automaticamente durante la fase di build/test.



## Quick Python smoke-test

```bash
pip install -r requirements.txt
dotnet publish DocQualityChecker -c Release -o bin/DocQualityChecker
# esporta il token di Hugging Face (opzionale)
export HF_TOKEN=<token>
# scarica il dataset (o genera campione sintetico)
python tools/download_midv500.py
# lancia il quality check via pythonnet
python run_smoke_test.py

## Quick .NET smoke-test

```bash
# Esegue la verifica sulle immagini di esempio incluse nel repository
dotnet run --project DocQualitySmoke -- --sample docs/dataset_samples/sample_paths.txt --outDir docs/dataset_samples
```

Esempio di output:

```
shape: (3, 7)
┌─────────────────────────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┬──────────────┐
│ path                                    ┆ BlurScor ┆ IsBlurry ┆ GlareAre ┆ HasGlare ┆ Exposure ┆ IsWellExpose │
│                                         ┆ e        ┆          ┆ a        ┆          ┆          ┆ d            │
╞═════════════════════════════════════════╪══════════╪══════════╪══════════╪══════════╪══════════╪══════════════╡
│ data/midv500/synthetic_00/000.jpg       ┆ 123.4    ┆ false    ┆ 0        ┆ false    ┆ 120.1    ┆ true         │
│ …                                       ┆ …        ┆ …        ┆ …        ┆ …        ┆ …        ┆ …            │
└─────────────────────────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┴──────────────┘
Aggregated metrics saved to data/smoke_metrics.csv
```

## Quick .NET smoke-test

```bash
python tools/download_midv500.py
dotnet run --project DocQualitySmoke -- --encode
```

I file `.base64` delle immagini elaborate saranno salvati in `data/encoded_samples/`.

### Metriche campione MIDV-500 (100 immagini)

| Metric              | PassRate | Mean     | Std      | Min    | Max       | Count |
|:--------------------|:---------|:---------|:---------|:-------|:----------|------:|
| IsBlurry            | 0.67     |          |          |        |           |   100 |
| HasGlare            | 0.55     |          |          |        |           |   100 |
| HasNoise            | 1.00     |          |          |        |           |   100 |
| HasLowContrast      | 0.76     |          |          |        |           |   100 |
| HasColorDominance   | 1.00     |          |          |        |           |   100 |
| !IsWellExposed      | 0.88     |          |          |        |           |   100 |
| BlurScore           |          | 306.89   | 282.63   | 25.52  | 1127.47   |   100 |
| MotionBlurScore     |          | 1.17     | 0.25     | 1.00   | 2.51      |   100 |
| GlareArea           |          | 14800.45 | 31764.28 | 0.00   | 108543.00 |   100 |
| Exposure            |          | 132.74   | 28.51    | 69.52  | 174.98    |   100 |
| Contrast            |          | 49.76    | 20.43    | 21.95  | 90.74     |   100 |
| Noise               |          | 29.24    | 26.23    | 2.49   | 99.19     |   100 |
| ColorDominance      |          | 1.19     | 0.14     | 1.00   | 1.46      |   100 |
| BandingScore        |          | 0.52     | 0.18     | 0.25   | 0.85      |   100 |
| BrisqueScore        |          | 4.45     | 3.47     | 0.77   | 12.62     |   100 |
| AvgProcessingTimeMs |          | 229.64   | 32.55    | 152.93 | 512.34    |   100 |



## Controlli di qualità
La classe `DocumentQualityChecker` esegue diversi controlli sull'immagine:

### BrisqueScore
Il BRISQUE (Blind/Referenceless Image Spatial Quality Evaluator) è un indice di qualità senza riferimento. In questa implementazione semplificata il punteggio è calcolato dalla varianza dei livelli di intensità e viene normalizzato tra 0 (immagine ideale) e 100 (scarsa qualità). Il valore ottimale è **inferiore a 50** (impostazione `BrisqueMax`).

### IsBlurry
Verifica la nitidezza calcolando la varianza del filtro Laplaciano. Restituisce un punteggio numerico e un booleano. Valori maggiori di **100** indicano un'immagine nitida; valori inferiori la rendono sfocata (`BlurThreshold`).

### HasGlare
Conta i pixel con intensità oltre **240** e verifica che l'area sia minore di **500** pixel. Se l'area luminosa supera la soglia (`AreaThreshold`) l'immagine contiene riflessi.

### Esposizione
Calcola la luminanza media usando la formula `0.299R + 0.587G + 0.114B` e verifica che sia compresa tra `ExposureMin` e `ExposureMax` (80-180 di default).

### Contrasto
Analizza la deviazione standard della luminanza. Valori inferiori a `ContrastMin` (30) indicano un contrasto insufficiente.

### Dominanza colore
Verifica se un canale RGB domina sugli altri calcolando il rapporto tra il canale più elevato e la media. Se il rapporto supera `DominanceThreshold` (1.5) l'immagine presenta una dominante.

### Rumore
Stima il rumore confrontando ogni pixel con la media dei vicini. Se il valore supera `NoiseThreshold` (20) il disturbo è considerato eccessivo.

### Motion blur
Calcola il rapporto tra le variazioni orizzontali e verticali dei pixel. Valori molto distanti da 1 indicano una sfocatura dovuta al movimento (`MotionBlurThreshold`).

### Bande orizzontali/verticali
Misura la varianza delle medie di righe e colonne per rilevare la presenza di bande uniformi. Un punteggio elevato (oltre `BandingThreshold`) segnala possibili difetti di scansione.

### CheckQuality
Combina i controlli precedenti usando le soglie definite in `QualitySettings` e restituisce un `DocumentQualityResult` con tutti i valori ottenuti.

Le soglie sono configurabili tramite l'oggetto `QualitySettings`. Impostando `GenerateHeatmaps` a `true` è inoltre possibile ottenere due bitmap (`BlurHeatmap` e `GlareHeatmap`) che evidenziano rispettivamente le zone sfocate e quelle colpite da riflessi. Nello stesso modo vengono calcolate anche le coordinate di tali aree tramite le liste `BlurRegions` e `GlareRegions` restituite nel `DocumentQualityResult`.

## Parametri configurabili
La classe `QualitySettings` consente di personalizzare le soglie utilizzate nei vari controlli. Nella tabella seguente sono riportati tutti i parametri disponibili, con i valori di default e alcune indicazioni su come regolarli a seconda delle esigenze:

| Proprietà | Descrizione | Valore predefinito | Note sull'utilizzo |
|-----------|-------------|--------------------|--------------------|
| `BrisqueMax` | Valore massimo accettabile del punteggio BRISQUE. | `50.0` | Ridurre il valore per richiedere una qualità più elevata, aumentarlo per essere meno restrittivi. |
| `BlurThreshold` | Soglia sulla varianza del Laplaciano sotto la quale l'immagine è considerata sfocata. | `100.0` | Se le immagini risultano troppo frequentemente "blurry" è possibile abbassare la soglia. |
| `BrightThreshold` | Intensità (0-255) oltre la quale un pixel è considerato riflesso. | `240` | Valori più alti evitano falsi positivi in condizioni luminose. |
| `AreaThreshold` | Numero minimo di pixel luminosi per dichiarare la presenza di glare. | `500` | Diminuire se si vogliono individuare anche riflessi di piccole dimensioni. |
| `ExposureMin` | Luminanza media minima accettabile. | `80.0` | Aumentare se le immagini tendono ad essere troppo scure. |
| `ExposureMax` | Luminanza media massima accettabile. | `180.0` | Ridurre se le immagini sono spesso sovraesposte. |
| `ContrastMin` | Deviazione standard minima della luminanza per considerare sufficiente il contrasto. | `30.0` | Alzare la soglia richiede un contrasto maggiore. |
| `DominanceThreshold` | Rapporto massimo tra il canale dominante e la media degli altri due. | `1.5` | Ridurre se è necessario rilevare anche dominanti cromatiche leggere. |
| `NoiseThreshold` | Livello massimo di rumore ammesso. | `500.0` | Diminuire per ottenere immagini molto pulite, aumentare se il rumore non è un problema. |
| `MotionBlurThreshold` | Rapporto massimo tra gradienti orizzontali e verticali prima di considerare il movimento. | `3.0` | Valori più bassi rendono il controllo più severo. |
| `BandingThreshold` | Soglia sul rapporto di varianza delle righe/colonne per individuare bande. | `0.5` | Aumentare se si vogliono rilevare solo bande marcate. |
| `GenerateHeatmaps` | Se `true` produce le mappe di calore e le coordinate delle aree problematiche. | `false` | Utile in fase di debug o per applicazioni che devono mostrare i punti da correggere. |

## API REST

Il progetto `DocQualityChecker.Api` espone un endpoint `POST /quality/check` per
eseguire i controlli via HTTP. L'input è un form con i campi:

- `image` (file) immagine da analizzare
- `pdf` (file, opzionale) documento PDF da cui estrarre le pagine
- `pageIndex` (opzionale) indice della pagina PDF da elaborare. Se omesso sono
  processate tutte le pagine
- `checks` (opzionale) lista di controlli da eseguire
- `settings` (opzionale) oggetto `QualitySettings` per personalizzare le soglie

Quando viene caricato un file PDF l'endpoint restituisce un array di risultati,
uno per ciascuna pagina elaborata.

Se `settings.generateHeatmaps` è impostato a `true` la risposta includerà le
mappe di calore in formato base64 (`BlurHeatmap` e `GlareHeatmap`) e le
relative regioni (`BlurRegions`, `GlareRegions`).

## Interfaccia web

Il progetto `DocQualityChecker.Api` include una pagina Razor per
analizzare le immagini direttamente dal browser. Il form consente di
personalizzare tutte le soglie dei controlli tramite **slider** e accanto a
ogni etichetta è presente un'icona con maggiori informazioni sul relativo
controllo. All'inizio del form è inoltre disponibile una sezione collassabile,
chiusa di default, che riporta i valori di riferimento predefiniti.

## Esecuzione dei test

1. Installare lo SDK .NET 9 (se non presente).
2. Dalla cartella del progetto eseguire:

```bash
dotnet test DocQualityChecker.Tests/DocQualityChecker.Tests.csproj -c Release
```

I test creeranno alcune immagini di prova e verificheranno le funzioni di blur, glare, esposizione, contrasto, dominante colore e rumore.

## Verifica corrispondenza .NET vs Python

Per assicurarsi che l'implementazione Python produca gli stessi risultati di quella .NET è disponibile una piccola pipeline di verifica.

```bash
# 1) Dataset & sample
python tools/download_midv500.py

# 2) .NET metrics
dotnet build DocQualityChecker -c Release
dotnet run --project DocQualitySmoke

# 3) Python metrics
pip install -r requirements.txt
python tools/compute_metrics_py.py

# 4) Confronto
python tools/compare_metrics.py
```

Esempio di output:

```text
| Metric    | MeanRelError/DisagreeRate | Status |
|-----------|---------------------------|--------|
| BlurScore | 0.02                      | OK     |
| Exposure  | 0.01                      | OK     |
| IsBlurry  | 0.00                      | OK     |
```

### Valutazione qualitativa

Sul campione di 150 fotogrammi le due implementazioni risultano quasi
identiche per esposizione, contrasto, dominanza del colore e corretta
esposizione. Persistono invece scostamenti rilevanti su blur, motion blur,
riflessi, rumore, banding e tempi di elaborazione, con tassi di disaccordo
superiori al 25 % per i relativi flag. Ciò suggerisce che l'algoritmo Python
necessiti di ulteriori ottimizzazioni per allinearsi alla versione .NET.


## Esempi di output dei test

Le immagini generate dai test possono essere replicate eseguendo il progetto `DocsGenerator`:

```bash
dotnet run --project DocsGenerator/DocsGenerator.csproj -c Release
```

I file saranno salvati nella cartella `docs/images`. Qui sotto sono riportati i principali casi di test con i valori ottenuti e le relative immagini.

### Immagine ad alta qualità

![Originale](docs/images/high_quality_original.png)
![Heatmap Blur](docs/images/high_quality_blur_heatmap.png)
![Heatmap Glare](docs/images/high_quality_glare_heatmap.png)
![Bounding box](docs/images/high_quality_bbox.png)

```
BrisqueScore: 6.50
BlurScore: 661.16
IsBlurry: False
GlareArea: 0
HasGlare: False
Exposure: 176.00
IsWellExposed: True
Contrast: 64.99
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 91.70
HasNoise: False
IsValidDocument: True
```

### Immagine sfocata

![Originale](docs/images/blurry_original.png)
![Heatmap Blur](docs/images/blurry_blur_heatmap.png)
![Heatmap Glare](docs/images/blurry_glare_heatmap.png)
![Bounding box](docs/images/blurry_bbox.png)

```
BrisqueScore: 4.02
BlurScore: 1.22
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 175.98
IsWellExposed: True
Contrast: 51.15
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 0.12
HasNoise: False
IsValidDocument: False
```

## Analisi dei PDF del dataset

Di seguito i risultati ottenuti elaborando i documenti presenti in `docs/dataset_samples/pdf` con `DocumentQualityChecker`.

### blur.pdf
| Pagina | BlurScore | Blurry | GlareArea | HasGlare |
|-------:|----------:|:------:|----------:|:-------:|
| 1 | 131.46 | False | 52997 | True |
| 2 | 190.63 | False | 0 | False |
| 3 | 993.39 | False | 52924 | True |
| 4 | 25.37 | True | 0 | False |
| 5 | 24.75 | True | 0 | False |
| 6 | 0.00 | True | 0 | False |
| 7 | 462.01 | False | 3655 | True |
| 8 | 1260.26 | False | 125 | False |
| 9 | 1616.03 | False | 3623 | True |

Totale pagine: 9, Pagine blur: 3, Pagine glare: 4, Blur medio: 522.66

### glare.pdf
| Pagina | BlurScore | Blurry | GlareArea | HasGlare |
|-------:|----------:|:------:|----------:|:-------:|
| 1 | 171.33 | False | 3016 | True |
| 2 | 428.69 | False | 3 | False |
| 3 | 3228.25 | False | 2825 | True |
| 4 | 159.39 | False | 777 | True |
| 5 | 625.54 | False | 21 | False |
| 6 | 700.75 | False | 736 | True |
| 7 | 376.89 | False | 2701 | True |
| 8 | 1953.58 | False | 446 | False |
| 9 | 2389.04 | False | 2642 | True |

Totale pagine: 9, Pagine blur: 0, Pagine glare: 6, Blur medio: 1114.83

## Valutazione con il dataset Roboflow

Per testare il rilevamento dei riflessi su immagini reali è possibile utilizzare il dataset [glare](https://universe.roboflow.com/pradeep-singh/glare-xw4ce) (49 immagini annotate) e il dataset [blur](https://universe.roboflow.com/yolov7-lwj30/blur-nv01n) per la sfocatura.
I dataset possono essere scaricati impostando la variabile d'ambiente `ROBOFLOW_API_KEY` ed eseguendo lo script `download_datasets.py` presente nel repository:

```bash
python download_datasets.py
```

Lo script salva le cartelle `glare_dataset` e `blur_dataset`. Da queste è sufficiente prelevare alcune immagini (massimo 20) da analizzare con il programma `DatasetEvaluator` che stampa i valori calcolati e salva la heatmap accanto all'immagine esaminata:

Per generare documenti PDF a partire dalle immagini presenti in `docs/dataset_samples` è disponibile lo script `generate_dataset_pdfs.py`:

```bash
python generate_dataset_pdfs.py
```

Dopo aver estratto le immagini, è disponibile il programma `DatasetEvaluator` che stampa i valori calcolati dalla libreria e salva la mappa di calore dei riflessi accanto all'immagine esaminata:

```bash
dotnet run --project DatasetEvaluator/DatasetEvaluator.csproj <percorso immagine>
```

Esempio eseguendo il tool sul file `docs/dataset_samples/glare/img1.jpg` fornito nel repository:

```bash
dotnet run --project DatasetEvaluator/DatasetEvaluator.csproj docs/dataset_samples/glare/img1.jpg
```

Output ottenuto:

```
File: img1.jpg
  BrisqueScore: 13.42
  BlurScore: 213.15
  IsBlurry: False
  GlareArea: 1540
  HasGlare: True
  Exposure: 105.08
  IsWellExposed: True
  Contrast: 94.68
  HasLowContrast: False
  ColorDominance: 1.04
  HasColorDominance: False
  Noise: 26.08
  HasNoise: False
  IsValidDocument: False
```

Confrontando le coordinate di `GlareRegions` con le annotazioni del dataset è possibile quantificare la precisione della libreria.

### Immagine con riflessi

![Originale](docs/images/glare_original.png)
![Heatmap Blur](docs/images/glare_blur_heatmap.png)
![Heatmap Glare](docs/images/glare_glare_heatmap.png)
![Bounding box](docs/images/glare_bbox.png)

```
BrisqueScore: 5.10
BlurScore: 948.94
IsBlurry: False
GlareArea: 2500
HasGlare: True
Exposure: 186.94
IsWellExposed: False
Contrast: 57.61
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 130.45
HasNoise: False
IsValidDocument: False
```

### Punteggio BRISQUE elevato

![Originale](docs/images/high_brisque_original.png)
![Heatmap Blur](docs/images/high_brisque_blur_heatmap.png)
![Heatmap Glare](docs/images/high_brisque_glare_heatmap.png)
![Bounding box](docs/images/high_brisque_bbox.png)

```
BrisqueScore: 6.50
BlurScore: 661.16
IsBlurry: False
GlareArea: 0
HasGlare: False
Exposure: 176.00
IsWellExposed: True
Contrast: 64.99
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 91.70
HasNoise: False
IsValidDocument: False
```

### Immagine sottoesposta

![Originale](docs/images/underexposed_original.png)
![Heatmap Blur](docs/images/underexposed_blur_heatmap.png)
![Heatmap Glare](docs/images/underexposed_glare_heatmap.png)
![Bounding box](docs/images/underexposed_bbox.png)

```
BrisqueScore: 0.00
BlurScore: 0.00
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 20.00
IsWellExposed: False
Contrast: 0.00
HasLowContrast: True
ColorDominance: 1.00
HasColorDominance: False
Noise: 0.00
HasNoise: False
IsValidDocument: False
```

### Contrasto molto basso

![Originale](docs/images/low_contrast_original.png)
![Heatmap Blur](docs/images/low_contrast_blur_heatmap.png)
![Heatmap Glare](docs/images/low_contrast_glare_heatmap.png)
![Bounding box](docs/images/low_contrast_bbox.png)

```
BrisqueScore: 0.00
BlurScore: 0.41
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 120.60
IsWellExposed: True
Contrast: 1.62
HasLowContrast: True
ColorDominance: 1.00
HasColorDominance: False
Noise: 0.06
HasNoise: False
IsValidDocument: False
```

### Dominante di colore

![Originale](docs/images/color_cast_original.png)
![Heatmap Blur](docs/images/color_cast_blur_heatmap.png)
![Heatmap Glare](docs/images/color_cast_glare_heatmap.png)
![Bounding box](docs/images/color_cast_bbox.png)

```
BrisqueScore: 0.00
BlurScore: 0.00
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 76.25
IsWellExposed: False
Contrast: 0.00
HasLowContrast: True
ColorDominance: 3.00
HasColorDominance: True
Noise: 0.00
HasNoise: False
IsValidDocument: False
```

### Immagine rumorosa

![Originale](docs/images/noise_original.png)
![Heatmap Blur](docs/images/noise_blur_heatmap.png)
![Heatmap Glare](docs/images/noise_glare_heatmap.png)
![Bounding box](docs/images/noise_bbox.png)

```
BrisqueScore: 7.81
BlurScore: 60581.41
IsBlurry: False
GlareArea: 755
HasGlare: True
Exposure: 161.65
IsWellExposed: True
Contrast: 71.27
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 3460.45
HasNoise: True
IsValidDocument: False
```

## Esempio dal dataset "blur"

Analizzando alcune immagini del dataset [blur](https://universe.roboflow.com/yolov7-lwj30/blur-nv01n) è possibile verificare la rilevazione della sfocatura. Di seguito l'output generato su `docs/dataset_samples/blur/img1.jpg`:

```bash
dotnet run --project DatasetEvaluator/DatasetEvaluator.csproj docs/dataset_samples/blur/img1.jpg
```

Esempio di risultato:

```
File: img1.jpg
  BrisqueScore: 6.36
  BlurScore: 79.86
  IsBlurry: True
  GlareArea: 76139
  HasGlare: True
  Exposure: 152.23
  IsWellExposed: True
  Contrast: 64.61
  HasLowContrast: False
  ColorDominance: 1.08
  HasColorDominance: False
  Noise: 10.55
  HasNoise: False
  IsValidDocument: False
```

## Analisi completa delle immagini del dataset

Di seguito l'output della libreria per tutte le immagini presenti nella cartella `docs/dataset_samples`.

### blur/img1.jpg

![Originale](docs/dataset_samples/blur/img1.jpg)
![Heatmap Blur](docs/dataset_samples/blur/img1_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/blur/img1_glare_heatmap.png)

```
BrisqueScore: 6.36
BlurScore: 79.86
IsBlurry: True
GlareArea: 76139
HasGlare: True
Exposure: 152.23
IsWellExposed: True
Contrast: 64.61
HasLowContrast: False
ColorDominance: 1.08
HasColorDominance: False
Noise: 10.55
HasNoise: False
IsValidDocument: False
```

### blur/img2.jpg

![Originale](docs/dataset_samples/blur/img2.jpg)
![Heatmap Blur](docs/dataset_samples/blur/img2_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/blur/img2_glare_heatmap.png)

```
BrisqueScore: 5.28
BlurScore: 12.08
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 84.98
IsWellExposed: True
Contrast: 58.27
HasLowContrast: False
ColorDominance: 1.09
HasColorDominance: False
Noise: 1.38
HasNoise: False
IsValidDocument: False
```

### blur/img3.jpg

![Originale](docs/dataset_samples/blur/img3.jpg)
![Heatmap Blur](docs/dataset_samples/blur/img3_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/blur/img3_glare_heatmap.png)

```
BrisqueScore: 2.40
BlurScore: 324.15
IsBlurry: False
GlareArea: 5458
HasGlare: True
Exposure: 31.92
IsWellExposed: False
Contrast: 36.84
HasLowContrast: False
ColorDominance: 1.43
HasColorDominance: False
Noise: 37.95
HasNoise: False
IsValidDocument: False
```

### glare/img1.jpg

![Originale](docs/dataset_samples/glare/img1.jpg)
![Heatmap Blur](docs/dataset_samples/glare/img1_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/glare/img1_glare_heatmap.png)

```
BrisqueScore: 13.42
BlurScore: 213.15
IsBlurry: False
GlareArea: 1540
HasGlare: True
Exposure: 105.08
IsWellExposed: True
Contrast: 94.68
HasLowContrast: False
ColorDominance: 1.04
HasColorDominance: False
Noise: 26.08
HasNoise: False
IsValidDocument: False
```

### glare/img2.jpg

![Originale](docs/dataset_samples/glare/img2.jpg)
![Heatmap Blur](docs/dataset_samples/glare/img2_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/glare/img2_glare_heatmap.png)

```
BrisqueScore: 5.36
BlurScore: 205.19
IsBlurry: False
GlareArea: 391
HasGlare: False
Exposure: 95.60
IsWellExposed: True
Contrast: 60.30
HasLowContrast: False
ColorDominance: 1.04
HasColorDominance: False
Noise: 21.80
HasNoise: False
IsValidDocument: False
```

### glare/img3.jpg

![Originale](docs/dataset_samples/glare/img3.jpg)
![Heatmap Blur](docs/dataset_samples/glare/img3_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/glare/img3_glare_heatmap.png)

```
BrisqueScore: 5.05
BlurScore: 551.69
IsBlurry: False
GlareArea: 1424
HasGlare: True
Exposure: 114.82
IsWellExposed: True
Contrast: 58.15
HasLowContrast: False
ColorDominance: 1.04
HasColorDominance: False
Noise: 39.41
HasNoise: False
IsValidDocument: False
```

### 93_HONOR-7X.png

![Originale](docs/dataset_samples/93_HONOR-7X.png)
![Heatmap Blur](docs/dataset_samples/93_HONOR-7X_blur_heatmap.png)
![Heatmap Glare](docs/dataset_samples/93_HONOR-7X_glare_heatmap.png)

```
BrisqueScore: 14.94
BlurScore: 1900.95
IsBlurry: False
GlareArea: 30889
HasGlare: True
Exposure: 124.03
IsWellExposed: True
Contrast: 98.01
HasLowContrast: False
ColorDominance: 1.02
HasColorDominance: False
Noise: 225.22
HasNoise: False
IsValidDocument: False
```

## Valutazione del subset MIDV-500

Per una valutazione preliminare è stato utilizzato un sottoinsieme di 20 immagini del dataset [MIDV-500](https://huggingface.co/datasets/Noaman/midv500). Le annotazioni includono una maschera del documento da cui è stata ricavata la percentuale di area occupata rispetto al frame. Le metriche ottenute da DocQualitySmoke sono riportate nella tabella seguente.

| Metric | PassRate | Mean | Std | Min | Max |
|---|---|---|---|---|---|
| IsBlurry | 0.95 |  |  |  |  |
| HasGlare | 0.35 |  |  |  |  |
| HasNoise | 1.00 |  |  |  |  |
| HasLowContrast | 1.00 |  |  |  |  |
| HasColorDominance | 1.00 |  |  |  |  |
| !IsWellExposed | 1.00 |  |  |  |  |
| BlurScore |  | 475.80 | 250.47 | 83.23 | 861.61 |
| MotionBlurScore |  | 1.05 | 0.07 | 1.00 | 1.30 |
| GlareArea |  | 6977.25 | 6635.07 | 52.00 | 18497.00 |
| Exposure |  | 133.46 | 11.28 | 117.72 | 146.28 |
| Contrast |  | 54.41 | 2.51 | 51.19 | 59.46 |
| Noise |  | 42.04 | 25.17 | 8.23 | 84.44 |
| ColorDominance |  | 1.24 | 0.10 | 1.15 | 1.39 |
| BandingScore |  | 0.38 | 0.15 | 0.27 | 0.80 |
| BrisqueScore |  | 4.50 | 0.36 | 4.11 | 5.36 |
| AvgProcessingTimeMs |  | 269.77 |  |  |  |
| DocumentAreaRatio |  | 0.22 | 0.04 | 0.14 | 0.27 |

Il rapporto tra area del documento e immagine è in media ~22%; considerando un fotogramma da 1920×1080 px ciò corrisponde a circa 4.5×10⁵ px. Il glare copre mediamente 6977 px (circa l'1.6% dell'area del documento).


## Tempi di esecuzione dei controlli

Di seguito sono riportati i tempi medi di esecuzione (in millisecondi) per ciascun controllo su ogni immagine del dataset.


### 93_HONOR-7X.png
| Controllo | ms |
|-----------|---|
| Brisque | 21.90 |
| Blur | 26.35 |
| MotionBlur | 10.03 |
| Glare | 11.37 |
| Exposure | 9.82 |
| Contrast | 2.13 |
| ColorDominance | 1.95 |
| Noise | 11.40 |
| Banding | 3.14 |
| BlurHeatmap | 12.37 |
| GlareHeatmap | 6.35 |
| BlurRegions | 15.35 |
| GlareRegions | 18.91 |
| Total | 73.68 |


### blur/img1.jpg
| Controllo | ms |
|-----------|---|

| Brisque | 48.38 |
| Blur | 43.56 |
| MotionBlur | 65.09 |
| Glare | 11.48 |
| Exposure | 7.49 |
| Contrast | 6.42 |
| ColorDominance | 5.91 |
| Noise | 30.31 |
| Banding | 9.83 |
| BlurHeatmap | 125.59 |
| GlareHeatmap | 21.20 |
| BlurRegions | 79.54 |
| GlareRegions | 38.17 |
| Total | 235.62 |

### blur/img2.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 45.17 |
| Blur | 51.26 |
| MotionBlur | 47.33 |
| Glare | 25.42 |
| Exposure | 8.13 |
| Contrast | 6.57 |
| ColorDominance | 5.91 |
| Noise | 33.59 |
| Banding | 9.11 |
| BlurHeatmap | 21.06 |
| GlareHeatmap | 17.32 |
| BlurRegions | 106.43 |
| GlareRegions | 17.43 |
| Total | 150.18 |

### blur/img3.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 10.20 |
| Blur | 13.70 |
| MotionBlur | 7.77 |
| Glare | 6.67 |
| Exposure | 5.06 |
| Contrast | 6.60 |
| ColorDominance | 5.48 |
| Noise | 16.68 |
| Banding | 8.31 |
| BlurHeatmap | 13.07 |
| GlareHeatmap | 7.41 |
| BlurRegions | 40.25 |
| GlareRegions | 23.19 |
| Total | 110.25 |

### glare/img1.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 6.25 |
| Blur | 6.18 |
| MotionBlur | 8.01 |
| Glare | 5.54 |
| Exposure | 4.34 |
| Contrast | 2.40 |
| ColorDominance | 2.29 |
| Noise | 7.10 |
| Banding | 3.55 |
| BlurHeatmap | 6.84 |
| GlareHeatmap | 4.68 |
| BlurRegions | 18.92 |
| GlareRegions | 10.17 |
| Total | 73.58 |

### glare/img2.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 15.04 |
| Blur | 5.52 |
| MotionBlur | 7.75 |
| Glare | 4.02 |
| Exposure | 2.23 |
| Contrast | 2.47 |
| ColorDominance | 2.47 |
| Noise | 13.32 |
| Banding | 3.80 |
| BlurHeatmap | 7.15 |
| GlareHeatmap | 12.20 |
| BlurRegions | 41.43 |
| GlareRegions | 15.41 |
| Total | 86.71 |

### glare/img3.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 4.55 |
| Blur | 5.59 |
| MotionBlur | 6.60 |
| Glare | 8.20 |
| Exposure | 2.39 |
| Contrast | 3.11 |
| ColorDominance | 2.37 |
| Noise | 13.32 |
| Banding | 7.15 |
| BlurHeatmap | 19.62 |
| GlareHeatmap | 13.38 |
| BlurRegions | 22.02 |
| GlareRegions | 5.19 |
| Total | 45.33 |

| Immagine | Tempo totale (ms) |
|----------|------------------|
| 93_HONOR-7X.png | 73.68 |
| blur/img1.jpg | 235.62 |
| blur/img2.jpg | 150.18 |
| blur/img3.jpg | 110.25 |
| glare/img1.jpg | 73.58 |
| glare/img2.jpg | 86.71 |
| glare/img3.jpg | 45.33 |

| Immagine | Prima (ms) | Dopo (ms) | Riduzione % |
|----------|-----------|----------|-------------|
| 93_HONOR-7X.png | 497.05 | 73.68 | 85.18 |
| blur/img1.jpg | 485.03 | 235.62 | 51.42 |
| blur/img2.jpg | 464.82 | 150.18 | 67.69 |
| blur/img3.jpg | 480.09 | 110.25 | 77.04 |
| glare/img1.jpg | 205.37 | 73.58 | 64.17 |
| glare/img2.jpg | 203.91 | 86.71 | 57.48 |
| glare/img3.jpg | 206.94 | 45.33 | 78.10 |

| Immagine | BrisqueScore pre | BrisqueScore post | BlurScore pre | BlurScore post | GlareArea pre | GlareArea post | Exposure pre | Exposure post | Contrast pre | Contrast post |
|------|------|------|------|------|------|------|------|------|------|------|
| 93_HONOR-7X | 14.94 | 14.94 | 1900.95 | 1900.95 | 30889 | 30889 | 124.03 | 124.03 | 98.01 | 98.01 |
| blur/img1 | 6.36 | 6.36 | 79.86 | 79.86 | 76139 | 76139 | 152.23 | 152.23 | 64.61 | 64.61 |
| blur/img2 | 5.28 | 5.28 | 12.08 | 12.08 | 0 | 0 | 84.98 | 84.98 | 58.27 | 58.27 |
| blur/img3 | 2.40 | 2.40 | 324.15 | 324.15 | 5458 | 5458 | 31.92 | 31.92 | 36.84 | 36.84 |
| glare/img1 | 13.42 | 13.42 | 213.15 | 213.15 | 1540 | 1540 | 105.08 | 105.08 | 94.68 | 94.68 |
| glare/img2 | 5.36 | 5.36 | 205.19 | 205.19 | 391 | 391 | 95.60 | 95.60 | 60.30 | 60.30 |
| glare/img3 | 5.05 | 5.05 | 551.69 | 551.69 | 1424 | 1424 | 114.82 | 114.82 | 58.15 | 58.15 |

| Image | Brisque pre | Brisque post | Blur pre | Blur post | MotionBlur pre | MotionBlur post | Glare pre | Glare post | Exposure pre | Exposure post | Contrast pre | Contrast post | ColorDominance pre | ColorDominance post | Noise pre | Noise post | Banding pre | Banding post | BlurHeatmap pre | BlurHeatmap post | GlareHeatmap pre | GlareHeatmap post | BlurRegions pre | BlurRegions post | GlareRegions pre | GlareRegions post | Total pre | Total post | Diff % |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 0.jpg | 15.7 | 24.8 | 19.1 | 16.5 | 20.8 | 12.4 | 15.2 | 13.3 | 9.0 | 12.2 | 9.8 | 12.8 | 10.5 | 13.0 | 177.7 | 26.4 | 14.6 | 21.6 | 24.3 | 20.9 | 23.5 | 13.7 | 80.0 | 89.5 | 24.1 | 29.1 | 184.5 | 194.1 | 5.2 |
| 10.jpg | 125.1 | 107.6 | 158.1 | 138.6 | 186.3 | 110.7 | 129.7 | 103.3 | 70.3 | 116.8 | 82.3 | 144.5 | 104.5 | 127.7 | 1472.7 | 102.1 | 144.3 | 167.7 | 233.4 | 170.1 | 137.2 | 143.4 | 796.8 | 905.8 | 193.8 | 179.5 | 1640.5 | 528.7 | -67.8 |
| 1001.jpg | 244.0 | 189.8 | 296.2 | 240.7 | 301.2 | 187.0 | 212.5 | 196.4 | 136.1 | 181.5 | 158.9 | 202.4 | 156.9 | 200.7 | 2743.9 | 165.2 | 229.0 | 309.2 | 424.5 | 298.9 | 244.2 | 245.8 | 1461.8 | 1727.3 | 352.6 | 380.2 | 3083.6 | 836.8 | -72.9 |
| 1004.jpg | 252.1 | 332.9 | 288.7 | 859.3 | 295.0 | 501.6 | 214.5 | 221.5 | 139.9 | 187.1 | 153.4 | 207.5 | 160.7 | 206.2 | 2736.4 | 169.6 | 226.8 | 378.2 | 446.4 | 380.3 | 263.0 | 306.3 | 1461.6 | 1934.2 | 373.3 | 439.9 | 3053.1 | 1104.9 | -63.8 |
| 1005.jpg | 223.6 | 191.6 | 280.2 | 258.2 | 302.2 | 200.4 | 218.4 | 180.6 | 145.9 | 185.4 | 189.1 | 214.2 | 197.4 | 217.5 | 2813.8 | 140.0 | 226.2 | 298.6 | 447.7 | 314.4 | 275.0 | 231.0 | 1501.1 | 1717.3 | 404.1 | 387.5 | 3079.8 | 747.2 | -75.7 |
| 22.jpg | 68.3 | 67.5 | 77.4 | 96.1 | 89.0 | 59.5 | 63.7 | 50.3 | 48.8 | 73.1 | 45.8 | 64.9 | 53.9 | 72.7 | 804.7 | 46.7 | 82.8 | 104.7 | 123.9 | 99.1 | 76.7 | 84.8 | 331.9 | 364.1 | 122.2 | 120.3 | 777.2 | 294.6 | -62.1 |
| 242.jpg | 491.0 | 190.9 | 430.6 | 241.2 | 274.7 | 168.0 | 209.9 | 180.2 | 128.8 | 192.8 | 135.7 | 206.7 | 139.6 | 207.6 | 2500.5 | 128.2 | 227.1 | 284.0 | 403.3 | 300.4 | 253.5 | 209.2 | 1388.7 | 1672.3 | 497.3 | 515.5 | 2962.2 | 741.6 | -75.0 |
| 266.jpg | 18.9 | 31.2 | 23.1 | 21.9 | 29.0 | 18.4 | 19.9 | 15.3 | 11.6 | 16.4 | 12.8 | 19.0 | 12.6 | 16.4 | 216.9 | 29.2 | 18.7 | 29.9 | 36.3 | 24.5 | 22.9 | 20.2 | 68.6 | 81.9 | 28.7 | 33.2 | 201.4 | 205.6 | 2.1 |
| 275.jpg | 79.8 | 92.3 | 91.8 | 97.3 | 101.1 | 56.5 | 75.2 | 62.2 | 45.4 | 63.3 | 54.3 | 94.1 | 52.8 | 80.3 | 912.4 | 61.6 | 78.9 | 114.4 | 136.7 | 103.7 | 88.4 | 96.1 | 303.5 | 447.4 | 128.1 | 191.1 | 862.1 | 337.6 | -60.8 |
| 279.jpg | 271.7 | 312.6 | 239.4 | 366.8 | 239.1 | 316.1 | 181.7 | 157.6 | 106.0 | 147.4 | 118.9 | 165.0 | 119.5 | 162.9 | 2202.4 | 95.8 | 193.5 | 237.1 | 326.9 | 263.5 | 222.6 | 213.5 | 1118.1 | 1311.6 | 300.7 | 303.5 | 2482.6 | 672.1 | -72.9 |
| 281.jpg | 194.8 | 148.8 | 229.9 | 204.8 | 243.2 | 141.1 | 191.8 | 152.6 | 105.0 | 163.6 | 117.1 | 179.7 | 125.9 | 171.2 | 2211.8 | 116.8 | 189.7 | 250.2 | 357.3 | 243.2 | 202.1 | 202.5 | 1032.1 | 1198.2 | 321.7 | 300.5 | 2324.8 | 646.8 | -72.2 |
| 293.jpg | 283.0 | 172.4 | 295.5 | 236.5 | 291.7 | 164.2 | 215.4 | 191.4 | 125.5 | 204.6 | 142.6 | 217.3 | 136.4 | 192.4 | 2514.2 | 130.1 | 201.5 | 276.6 | 401.7 | 281.6 | 234.7 | 207.7 | 1206.2 | 1460.4 | 327.7 | 367.8 | 2707.6 | 674.8 | -75.1 |
| 313.jpg | 129.1 | 124.0 | 150.5 | 133.7 | 158.9 | 110.8 | 113.4 | 101.7 | 76.3 | 109.7 | 81.3 | 121.2 | 88.0 | 117.0 | 1528.6 | 71.3 | 171.6 | 157.0 | 286.2 | 161.3 | 132.9 | 141.2 | 692.0 | 828.6 | 190.8 | 186.1 | 1574.1 | 483.1 | -69.3 |
| 326.jpg | 282.9 | 151.5 | 439.9 | 209.0 | 310.1 | 137.0 | 172.3 | 147.9 | 114.2 | 151.8 | 120.4 | 161.9 | 119.8 | 167.6 | 2377.0 | 112.2 | 185.3 | 264.2 | 344.0 | 242.7 | 220.5 | 179.2 | 1390.4 | 1398.1 | 311.7 | 293.2 | 3060.3 | 623.5 | -79.6 |
| 447.jpg | 188.3 | 149.0 | 223.3 | 202.0 | 238.7 | 134.0 | 177.1 | 143.5 | 131.3 | 150.0 | 140.0 | 179.8 | 129.9 | 201.5 | 2189.4 | 145.3 | 178.6 | 303.7 | 312.5 | 285.3 | 217.9 | 199.8 | 1145.5 | 1413.4 | 283.6 | 281.1 | 2451.5 | 678.8 | -72.3 |
| 482.jpg | 114.0 | 101.3 | 128.1 | 130.6 | 145.6 | 81.9 | 116.3 | 95.1 | 70.2 | 99.4 | 70.8 | 110.0 | 71.2 | 110.7 | 1289.4 | 77.2 | 112.3 | 138.4 | 186.4 | 139.5 | 118.3 | 115.8 | 584.7 | 674.5 | 186.3 | 197.5 | 1342.8 | 391.0 | -70.9 |
| 497.jpg | 115.1 | 113.9 | 155.7 | 147.6 | 142.8 | 98.7 | 112.2 | 87.7 | 66.7 | 96.0 | 73.6 | 106.2 | 73.9 | 107.9 | 1302.9 | 80.4 | 106.5 | 144.8 | 209.8 | 174.5 | 119.7 | 118.4 | 614.4 | 721.2 | 215.3 | 260.5 | 1418.2 | 437.4 | -69.2 |
| 523.jpg | 102.1 | 99.6 | 141.9 | 122.3 | 142.1 | 83.8 | 103.7 | 97.0 | 68.3 | 88.8 | 69.2 | 96.6 | 75.9 | 102.8 | 1274.9 | 69.4 | 113.3 | 147.3 | 200.1 | 140.9 | 135.5 | 104.8 | 570.5 | 662.5 | 191.8 | 193.7 | 1360.0 | 524.2 | -61.5 |
| 65.jpg | 189.5 | 170.2 | 238.3 | 254.5 | 287.9 | 145.3 | 320.6 | 170.1 | 124.6 | 194.9 | 128.6 | 233.0 | 126.4 | 245.5 | 2363.7 | 186.1 | 188.7 | 270.5 | 373.5 | 285.3 | 229.8 | 211.1 | 1187.3 | 1537.7 | 320.8 | 323.7 | 2679.1 | 688.4 | -74.3 |
| 743.jpg | 247.5 | 193.5 | 272.1 | 258.3 | 335.5 | 183.5 | 230.2 | 177.4 | 143.6 | 183.7 | 160.9 | 206.6 | 152.8 | 217.6 | 2807.3 | 180.1 | 229.3 | 319.3 | 403.2 | 328.1 | 282.9 | 234.6 | 1433.9 | 1691.7 | 361.9 | 388.8 | 3094.0 | 726.5 | -76.5 |
| 988.jpg | 225.1 | 210.3 | 278.2 | 256.1 | 313.3 | 171.3 | 225.7 | 191.7 | 139.2 | 204.6 | 153.2 | 220.3 | 152.8 | 226.3 | 2709.9 | 140.1 | 233.9 | 332.1 | 437.8 | 396.0 | 261.3 | 244.9 | 1440.4 | 1791.9 | 360.6 | 402.8 | 3122.0 | 907.5 | -70.9 |
| 997.jpg | 222.8 | 191.6 | 263.5 | 285.3 | 312.8 | 218.3 | 232.7 | 225.9 | 145.5 | 188.6 | 150.2 | 205.4 | 145.8 | 227.4 | 2713.5 | 144.3 | 231.5 | 296.0 | 424.6 | 325.3 | 265.8 | 253.2 | 1397.9 | 1678.4 | 418.8 | 378.7 | 3100.8 | 763.4 | -75.4 |
| Average | 185.7 | 153.1 | 214.6 | 217.2 | 216.4 | 150.0 | 161.5 | 134.7 | 97.8 | 136.9 | 107.7 | 153.1 | 109.4 | 154.2 | 1902.9 | 109.9 | 162.9 | 220.2 | 297.3 | 226.3 | 183.1 | 171.7 | 964.0 | 1150.4 | 268.9 | 279.7 | 2116.5 | 600.4 | -71.6 |

## Ottimizzazioni delle performance

Le ultime versioni della libreria includono alcune migliorie mirate a ridurre drasticamente i tempi di analisi:

- **Buffer di intensità ridotto**: per le immagini con lato superiore a 512 px le operazioni sfruttano un buffer downsampled in modo da limitare il numero di pixel elaborati.
- **Calcolo del rumore ottimizzato**: `ComputeNoise` esegue un campionamento dei pixel con passo adattativo, mantenendo accuratezza ma riducendo i cicli annidati.
- **Risultati memorizzati**: i valori di motion blur, rumore e banding vengono calcolati una sola volta all'interno di `CheckQuality` e riutilizzati, evitando ricalcoli costosi.
- **Sampler più rapido**: lo strumento `PerformanceSampler` ignora le immagini PNG di output e processa solo i file `.jpg`, prevenendo elaborazioni accidentali.

### Dettagli tecnici delle ottimizzazioni

- Diversi cicli nidificati sono stati sostituiti con `Parallel.For` in modo da sfruttare tutte le CPU disponibili.
- `GetIntensityBuffer` accetta un parametro di passo che consente di ottenere un buffer ridotto quando le dimensioni dell'immagine superano i 512 px.
- `ComputeNoise` utilizza un campionamento adattativo dei pixel, riducendo drasticamente il numero di iterazioni senza perdere precisione.
- In `CheckQuality` i punteggi di motion blur, rumore e banding vengono calcolati una sola volta e riutilizzati nelle verifiche successive.

Con queste ottimizzazioni i tempi di esecuzione per le immagini del dataset `glare` sono scesi ben al di sotto del secondo anche su file di grandi dimensioni.

### Considerazioni sui tempi di risposta
L'esecuzione dei controlli base (BRISQUE, sfocatura, glare, esposizione e simili) richiede ora una frazione di secondo. La generazione delle heatmap e delle regioni resta l'operazione più onerosa ma i tempi complessivi oscillano tra circa 23 e 71 ms a seconda dell'immagine.

### Analisi campione cartella `docs/images/glare`
Per verificare le cause di alcune lentezze riscontrate nello script sono state analizzate tre immagini di esempio (`0.jpg`, `10.jpg` e `1001.jpg`). Per ciascun file sono state misurate le tempistiche dei singoli controlli e generati gli heatmap.

L'analisi completa di tutte le immagini è disponibile in [docs/images/glare/README.md](docs/images/glare/README.md).
#### 0.jpg
![Originale](docs/images/glare/0.jpg)
![Glare Heatmap](docs/images/glare/0_glare_heatmap.png)

Valori ottenuti:

```
BrisqueScore: 2.25
BlurScore: 47.17
IsBlurry: True
GlareArea: 15565
HasGlare: True
Exposure: 185.57
IsWellExposed: False
Contrast: 39.63
Noise: 5.14
BandingScore: 0.30
IsValidDocument: False
```

Tempi di esecuzione (ms):

```
Brisque: 64.9
Blur: 61.3
MotionBlur: 41.6
Glare: 23.3
Exposure: 26.8
Contrast: 15.5
ColorDominance: 14.9
Noise: 456.6
Banding: 32.8
BlurHeatmap: 63.5
GlareHeatmap: 62.0
BlurRegions: 184.4
GlareRegions: 33.5
Total: 315.8
```

#### 10.jpg
![Originale](docs/images/glare/10.jpg)
![Glare Heatmap](docs/images/glare/10_glare_heatmap.png)

Valori ottenuti:

```
BrisqueScore: 2.51
BlurScore: 159.73
IsBlurry: False
GlareArea: 20889
HasGlare: True
Exposure: 140.98
IsWellExposed: True
Contrast: 39.59
Noise: 13.56
BandingScore: 0.50
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):

```
Brisque: 475.7
Blur: 499.6
MotionBlur: 334.6
Glare: 164.8
Exposure: 201.0
Contrast: 209.1
ColorDominance: 205.8
Noise: 2270.0
Banding: 259.4
BlurHeatmap: 814.9
GlareHeatmap: 330.6
BlurRegions: 1545.8
GlareRegions: 268.8
Total: 8738.2
```

#### 1001.jpg

![Originale](docs/images/glare/1001.jpg)
![Heatmap Blur](docs/images/glare/1001_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/1001_glare_heatmap.png)

```
BrisqueScore: 5.58
BlurScore: 150.35
IsBlurry: False
GlareArea: 26488
HasGlare: True
Exposure: 108.68
IsWellExposed: True
Contrast: 62.12
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 16.28
HasNoise: False
BandingScore: 0.42
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):

```
{
  "Brisque": 243.9552,
  "Blur": 296.2407,
  "MotionBlur": 301.1525,
  "Glare": 212.5219,
  "Exposure": 136.0848,
  "Contrast": 158.9248,
  "ColorDominance": 156.9493,
  "Noise": 2743.851,
  "Banding": 229.0184,
  "BlurHeatmap": 424.456,
  "GlareHeatmap": 244.2194,
  "BlurRegions": 1461.8202,
  "GlareRegions": 352.5871,
  "Total": 3083.5516
}```

La tabella seguente riassume i tempi totali di esecuzione per tutte le immagini presenti in `docs/images/glare`.

| Immagine | Tempo totale (ms) |
|----------|------------------|
| 0.jpg | 154.33 |
| 10.jpg | 352.64 |
| 1001.jpg | 629.43 |
| 1004.jpg | 628.21 |
| 1005.jpg | 585.75 |
| 22.jpg | 191.94 |
| 242.jpg | 509.52 |
| 266.jpg | 143.27 |
| 275.jpg | 235.62 |
| 279.jpg | 460.50 |
| 281.jpg | 478.75 |
| 293.jpg | 520.03 |
| 313.jpg | 328.38 |
| 326.jpg | 487.76 |
| 447.jpg | 470.26 |
| 482.jpg | 284.83 |
| 497.jpg | 292.72 |
| 523.jpg | 316.69 |
| 65.jpg | 481.15 |
| 743.jpg | 547.07 |
| 988.jpg | 576.09 |
| 997.jpg | 591.75 |

Per l'elenco completo delle analisi consultare [docs/images/glare/README_full.md](docs/images/glare/README_full.md).

Durante l'analisi è emerso che le funzioni di calcolo del rumore e di individuazione delle regioni (in particolare `FindBlurRegions`) impiegano la maggior parte del tempo a causa di doppi cicli annidati su tutti i pixel. Inoltre alcune metriche venivano ricalcolate più volte all'interno di `CheckQuality`. Ottimizzando questi passaggi e memorizzando i risultati di `ComputeMotionBlurScore`, `ComputeNoise` e `ComputeBandingScore` il tempo complessivo è stato ridotto di circa il 20‑30 % su entrambe le immagini.

## Web app

Il progetto `DocQualityChecker.Api` fornisce una semplice interfaccia Razor Pages per verificare la qualità delle immagini insieme agli endpoint API.
Per avviarla:

```bash
dotnet run --project DocQualityChecker.Api/DocQualityChecker.Api.csproj
```

L'applicazione consente di caricare un file e mostrare i risultati dei controlli direttamente nel browser.

## Docker

È disponibile un'immagine Docker che esegue l'applicazione con le pagine Razor integrate.
Per crearla eseguire dalla cartella radice:

```bash
docker build -t image-quality-scanner .
```

Avvio del container:

```bash
docker run -p 8080:8080 image-quality-scanner
```

L'applicazione sarà raggiungibile su `http://localhost:8080`.
