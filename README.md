# Image Quality Scanner

Questa libreria fornisce semplici controlli di qualità su immagini di documenti tramite API gestite.
L'implementazione utilizza [SkiaSharp](https://github.com/mono/SkiaSharp) per la manipolazione delle immagini.

## Requisiti

 - .NET 9 SDK

Le dipendenze NuGet vengono ripristinate automaticamente durante la fase di build/test.


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
- `checks` (opzionale) lista di controlli da eseguire
- `settings` (opzionale) oggetto `QualitySettings` per personalizzare le soglie

Se `settings.generateHeatmaps` è impostato a `true` la risposta includerà le
mappe di calore in formato base64 (`BlurHeatmap` e `GlareHeatmap`) e le
relative regioni (`BlurRegions`, `GlareRegions`).

## Esecuzione dei test

1. Installare lo SDK .NET 9 (se non presente).
2. Dalla cartella del progetto eseguire:

```bash
dotnet test DocQualityChecker.Tests/DocQualityChecker.Tests.csproj -c Release
```

I test creeranno alcune immagini di prova e verificheranno le funzioni di blur, glare, esposizione, contrasto, dominante colore e rumore.


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

## Valutazione con il dataset Roboflow

Per testare il rilevamento dei riflessi su immagini reali è possibile utilizzare il dataset [glare](https://universe.roboflow.com/pradeep-singh/glare-xw4ce) (49 immagini annotate) e il dataset [blur](https://universe.roboflow.com/yolov7-lwj30/blur-nv01n) per la sfocatura.
I dataset possono essere scaricati con la chiave API `tcaZqJkWcEENQPa2p2H1` tramite lo script `download_datasets.py` presente nel repository:

```bash
python download_datasets.py
```

Lo script salva le cartelle `glare_dataset` e `blur_dataset`. Da queste è sufficiente prelevare alcune immagini (massimo 20) da analizzare con il programma `DatasetEvaluator` che stampa i valori calcolati e salva la heatmap accanto all'immagine esaminata:

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

## Tempi di esecuzione dei controlli

Di seguito sono riportati i tempi medi di esecuzione (in millisecondi) per ciascun controllo su ogni immagine del dataset.


### 93_HONOR-7X.png
| Controllo | ms |
|-----------|---|
| Brisque | 3.51 |
| Blur | 28.84 |
| MotionBlur | 38.18 |
| Glare | 2.43 |
| Exposure | 9.01 |
| Contrast | 1.12 |
| ColorDominance | 0.93 |
| Noise | 31.66 |
| Banding | 5.11 |
| BlurHeatmap | 32.13 |
| GlareHeatmap | 4.52 |
| BlurRegions | 54.45 |
| GlareRegions | 17.26 |
| Total | 440.83 |

| Controllo | ms |
|-----------|---|
| Brisque | 9.49 |
| Blur | 145.94 |
| MotionBlur | 118.90 |
| Glare | 2.48 |
| Exposure | 2.83 |
| Contrast | 4.36 |
| ColorDominance | 2.44 |
| Noise | 63.54 |
| Banding | 12.06 |
| BlurHeatmap | 56.43 |
| GlareHeatmap | 2.60 |
| BlurRegions | 125.02 |
| GlareRegions | 27.69 |
| Total | 426.45 |

### blur/img2.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 2.43 |
| Blur | 62.15 |
| MotionBlur | 59.28 |
| Glare | 7.32 |
| Exposure | 9.32 |
| Contrast | 7.37 |
| ColorDominance | 8.52 |
| Noise | 69.86 |
| Banding | 10.43 |
| BlurHeatmap | 59.95 |
| GlareHeatmap | 9.08 |
| BlurRegions | 133.39 |
| GlareRegions | 15.96 |
| Total | 411.44 |

### blur/img3.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 2.48 |
| Blur | 54.69 |
| MotionBlur | 61.85 |
| Glare | 7.32 |
| Exposure | 8.81 |
| Contrast | 7.39 |
| ColorDominance | 8.37 |
| Noise | 72.20 |
| Banding | 9.62 |
| BlurHeatmap | 64.73 |
| GlareHeatmap | 8.88 |
| BlurRegions | 137.39 |
| GlareRegions | 20.10 |
| Total | 430.61 |

### glare/img1.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 1.81 |
| Blur | 23.23 |
| MotionBlur | 21.90 |
| Glare | 0.96 |
| Exposure | 1.05 |
| Contrast | 6.41 |
| ColorDominance | 3.16 |
| Noise | 29.34 |
| Banding | 3.43 |
| BlurHeatmap | 25.38 |
| GlareHeatmap | 5.27 |
| BlurRegions | 55.53 |
| GlareRegions | 9.39 |
| Total | 197.71 |

### glare/img2.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 3.16 |
| Blur | 24.90 |
| MotionBlur | 23.92 |
| Glare | 3.52 |
| Exposure | 5.81 |
| Contrast | 3.21 |
| ColorDominance | 3.17 |
| Noise | 29.19 |
| Banding | 3.43 |
| BlurHeatmap | 26.95 |
| GlareHeatmap | 3.52 |
| BlurRegions | 55.91 |
| GlareRegions | 7.74 |
| Total | 194.54 |

### glare/img3.jpg
| Controllo | ms |
|-----------|---|
| Brisque | 3.32 |
| Blur | 25.73 |
| MotionBlur | 24.56 |
| Glare | 5.71 |
| Exposure | 3.01 |
| Contrast | 3.19 |
| ColorDominance | 3.24 |
| Noise | 29.30 |
| Banding | 4.53 |
| BlurHeatmap | 25.67 |
| GlareHeatmap | 3.16 |
| BlurRegions | 54.46 |
| GlareRegions | 10.05 |
| Total | 189.91 |

| Immagine | Tempo totale (ms) |
|----------|------------------|
| 93_HONOR-7X.png | 440.8288 |
| blur/img1.jpg | 426.4511 |
| blur/img2.jpg | 411.4420 |
| blur/img3.jpg | 430.6078 |
| glare/img1.jpg | 197.7098 |
| glare/img2.jpg | 194.5410 |
| glare/img3.jpg | 189.9107 |
### Considerazioni sui tempi di risposta
L'esecuzione dei controlli base (BRISQUE, sfocatura, glare, esposizione e simili) richiede ora solo pochi millisecondi per immagine. Anche la generazione delle heatmap e il calcolo delle regioni risultano molto più veloci: i tempi totali si attestano intorno ai 200–400 ms a seconda della complessità dell'immagine.

Nella cartella `webapp` è presente un piccolo client React (Vite + Ant Design) scritto in **TypeScript**.
Per testarlo occorre prima avviare l'API:

```bash
dotnet run --project DocQualityChecker.Api/DocQualityChecker.Api.csproj
```

In un secondo terminale:

```bash
cd webapp
npm install
npm run dev
```

La webapp presuppone che l'API sia raggiungibile su `http://localhost:5274` e consente di caricare un'immagine tramite drag&drop, regolare alcune soglie e inviare la richiesta all'endpoint `/quality/check`. Il risultato viene mostrato a video e, se richiesto, anche le heatmap generate.
