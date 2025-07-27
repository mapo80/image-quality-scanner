### Analisi completa delle immagini della cartella `docs/images/glare`
Di seguito sono riportati i risultati ottenuti eseguendo la libreria su tutte le immagini del dataset. Per ogni file vengono mostrati i valori dei singoli controlli, i tempi di esecuzione e le heatmap generate.


#### 0.jpg

![Originale](docs/images/glare/0.jpg)
![Heatmap Blur](docs/images/glare/0_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/0_glare_heatmap.png)

```
BrisqueScore: 2.25
BlurScore: 47.17
IsBlurry: True
GlareArea: 15565
HasGlare: True
Exposure: 185.57
IsWellExposed: False
Contrast: 39.63
HasLowContrast: False
ColorDominance: 1.03
HasColorDominance: False
Noise: 5.14
HasNoise: False
BandingScore: 0.30
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 15.6877,
  "Blur": 19.1116,
  "MotionBlur": 20.7669,
  "Glare": 15.1851,
  "Exposure": 8.9967,
  "Contrast": 9.7586,
  "ColorDominance": 10.5393,
  "Noise": 177.6526,
  "Banding": 14.6195,
  "BlurHeatmap": 24.3326,
  "GlareHeatmap": 23.4949,
  "BlurRegions": 80.0159,
  "GlareRegions": 24.133,
  "Total": 184.4516
}```

#### 10.jpg

![Originale](docs/images/glare/10.jpg)
![Heatmap Blur](docs/images/glare/10_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/10_glare_heatmap.png)

```
BrisqueScore: 2.51
BlurScore: 159.73
IsBlurry: False
GlareArea: 20889
HasGlare: True
Exposure: 140.98
IsWellExposed: True
Contrast: 39.59
HasLowContrast: False
ColorDominance: 1.03
HasColorDominance: False
Noise: 13.56
HasNoise: False
BandingScore: 0.50
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 125.1084,
  "Blur": 158.1481,
  "MotionBlur": 186.3025,
  "Glare": 129.6533,
  "Exposure": 70.3157,
  "Contrast": 82.3145,
  "ColorDominance": 104.538,
  "Noise": 1472.6693,
  "Banding": 144.2836,
  "BlurHeatmap": 233.3951,
  "GlareHeatmap": 137.1672,
  "BlurRegions": 796.7576,
  "GlareRegions": 193.7917,
  "Total": 1640.5251
}```

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

#### 1004.jpg

![Originale](docs/images/glare/1004.jpg)
![Heatmap Blur](docs/images/glare/1004_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/1004_glare_heatmap.png)

```
BrisqueScore: 6.23
BlurScore: 142.29
IsBlurry: False
GlareArea: 109010
HasGlare: True
Exposure: 108.29
IsWellExposed: True
Contrast: 65.04
HasLowContrast: False
ColorDominance: 1.03
HasColorDominance: False
Noise: 14.32
HasNoise: False
BandingScore: 0.37
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 252.135,
  "Blur": 288.6755,
  "MotionBlur": 295.0154,
  "Glare": 214.4696,
  "Exposure": 139.9129,
  "Contrast": 153.3735,
  "ColorDominance": 160.7234,
  "Noise": 2736.3722,
  "Banding": 226.7632,
  "BlurHeatmap": 446.3935,
  "GlareHeatmap": 262.9878,
  "BlurRegions": 1461.6212,
  "GlareRegions": 373.313,
  "Total": 3053.1142
}```

#### 1005.jpg

![Originale](docs/images/glare/1005.jpg)
![Heatmap Blur](docs/images/glare/1005_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/1005_glare_heatmap.png)

```
BrisqueScore: 5.60
BlurScore: 136.16
IsBlurry: False
GlareArea: 187419
HasGlare: True
Exposure: 105.35
IsWellExposed: True
Contrast: 61.57
HasLowContrast: False
ColorDominance: 1.03
HasColorDominance: False
Noise: 13.55
HasNoise: False
BandingScore: 0.34
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 223.6276,
  "Blur": 280.1835,
  "MotionBlur": 302.2284,
  "Glare": 218.4356,
  "Exposure": 145.8536,
  "Contrast": 189.1261,
  "ColorDominance": 197.4206,
  "Noise": 2813.8327,
  "Banding": 226.1586,
  "BlurHeatmap": 447.7396,
  "GlareHeatmap": 275.0399,
  "BlurRegions": 1501.1295,
  "GlareRegions": 404.0524,
  "Total": 3079.7961
}```

#### 22.jpg

![Originale](docs/images/glare/22.jpg)
![Heatmap Blur](docs/images/glare/22_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/22_glare_heatmap.png)

```
BrisqueScore: 3.57
BlurScore: 1712.19
IsBlurry: False
GlareArea: 27041
HasGlare: True
Exposure: 112.69
IsWellExposed: True
Contrast: 49.71
HasLowContrast: False
ColorDominance: 1.31
HasColorDominance: False
Noise: 157.24
HasNoise: False
BandingScore: 0.63
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 68.327,
  "Blur": 77.388,
  "MotionBlur": 88.9665,
  "Glare": 63.7216,
  "Exposure": 48.807,
  "Contrast": 45.7857,
  "ColorDominance": 53.9152,
  "Noise": 804.7124,
  "Banding": 82.8009,
  "BlurHeatmap": 123.8703,
  "GlareHeatmap": 76.7446,
  "BlurRegions": 331.863,
  "GlareRegions": 122.1881,
  "Total": 777.1683
}```

#### 242.jpg

![Originale](docs/images/glare/242.jpg)
![Heatmap Blur](docs/images/glare/242_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/242_glare_heatmap.png)

```
BrisqueScore: 7.12
BlurScore: 72.30
IsBlurry: True
GlareArea: 1960351
HasGlare: True
Exposure: 137.63
IsWellExposed: True
Contrast: 69.02
HasLowContrast: False
ColorDominance: 1.53
HasColorDominance: True
Noise: 7.50
HasNoise: False
BandingScore: 0.50
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 491.0379,
  "Blur": 430.6366,
  "MotionBlur": 274.715,
  "Glare": 209.9021,
  "Exposure": 128.7669,
  "Contrast": 135.6828,
  "ColorDominance": 139.5617,
  "Noise": 2500.5458,
  "Banding": 227.0688,
  "BlurHeatmap": 403.2514,
  "GlareHeatmap": 253.4803,
  "BlurRegions": 1388.6789,
  "GlareRegions": 497.3371,
  "Total": 2962.1647
}```

#### 266.jpg

![Originale](docs/images/glare/266.jpg)
![Heatmap Blur](docs/images/glare/266_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/266_glare_heatmap.png)

```
BrisqueScore: 2.43
BlurScore: 2650.61
IsBlurry: False
GlareArea: 0
HasGlare: False
Exposure: 154.97
IsWellExposed: True
Contrast: 38.45
HasLowContrast: False
ColorDominance: 1.07
HasColorDominance: False
Noise: 338.73
HasNoise: False
BandingScore: 0.34
HasBanding: False
IsValidDocument: True
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 18.9095,
  "Blur": 23.1455,
  "MotionBlur": 29.0158,
  "Glare": 19.9292,
  "Exposure": 11.5838,
  "Contrast": 12.7653,
  "ColorDominance": 12.6378,
  "Noise": 216.8948,
  "Banding": 18.7364,
  "BlurHeatmap": 36.2844,
  "GlareHeatmap": 22.9233,
  "BlurRegions": 68.5835,
  "GlareRegions": 28.7274,
  "Total": 201.4219
}```

#### 275.jpg

![Originale](docs/images/glare/275.jpg)
![Heatmap Blur](docs/images/glare/275_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/275_glare_heatmap.png)

```
BrisqueScore: 4.13
BlurScore: 2955.31
IsBlurry: False
GlareArea: 49883
HasGlare: True
Exposure: 139.58
IsWellExposed: True
Contrast: 53.12
HasLowContrast: False
ColorDominance: 1.02
HasColorDominance: False
Noise: 370.85
HasNoise: False
BandingScore: 0.38
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 79.7764,
  "Blur": 91.8129,
  "MotionBlur": 101.065,
  "Glare": 75.1856,
  "Exposure": 45.3944,
  "Contrast": 54.2853,
  "ColorDominance": 52.8317,
  "Noise": 912.4437,
  "Banding": 78.9319,
  "BlurHeatmap": 136.6748,
  "GlareHeatmap": 88.374,
  "BlurRegions": 303.4624,
  "GlareRegions": 128.0904,
  "Total": 862.1312
}```

#### 279.jpg

![Originale](docs/images/glare/279.jpg)
![Heatmap Blur](docs/images/glare/279_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/279_glare_heatmap.png)

```
BrisqueScore: 2.04
BlurScore: 57.94
IsBlurry: True
GlareArea: 0
HasGlare: False
Exposure: 130.99
IsWellExposed: True
Contrast: 36.31
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 6.69
HasNoise: False
BandingScore: 0.51
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 271.7349,
  "Blur": 239.4428,
  "MotionBlur": 239.0958,
  "Glare": 181.7388,
  "Exposure": 105.9595,
  "Contrast": 118.8767,
  "ColorDominance": 119.5181,
  "Noise": 2202.3986,
  "Banding": 193.4616,
  "BlurHeatmap": 326.9327,
  "GlareHeatmap": 222.6497,
  "BlurRegions": 1118.1392,
  "GlareRegions": 300.6679,
  "Total": 2482.5766
}```

#### 281.jpg

![Originale](docs/images/glare/281.jpg)
![Heatmap Blur](docs/images/glare/281_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/281_glare_heatmap.png)

```
BrisqueScore: 8.73
BlurScore: 294.10
IsBlurry: False
GlareArea: 86224
HasGlare: True
Exposure: 66.86
IsWellExposed: False
Contrast: 77.73
HasLowContrast: False
ColorDominance: 1.05
HasColorDominance: False
Noise: 35.50
HasNoise: False
BandingScore: 0.36
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 194.7935,
  "Blur": 229.9015,
  "MotionBlur": 243.1966,
  "Glare": 191.7702,
  "Exposure": 104.9622,
  "Contrast": 117.1058,
  "ColorDominance": 125.9274,
  "Noise": 2211.8352,
  "Banding": 189.6591,
  "BlurHeatmap": 357.2618,
  "GlareHeatmap": 202.1391,
  "BlurRegions": 1032.062,
  "GlareRegions": 321.7403,
  "Total": 2324.7831
}```

#### 293.jpg

![Originale](docs/images/glare/293.jpg)
![Heatmap Blur](docs/images/glare/293_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/293_glare_heatmap.png)

```
BrisqueScore: 6.74
BlurScore: 118.15
IsBlurry: False
GlareArea: 2289
HasGlare: True
Exposure: 144.72
IsWellExposed: True
Contrast: 66.52
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 14.82
HasNoise: False
BandingScore: 0.65
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 282.9631,
  "Blur": 295.4545,
  "MotionBlur": 291.7132,
  "Glare": 215.4066,
  "Exposure": 125.4952,
  "Contrast": 142.5822,
  "ColorDominance": 136.4146,
  "Noise": 2514.1507,
  "Banding": 201.4681,
  "BlurHeatmap": 401.663,
  "GlareHeatmap": 234.6935,
  "BlurRegions": 1206.2018,
  "GlareRegions": 327.7038,
  "Total": 2707.5956
}```

#### 313.jpg

![Originale](docs/images/glare/313.jpg)
![Heatmap Blur](docs/images/glare/313_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/313_glare_heatmap.png)

```
BrisqueScore: 1.33
BlurScore: 70.67
IsBlurry: True
GlareArea: 4336
HasGlare: True
Exposure: 130.16
IsWellExposed: True
Contrast: 29.84
HasLowContrast: True
ColorDominance: 1.05
HasColorDominance: False
Noise: 7.91
HasNoise: False
BandingScore: 0.32
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 129.0987,
  "Blur": 150.4935,
  "MotionBlur": 158.9169,
  "Glare": 113.3555,
  "Exposure": 76.2796,
  "Contrast": 81.3018,
  "ColorDominance": 88.0414,
  "Noise": 1528.6146,
  "Banding": 171.6084,
  "BlurHeatmap": 286.1687,
  "GlareHeatmap": 132.9045,
  "BlurRegions": 692.0074,
  "GlareRegions": 190.7878,
  "Total": 1574.1277
}```

#### 326.jpg

![Originale](docs/images/glare/326.jpg)
![Heatmap Blur](docs/images/glare/326_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/326_glare_heatmap.png)

```
BrisqueScore: 4.50
BlurScore: 192.51
IsBlurry: False
GlareArea: 2737
HasGlare: True
Exposure: 150.70
IsWellExposed: True
Contrast: 54.76
HasLowContrast: False
ColorDominance: 1.08
HasColorDominance: False
Noise: 18.76
HasNoise: False
BandingScore: 0.24
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 282.8827,
  "Blur": 439.9489,
  "MotionBlur": 310.1481,
  "Glare": 172.3087,
  "Exposure": 114.1752,
  "Contrast": 120.4066,
  "ColorDominance": 119.7868,
  "Noise": 2377.0095,
  "Banding": 185.343,
  "BlurHeatmap": 343.9891,
  "GlareHeatmap": 220.4908,
  "BlurRegions": 1390.3561,
  "GlareRegions": 311.7037,
  "Total": 3060.3246
}```

#### 447.jpg

![Originale](docs/images/glare/447.jpg)
![Heatmap Blur](docs/images/glare/447_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/447_glare_heatmap.png)

```
BrisqueScore: 2.21
BlurScore: 151.87
IsBlurry: False
GlareArea: 3425
HasGlare: True
Exposure: 104.84
IsWellExposed: True
Contrast: 38.48
HasLowContrast: False
ColorDominance: 1.12
HasColorDominance: False
Noise: 14.53
HasNoise: False
BandingScore: 0.47
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 188.293,
  "Blur": 223.2637,
  "MotionBlur": 238.6637,
  "Glare": 177.0549,
  "Exposure": 131.3287,
  "Contrast": 140.0386,
  "ColorDominance": 129.8762,
  "Noise": 2189.3721,
  "Banding": 178.6488,
  "BlurHeatmap": 312.4791,
  "GlareHeatmap": 217.9117,
  "BlurRegions": 1145.5336,
  "GlareRegions": 283.6001,
  "Total": 2451.4645
}```

#### 482.jpg

![Originale](docs/images/glare/482.jpg)
![Heatmap Blur](docs/images/glare/482_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/482_glare_heatmap.png)

```
BrisqueScore: 8.39
BlurScore: 462.68
IsBlurry: False
GlareArea: 235787
HasGlare: True
Exposure: 164.06
IsWellExposed: True
Contrast: 74.36
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 54.36
HasNoise: False
BandingScore: 0.42
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 114.0162,
  "Blur": 128.1285,
  "MotionBlur": 145.6131,
  "Glare": 116.2612,
  "Exposure": 70.2102,
  "Contrast": 70.8071,
  "ColorDominance": 71.1871,
  "Noise": 1289.3685,
  "Banding": 112.3391,
  "BlurHeatmap": 186.4025,
  "GlareHeatmap": 118.3083,
  "BlurRegions": 584.7136,
  "GlareRegions": 186.2527,
  "Total": 1342.7577
}```

#### 497.jpg

![Originale](docs/images/glare/497.jpg)
![Heatmap Blur](docs/images/glare/497_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/497_glare_heatmap.png)

```
BrisqueScore: 9.77
BlurScore: 279.96
IsBlurry: False
GlareArea: 865752
HasGlare: True
Exposure: 139.96
IsWellExposed: True
Contrast: 80.17
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 29.06
HasNoise: False
BandingScore: 0.59
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 115.1036,
  "Blur": 155.6974,
  "MotionBlur": 142.7669,
  "Glare": 112.2457,
  "Exposure": 66.7368,
  "Contrast": 73.5901,
  "ColorDominance": 73.9079,
  "Noise": 1302.9052,
  "Banding": 106.5344,
  "BlurHeatmap": 209.8346,
  "GlareHeatmap": 119.7383,
  "BlurRegions": 614.4406,
  "GlareRegions": 215.3219,
  "Total": 1418.2305
}```

#### 523.jpg

![Originale](docs/images/glare/523.jpg)
![Heatmap Blur](docs/images/glare/523_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/523_glare_heatmap.png)

```
BrisqueScore: 5.01
BlurScore: 588.79
IsBlurry: False
GlareArea: 188040
HasGlare: True
Exposure: 181.62
IsWellExposed: False
Contrast: 57.42
HasLowContrast: False
ColorDominance: 1.00
HasColorDominance: False
Noise: 69.66
HasNoise: False
BandingScore: 0.40
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 102.1183,
  "Blur": 141.8516,
  "MotionBlur": 142.0966,
  "Glare": 103.7393,
  "Exposure": 68.2897,
  "Contrast": 69.1921,
  "ColorDominance": 75.9115,
  "Noise": 1274.9492,
  "Banding": 113.2861,
  "BlurHeatmap": 200.1032,
  "GlareHeatmap": 135.4984,
  "BlurRegions": 570.5423,
  "GlareRegions": 191.8435,
  "Total": 1360.002
}```

#### 65.jpg

![Originale](docs/images/glare/65.jpg)
![Heatmap Blur](docs/images/glare/65_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/65_glare_heatmap.png)

```
BrisqueScore: 3.54
BlurScore: 58.27
IsBlurry: True
GlareArea: 4353
HasGlare: True
Exposure: 158.11
IsWellExposed: True
Contrast: 50.77
HasLowContrast: False
ColorDominance: 1.06
HasColorDominance: False
Noise: 5.81
HasNoise: False
BandingScore: 0.41
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 189.5197,
  "Blur": 238.3456,
  "MotionBlur": 287.8923,
  "Glare": 320.6492,
  "Exposure": 124.587,
  "Contrast": 128.5518,
  "ColorDominance": 126.3569,
  "Noise": 2363.6733,
  "Banding": 188.6989,
  "BlurHeatmap": 373.465,
  "GlareHeatmap": 229.7972,
  "BlurRegions": 1187.2833,
  "GlareRegions": 320.8455,
  "Total": 2679.073
}```

#### 743.jpg

![Originale](docs/images/glare/743.jpg)
![Heatmap Blur](docs/images/glare/743_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/743_glare_heatmap.png)

```
BrisqueScore: 4.07
BlurScore: 154.81
IsBlurry: False
GlareArea: 38204
HasGlare: True
Exposure: 99.34
IsWellExposed: True
Contrast: 52.49
HasLowContrast: False
ColorDominance: 1.01
HasColorDominance: False
Noise: 16.46
HasNoise: False
BandingScore: 0.50
HasBanding: True
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 247.4739,
  "Blur": 272.0974,
  "MotionBlur": 335.4817,
  "Glare": 230.2392,
  "Exposure": 143.6215,
  "Contrast": 160.9321,
  "ColorDominance": 152.7799,
  "Noise": 2807.3469,
  "Banding": 229.3334,
  "BlurHeatmap": 403.225,
  "GlareHeatmap": 282.8826,
  "BlurRegions": 1433.947,
  "GlareRegions": 361.9329,
  "Total": 3093.9929
}```

#### 988.jpg

![Originale](docs/images/glare/988.jpg)
![Heatmap Blur](docs/images/glare/988_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/988_glare_heatmap.png)

```
BrisqueScore: 5.33
BlurScore: 164.77
IsBlurry: False
GlareArea: 34638
HasGlare: True
Exposure: 102.00
IsWellExposed: True
Contrast: 60.47
HasLowContrast: False
ColorDominance: 1.02
HasColorDominance: False
Noise: 17.77
HasNoise: False
BandingScore: 0.35
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 225.0559,
  "Blur": 278.1519,
  "MotionBlur": 313.3395,
  "Glare": 225.6523,
  "Exposure": 139.2023,
  "Contrast": 153.173,
  "ColorDominance": 152.8469,
  "Noise": 2709.8974,
  "Banding": 233.9327,
  "BlurHeatmap": 437.7544,
  "GlareHeatmap": 261.308,
  "BlurRegions": 1440.3591,
  "GlareRegions": 360.6462,
  "Total": 3122.0476
}```

#### 997.jpg

![Originale](docs/images/glare/997.jpg)
![Heatmap Blur](docs/images/glare/997_blur_heatmap.png)
![Heatmap Glare](docs/images/glare/997_glare_heatmap.png)

```
BrisqueScore: 5.90
BlurScore: 213.89
IsBlurry: False
GlareArea: 175145
HasGlare: True
Exposure: 112.30
IsWellExposed: True
Contrast: 63.19
HasLowContrast: False
ColorDominance: 1.02
HasColorDominance: False
Noise: 21.34
HasNoise: False
BandingScore: 0.32
HasBanding: False
IsValidDocument: False
```

Tempi di esecuzione (ms):
```
{
  "Brisque": 222.8432,
  "Blur": 263.4741,
  "MotionBlur": 312.7626,
  "Glare": 232.7479,
  "Exposure": 145.4851,
  "Contrast": 150.1722,
  "ColorDominance": 145.8296,
  "Noise": 2713.544,
  "Banding": 231.4641,
  "BlurHeatmap": 424.6189,
  "GlareHeatmap": 265.7964,
  "BlurRegions": 1397.9359,
  "GlareRegions": 418.7523,
  "Total": 3100.7624
}```

